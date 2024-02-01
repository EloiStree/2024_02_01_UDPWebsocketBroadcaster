using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class UdpSender : MonoBehaviour
{
    public string ipAddress = "127.0.0.1";
    public int port = 12345;

    private UdpClient udpClient;
    public string m_quickStringTest;
    public string m_startWith;
    public string m_endWith;


    void Start()
    {
        udpClient = new UdpClient();
        SendStringUTF8("Hello, UDP!");
    }

    [ContextMenu("Quick String Test")]
    public void SendStringUTF8_StringTest() => SendStringUTF8(m_quickStringTest);
    [ContextMenu("Ping")]
    public void SendStringUTF8_Ping() => SendStringUTF8("Ping");
    [ContextMenu("Hello")]
    public void SendStringUTF8_Hello() => SendStringUTF8("Hello");
    [ContextMenu("Date")]
    public void SendStringUTF8_Date() => SendStringUTF8(DateTime.Now.ToString());

    public void SendStringUTF8(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(m_startWith+message+m_endWith);
            udpClient.Send(data, data.Length, ipAddress, port);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending UDP message: {e.Message}");
        }
    }
    public void SendStringUnicode(string message)
    {
        try
        {
            byte[] data = Encoding.Unicode.GetBytes(m_startWith + message + m_endWith);
            udpClient.Send(data, data.Length, ipAddress, port);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending UDP message: {e.Message}");
        }
    }

    void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
