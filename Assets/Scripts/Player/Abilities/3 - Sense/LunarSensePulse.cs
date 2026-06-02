using UnityEngine;

public class LunarSensePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float duration = 0.75f;    // keep the same as duration in PlayerLunarSense.cs

    [Header("Visual")]
    [SerializeField] private float pulseHeight = 0.05f;

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
        // Unity cylinder scale uses diameter
        targetScale = radius * 2f;

        timer = 0f;
        playing = true;

        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }

        // reset opacity every time the pulse plays
        if (pulseMaterial != null)
        {
            Color color = baseColor;
            color.a = baseColor.a;

            pulseMaterial.color = color;
        }

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

        float t = Mathf.Clamp01(
            timer / duration
        );

        // smooth expansion
        float scaleT =
            Mathf.SmoothStep(
                0f,
                1f,
                t
            );

        float scale =
            Mathf.Lerp(
                0f,
                targetScale,
                scaleT
            );

        transform.localScale =
            new Vector3(
                scale,
                pulseHeight,
                scale
            );

        // fade stays visible longer, then rapidly near the end
        if (pulseMaterial != null)
        {
            float fadeT =
                1f - Mathf.Pow(
                    t,
                    3f
                );

            Color color = baseColor;

            color.a =
                baseColor.a *
                fadeT;

            pulseMaterial.color = color;
        }

        if (timer >= duration)
        {
            playing = false;

            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
        }
    }
}