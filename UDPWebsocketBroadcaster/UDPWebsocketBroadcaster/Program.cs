using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class WebSocketServer
{
    private const int BufferSize = 4096;
    private HttpListener httpListener;
    private UdpClient udpListener;
    private readonly ConcurrentDictionary<string, WebSocket> connectedClients = new ConcurrentDictionary<string, WebSocket>();
    private string broadcastMessage = "Default broadcast message";

    public async Task Start(string httpListenerPrefix, int udpListenerPort)
    {
        httpListener = new HttpListener();
        httpListener.Prefixes.Add(httpListenerPrefix);
        httpListener.Start();

        udpListener = new UdpClient(udpListenerPort);
        Console.WriteLine($"UDP listener is running on port {udpListenerPort}");

        Console.WriteLine("WebSocket server is running...");

        // Start a background task to broadcast messages every 1 second
        Task.Run(() => BroadcastMessages());

        // Start a background task to listen for UDP messages
        Task.Run(() => ListenForUdpMessages());

        while (true)
        {
            HttpListenerContext context = await httpListener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                ProcessWebSocketRequest(context);
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private async void ProcessWebSocketRequest(HttpListenerContext context)
    {
        HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);

        WebSocket webSocket = webSocketContext.WebSocket;
        string clientId = Guid.NewGuid().ToString(); // Assign a unique identifier to each client

        connectedClients.TryAdd(clientId, webSocket);

        try
        {
            byte[] buffer = new byte[BufferSize];

            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received message from {clientId}: {receivedMessage}");

                    // You can handle the received message here if needed
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    connectedClients.TryRemove(clientId, out _);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error for client {clientId}: {ex.Message}");
        }
    }

    private async Task BroadcastMessages()
    {
        while (true)
        {
            foreach (var client in connectedClients)
            {
                try
                {
                    if (client.Value.State == WebSocketState.Open)
                    {
                        string message = broadcastMessage;
                        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                        await client.Value.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        // Remove disconnected client
                        connectedClients.TryRemove(client.Key, out _);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to broadcast message to client {client.Key}: {ex.Message}");
                }
            }

            // Wait for 1 second before sending the next broadcast
            await Task.Delay(1000);
        }
    }

    private async Task ListenForUdpMessages()
    {
        while (true)
        {
            UdpReceiveResult result = await udpListener.ReceiveAsync();

            string udpMessage = Encoding.UTF8.GetString(result.Buffer);
            TimeWatch.Start();

            Console.WriteLine($"Received UDP message: {udpMessage}");

            // Set the broadcast message to the received UDP message
            broadcastMessage = udpMessage;

            // Broadcast the UDP message to connected WebSocket clients
            foreach (var client in connectedClients)
            {
                try
                {
                    if (client.Value.State == WebSocketState.Open)
                    {
                        byte[] udpMessageBytes = Encoding.UTF8.GetBytes(udpMessage);
                        await client.Value.SendAsync(new ArraySegment<byte>(udpMessageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        // Remove disconnected client
                        connectedClients.TryRemove(client.Key, out _);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send UDP message to client {client.Key}: {ex.Message}");
                }
            }
            TimeWatch.End();
            Console.WriteLine($"Push UDP to clients (Read and push): {TimeWatch.GetSeconds()}");

        }
    }

    public void Stop()
    {
        httpListener.Stop();
        httpListener.Close();
        udpListener.Close();
    }
}

public class TimeWatch
{
    public static DateTime m_startTime;
    public static DateTime m_endTime;

    public static void Start() { m_startTime = DateTime.Now; }
    public static void End() { m_endTime = DateTime.Now; }
    public static double GetSeconds() { return (m_endTime - m_startTime).TotalSeconds; }
}

class Program
{
    static async Task Main(string[] args)
    {
        string httpListenerPrefix = "http://localhost:7070/";
        int udpListenerPort = 12345;

        WebSocketServer server = new WebSocketServer();
        await server.Start(httpListenerPrefix, udpListenerPort);

        Console.WriteLine("Press any key to stop the server...");
        Console.ReadKey();

        server.Stop();
    }
}
