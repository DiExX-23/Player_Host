using UnityEngine;

public class ClientDisconnectController : MonoBehaviour
{
    [SerializeField] private TCPClient tcpClient;

    public void DisconnectClient()
    {
        if (tcpClient == null) tcpClient = GetComponent<TCPClient>();
        if (tcpClient == null) return;
        try
        {
            var id = ClientPlayer.LocalClientId;
            if (!string.IsNullOrEmpty(id))
            {
                tcpClient.Send(id + "|DISCONNECT");
            }
        }
        catch { }
        try { tcpClient.Disconnect(); } catch { }
    }
}