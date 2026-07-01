using UnityEngine;

[CreateAssetMenu(
    fileName = "New Music Set",
    menuName = "Moonstray/Audio/Music Set")]
public class MusicSet : ScriptableObject
{
    [Header("Music Variants")]

    [SerializeField]
    [Tooltip("Main music clip used for this area.")]
    private AudioClip primaryClip;

    [SerializeField]
    [Tooltip("Optional. Used when the player is in wolf form. Leave empty if this music has no alternate variant.")]
    private AudioClip nightClip;

    [Header("Transition")]

    [SerializeField]
    [Min(0f)]
    [Tooltip("Crossfade duration when switching music or variants.")]
    private float transitionDuration = 1f;

    /// <summary>
    /// Main music clip used for this area.
    /// </summary>
    public AudioClip PrimaryClip => primaryClip;

    /// <summary>
    /// Optional wolf-form music variant.
    /// Returns null if this MusicSet has no alternate variant.
    /// </summary>
    public AudioClip NightClip => nightClip;

    /// <summary>
    /// Crossfade duration when switching
    /// between music sets or variants.
    /// </summary>
    public float TransitionDuration => transitionDuration;

    /// <summary>
    /// Returns true if this music set supports
    /// switching between primary and night variants.
    /// </summary>
    public bool HasNightVariant => nightClip != null;
}