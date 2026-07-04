using UnityEngine;

public class TargetHighlightBehaviour : MonoBehaviour, IHighlightable
{
    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Highlight")]
    [SerializeField] private Color highlightColor = new(1f, 0.95f, 0.35f);
    [SerializeField] private float highlightOutlineWidth = 20f;

    private Material matInstance;

    private Color originalOutlineColor;
    private float originalOutlineWidth;
    private float originalBlendBaseColor;
    private float originalLightColorOutline;

    private bool hasOutline;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogError($"{name}: No Renderer found.");
            return;
        }

        matInstance = targetRenderer.material;

        hasOutline =
            matInstance.HasProperty("_Outline_Color") &&
            matInstance.HasProperty("_Outline_Width");

        if (!hasOutline)
        {
            Debug.LogWarning($"{name}: Material does not support outline highlighting.");
            return;
        }

        originalOutlineColor = matInstance.GetColor("_Outline_Color");
        originalOutlineWidth = matInstance.GetFloat("_Outline_Width");

        if (matInstance.HasProperty("_Is_BlendBaseColor"))
            originalBlendBaseColor = matInstance.GetFloat("_Is_BlendBaseColor");

        if (matInstance.HasProperty("_Is_LightColor_Outline"))
            originalLightColorOutline = matInstance.GetFloat("_Is_LightColor_Outline");
    }

    public void Highlight()
    {
        if (!IsValid())
            return;

        if (matInstance.HasProperty("_OUTLINE"))
            matInstance.SetFloat("_OUTLINE", 1f);

        if (matInstance.HasProperty("_Is_BlendBaseColor"))
            matInstance.SetFloat("_Is_BlendBaseColor", 0f);

        if (matInstance.HasProperty("_Is_LightColor_Outline"))
            matInstance.SetFloat("_Is_LightColor_Outline", 0f);

        matInstance.SetColor("_Outline_Color", highlightColor);
        matInstance.SetFloat("_Outline_Width", highlightOutlineWidth);
    }

    public void Unhighlight()
    {
        if (!IsValid())
            return;

        matInstance.SetColor("_Outline_Color", originalOutlineColor);
        matInstance.SetFloat("_Outline_Width", originalOutlineWidth);

        if (matInstance.HasProperty("_Is_BlendBaseColor"))
            matInstance.SetFloat("_Is_BlendBaseColor", originalBlendBaseColor);

        if (matInstance.HasProperty("_Is_LightColor_Outline"))
            matInstance.SetFloat("_Is_LightColor_Outline", originalLightColorOutline);
    }

    private bool IsValid()
    {
        return targetRenderer != null &&
               matInstance != null &&
               hasOutline &&
               targetRenderer.gameObject.activeInHierarchy;
    }
}