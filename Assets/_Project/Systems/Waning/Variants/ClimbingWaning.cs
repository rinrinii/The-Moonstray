using UnityEngine;

public class ClimbingWaning : WaningBase
{
    [Header("Climbing Damage")]
    [SerializeField] private float climbingDamagePerSecond = 50f;

    private PlayerClimbing playerClimbing;
    private ClimbableObject ownClimbable;
    private bool playerInThorns = false;

    protected virtual void Awake()
    {
        playerClimbing = FindFirstObjectByType<PlayerClimbing>();
        ownClimbable = GetComponent<ClimbableObject>();

        if (ownClimbable == null)
            Debug.LogError("ClimbingWaning requires ClimbableObject.", this);
    }

    private void Update()
    {

        if (playerClimbing == null ||
            ownClimbable == null ||
            !playerClimbing.IsClimbing())
        {
            if (playerInThorns)
            {
                playerInThorns = false;
                StatusEffectManager.Instance?.SetThorn(false);
            }
            return;
        }

        Collider[] hits = Physics.OverlapSphere(
            playerClimbing.climbCheck.position,
            playerClimbing.climbCheckRadius,
            playerClimbing.climbLayer
        );

        bool onThisWall = false;
        foreach (Collider hit in hits)
        {
            ClimbableObject detected = hit.GetComponent<ClimbableObject>();
            if (detected == ownClimbable)
            {
                onThisWall = true;


                if (!playerInThorns)
                {
                    playerInThorns = true;
                    StatusEffectManager.Instance?.SetThorn(true);
                }

                ApplyDamage(playerClimbing.gameObject);
                break;
            }
        }


        if (!onThisWall && playerInThorns)
        {
            playerInThorns = false;
            StatusEffectManager.Instance?.SetThorn(false);
        }
    }

    protected override void ApplyDamage(GameObject player)
    {
        float damage = climbingDamagePerSecond * Time.deltaTime;
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }
}