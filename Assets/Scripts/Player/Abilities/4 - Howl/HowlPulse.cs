using UnityEngine;

public class HowlPulse : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private float pulseHeight = 0.08f;

    private float timer;
    private bool playing;
    private float targetScale;

    private MeshRenderer meshRenderer;

    private Material pulseMaterial;
    private Color baseColor;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;

            pulseMaterial = meshRenderer.material;
            baseColor = pulseMaterial.color;
        }
    }

    public void Play(float radius)
    {
        targetScale = radius * 2f;

        timer = 0f;
        playing = true;

        meshRenderer.enabled = true;

        Color color = baseColor;
        color.a = baseColor.a;
        pulseMaterial.color = color;

        transform.localScale =
            new Vector3(
                0f,
                pulseHeight,
                0f
            );
    }

    private void Update()
    {
        if (!playing)
            return;

        timer += Time.deltaTime;

        float t =
            Mathf.Clamp01(
                timer / duration
            );

        // smoother expansion
        float eased =
            1f - Mathf.Pow(
                1f - t,
                3f
            );

        float scale =
            Mathf.Lerp(
                0f,
                targetScale,
                eased
            );

        transform.localScale =
            new Vector3(
                scale,
                pulseHeight,
                scale
            );

        Color color = baseColor;

        color.a =
            Mathf.Lerp(
                baseColor.a,
                0f,
                t * t
            );

        pulseMaterial.color = color;

        if (t >= 1f)
        {
            playing = false;
            meshRenderer.enabled = false;
        }
    }
}