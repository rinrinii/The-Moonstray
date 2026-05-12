using UnityEngine;

public class StateInteraction : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private string objectID;
    [SerializeField] private StateAction stateAction;

    public void OnInteract()
    {
        if (stateAction == null)
        {
            Debug.LogWarning("StateAction is missing.");
            return;
        }

        if (string.IsNullOrEmpty(objectID))
        {
            Debug.LogWarning("Object ID is missing.");
            return;
        }

        stateAction.Execute(objectID);
    }
}