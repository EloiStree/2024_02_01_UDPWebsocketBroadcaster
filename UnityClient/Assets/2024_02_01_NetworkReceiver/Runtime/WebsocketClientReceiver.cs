using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;
using UnityEngine.Events;

public class WebsocketClientReceiver : MonoBehaviour
{
    public string m_websocketServer = "ws://localhost:7070";
    WebSocket websocket;

    public string m_lastReceived;



    public StringEvent m_onReceivedMessageUTF8;
    public BytesEvent m_onReceivedAsBytes;
    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }
    [System.Serializable]
    public class BytesEvent : UnityEvent<byte[]> { }

    public bool m_isConnected;
    async void Start()
    {
        websocket = new WebSocket(m_websocketServer);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            m_isConnected = true;
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
            m_isConnected = false;
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            m_isConnected = false;
        };

        websocket.OnMessage += (bytes) =>
        {
            m_onReceivedAsBytes.Invoke(bytes);
            //getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            m_lastReceived = message;
            m_onReceivedMessageUTF8.Invoke(message);
        };
        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

  
    private async void OnApplicationQuit()
    {
        await websocket.Close();
        m_isConnected = false;
    }

}