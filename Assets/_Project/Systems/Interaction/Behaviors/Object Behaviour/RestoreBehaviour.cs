using UnityEngine;

public class RestoreBehaviour : MonoBehaviour, IObjectBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject targetObject;

    [SerializeField] private GameObject highlightMarker;

    [Header("State Objects")]
    [SerializeField] private GameObject brokenVersion;
    [SerializeField] private GameObject fixedVersion;

    private bool runtimeReplacementPrepared;

    public void PrepareRuntimeReplacement()
    {
        if (runtimeReplacementPrepared || brokenVersion == null ||
            fixedVersion == null)
        {
            return;
        }

        // The Village Basin hierarchy uses an existing repaired canal as the
        // model reference. Clone it so restoring this canal does not move or
        // disable the original piece elsewhere in the irrigation layout.
        GameObject replacement = Instantiate(
            fixedVersion,
            brokenVersion.transform.parent);
        replacement.name = $"{brokenVersion.name} (Restored)";
        replacement.transform.SetLocalPositionAndRotation(
            brokenVersion.transform.localPosition,
            brokenVersion.transform.localRotation);
        replacement.transform.localScale = brokenVersion.transform.localScale;
        replacement.SetActive(false);

        fixedVersion = replacement;
        runtimeReplacementPrepared = true;
    }

    public void Execute()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("RestoreBehaviour: targetObject missing.");
            return;
        }

        PrepareRuntimeReplacement();

        if (brokenVersion != null)
            brokenVersion.SetActive(false);

        if (fixedVersion != null)
            fixedVersion.SetActive(true);

        if (highlightMarker != null)
        {
            highlightMarker.SetActive(false);
        }

        Debug.Log($"{targetObject.name} restored.");
    }
}
