using UnityEngine;
using System.Collections;

public class StillnessWaning : WaningBase
{
    // manually change damage to 20 in inspector
    [Header("Visuals")]
    [SerializeField] private ParticleSystem fogParticles;
    [SerializeField] private ParticleSystem iceParticles;
    [SerializeField] private Renderer frostDecal;

    [Header("Disperse")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Colliders")]
    [SerializeField] private Collider damageZoneCollider; // drag the trigger collider here

    [Header("Movement Slow")]
    [SerializeField] private float movementMultiplier = 0.75f;

    private Collider zoneCollider;
    private bool isDispersed;
    private Material decalMaterial;
    private Color decalColor;

    private void Awake()
    {
        zoneCollider = damageZoneCollider;
        if (frostDecal != null)
        {
            decalMaterial = frostDecal.material;
            decalColor = decalMaterial.color;
        }
    }

    protected override void OnPlayerEntered(GameObject player)
    {
        if (isDispersed) return;

        PlayerTransformation playerTransform = player.GetComponent<PlayerTransformation>();
        if (playerTransform != null)
            playerTransform.SetSpeedModifier(movementMultiplier);

        StatusEffectManager.Instance?.AddSlow();
        StatusEffectManager.Instance?.SetFrostbite(true);
    }


    protected override void OnPlayerExited(GameObject player)
    {
        PlayerTransformation playerTransform = player.GetComponent<PlayerTransformation>();
        if (playerTransform != null)
            playerTransform.SetSpeedModifier(1f);

        StatusEffectManager.Instance?.RemoveSlow();
        StatusEffectManager.Instance?.SetFrostbite(false);
    }

    protected override void OnTriggerStay(Collider other)
    {
        if (isDispersed) return;
        base.OnTriggerStay(other);
    }


    public void Disperse()
    {
        if (isDispersed) return;
        StartCoroutine(DisperseRoutine());
        PlayerTransformation playerTransform = FindFirstObjectByType<PlayerTransformation>();
        if (playerTransform != null)
            playerTransform.SetSpeedModifier(1f);
    }

    private IEnumerator DisperseRoutine()
    {
        StatusEffectManager.Instance?.RemoveSlow();
        StatusEffectManager.Instance?.SetFrostbite(false);

        isDispersed = true;

        if (zoneCollider != null)
            zoneCollider.enabled = false;

        if (fogParticles != null)
            fogParticles.Stop();

        if (iceParticles != null)
            iceParticles.Stop();

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            if (decalMaterial != null)
            {
                Color color = decalColor;
                color.a = Mathf.Lerp(decalColor.a, 0f, t);
                decalMaterial.color = color;
            }

            yield return null;
        }


        Destroy(gameObject);
    }
}