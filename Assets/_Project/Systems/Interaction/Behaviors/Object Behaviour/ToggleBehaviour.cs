using UnityEngine;

public class ToggleBehaviour : MonoBehaviour, IObjectBehaviour
{
    [SerializeField] private GameObject targetObject;

    public void Execute()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("ToggleBehaviour: targetObject missing.");
            return;
        }

        targetObject.SetActive(false);

        Debug.Log($"{targetObject.name} picked up (set inactive).");
    }
}