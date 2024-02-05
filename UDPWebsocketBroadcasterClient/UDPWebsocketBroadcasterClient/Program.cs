using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class WebSocketClient
{
    private const string ServerUri = "ws://81.240.94.97:7070/";

    public async Task ConnectAndRun()
    {
        while (true)
        {
            using (ClientWebSocket webSocket = new ClientWebSocket())
            {
                try
                {
                    Console.WriteLine($"Connecting to server: {ServerUri}");
                    await webSocket.ConnectAsync(new Uri(ServerUri), CancellationToken.None);

                    // Start a background task to receive messages from the server
                    Task.Run(() => ReceiveMessages(webSocket));

                    // Send a message to the server every 3 seconds
                    while (webSocket.State == WebSocketState.Open)
                    {
                        string message = $"Client message at {DateTime.Now}";
                        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                        await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                        // Wait for 3 seconds before sending the next message
                        await Task.Delay(3000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebSocket error: {ex.Message}");

                    // Handle reconnection logic
                    Console.WriteLine("Reconnecting in 5 seconds...");
                    await Task.Delay(5000);
                }
            }
        }
    }

    private async Task ReceiveMessages(ClientWebSocket webSocket)
    {
        byte[] buffer = new byte[4096];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received message from server: {receivedMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");

            // Handle reconnection logic
            Console.WriteLine("Reconnecting in 5 seconds...");
            await Task.Delay(5000);
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        WebSocketClient client = new WebSocketClient();
        await client.ConnectAndRun();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
