using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ClimbingWaning : WaningBase
{
    [Header("References")]
    [Tooltip("The Box Collider on this object acting as the damage zone.")]
    [SerializeField] private BoxCollider damageTrigger;

    private BoxCollider climbTrigger;

    protected virtual void Awake()
    {
        if (damageTrigger == null)
        {
            damageTrigger = GetComponent<BoxCollider>();
        }

        if (transform.parent != null)
        {
            climbTrigger = transform.parent.GetComponentInChildren<ClimbableObject>()?.GetComponent<BoxCollider>();
        }

        if (damageTrigger == null)
        {
            Debug.LogError("Damage Trigger (Box Collider) is missing from this GameObject.", this);
        }

        if (climbTrigger == null)
        {
            Debug.LogError("ClimbingWaning could not find a sibling ClimbTrigger with a Box Collider and ClimbableObject script under the parent.", this);
        }
    }

    protected override void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Check if the player is actively touching the climbing zone
        if (IsPlayerClimbing(other))
        {
            ApplyDamage(other.gameObject);
        }
    }

    private bool IsPlayerClimbing(Collider playerCollider)
    {
        if (climbTrigger == null) return false;

        // Use Unity's precise penetration check to see if the colliders overlap at all.
        // This is significantly more accurate than checking bounds.Intersects().
        Vector3 direction;
        float distance;
        
        bool isOverlapping = Physics.ComputePenetration(
            playerCollider, playerCollider.transform.position, playerCollider.transform.rotation,
            climbTrigger, climbTrigger.transform.position, climbTrigger.transform.rotation,
            out direction, out distance
        );

        return isOverlapping;
    }
}