using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndController : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject losePanel;
    public string mainMenuSceneName = "MenuPrincipal";
    public bool isHost = false;

    private TCPServer tcpServer;
    private TCPClient tcpClient;
    private bool gameEnded = false;

    private readonly Queue<string> incoming = new Queue<string>();
    private readonly object incomingLock = new object();
    private System.Action<string> enqueueHandler;

    private void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        tcpServer = FindFirstObjectByType<TCPServer>();
        tcpClient = FindFirstObjectByType<TCPClient>();

        isHost = isHost || (tcpServer != null && tcpServer.isActiveAndEnabled);

        enqueueHandler = (m) => Enqueue(m);

        if (tcpServer != null) tcpServer.OnDataReceived += enqueueHandler;
        if (tcpClient != null) tcpClient.OnDataReceived += enqueueHandler;
    }

    private void OnDestroy()
    {
        if (tcpServer != null && enqueueHandler != null) tcpServer.OnDataReceived -= enqueueHandler;
        if (tcpClient != null && enqueueHandler != null) tcpClient.OnDataReceived -= enqueueHandler;
    }

    private void Update()
    {
        while (true)
        {
            string msg = null;
            lock (incomingLock)
            {
                if (incoming.Count > 0) msg = incoming.Dequeue();
                else break;
            }

            if (!string.IsNullOrEmpty(msg)) ProcessNetworkMessage(msg);
        }
    }

    private void Enqueue(string msg)
    {
        lock (incomingLock) incoming.Enqueue(msg);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameEnded) return;

        var clientComp = other.GetComponent<ClientPlayer>();
        if (clientComp != null && !string.IsNullOrEmpty(ClientPlayer.LocalClientId))
        {
            ShowWinLocalAndNotify(ClientPlayer.LocalClientId);
            return;
        }

        var hostComp = other.GetComponent<HostPlayer>();
        if (hostComp != null && isHost)
        {
            ShowWinLocalAndNotify(HostPlayer.HostId);
            return;
        }
    }

    private void ShowWinLocalAndNotify(string winnerId)
    {
        if (gameEnded || string.IsNullOrEmpty(winnerId)) return;

        gameEnded = true;
        ShowWin();

        if (isHost)
        {
            tcpServer?.Broadcast($"WINNER|{winnerId}");
        }
        else
        {
            tcpClient?.Send($"{winnerId}|WIN");
        }
    }

    private void ProcessNetworkMessage(string msg)
    {
        if (string.IsNullOrEmpty(msg)) return;

        // Mensaje enviado por el host a todos los clientes
        if (msg.StartsWith("WINNER|"))
        {
            string id = msg.Substring("WINNER|".Length);
            if (gameEnded) return;

            bool amILocalWinner = (isHost && id == HostPlayer.HostId) ||
                                  (!isHost && id == ClientPlayer.LocalClientId);

            if (amILocalWinner) ShowWin();
            else ShowLose();

            gameEnded = true;
            return;
        }

        // Mensaje enviado por un cliente al host
        var parts = msg.Split('|');
        if (parts.Length >= 2 && parts[1] == "WIN")
        {
            string winnerId = parts[0];

            if (isHost && tcpServer != null)
            {
                // El host retransmite el ID del ganador que recibió (no el suyo)
                tcpServer.Broadcast($"WINNER|{winnerId}");

                // El host mismo solo gana si el ID recibido es el suyo
                bool hostIsWinner = (winnerId == HostPlayer.HostId);
                if (!gameEnded)
                {
                    if (hostIsWinner) ShowWin();
                    else ShowLose();
                    gameEnded = true;
                }
            }
        }
    }

    private void ShowWin()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (losePanel != null) losePanel.SetActive(false);
        Time.timeScale = 0f;
    }

    private void ShowLose()
    {
        if (losePanel != null) losePanel.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        try
        {
            tcpClient?.Disconnect();
            tcpServer?.StopServer();
        }
        catch { }
        SceneManager.LoadScene(mainMenuSceneName);
    }
}