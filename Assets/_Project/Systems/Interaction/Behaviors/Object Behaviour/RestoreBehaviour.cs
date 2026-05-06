using UnityEngine;

public class RestoreBehaviour : MonoBehaviour, IObjectBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject targetObject;

    [SerializeField] private GameObject highlightMarker;

    [Header("State Objects")]
    [SerializeField] private GameObject brokenVersion;
    [SerializeField] private GameObject fixedVersion;

    public void Execute()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("RestoreBehaviour: targetObject missing.");
            return;
        }

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