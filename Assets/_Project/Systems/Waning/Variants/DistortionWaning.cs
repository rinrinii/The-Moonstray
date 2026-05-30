using UnityEngine;

// Adding IObjectBehaviour lets ObjectStateAction talk to this script directly!
public class DistortionWaning : MonoBehaviour, IObjectBehaviour
{
    [Header("Sibling Link")]
    [Tooltip("Drag the 'HiddenMesh' GameObject (the sibling) here.")]
    [SerializeField] private GameObject hiddenMesh;

    [Header("Glow Visual Settings")]
    [Tooltip("The color the hidden pathway will change to upon activation.")]
    [SerializeField] private Color revealedColor = new Color(1f, 0.4f, 0f); // Vibrant Orange
    [SerializeField] private float emissionIntensity = 2.0f;

    private Renderer hiddenMeshRenderer;
    private Renderer visualMeshRenderer;
    private Collider visualMeshCollider;

    private void Awake()
    {
        // Cache our own door components so we can hide them
        visualMeshRenderer = GetComponent<Renderer>();
        visualMeshCollider = GetComponent<Collider>();

        // Cache the sibling pathway's renderer
        if (hiddenMesh != null)
        {
            hiddenMeshRenderer = hiddenMesh.GetComponent<Renderer>();
        }
    }

    private void Start()
    {
        // Ensure the hidden pathway starts completely invisible at launch
        if (hiddenMesh != null)
        {
            hiddenMesh.SetActive(false);
        }
    }

    /// <summary>
    /// This fulfills the IObjectBehaviour requirement. 
    /// ObjectStateAction will call this automatically on interaction!
    /// </summary>
    public void Execute()
    {
        // 1. Hide this VisualMesh door (turn off renderer and collider so the player can pass)
        if (visualMeshRenderer != null) visualMeshRenderer.enabled = false;
        if (visualMeshCollider != null) visualMeshCollider.enabled = false;

        // 2. Reveal the sibling pathway
        if (hiddenMesh != null)
        {
            hiddenMesh.SetActive(true);

            // 3. Turn it glowing orange
            if (hiddenMeshRenderer != null)
            {
                Material matInstance = hiddenMeshRenderer.material;

                if (matInstance.HasProperty("_Color"))
                {
                    matInstance.SetColor("_Color", revealedColor);
                }

                if (matInstance.HasProperty("_EmissionColor"))
                {
                    matInstance.EnableKeyword("_EMISSION");
                    matInstance.SetColor("_EmissionColor", revealedColor * emissionIntensity);
                }
            }
        }

        Debug.Log($"[{name}] ObjectStateAction executed! VisualMesh hidden, HiddenMesh path revealed glowing orange.");
    }
}