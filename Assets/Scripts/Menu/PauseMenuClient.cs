// PauseMenuClient.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuClient : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public GameObject buttonsGroup;

    [Header("Network References")]
    public TCPClient tcpClient;

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (tcpClient == null) tcpClient = GetComponent<TCPClient>();
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

    public void DisconnectClient()
    {
        try
        {
            if (tcpClient == null) tcpClient = GetComponent<TCPClient>();
            if (tcpClient != null)
            {
                var id = ClientPlayer.LocalClientId;
                if (!string.IsNullOrEmpty(id)) tcpClient.Send(id + "|DISCONNECT");
                tcpClient.Disconnect();
            }
        }
        catch { }
        Resume();
    }

    public void ReturnToMainMenu(string sceneName)
    {
        Time.timeScale = 1f;
        try
        {
            if (tcpClient == null) tcpClient = GetComponent<TCPClient>();
            if (tcpClient != null) tcpClient.Disconnect();
        }
        catch { }

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}