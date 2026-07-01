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

    private string currentMusicID;

    private MusicSet currentMusicSet;

    private Coroutine transitionCoroutine;

    private bool isNightMode;

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

        PlayMusic(sceneMusic.MusicID);
    }

    private void PlayMusic(string musicID)
    {
        if (musicID == currentMusicID)
            return;

        currentMusicID = musicID;

        currentMusicSet =
            musicLibrary.GetMusicSet(musicID);

        if (currentMusicSet == null)
            return;

        // Stop previous playback
        daySource.Stop();
        nightSource.Stop();

        // Assign clips
        daySource.clip =
            currentMusicSet.PrimaryClip;

        nightSource.clip =
            currentMusicSet.NightClip;

        // Reset playback position
        if (daySource.clip != null)
            daySource.time = 0f;

        if (nightSource.clip != null)
            nightSource.time = 0f;

        // Start both clips together
        if (daySource.clip != null)
            daySource.Play();

        if (nightSource.clip != null)
            nightSource.Play();

        // Apply current form
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

        Debug.Log($"Now playing: {musicID}");
    }

    public void SetNightMode(bool isNight)
    {
        isNightMode = isNight;

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
}