using UnityEngine;

public class CollectBehaviour : MonoBehaviour, IObjectBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject targetObject;

    [Header("Item")]
    [SerializeField] private ItemData item;
    [SerializeField] private int amount = 1;

    [Header("Collection")]
    [SerializeField] private string collectMessage;

    public void Execute()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("CollectBehaviour: targetObject missing.");
            return;
        }

        if (item != null && InventorySystem.Instance != null)
        {
            bool added = InventorySystem.Instance.Add(item, amount);
            if (!added)
            {
                Debug.Log("Inventory full.");
                return;
            }
        }

        string message = string.IsNullOrEmpty(collectMessage)
            ? $"{targetObject.name} collected."
            : collectMessage;

        Debug.Log(message);
        targetObject.SetActive(false);
    }
}