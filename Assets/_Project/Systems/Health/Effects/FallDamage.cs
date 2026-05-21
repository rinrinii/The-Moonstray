using UnityEngine;

public class FallDamage : MonoBehaviour
{
    [Header("Fall Damage")]
    [SerializeField] private float minimumFallDistance = 5f;

    [SerializeField] private float damageMultiplier = 5f;

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

        bool isGrounded = controller.isGrounded && !isClimbing;

        // =========================
        // Started Falling
        // =========================

        if (!isGrounded && !isClimbing && wasGrounded)
        {
            fallStartHeight =
                transform.position.y;
        }

        // =========================
        // Landed
        // =========================

        if (isGrounded && !isClimbing && !wasGrounded)
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
        }

        wasGrounded =
            isGrounded;
    }
}