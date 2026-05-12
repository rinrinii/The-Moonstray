using UnityEngine;

[System.Serializable]
public class ObjectStateAction
{
    public int requiredState;
    public int nextState;

    [Header("Behaviour")]
    [SerializeField] private MonoBehaviour behaviour;

    public void Execute(string objectID)
    {
        int currentState = GameStateManager.Instance.GetState(objectID);

        if (currentState != requiredState)
            return;

        if (behaviour is IObjectBehaviour objBehaviour)
        {
            objBehaviour.Execute();
        }
        else
        {
            Debug.LogWarning("Assigned behaviour does not implement IObjectBehaviour.");
        }

        GameStateManager.Instance.SetState(objectID, nextState);
    }
}