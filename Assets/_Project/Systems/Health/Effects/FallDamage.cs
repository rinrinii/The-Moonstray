using UnityEngine;

public class FallDamage : MonoBehaviour
{
    [Header("Fall Damage")]
    [SerializeField] private float minimumFallDistance = 15f;
    [SerializeField] private float damageMultiplier = 3f;

    private CharacterController controller;
    private PlayerHealth playerHealth;
    private PlayerClimbing climbing;

    private bool wasGrounded;

    private float fallStartHeight;

    private void Start()
    {
        controller =
            GetComponent<CharacterController>();

        playerHealth =
            GetComponent<PlayerHealth>();

        climbing =
            GetComponent<PlayerClimbing>();

        // Initialize starting height
        fallStartHeight =
            transform.position.y;
    }

    private void Update()
    {
        if (controller == null ||
            playerHealth == null)
        {
            return;
        }

        bool isClimbing =
            climbing != null &&
            climbing.IsClimbing();

        bool isGrounded =
            controller.isGrounded &&
            !isClimbing;

        // =========================
        // Left Ground
        // =========================

        if (!isGrounded &&
            wasGrounded)
        {
            fallStartHeight =
                transform.position.y;
        }

        // =========================
        // Landed
        // =========================

        if (isGrounded &&
            !wasGrounded)
        {
            float fallDistance =
                fallStartHeight -
                transform.position.y;

            if (fallDistance >
                minimumFallDistance)
            {
                float excessFall =
                    fallDistance -
                    minimumFallDistance;

                float damage =
                    excessFall *
                    damageMultiplier;

                playerHealth.TakeDamage(
                    damage
                );

                Debug.Log(
                    $"Fall Damage: {damage}"
                );
            }

            // Reset after landing
            fallStartHeight =
                transform.position.y;
        }

        wasGrounded =
            isGrounded;
    }
}