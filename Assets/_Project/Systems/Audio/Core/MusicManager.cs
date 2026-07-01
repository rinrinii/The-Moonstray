using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [Header("Library")]
    [SerializeField]
    private MusicLibrary musicLibrary;

    [Header("Sources")]
    [SerializeField]
    private AudioSource daySource;

    [SerializeField]
    private AudioSource nightSource;

    [Header("Transitions")]

    [SerializeField]
    [Range(0f, 0.5f)]
    [Tooltip("Brief pause between fading out the current music and fading in the next track.")]
    private float regionTransitionPause = 0.15f;

    private string currentMusicID;
    private MusicSet currentMusicSet;

    private Coroutine transitionCoroutine;

    private bool isTransitioningMusic;
    private bool isNightMode;

    public static MusicManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        SceneMusic sceneMusic =
            FindFirstObjectByType<SceneMusic>();

        if (sceneMusic == null)
            return;

        TransitionToMusic(sceneMusic.MusicID);
    }

    /// <summary>
    /// Changes the currently playing music.
    /// If the requested music is already playing,
    /// nothing happens.
    /// </summary>
    private void TransitionToMusic(string musicID)
    {
        if (musicID == currentMusicID)
            return;

        if (isTransitioningMusic)
            return;

        MusicSet musicSet =
            musicLibrary.GetMusicSet(musicID);

        if (musicSet == null)
            return;

        StartCoroutine(
            TransitionMusicRoutine(
                musicID,
                musicSet));
    }

    private IEnumerator TransitionMusicRoutine(
    string musicID,
    MusicSet musicSet)
    {
        isTransitioningMusic = true;

        float duration =
            musicSet.TransitionDuration;

        float fadeDuration =
            duration * 0.5f;

        AudioSource activeSource =
            isNightMode &&
            currentMusicSet != null &&
            currentMusicSet.HasNightVariant
                ? nightSource
                : daySource;

        // Fade out currently audible music
        if (activeSource.isPlaying)
        {
            yield return FadeAudioSource(
                activeSource,
                0f,
                fadeDuration);
        }

        // Stop previous playback
        daySource.Stop();
        nightSource.Stop();

        currentMusicID = musicID;
        currentMusicSet = musicSet;

        // Assign clips
        daySource.clip =
            currentMusicSet.PrimaryClip;

        nightSource.clip =
            currentMusicSet.NightClip;

        // Reset playback
        if (daySource.clip != null)
            daySource.time = 0f;

        if (nightSource.clip != null)
            nightSource.time = 0f;

        // Apply initial volumes
        if (currentMusicSet.HasNightVariant)
        {
            daySource.volume =
                isNightMode ? 0f : 1f;

            nightSource.volume =
                isNightMode ? 1f : 0f;
        }
        else
        {
            daySource.volume = 1f;
            nightSource.volume = 0f;
        }

        // Tiny pause between regions
        yield return new WaitForSeconds(regionTransitionPause);

        // Start playback
        if (daySource.clip != null)
            daySource.Play();

        if (nightSource.clip != null)
            nightSource.Play();

        AudioSource newActiveSource =
            isNightMode &&
            currentMusicSet.HasNightVariant
                ? nightSource
                : daySource;

        newActiveSource.volume = 0f;

        yield return FadeAudioSource(
            newActiveSource,
            1f,
            fadeDuration);

        isTransitioningMusic = false;

        Debug.Log($"Now playing: {musicID}");
    }

    /// <summary>
    /// Called by PlayerTransformation whenever
    /// Serin changes form.
    /// </summary>
    public void SetNightMode(bool isNight)
    {
        isNightMode = isNight;

        if (isTransitioningMusic)
            return;

        if (currentMusicSet == null)
            return;

        if (!currentMusicSet.HasNightVariant)
        {
            daySource.volume = 1f;
            nightSource.volume = 0f;
            return;
        }

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine =
            StartCoroutine(CrossfadeDayNight());
    }

    private IEnumerator CrossfadeDayNight()
    {
        float duration =
            currentMusicSet.TransitionDuration;

        float startDay =
            daySource.volume;

        float startNight =
            nightSource.volume;

        float targetDay =
            isNightMode ? 0f : 1f;

        float targetNight =
            isNightMode ? 1f : 0f;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t =
                Mathf.Clamp01(time / duration);

            daySource.volume =
                Mathf.Lerp(
                    startDay,
                    targetDay,
                    t);

            nightSource.volume =
                Mathf.Lerp(
                    startNight,
                    targetNight,
                    t);

            yield return null;
        }

        daySource.volume = targetDay;
        nightSource.volume = targetNight;

        transitionCoroutine = null;
    }

    /// <summary>
    /// Generic fade helper.
    /// Will be used for region transitions.
    /// </summary>
    private IEnumerator FadeAudioSource(
    AudioSource source,
    float targetVolume,
    float duration)
    {
        float startVolume =
            source.volume;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t =
                Mathf.Clamp01(time / duration);

            source.volume =
                Mathf.Lerp(
                    startVolume,
                    targetVolume,
                    t);

            yield return null;
        }

        source.volume = targetVolume;
    }

    // Temporary testing
    private bool night;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            night = !night;
            SetNightMode(night);
        }
    }
}