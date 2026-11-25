
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private const string MASTER_KEY = "Nexward_MasterVolume";
    private const string MUSIC_KEY = "Nexward_MusicVolume";
    private const string SFX_KEY = "Nexward_SFXVolume";

    private void Awake()
    {
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void Start()
    {
        // Load previously saved settings.
        float master = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        float music = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        // Update sliders without triggering their callbacks.
        masterVolumeSlider.SetValueWithoutNotify(master);
        musicVolumeSlider.SetValueWithoutNotify(music);
        sfxVolumeSlider.SetValueWithoutNotify(sfx);

        // Apply saved values to the mixer.
        ApplyVolume("MasterVolume", master);
        ApplyVolume("MusicVolume", music);
        ApplyVolume("SFXVolume", sfx);
    }

    private void SetMasterVolume(float value)
    {
        PlayerPrefs.SetFloat(MASTER_KEY, value);
        ApplyVolume("MasterVolume", value);
    }

    private void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
        ApplyVolume("MusicVolume", value);
    }

    private void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat(SFX_KEY, value);
        ApplyVolume("SFXVolume", value);
    }

    private void ApplyVolume(string parameter, float value)
    {
        if (audioMixer == null)
            return;

        float decibels = value <= 0.0001f
            ? -80f
            : Mathf.Log10(value) * 20f;

        audioMixer.SetFloat(parameter, decibels);
    }

    private void OnDestroy()
    {
        masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }
}

