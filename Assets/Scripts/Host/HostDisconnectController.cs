using UnityEngine;

public class HostDisconnectController : MonoBehaviour
{
    [SerializeField] private TCPServer tcpServer;
    [SerializeField] private GameObject hostPlayerObject;

    public void DisconnectHostAndNotify()
    {
        if (tcpServer == null) tcpServer = GetComponent<TCPServer>();
        if (tcpServer == null) return;
        try
        {
            tcpServer.Broadcast("SERVER_SHUTDOWN");
        }
        catch { }
        try
        {
            tcpServer.StopServer();
        }
        catch { }
    }

    public void LocalDisconnectHost()
    {
        if (tcpServer == null) tcpServer = GetComponent<TCPServer>();
        try
        {
            tcpServer?.Broadcast("HOST|DISCONNECT");
        }
        catch { }

        if (hostPlayerObject == null)
        {
            var hp = FindFirstObjectByType<HostPlayer>();
            if (hp != null) hostPlayerObject = hp.gameObject;
        }

        try
        {
            if (hostPlayerObject != null) Destroy(hostPlayerObject);
        }
        catch { }
    }
}