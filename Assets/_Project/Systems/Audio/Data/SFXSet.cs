using UnityEngine;

[CreateAssetMenu(
    fileName = "New SFX Set",
    menuName = "Moonstray/Audio/SFX Set")]
public class SFXSet : ScriptableObject
{
    [Header("General Clips")]
    [SerializeField] private AudioClip[] clips;

    [Header("Settings")]
    [SerializeField] private bool randomizePitch = true;

    [SerializeField]
    [Range(0.5f, 2f)]
    private float minPitch = 0.95f;

    [SerializeField]
    [Range(0.5f, 2f)]
    private float maxPitch = 1.05f;

    [SerializeField]
    [Range(0f, 1f)]
    private float volume = 1f;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
            return null;

        return clips[Random.Range(0, clips.Length)];
    }

    public bool RandomizePitch => randomizePitch;

    public float MinPitch => minPitch;

    public float MaxPitch => maxPitch;

    public float Volume => volume;
}