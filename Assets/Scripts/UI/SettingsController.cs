using System.Collections.Generic;
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

    private DropdownField resolutionDropdown;
    private DropdownField qualityDropdown;
    private Toggle fullscreenToggle;

    private readonly List<Vector2Int> resolutions = new()
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080)
    };

    private readonly List<string> resolutionOptions = new()
    {
        "1280 × 720",
        "1600 × 900",
        "1920 × 1080"
    };

    private readonly List<string> qualityOptions = new()
    {
        "Low",
        "Medium",
        "High"
    };

    private void Start()
    {
        UIDocument ui = GetComponent<UIDocument>();

        if (ui == null)
        {
            Debug.LogError("SettingsController: UIDocument missing.");
            return;
        }

        VisualElement root = ui.rootVisualElement;

        settingsRoot = root.Query<VisualElement>("SettingsRoot").First();

        if (settingsRoot == null)
        {
            Debug.LogError("SettingsRoot not found.");
            return;
        }

        audioContainer = settingsRoot.Q<VisualElement>("AudioSettingsContainer");
        videoContainer = settingsRoot.Q<VisualElement>("VideoSettingsContainer");

        audioButton = settingsRoot.Q<Button>("AudioButton");
        videoButton = settingsRoot.Q<Button>("VideoButton");
        saveSettingsButton = settingsRoot.Q<Button>("SaveSettingsButton");

        masterSlider = settingsRoot.Q<SliderInt>("MasterSlider");
        musicSlider = settingsRoot.Q<SliderInt>("MusicSlider");
        sfxSlider = settingsRoot.Q<SliderInt>("SFXSlider");

        resolutionDropdown = settingsRoot.Q<DropdownField>("ResolutionDropdown");
        qualityDropdown = settingsRoot.Q<DropdownField>("QualityDropdown");
        fullscreenToggle = settingsRoot.Q<Toggle>("FullscreenToggle");

        if (audioButton != null)
            audioButton.clicked += ShowAudio;

        if (videoButton != null)
            videoButton.clicked += ShowVideo;

        if (saveSettingsButton != null)
            saveSettingsButton.clicked += SaveSettings;

        SetupAudioSliders();
        SetupVideoSettings();

        RegisterAudioCallbacks();

        ApplySavedAudioSettings();

        ShowAudio();
    }

    private void SetupAudioSliders()
    {
        SetupSlider(masterSlider, "MasterVolume");
        SetupSlider(musicSlider, "MusicVolume");
        SetupSlider(sfxSlider, "SFXVolume");
    }

    private void SetupSlider(SliderInt slider, string prefKey)
    {
        if (slider == null)
        {
            Debug.LogError(prefKey + " slider not found.");
            return;
        }

        slider.lowValue = 0;
        slider.highValue = 100;
        slider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat(prefKey, 1f) * 100f);
    }

    private void SetupVideoSettings()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.choices = resolutionOptions;

            int savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 2);
            savedResolutionIndex = Mathf.Clamp(savedResolutionIndex, 0, resolutionOptions.Count - 1);

            resolutionDropdown.index = savedResolutionIndex;
        }
        else
        {
            Debug.LogError("ResolutionDropdown not found.");
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.choices = qualityOptions;

            int savedQualityIndex = PlayerPrefs.GetInt("QualityIndex", 2);
            savedQualityIndex = Mathf.Clamp(savedQualityIndex, 0, qualityOptions.Count - 1);

            qualityDropdown.index = savedQualityIndex;
        }
        else
        {
            Debug.LogError("QualityDropdown not found.");
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.value = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        }
        else
        {
            Debug.LogError("FullscreenToggle not found.");
        }

        ApplySavedVideoSettings();
    }

    private void RegisterAudioCallbacks()
    {
        if (masterSlider != null)
        {
            masterSlider.RegisterValueChangedCallback(evt =>
            {
                ApplyMasterVolume(evt.newValue / 100f);
            });
        }

        if (musicSlider != null)
        {
            musicSlider.RegisterValueChangedCallback(evt =>
            {
                ApplyMusicVolume(evt.newValue / 100f);
            });
        }

        if (sfxSlider != null)
        {
            sfxSlider.RegisterValueChangedCallback(evt =>
            {
                ApplySFXVolume(evt.newValue / 100f);
            });
        }
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

    private void ApplySavedAudioSettings()
    {
        ApplyMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
        ApplyMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        ApplySFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
    }

    private void ApplySavedVideoSettings()
    {
        int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 2);
        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Count - 1);

        int qualityIndex = PlayerPrefs.GetInt("QualityIndex", 2);
        qualityIndex = Mathf.Clamp(qualityIndex, 0, qualityOptions.Count - 1);

        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        Vector2Int selectedResolution = resolutions[resolutionIndex];

        Screen.SetResolution(
            selectedResolution.x,
            selectedResolution.y,
            fullscreen
        );

        QualitySettings.SetQualityLevel(qualityIndex);

        Screen.fullScreen = fullscreen;
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
        value = Mathf.Clamp01(value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
        else
            Debug.LogWarning("AudioManager.Instance is missing.");
    }

    private void SaveSettings()
    {
        if (masterSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterSlider.value / 100f);

        if (musicSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicSlider.value / 100f);

        if (sfxSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value / 100f);

        if (resolutionDropdown != null)
            PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.index);

        if (qualityDropdown != null)
            PlayerPrefs.SetInt("QualityIndex", qualityDropdown.index);

        if (fullscreenToggle != null)
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.value ? 1 : 0);

        PlayerPrefs.Save();

        ApplySavedAudioSettings();
        ApplySavedVideoSettings();

        Debug.Log("Settings saved and applied.");
    }

    public void RevertToSavedSettings()
    {
        if (masterSlider != null)
            masterSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("MasterVolume", 1f) * 100f);

        if (musicSlider != null)
            musicSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("MusicVolume", 1f) * 100f);

        if (sfxSlider != null)
            sfxSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("SFXVolume", 1f) * 100f);

        if (resolutionDropdown != null)
            resolutionDropdown.index = PlayerPrefs.GetInt("ResolutionIndex", 2);

        if (qualityDropdown != null)
            qualityDropdown.index = PlayerPrefs.GetInt("QualityIndex", 2);

        if (fullscreenToggle != null)
            fullscreenToggle.value = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        ApplySavedAudioSettings();
        ApplySavedVideoSettings();
    }
}