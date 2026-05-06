using UnityEngine;
using System.Collections.Generic;

public class ObjectStateInteraction : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private string objectID;
    [SerializeField] private List<ObjectStateAction> actions;

    public void OnInteract()
    {
        if (string.IsNullOrEmpty(objectID))
        {
            Debug.LogWarning("Object ID missing.");
            return;
        }

        int currentState = GameStateManager.Instance.GetState(objectID);

        foreach (var action in actions)
        {
            if (action.requiredState == currentState)
            {
                action.Execute(objectID);
                return;
            }
        }

        Debug.Log($"No matching object state for {objectID}: {currentState}");
    }
}