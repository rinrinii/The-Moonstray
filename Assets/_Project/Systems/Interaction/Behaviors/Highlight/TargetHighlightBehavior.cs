using UnityEngine;

public class TargetHighlightBehaviour : MonoBehaviour, IHighlightable
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private float emissionIntensity = 1.2f;

    private Material matInstance;
    private Color originalEmission;
    private bool hasEmission;

    private void Awake()
    {
        // auto-find renderer (for those with child object setup)
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
        {
            matInstance = targetRenderer.material;

            if (matInstance.HasProperty("_EmissionColor"))
            {
                hasEmission = true;
                originalEmission = matInstance.GetColor("_EmissionColor");
            }
        }
        else
        {
            Debug.LogWarning($"[{name}] No Renderer found for TargetHighlightBehaviour.");
        }
    }

    public void Highlight()
    {
        if (!IsValid()) return;

        matInstance.EnableKeyword("_EMISSION");
        matInstance.SetColor("_EmissionColor", highlightColor * emissionIntensity);
    }

    public void Unhighlight()
    {
        if (!IsValid()) return;

        matInstance.SetColor("_EmissionColor", originalEmission);
    }

    private bool IsValid()
    {
        if (targetRenderer == null || matInstance == null || !hasEmission)
            return false;

        // prevent highlighting inactive objects (restore case)
        if (!targetRenderer.gameObject.activeInHierarchy)
            return false;

        return true;
    }
}