using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Library")]
    [SerializeField] private SFXLibrary sfxLibrary;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region SFX

    public void PlaySFX(string id)
    {
        Play(id, sfxSource);
    }

    #endregion

    #region UI

    public void PlayUI(string id)
    {
        Play(id, uiSource);
    }

    #endregion

    #region Ambient

    public void PlayAmbient(string id)
    {
        if (ambientSource == null)
        {
            Debug.LogWarning("Ambient Source is missing.");
            return;
        }

        if (sfxLibrary == null)
        {
            Debug.LogWarning("SFX Library is missing.");
            return;
        }

        SFXSet set = sfxLibrary.GetSFXSet(id);

        if (set == null)
            return;

        AudioClip clip = set.GetRandomClip();

        if (clip == null)
            return;

        // Don't restart the same ambience
        if (ambientSource.clip == clip && ambientSource.isPlaying)
            return;

        ambientSource.Stop();

        ambientSource.clip = clip;
        ambientSource.loop = true;
        ambientSource.volume = set.Volume;
        ambientSource.pitch = 1f;

        ambientSource.Play();
    }

    public void StopAmbient()
    {
        if (ambientSource != null)
            ambientSource.Stop();
    }

    #endregion

    private void Play(string id, AudioSource source)
    {
        if (source == null)
        {
            Debug.LogWarning("Audio Source is missing.");
            return;
        }

        if (sfxLibrary == null)
        {
            Debug.LogWarning("SFX Library is missing.");
            return;
        }

        SFXSet set = sfxLibrary.GetSFXSet(id);

        if (set == null)
            return;

        AudioClip clip = set.GetRandomClip();

        if (clip == null)
            return;

        float originalPitch = source.pitch;

        if (set.RandomizePitch)
            source.pitch = Random.Range(set.MinPitch, set.MaxPitch);
        else
            source.pitch = 1f;

        source.PlayOneShot(clip, set.Volume);

        source.pitch = originalPitch;
    }
}