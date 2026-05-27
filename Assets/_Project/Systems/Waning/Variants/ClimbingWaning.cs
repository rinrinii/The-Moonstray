using UnityEngine;

public class ClimbingWaning : WaningBase
{
    [Header("Climbing Damage")]
    [SerializeField] private float climbingDamagePerSecond = 50f;

    private PlayerClimbing playerClimbing;
    private ClimbableObject ownClimbable;

    protected virtual void Awake()
    {
        playerClimbing =
            FindFirstObjectByType<PlayerClimbing>();

        ownClimbable =
            GetComponent<ClimbableObject>();

        if (ownClimbable == null)
        {
            Debug.LogError(
                "ClimbingWaning requires ClimbableObject.",
                this
            );
        }
    }

    private void Update()
    {
        if (playerClimbing == null)
            return;

        if (ownClimbable == null)
            return;

        if (!playerClimbing.IsClimbing())
            return;

        Collider[] hits =
            Physics.OverlapSphere(
                playerClimbing.climbCheck.position,
                playerClimbing.climbCheckRadius,
                playerClimbing.climbLayer
            );

        foreach (Collider hit in hits)
        {
            ClimbableObject detected =
                hit.GetComponent<ClimbableObject>();

            if (detected == ownClimbable)
            {
                ApplyDamage(
                    playerClimbing.gameObject
                );

                break;
            }
        }
    }

    protected override void ApplyDamage(GameObject player)
    {
        float damage =
            climbingDamagePerSecond *
            Time.deltaTime;

        PlayerHealth health =
            player.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}