using UnityEngine;

public class InspectBehaviour : MonoBehaviour, IObjectBehaviour
{
    [SerializeField] private string inspectMessage;
    [SerializeField] private GameObject highlightMarker;

    public void Execute()
    {
        string message = string.IsNullOrEmpty(inspectMessage)
            ? $"You inspected {gameObject.name}"
            : inspectMessage;

        if (highlightMarker != null)
        {
            highlightMarker.SetActive(false);
        }

        Debug.Log(message);
    }
}