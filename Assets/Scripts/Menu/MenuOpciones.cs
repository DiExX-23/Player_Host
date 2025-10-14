using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MenuOpciones : MonoBehaviour
{
    [Header("Brillo")]
    public Slider sliderBrillo;
    public float sliderValueBrillo;
    public Image panelBrillo;

    [Header("Volumen")]
    public Slider sliderVolumen;
    public float sliderValueVolumen;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    void Start()
    {
        sliderBrillo.value = PlayerPrefs.GetFloat("brillo", 0.5f);
        float alpha = 1f - sliderBrillo.value;
        alpha = Mathf.Clamp(alpha, 0f, 1f);
        panelBrillo.color = new Color(panelBrillo.color.r, panelBrillo.color.g, panelBrillo.color.b, alpha);

        sliderVolumen.value = PlayerPrefs.GetFloat("volumenAudio", 0.5f);
        AudioListener.volume = sliderVolumen.value;
        RevisarSiEstoyMute();
    }

    public void PantallaCompleta(bool pantallaCompleta)
    {
        Screen.fullScreen = pantallaCompleta;
    }

    public void ChangeBrillo(float valor)
    {
        sliderValueBrillo = valor;
        PlayerPrefs.SetFloat("brillo", sliderValueBrillo);
        float alpha = 1f - sliderValueBrillo;
        alpha = Mathf.Clamp(alpha, 0f, 1f);
        panelBrillo.color = new Color(panelBrillo.color.r, panelBrillo.color.g, panelBrillo.color.b, alpha);
    }

    public void ChangeVolumen(float valor)
    {
        sliderValueVolumen = valor;
        PlayerPrefs.SetFloat("volumenAudio", sliderValueVolumen);
        AudioListener.volume = sliderVolumen.value;
        RevisarSiEstoyMute();
    }

    void RevisarSiEstoyMute()
    {
        if (sliderVolumen.value <= 0.01f)
        {
            Debug.Log("El juego está en mute.");
        }
        else
        {
            Debug.Log("El juego tiene volumen.");
        }
    }

    public void CambiarCalidad(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }
}
