// ServerRebroadcaster.cs
using UnityEngine;

public class ServerRebroadcaster : MonoBehaviour
{
    [SerializeField] private TCPServer tcpServer;

    private void Start()
    {
        if (tcpServer == null) tcpServer = GetComponent<TCPServer>();
        if (tcpServer != null) tcpServer.OnDataReceived += Rebroadcast;
    }

    private void OnDestroy()
    {
        if (tcpServer != null) tcpServer.OnDataReceived -= Rebroadcast;
    }

    private void Rebroadcast(string msg)
    {
        if (tcpServer == null) return;
        tcpServer.Broadcast(msg);
    }
}