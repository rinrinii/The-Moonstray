using UnityEngine;
using System.Collections;

public class ExpandingWaning : WaningBase
{
    [Header("References")]
    [SerializeField] private Transform visualMesh;

    [Header("Spread Settings")]
    [SerializeField] private BoxCollider damageTrigger;

    [Header("Movement Slow")]
    [SerializeField] private float movementMultiplier = 0.75f;

    [SerializeField] private Vector3 targetTriggerSize = new Vector3(1.5f, 1f, 1.5f);
    [SerializeField] private Vector3 targetVisualScale = new Vector3(1.5f, 1f, 1.5f);
    [SerializeField] private float spreadSpeed = 1.5f;

    private bool hasExpanded;
    private bool cleansed;
    private Vector3 initialTriggerSize;
    private Vector3 initialVisualScale;

    protected virtual void Awake()
    {
        if (damageTrigger == null)
        {
            Debug.LogError("Damage Trigger is missing.");
            return;
        }
        if (visualMesh == null)
        {
            Debug.LogError("Visual Mesh is missing.");
            return;
        }

        initialTriggerSize = damageTrigger.size;
        initialVisualScale = visualMesh.localScale;
    }

    protected override void OnPlayerEntered(GameObject player)
    {
        if (!hasExpanded)
            StartCoroutine(Expand());

        PlayerTransformation playerTransform =
            player.GetComponent<PlayerTransformation>();
        if (playerTransform != null)
            playerTransform.SetSpeedModifier(movementMultiplier);

        // Summer: slow + poison
        StatusEffectManager.Instance?.AddSlow();
        StatusEffectManager.Instance?.SetPoison(true);
    }

    protected override void OnPlayerExited(GameObject player)
    {
        PlayerTransformation playerTransform =
            player.GetComponent<PlayerTransformation>();
        if (playerTransform != null)
            playerTransform.SetSpeedModifier(1f);

        StatusEffectManager.Instance?.RemoveSlow();
        StatusEffectManager.Instance?.SetPoison(false);
    }

    protected override void ApplyDamage(GameObject player)
    {
        PlayerDash dash = player.GetComponent<PlayerDash>();
        if (dash != null && dash.IsDashing())
            return;

        base.ApplyDamage(player);
    }

    private IEnumerator Expand()
    {
        hasExpanded = true;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * spreadSpeed;

            damageTrigger.size = Vector3.Lerp(
                initialTriggerSize, targetTriggerSize, progress);

            visualMesh.localScale = Vector3.Lerp(
                initialVisualScale, targetVisualScale, progress);

            yield return null;
        }

        damageTrigger.size = targetTriggerSize;
        visualMesh.localScale = targetVisualScale;
    }

    public void Cleanse()
    {
        if (cleansed) return;
        cleansed = true;

        StatusEffectManager.Instance?.RemoveSlow();
        StatusEffectManager.Instance?.SetPoison(false);

        Debug.Log("Summer Waning Cleansed");

        damageTrigger.enabled = false;

        Collider rootCollider = GetComponent<Collider>();
        if (rootCollider != null)
            rootCollider.enabled = false;

        if (visualMesh != null)
            visualMesh.gameObject.SetActive(false);

        Destroy(gameObject, 0.2f);
    }
}