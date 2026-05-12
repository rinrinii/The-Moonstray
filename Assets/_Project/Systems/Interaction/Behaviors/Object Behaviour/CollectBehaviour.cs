using UnityEngine;

public class CollectBehaviour : MonoBehaviour, IObjectBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject targetObject;

    [Header("Collection")]
    [SerializeField] private string collectMessage;

    public void Execute()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("CollectBehaviour: targetObject missing.");
            return;
        }

        string message = string.IsNullOrEmpty(collectMessage)
            ? $"{targetObject.name} collected."
            : collectMessage;

        Debug.Log(message);

        // temporary collection behavior
        // simulates pickup until inventory system is implemented
        targetObject.SetActive(false);

        /*
         * FUTURE EXPANSION
         * - Add to inventory
         * - Play pickup sound
         * - Spawn pickup VFX
         * - Update quests/objectives
         * - Save collected state
         */
    }
}