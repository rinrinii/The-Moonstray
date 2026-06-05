using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FootprintFade : MonoBehaviour
{
    public float visibleTime = 20f;
    public float fadeDuration = 10f;

    private DecalProjector decal;
    private float timer;

    void Awake()
    {
        decal = GetComponent<DecalProjector>();

        if (decal == null)
        {
            Debug.LogError("Missing DecalProjector on " + gameObject.name);
            enabled = false;
            return;
        }

        // Ensure it starts fully visible
        decal.fadeFactor = 1f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < visibleTime)
            return;

        // Calculate the fading alpha factor (1 down to 0)
        float t = (timer - visibleTime) / fadeDuration;
        float alpha = Mathf.Clamp01(1f - t);

        decal.fadeFactor = alpha;

        if (alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}