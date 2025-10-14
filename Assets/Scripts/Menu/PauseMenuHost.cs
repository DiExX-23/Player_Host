// PauseMenuHost.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuHost : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public GameObject buttonsGroup;

    [Header("Network References")]
    public TCPServer tcpServer;
    public GameObject hostPlayerObject;

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (tcpServer == null) tcpServer = GetComponent<TCPServer>();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void CloseServerAndNotify()
    {
        try
        {
            if (tcpServer != null)
            {
                tcpServer.Broadcast("SERVER_SHUTDOWN");
                tcpServer.StopServer();
            }
        }
        catch { }
        Resume();
    }

    public void LocalDisconnectHost()
    {
        try
        {
            if (tcpServer == null) tcpServer = GetComponent<TCPServer>();
            if (tcpServer != null) tcpServer.Broadcast("HOST|DISCONNECT");
        }
        catch { }

        if (hostPlayerObject == null)
        {
#if UNITY_2023_1_OR_NEWER
            var hp = Object.FindFirstObjectByType<HostPlayer>();
#else
            var hp = Object.FindObjectOfType<HostPlayer>();
#endif
            if (hp != null) hostPlayerObject = hp.gameObject;
        }

        try
        {
            if (hostPlayerObject != null) Destroy(hostPlayerObject);
        }
        catch { }

        Resume();
    }

    public void ReturnToMainMenu(string sceneName)
    {
        Time.timeScale = 1f;
        try
        {
            if (tcpServer == null) tcpServer = GetComponent<TCPServer>();
            if (tcpServer != null)
            {
                tcpServer.Broadcast("SERVER_SHUTDOWN");
                tcpServer.StopServer();
            }
        }
        catch { }

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}