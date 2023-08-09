using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider volumeSlider;


    [Header("Sensitivity")]
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] TMP_InputField sensitivityInputField;

    [Header("Quality")]
    [SerializeField] TMP_Dropdown qualityDropdown;

    private void Awake()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("volume");

        sensitivitySlider.value = PlayerPrefs.GetFloat("sensitivity");
        sensitivityInputField.text = PlayerPrefs.GetFloat("sensitivity").ToString("0");

        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("quality"));
        qualityDropdown.value = PlayerPrefs.GetInt("quality");
    }

    private void Update()
    {
        sensitivitySlider.value = PlayerPrefs.GetFloat("sensitivity");
    }

    public void SetSensitivity(float sens)
    {
        PlayerPrefs.SetFloat("sensitivity", sens);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("master", volume);
        PlayerPrefs.SetFloat("volume", volume);
    }

    public void UpdateSensInputField()
    {
        sensitivityInputField.text = PlayerPrefs.GetFloat("sensitivity").ToString("0");
    }

    public void onValueChange()
    {
        PlayerPrefs.SetFloat("sensitivity", float.Parse(sensitivityInputField.text));
    }

    public void SetQuality (int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("quality", qualityIndex);
    }

    public void SetFullscreen(bool fullScreen)
    {
        Screen.fullScreen = fullScreen;
    }
}
