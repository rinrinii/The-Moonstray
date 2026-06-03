using System.Collections;
using UnityEngine;

public class PlayerHowl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HowlPulse pulse;

    [Header("Howl")]
    [SerializeField] private float howlRadius = 5f;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 1.5f;

    [Header("Pulse Timing")]
    [SerializeField] private float pulseDuration = 1f;

    private float nextHowlTime;

    private CharacterController controller;
    private PlayerTransformation transformation;

    private void Awake()
    {
        controller =
            GetComponent<CharacterController>();

        transformation =
            GetComponent<PlayerTransformation>();
    }

    private Animator GetCurrentAnimator()
    {
        return GetComponentInChildren<Animator>();
    }

    public void ActivateHowl()
    {
        if (!AbilityManager.Instance.IsUnlocked(AbilityType.PurgeHowl))
        {
            return;
        }

        if (!CanHowl())
            return;

        nextHowlTime =
            Time.time + cooldown;

        Animator currentAnim =
            GetCurrentAnimator();

        if (currentAnim != null)
        {
            currentAnim.SetTrigger("Howl");
        }

        if (pulse != null)
        {
            pulse.Play(howlRadius);
        }

        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                howlRadius
            );

        foreach (Collider hit in hits)
        {
            StillnessWaning fog =
                hit.GetComponent<StillnessWaning>();

            if (fog == null)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    fog.transform.position
                );

            float normalizedDistance =
                distance / howlRadius;

            float revealDelay =
                normalizedDistance *
                pulseDuration;

            StartCoroutine(
                DisperseAfterDelay(
                    fog,
                    revealDelay
                )
            );
        }
    }

    private IEnumerator DisperseAfterDelay(
        StillnessWaning fog,
        float delay)
    {
        yield return new WaitForSeconds(delay);

        if (fog != null)
        {
            fog.Disperse();
        }
    }

    private bool CanHowl()
    {
        if (Time.time < nextHowlTime)
            return false;

        Animator currentAnim =
            GetCurrentAnimator();

        if (currentAnim == null)
            return false;

        if (!controller.isGrounded)
            return false;

        if (!transformation.CanMove())
            return false;

        if (transformation.currentForm !=
            PlayerTransformation.FormState.Wolf)
            return false;

        AnimatorStateInfo state =
            currentAnim.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Jump-Start") ||
            state.IsName("Jump-Air") ||
            state.IsName("Jump-End"))
        {
            return false;
        }

        if (state.IsName("Interact-Kneel") ||
            state.IsName("Interact-Stand"))
        {
            return false;
        }

        if (state.IsName("Howl"))
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            howlRadius
        );
    }
}