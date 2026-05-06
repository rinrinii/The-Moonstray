using UnityEngine;

public class ToggleBehaviour : MonoBehaviour, IObjectBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject targetObject;

    [Header("Toggle State")]
    [SerializeField] private bool startActive = true;

    private bool currentState;

    private void Awake()
    {
        currentState = startActive;

        if (targetObject != null)
        {
            targetObject.SetActive(currentState);
        }
    }

    public void Execute()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("ToggleBehaviour: targetObject missing.");
            return;
        }

        // temporarily set to inactive to simulate state change
        currentState = !currentState;

        targetObject.SetActive(currentState);

        Debug.Log($"{targetObject.name} toggled: {currentState}");
    }
}