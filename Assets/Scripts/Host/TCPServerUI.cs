using UnityEngine;

public class TCPServerUI : MonoBehaviour
{
    public int serverPort = 5555;
    private TCPServer server;

    private void Start()
    {
        server = FindFirstObjectByType<TCPServer>();
        if (server == null)
        {
            Debug.LogError("No se encontró TCPServer en la escena.");
            return;
        }
        server.StartServer(serverPort);
        Debug.Log($"[TCPServerUI] Servidor iniciado en puerto {serverPort}");
    }
}