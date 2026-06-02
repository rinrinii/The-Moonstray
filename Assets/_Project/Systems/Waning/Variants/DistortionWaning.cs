using System.Collections;
using UnityEngine;

public class DistortionWaning : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    private Renderer meshRenderer;
    private Collider meshCollider;

    private Material materialInstance;

    private bool revealed;

    private void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
        meshCollider = GetComponent<Collider>();

        if (meshRenderer != null)
        {
            materialInstance = meshRenderer.material;
        }
    }

    public void Reveal()
    {
        if (revealed)
            return;

        revealed = true;

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        if (meshCollider != null)
        {
            meshCollider.enabled = false;
        }

        if (materialInstance == null)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            yield break;
        }

        Color startColor = materialInstance.color;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / fadeDuration
            );

            float fade =
                1f - Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            Color color = startColor;
            color.a = startColor.a * fade;

            materialInstance.color = color;

            yield return null;
        }

        meshRenderer.enabled = false;
    }
}