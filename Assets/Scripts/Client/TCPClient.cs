// TCPClient.cs
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;
    private Thread receiveThread;
    private volatile bool connected;

    public Action<string> OnDataReceived;

    public void ConnectToServer(string ipAddress, int port)
    {
        if (connected) return;
        client = new TcpClient();
        client.Connect(ipAddress, port);
        stream = client.GetStream();
        reader = new StreamReader(stream, Encoding.UTF8);
        writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        connected = true;
        receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
        receiveThread.Start();
    }

    public void Disconnect()
    {
        connected = false;
        try { client?.Close(); } catch { }
        try { receiveThread?.Join(); } catch { }
    }

    private void ReceiveLoop()
    {
        while (connected && client != null && client.Connected)
        {
            try
            {
                var line = reader.ReadLine();
                if (line == null) break;
                try { OnDataReceived?.Invoke(line); } catch { }
            }
            catch
            {
                break;
            }
        }
        connected = false;
    }

    public void Send(string message)
    {
        if (writer == null) return;
        try { writer.WriteLine(message); } catch { }
    }
}