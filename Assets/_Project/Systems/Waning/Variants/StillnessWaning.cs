using UnityEngine;
using System.Collections;

public class StillnessWaning : WaningBase
{
    // manually change damage to 20 in inspector

    [Header("Visuals")]
    [SerializeField] private ParticleSystem fogParticles;
    [SerializeField] private Renderer frostDecal;

    [Header("Disperse")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Collider zoneCollider;
    private bool isDispersed;

    private Material decalMaterial;
    private Color decalColor;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();

        if (frostDecal != null)
        {
            decalMaterial = frostDecal.material;
            decalColor = decalMaterial.color;
        }
    }

    protected override void OnTriggerStay(Collider other)
    {
        if (isDispersed)
            return;

        base.OnTriggerStay(other);
    }

    public void Disperse()
    {
        if (isDispersed)
            return;

        StartCoroutine(DisperseRoutine());
    }

    private IEnumerator DisperseRoutine()
    {
        isDispersed = true;

        // Stop damage instantly
        if (zoneCollider != null)
        {
            zoneCollider.enabled = false;
        }

        // Stop spawning new fog particles
        if (fogParticles != null)
        {
            fogParticles.Stop();
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;

            if (decalMaterial != null)
            {
                Color color = decalColor;
                color.a = Mathf.Lerp(
                    decalColor.a,
                    0f,
                    t
                );

                decalMaterial.color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}