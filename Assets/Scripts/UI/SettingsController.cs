using UnityEngine;
using UnityEngine.UIElements;

public class SettingsController : MonoBehaviour
{
    private VisualElement settingsRoot;
    private VisualElement audioContainer;
    private VisualElement videoContainer;

    private Button audioButton;
    private Button videoButton;
    private Button saveSettingsButton;

    private SliderInt masterSlider;
    private SliderInt musicSlider;
    private SliderInt sfxSlider;

    private void Start()
    {
        UIDocument ui = GetComponent<UIDocument>();
        VisualElement root = ui.rootVisualElement;

        settingsRoot = root.Q<VisualElement>("SettingsRoot");

        audioContainer = settingsRoot.Q<VisualElement>("AudioSettingsContainer");
        videoContainer = settingsRoot.Q<VisualElement>("VideoSettingsContainer");

        audioButton = settingsRoot.Q<Button>("AudioButton");
        videoButton = settingsRoot.Q<Button>("VideoButton");
        saveSettingsButton = settingsRoot.Q<Button>("SaveSettingsButton");

        masterSlider = settingsRoot.Q<SliderInt>("MasterSlider");
        musicSlider = settingsRoot.Q<SliderInt>("MusicSlider");
        sfxSlider = settingsRoot.Q<SliderInt>("SFXSlider");

        if (masterSlider == null) Debug.LogError("MasterSlider not found.");
        if (musicSlider == null) Debug.LogError("MusicSlider not found.");
        if (sfxSlider == null) Debug.LogError("SFXSlider not found.");

        if (audioButton != null) audioButton.clicked += ShowAudio;
        if (videoButton != null) videoButton.clicked += ShowVideo;
        if (saveSettingsButton != null) saveSettingsButton.clicked += SaveSettings;

        SetupSlider(masterSlider, "MasterVolume");
        SetupSlider(musicSlider, "MusicVolume");
        SetupSlider(sfxSlider, "SFXVolume");

        if (masterSlider != null)
        {
            ApplyMasterVolume(masterSlider.value / 100f);
            masterSlider.RegisterValueChangedCallback(evt =>
            {
                ApplyMasterVolume(evt.newValue / 100f);
            });
        }

        if (musicSlider != null)
        {
            ApplyMusicVolume(musicSlider.value / 100f);
            musicSlider.RegisterValueChangedCallback(evt =>
            {
                ApplyMusicVolume(evt.newValue / 100f);
            });
        }

        if (sfxSlider != null)
        {
            ApplySFXVolume(sfxSlider.value / 100f);
            sfxSlider.RegisterValueChangedCallback(evt =>
            {
                ApplySFXVolume(evt.newValue / 100f);
            });
        }

        ShowAudio();
    }

    private void SetupSlider(SliderInt slider, string prefKey)
    {
        if (slider == null) return;

        slider.lowValue = 0;
        slider.highValue = 100;
        slider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat(prefKey, 1f) * 100f);
    }

    private void ShowAudio()
    {
        if (audioContainer != null)
            audioContainer.style.display = DisplayStyle.Flex;

        if (videoContainer != null)
            videoContainer.style.display = DisplayStyle.None;
    }

    private void ShowVideo()
    {
        if (audioContainer != null)
            audioContainer.style.display = DisplayStyle.None;

        if (videoContainer != null)
            videoContainer.style.display = DisplayStyle.Flex;
    }

    private void ApplyMasterVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }

    private void ApplyMusicVolume(float value)
    {
        MusicManager.Instance?.SetMusicVolume(Mathf.Clamp01(value));
    }

    private void ApplySFXVolume(float value)
    {
        Debug.Log("SFX Volume: " + Mathf.Clamp01(value));
    }

    private void SaveSettings()
    {
        if (masterSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterSlider.value / 100f);

        if (musicSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicSlider.value / 100f);

        if (sfxSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value / 100f);

        PlayerPrefs.Save();

        Debug.Log("Settings saved.");
    }

    public void RevertToSavedSettings()
    {
        if (masterSlider != null)
            masterSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("MasterVolume", 1f) * 100f);

        if (musicSlider != null)
            musicSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("MusicVolume", 1f) * 100f);

        if (sfxSlider != null)
            sfxSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("SFXVolume", 1f) * 100f);
    }
}