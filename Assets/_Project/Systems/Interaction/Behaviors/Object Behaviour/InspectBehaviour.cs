using UnityEngine;

public class InspectBehaviour : MonoBehaviour, IObjectBehaviour
{
    [SerializeField] private string inspectMessage;

    public void Execute()
    {
        string message = string.IsNullOrEmpty(inspectMessage)
            ? $"You inspected {gameObject.name}"
            : inspectMessage;

        Debug.Log(message);
    }
}