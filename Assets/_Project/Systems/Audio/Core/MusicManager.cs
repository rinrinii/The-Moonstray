using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [Header("Library")]
    [SerializeField] private MusicLibrary musicLibrary;

    [Header("Sources")]
    [SerializeField] private AudioSource daySource;
    [SerializeField] private AudioSource nightSource;

    [Header("Transitions")]
    [SerializeField]
    [Range(0f, 0.5f)]
    private float regionTransitionPause = 0.15f;

    private string currentMusicID;
    private MusicSet currentMusicSet;

    private Coroutine transitionCoroutine;

    private bool isTransitioningMusic;
    private bool isNightMode;

    private bool musicSuspended;

    private Coroutine cutsceneFadeCoroutine;

    private float musicVolumeMultiplier = 1f;
    private float cutsceneVolumeMultiplier = 1f;

    public static MusicManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ConfigureSources();
    }

    private void Start()
    {
        // The persistent systems can be created after the Main Menu scene has
        // already loaded, so the sceneLoaded event alone is not sufficient.
        PlayMusicForScene(SceneManager.GetActiveScene());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene);
    }

    private void PlayMusicForScene(Scene scene)
    {
        SceneMusic sceneMusic = FindFirstObjectByType<SceneMusic>();

        if (sceneMusic == null)
        {
            Debug.LogWarning($"No SceneMusic found in scene '{scene.name}'.");
            return;
        }

        TransitionToMusic(sceneMusic.MusicID);
    }

    private void ConfigureSources()
    {
        if (daySource != null)
        {
            daySource.loop = true;
            daySource.playOnAwake = false;
        }

        if (nightSource != null)
        {
            nightSource.loop = true;
            nightSource.playOnAwake = false;
        }
    }

    public void SetMusicVolume(float value)
    {
        musicVolumeMultiplier = Mathf.Clamp01(value);
        ApplyCurrentMusicVolumes();
    }

    private void TransitionToMusic(string musicID)
    {
        if (musicLibrary == null || daySource == null || nightSource == null)
        {
            Debug.LogError(
                "MusicManager requires a MusicLibrary and both persistent AudioSources.");
            return;
        }

        if (musicID == currentMusicID)
            return;

        if (isTransitioningMusic)
            return;

        MusicSet musicSet = musicLibrary.GetMusicSet(musicID);

        if (musicSet == null)
        {
            Debug.LogWarning($"No MusicSet found for music ID '{musicID}'.");
            return;
        }

        StartCoroutine(TransitionMusicRoutine(musicID, musicSet));
    }

    private IEnumerator TransitionMusicRoutine(string musicID, MusicSet musicSet)
    {
        isTransitioningMusic = true;

        float duration = musicSet.TransitionDuration;
        float fadeDuration = duration * 0.5f;

        AudioSource activeSource =
            isNightMode &&
            currentMusicSet != null &&
            currentMusicSet.HasNightVariant
                ? nightSource
                : daySource;

        if (activeSource.isPlaying)
            yield return FadeAudioSource(activeSource, 0f, fadeDuration);

        daySource.Stop();
        nightSource.Stop();

        currentMusicID = musicID;
        currentMusicSet = musicSet;

        daySource.clip = currentMusicSet.PrimaryClip;
        nightSource.clip = currentMusicSet.NightClip;

        if (daySource.clip != null)
            daySource.time = 0f;

        if (nightSource.clip != null)
            nightSource.time = 0f;

        ApplyCurrentMusicVolumes();

        yield return new WaitForSeconds(regionTransitionPause);

        if (daySource.clip != null)
            daySource.Play();

        if (nightSource.clip != null)
            nightSource.Play();

        AudioSource newActiveSource =
            isNightMode &&
            currentMusicSet.HasNightVariant
                ? nightSource
                : daySource;

        SetSourceVolume(newActiveSource, 0f);

        yield return FadeAudioSource(newActiveSource, 1f, fadeDuration);

        isTransitioningMusic = false;

        Debug.Log($"Now playing: {musicID}");
    }

    public void SetNightMode(bool isNight)
    {
        isNightMode = isNight;

        if (isTransitioningMusic)
            return;

        if (currentMusicSet == null)
            return;

        if (!currentMusicSet.HasNightVariant)
        {
            SetSourceVolume(daySource, 1f);
            SetSourceVolume(nightSource, 0f);
            return;
        }

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(CrossfadeDayNight());
    }

    private IEnumerator CrossfadeDayNight()
    {
        float duration = currentMusicSet.TransitionDuration;

        float startDay = GetBaseVolume(daySource.volume);
        float startNight = GetBaseVolume(nightSource.volume);

        float targetDay = isNightMode ? 0f : 1f;
        float targetNight = isNightMode ? 1f : 0f;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            SetSourceVolume(daySource, Mathf.Lerp(startDay, targetDay, t));
            SetSourceVolume(nightSource, Mathf.Lerp(startNight, targetNight, t));

            yield return null;
        }

        SetSourceVolume(daySource, targetDay);
        SetSourceVolume(nightSource, targetNight);

        transitionCoroutine = null;
    }

    private IEnumerator FadeAudioSource(AudioSource source, float targetBaseVolume, float duration)
    {
        float startBaseVolume = GetBaseVolume(source.volume);

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            SetSourceVolume(
                source,
                Mathf.Lerp(startBaseVolume, targetBaseVolume, t)
            );

            yield return null;
        }

        SetSourceVolume(source, targetBaseVolume);
    }

    private void ApplyCurrentMusicVolumes()
    {
        if (currentMusicSet == null)
        {
            SetSourceVolume(daySource, 0f);
            SetSourceVolume(nightSource, 0f);
            return;
        }

        if (currentMusicSet.HasNightVariant)
        {
            SetSourceVolume(daySource, isNightMode ? 0f : 1f);
            SetSourceVolume(nightSource, isNightMode ? 1f : 0f);
        }
        else
        {
            SetSourceVolume(daySource, 1f);
            SetSourceVolume(nightSource, 0f);
        }
    }

    private void SetSourceVolume(AudioSource source, float baseVolume)
    {
        if (source == null)
            return;

        source.volume =
            Mathf.Clamp01(baseVolume)
            * musicVolumeMultiplier
            * cutsceneVolumeMultiplier;
    }

    private float GetBaseVolume(float actualVolume)
    {
        if (musicVolumeMultiplier <= 0f)
            return 0f;

        return actualVolume / musicVolumeMultiplier;
    }

    private bool night;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            night = !night;
            SetNightMode(night);
        }
    }

    public void FadeOutForCutscene(float duration = 1f)
    {
        StartCutsceneFade(0f, duration);
    }

    public void FadeInAfterCutscene(float duration = 1f)
    {
        StartCutsceneFade(1f, duration);
    }

    private void StartCutsceneFade(float targetMultiplier, float duration)
    {
        if (cutsceneFadeCoroutine != null)
            StopCoroutine(cutsceneFadeCoroutine);

        cutsceneFadeCoroutine =
            StartCoroutine(CutsceneFadeRoutine(targetMultiplier, duration));
    }

    private IEnumerator CutsceneFadeRoutine(float targetMultiplier, float duration)
    {
        float start = cutsceneVolumeMultiplier;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            cutsceneVolumeMultiplier =
                Mathf.Lerp(
                    start,
                    targetMultiplier,
                    elapsed / duration);

            ApplyCurrentMusicVolumes();

            yield return null;
        }

        cutsceneVolumeMultiplier = targetMultiplier;

        ApplyCurrentMusicVolumes();

        cutsceneFadeCoroutine = null;
    }
}
