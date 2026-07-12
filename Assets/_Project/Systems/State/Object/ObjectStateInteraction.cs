using UnityEngine;
using System.Collections.Generic;

public class ObjectStateInteraction : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private string objectID;
    [SerializeField] private bool useInstanceID;
    [SerializeField, HideInInspector] private string instanceID;

    [SerializeField] private List<ObjectStateAction> actions;

    public void OnInteract()
    {
        string stateID = GetStateID();

        if (string.IsNullOrEmpty(stateID))
        {
            Debug.LogWarning("Object ID missing.");
            return;
        }

        int currentState = GameStateManager.Instance.GetState(stateID);

        foreach (var action in actions)
        {
            if (action.requiredState == currentState)
            {
                action.Execute(stateID);
                return;
            }
        }

        Debug.Log($"No matching object state for {stateID}: {currentState}");
    }

    private string GetStateID()
    {
        if (!useInstanceID)
            return objectID;

        if (string.IsNullOrEmpty(instanceID))
            EnsureInstanceID();

        if (string.IsNullOrEmpty(objectID))
            return instanceID;

        return $"{objectID}:{instanceID}";
    }

    private void EnsureInstanceID()
    {
        if (!string.IsNullOrEmpty(instanceID))
            return;

        instanceID = System.Guid.NewGuid().ToString("N");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!useInstanceID)
            return;

        if (UnityEditor.EditorUtility.IsPersistent(gameObject))
            return;

        EnsureInstanceID();
    }
#endif
}
