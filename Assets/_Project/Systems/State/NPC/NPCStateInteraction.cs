using UnityEngine;
using System.Collections.Generic;

public class NPCStateInteraction : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private string objectID;
    [SerializeField] private List<NPCStateAction> actions;

    public void OnInteract()
    {
        if (string.IsNullOrEmpty(objectID))
        {
            Debug.LogWarning("NPC objectID missing.");
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

        Debug.Log($"No matching NPC state for {objectID}: {currentState}");
    }
}