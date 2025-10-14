using UnityEngine;

public class TCPClientUI : MonoBehaviour
{
    public string serverAddress = "127.0.0.1";
    public int serverPort = 5555;
    private TCPClient client;

    private void Start()
    {
        client = FindFirstObjectByType<TCPClient>();
        if (client == null)
        {
            Debug.LogError("No se encontró TCPClient en la escena.");
            return;
        }
        client.ConnectToServer(serverAddress, serverPort);
        Debug.Log($"[TCPClientUI] Intentando conectar a {serverAddress}:{serverPort}");
    }
}