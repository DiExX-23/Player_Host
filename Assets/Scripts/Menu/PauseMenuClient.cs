// PauseMenuClient.cs (versión revisada)
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuClient : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public GameObject buttonsGroup;

    [Header("Network References")]
    public TCPClient tcpClient;
    public GameObject localPlayerObject; // Nuevo: para limpiar visualmente al jugador local

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (tcpClient == null) tcpClient = GetComponent<TCPClient>();
        Time.timeScale = 1f;
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

    /// <summary>
    /// Realiza la desconexión limpia del cliente sin afectar a otros jugadores.
    /// Envía el mensaje de desconexión si el servidor lo requiere y corta la conexión TCP.
    /// </summary>
    public void DisconnectClient()
    {
        SafeDisconnect();
        Resume();
    }

    /// <summary>
    /// Desconecta al cliente y carga la escena del menú principal.
    /// </summary>
    public void ReturnToMainMenu(string sceneName)
    {
        SafeDisconnect();
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Encapsula la lógica de desconexión de red y limpieza local.
    /// </summary>
    private void SafeDisconnect()
    {
        try
        {
            if (tcpClient == null)
                tcpClient = GetComponent<TCPClient>();

            if (tcpClient != null)
            {
                var id = ClientPlayer.LocalClientId;

                if (!string.IsNullOrEmpty(id))
                {
                    // Notifica al servidor que este cliente se desconecta
                    tcpClient.Send($"{id}|DISCONNECT");
                }

                tcpClient.Disconnect();
            }

            // Limpieza local del jugador para evitar artefactos visuales
            if (localPlayerObject != null)
            {
                Destroy(localPlayerObject);
            }
            else
            {
                var possibleLocal = GameObject.FindWithTag("Player");
                if (possibleLocal != null)
                    Destroy(possibleLocal);
            }
        }
        catch (System.Exception ex)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Error al intentar desconectar cliente: {ex.Message}");
#endif
        }
    }
}