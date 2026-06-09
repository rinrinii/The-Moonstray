using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public float range = 2.5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        if (InventorySystem.Instance == null) return;
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (var hit in hits)
        {
            WorldItem worldItem = hit.GetComponent<WorldItem>();
            if (worldItem != null && worldItem.item != null)
            {
                bool added = InventorySystem.Instance.Add(worldItem.item, worldItem.amount);
                if (added)
                    Destroy(worldItem.gameObject);
                break;
            }
        }
    }
}