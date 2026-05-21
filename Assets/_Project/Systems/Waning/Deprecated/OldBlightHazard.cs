using UnityEngine;
using System.Collections;

public class BlightHazard : MonoBehaviour
{
    [Header("Blight Damage Settings")]
    [Tooltip("Damage applied per second at the very edge of the blight zone.")]
    public float minDamagePerSecond = 5f;
    [Tooltip("Damage applied per second when standing directly on the core object.")]
    public float maxDamagePerSecond = 25f;
    
    [Header("Blight Spread Settings")]
    [Tooltip("How much larger the blight zone collider gets when triggered.")]
    public float targetSpreadRadius = 1f;
    [Tooltip("How much the physical visual object scales up horizontally (X and Z) from its starting size.")]
    public float visualScaleMultiplier = 3f;
    [Tooltip("How fast the blight zone expands to its target size.")]
    public float spreadSpeed = 1.5f;

    private SphereCollider blightZone;
    private bool hasSpread = false;
    private float initialColliderRadius;
    private Vector3 initialVisualScale;

    void Start()
    {
        // Get the trigger zone and record its starting size
        blightZone = GetComponent<SphereCollider>();
        if (blightZone != null)
        {
            initialColliderRadius = blightZone.radius;
        }
        else
        {
            Debug.LogError("BlightHazard requires a SphereCollider set to 'Is Trigger'!");
        }

        // Record the starting scale of the physical asset object
        initialVisualScale = transform.localScale;
    }

    // Triggers the exact moment the player touches the blight object/zone
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasSpread)
        {
            // Start expanding the hazard zone and the object together
            StartCoroutine(SpreadBlight());
        }
    }

    // Triggers every frame the player remains inside the blight zone
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Calculate how deep the player is into the zone (0.0 at edge, 1.0 at center core)
            float distanceToCenter = Vector3.Distance(transform.position, other.transform.position);
            float maxDistance = blightZone.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            
            // Normalize the distance to a 0-1 percentage factor, inversion makes center = 1
            float proximityFactor = 1f - Mathf.Clamp01(distanceToCenter / maxDistance);

            // 2. Scale damage smoothly based on proximity (closer to center = higher damage)
            float currentDamagePerSecond = Mathf.Lerp(minDamagePerSecond, maxDamagePerSecond, proximityFactor);

            // 3. Process the damage calculated for this specific frame
            ProcessBlightDamage(other.gameObject, currentDamagePerSecond * Time.deltaTime);
        }
    }

    // Coroutine to smoothly expand BOTH the trigger zone and the asset horizontal scale over time
    IEnumerator SpreadBlight()
    {
        hasSpread = true;
        
        float currentRadius = blightZone.radius;
        float progress = 0f;

        // Calculate the target scale, modifying only X and Z while keeping the original Y scale intact
        Vector3 targetVisualScale = new Vector3(
            initialVisualScale.x * visualScaleMultiplier,
            initialVisualScale.y, 
            initialVisualScale.z * visualScaleMultiplier
        );

        // Cache starting parameters for smooth interpolation
        float startRadius = currentRadius;
        Vector3 startScale = transform.localScale;

        while (progress < 1f)
        {
            // Advance progress based on speed
            progress += (spreadSpeed / targetSpreadRadius) * Time.deltaTime;
            
            // Interpolate the collider radius
            blightZone.radius = Mathf.Lerp(startRadius, targetSpreadRadius, progress);
            
            // Interpolate the actual physical 3D object size
            transform.localScale = Vector3.Lerp(startScale, targetVisualScale, progress);
            
            yield return null; // Wait for the next frame
        }

        // Snap precisely to final values to account for floating point math
        blightZone.radius = targetSpreadRadius;
        transform.localScale = targetVisualScale;
    }

    // This method handles the output. Connect your own damage/UI system here!
    void ProcessBlightDamage(GameObject player, float calculatedDamage)
    {
        // Print to console to verify the math is working perfectly
        Debug.Log($"Player is taking {calculatedDamage} Blight damage this frame.");
    }
}