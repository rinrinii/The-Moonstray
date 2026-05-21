using UnityEngine;
using System.Collections;

public class ExpandingWaning : WaningBase
{
    [Header("References")]
    [SerializeField] private Transform visualMesh;

    [Header("Spread Settings")]
    [SerializeField] private BoxCollider damageTrigger;

    [SerializeField] private Vector3 targetTriggerSize = new Vector3(1.5f, 1f, 1.5f);
    [SerializeField] private Vector3 targetVisualScale = new Vector3(1.5f, 1f, 1.5f);

    [SerializeField] private float spreadSpeed = 1.5f;

    private bool hasExpanded;

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
        {
            StartCoroutine(Expand());
        }
    }

    private IEnumerator Expand()
    {
        hasExpanded = true;

        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * spreadSpeed;

            damageTrigger.size = Vector3.Lerp(
                initialTriggerSize,
                targetTriggerSize,
                progress
            );

            visualMesh.localScale = Vector3.Lerp(
                initialVisualScale,
                targetVisualScale,
                progress
            );

            yield return null;
        }

        damageTrigger.size = targetTriggerSize;
        visualMesh.localScale = targetVisualScale;
    }
}