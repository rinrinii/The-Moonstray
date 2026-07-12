using UnityEngine;
using System.Collections.Generic;

public class ObjectStateInteraction : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private string objectID;
    [SerializeField] private bool useInstanceID;
    [SerializeField, HideInInspector] private string instanceID;

    [SerializeField] private List<ObjectStateAction> actions;

    private string cachedStateID;

    private void Start()
    {
        cachedStateID = GetStateID();

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnObjectStateChanged += HandleObjectStateChanged;

        RefreshState();
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnObjectStateChanged -= HandleObjectStateChanged;
    }

    public void OnInteract()
    {
        string stateID = GetStateID();
        cachedStateID = stateID;

        if (string.IsNullOrEmpty(stateID))
        {
            Debug.LogWarning("Object ID missing.");
            return;
        }

        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("GameStateManager missing.");
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

    private void RefreshState()
    {
        if (string.IsNullOrEmpty(cachedStateID) ||
            GameStateManager.Instance == null)
        {
            return;
        }

        int currentState = GameStateManager.Instance.GetState(cachedStateID);

        if (HasActionForState(currentState))
            return;

        CollectBehaviour collectBehaviour =
            GetComponent<CollectBehaviour>();

        collectBehaviour?.ApplyCollectedState();
    }

    private bool HasActionForState(int state)
    {
        if (actions == null)
            return false;

        foreach (ObjectStateAction action in actions)
        {
            if (action != null && action.requiredState == state)
                return true;
        }

        return false;
    }

    private void HandleObjectStateChanged(string changedObjectID, int newState)
    {
        if (changedObjectID != cachedStateID)
            return;

        RefreshState();
    }

    private string GetStateID()
    {
        if (!useInstanceID)
            return objectID;

        if (string.IsNullOrEmpty(instanceID))
            EnsureInstanceID();

        string instanceKey = GetSceneInstanceKey();

        if (string.IsNullOrEmpty(objectID))
            return instanceKey;

        return $"{objectID}:{instanceKey}";
    }

    private void EnsureInstanceID()
    {
        if (!string.IsNullOrEmpty(instanceID))
            return;

        instanceID = System.Guid.NewGuid().ToString("N");
    }

    private string GetSceneInstanceKey()
    {
        string sceneName = gameObject.scene.IsValid()
            ? gameObject.scene.name
            : "NoScene";

        return $"{sceneName}/{GetTransformPath(transform)}:{instanceID}";
    }

    private static string GetTransformPath(Transform current)
    {
        if (current == null)
            return string.Empty;

        string path = $"{current.name}[{current.GetSiblingIndex()}]";
        Transform parent = current.parent;

        while (parent != null)
        {
            path = $"{parent.name}[{parent.GetSiblingIndex()}]/{path}";
            parent = parent.parent;
        }

        return path;
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
