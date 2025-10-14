using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    [Header("Paneles del menú")]
    public GameObject panelPrincipal;
    public GameObject panelOpciones;

    public void CambiarPanel(GameObject panelActivo)
    {
        panelPrincipal.SetActive(false);
        panelOpciones.SetActive(false);
        panelActivo.SetActive(true);
    }

    public void VolverAlMenuPrincipal()
    {
        panelPrincipal.SetActive(true);
        panelOpciones.SetActive(false);
    }

    public void JugarEscena(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        SceneManager.LoadScene(sceneName);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}