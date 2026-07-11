using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Library")]
    [SerializeField] private SFXLibrary sfxLibrary;

    [Header("Sources")]
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
    }

    public void PlaySFX(string id)
    {
        Play(id, sfxSource);
    }

    public void PlayUI(string id)
    {
        Play(id, uiSource);
    }

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