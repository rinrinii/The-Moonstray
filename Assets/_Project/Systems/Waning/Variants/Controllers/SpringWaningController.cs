using UnityEngine;

public class SpringWaningController : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool isCleansed = false;

    [Header("References")]
    [SerializeField] private ClimbingWaning climbingWaning;
    [SerializeField] private Renderer visualRenderer;

    [Header("Visuals")]
    [SerializeField] private Material corruptedMaterial;
    [SerializeField] private Material cleansedMaterial;

    private void Start()
    {
        UpdateState();
    }

    public void Cleanse()
    {
        if (isCleansed)
            return;

        isCleansed = true;

        UpdateState();

        Debug.Log($"{gameObject.name} cleansed.");
    }

    public void Corrupt()
    {
        isCleansed = false;

        UpdateState();

        Debug.Log($"{gameObject.name} corrupted.");
    }

    private void UpdateState()
    {
        // Disable ONLY waning damage
        if (climbingWaning != null)
        {
            climbingWaning.enabled = !isCleansed;
        }

        // Swap material
        if (visualRenderer != null)
        {
            visualRenderer.material =
                isCleansed
                ? cleansedMaterial
                : corruptedMaterial;
        }
    }

    public bool IsCleansed()
    {
        return isCleansed;
    }
}