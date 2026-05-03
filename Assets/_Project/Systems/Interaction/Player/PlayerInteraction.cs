using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;
    public float sphereRadius = 0.5f;
    public LayerMask interactLayer;

    [Header("References")]
    public Transform interactionPoint;

    private IInteractable currentInteractable;
    private IHighlightable currentHighlight;

    private IInteractable lastLoggedInteractable;

    void Update()
    {
        UpdateTarget();

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable?.Interact();
        }
    }

    // MAIN FLOW
    void UpdateTarget()
    {
        Collider target = FindBestTarget();

        if (target != null)
        {
            ApplyTarget(target);
        }
        else
        {
            ClearCurrent();
        }
    }

    // DETECTION
    Collider FindBestTarget()
    {
        Vector3 origin = interactionPoint.position;
        Vector3 direction = (interactionPoint.forward + Vector3.down * 0.5f).normalized;

        Debug.DrawRay(origin, direction * interactRange, Color.green);

        RaycastHit hit;

        // 1. SphereCast (forward)
        if (Physics.SphereCast(origin, sphereRadius, direction, out hit, interactRange, interactLayer))
        {
            return hit.collider;
        }

        // 2. OverlapSphere (fallback)
        Collider[] nearby = Physics.OverlapSphere(origin, sphereRadius, interactLayer);

        if (nearby.Length == 0)
            return null;

        return GetBestCollider(nearby, origin, direction);
    }

    // SCORING SYSTEM
    Collider GetBestCollider(Collider[] colliders, Vector3 origin, Vector3 forward)
    {
        Collider best = null;
        float bestScore = float.MinValue;

        foreach (var col in colliders)
        {
            Vector3 toTarget = (col.bounds.center - origin).normalized;

            float distance = Vector3.Distance(origin, col.bounds.center);
            float distanceScore = -distance;

            float directionScore = Vector3.Dot(forward, toTarget);

            float totalScore = directionScore * 2f + distanceScore;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                best = col;
            }
        }

        return best;
    }

    // APPLY TARGET
    void ApplyTarget(Collider col)
    {
        var interactable = col.GetComponentInParent<IInteractable>();
        var highlightable = col.GetComponentInParent<IHighlightable>();

        currentInteractable = interactable;

        // Handle highlight switching
        if (currentHighlight != highlightable)
        {
            currentHighlight?.Unhighlight();
            currentHighlight = highlightable;
            currentHighlight?.Highlight();
        }

        // Log only when changed
        if (interactable != null && interactable != lastLoggedInteractable)
        {
            Debug.Log("Interactable detected: " + col.name);
            lastLoggedInteractable = interactable;
        }
    }

    // CLEAR
    void ClearCurrent()
    {
        currentInteractable = null;

        if (currentHighlight != null)
        {
            currentHighlight.Unhighlight();
            currentHighlight = null;
        }

        lastLoggedInteractable = null;
    }
}