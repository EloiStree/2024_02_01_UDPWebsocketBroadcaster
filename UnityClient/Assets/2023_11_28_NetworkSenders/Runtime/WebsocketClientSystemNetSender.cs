using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Threading;
using System.Net.WebSockets;

//Source: https://gamedev.stackexchange.com/questions/148975/websockets-server-for-unity3d

public class WebsocketClientSystemNetSender: MonoBehaviour
{
    public string m_serverUrl = "ws://127.0.0.1:8080";
    ClientWebSocket cws = null;
    ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);

    void Start() { Connect(); }

    async void Connect()
    {
        Uri u = new Uri(m_serverUrl);
        cws = new ClientWebSocket();
        try
        {
            await cws.ConnectAsync(u, CancellationToken.None);
            if (cws.State == WebSocketState.Open) Debug.Log("connected");
            SayHello();
            GetStuff();
        }
        catch (Exception e) { Debug.Log("woe " + e.Message); }
    }

    [ContextMenu("Say Hello")]
    async void SayHello()
    {
        ArraySegment<byte> b = new ArraySegment<byte>(Encoding.UTF8.GetBytes("hello"));
        await cws.SendAsync(b, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    async void GetStuff()
    {
        WebSocketReceiveResult r = await cws.ReceiveAsync(buf, CancellationToken.None);
        Debug.Log("Got: " + Encoding.UTF8.GetString(buf.Array, 0, r.Count));
        GetStuff();
    }
}
