// TCPServer.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    private TcpListener listener;
    private readonly List<TcpClient> clients = new List<TcpClient>();
    private Thread acceptThread;
    private readonly List<Thread> clientThreads = new List<Thread>();
    private volatile bool running;

    public Action<string> OnDataReceived;
    public bool IsRunning => running;

    public void StartServer(int port)
    {
        if (running) return;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        running = true;
        acceptThread = new Thread(AcceptLoop) { IsBackground = true };
        acceptThread.Start();
    }

    public void StopServer()
    {
        running = false;
        try { listener?.Stop(); } catch { }
        lock (clients)
        {
            foreach (var c in clients) try { c.Close(); } catch { }
            clients.Clear();
        }
        foreach (var t in clientThreads) try { t.Join(); } catch { }
        try { acceptThread?.Join(); } catch { }
    }

    private void AcceptLoop()
    {
        while (running)
        {
            try
            {
                var client = listener.AcceptTcpClient();
                lock (clients) clients.Add(client);
                var t = new Thread(() => ClientLoop(client)) { IsBackground = true };
                clientThreads.Add(t);
                t.Start();
            }
            catch
            {
                if (!running) break;
            }
        }
    }

    private void ClientLoop(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            while (running && client.Connected)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                try { OnDataReceived?.Invoke(line); } catch { }
            }
        }
        catch { }
        lock (clients) clients.Remove(client);
        try { client.Close(); } catch { }
    }

    public void Broadcast(string message)
    {
        List<TcpClient> snapshot;
        lock (clients) snapshot = new List<TcpClient>(clients);
        foreach (var c in snapshot)
        {
            if (!c.Connected) continue;
            try
            {
                var stream = c.GetStream();
                var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                writer.WriteLine(message);
            }
            catch { }
        }
    }
}