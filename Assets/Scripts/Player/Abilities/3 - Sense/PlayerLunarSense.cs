using System.Collections;
using UnityEngine;

public class PlayerLunarSense : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LunarSensePulse pulse;

    [Header("Lunar Sense")]
    [SerializeField] private float senseRadius = 5f;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 3f;

    [Header("Pulse Timing")]
    [SerializeField] private float pulseDuration = 0.75f;   // maintain same duration as LunarSensePulse.cs

    private float nextSenseTime;

    public void ActivateSense()
    {
        if (Time.time < nextSenseTime)
            return;

        nextSenseTime = Time.time + cooldown;

        Debug.Log("Lunar Sense Activated");

        if (pulse != null)
        {
            pulse.Play(senseRadius);
        }

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            senseRadius
        );

        foreach (Collider hit in hits)
        {
            DistortionWaning distortion =
                hit.GetComponent<DistortionWaning>();

            if (distortion == null)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    distortion.transform.position
                );

            float normalizedDistance =
                distance / senseRadius;

            float revealDelay =
                normalizedDistance * pulseDuration;

            StartCoroutine(
                RevealAfterDelay(
                    distortion,
                    revealDelay
                )
            );
        }
    }

    private IEnumerator RevealAfterDelay(
        DistortionWaning distortion,
        float delay)
    {
        yield return new WaitForSeconds(delay);

        if (distortion != null)
        {
            distortion.Reveal();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            senseRadius
        );
    }
}