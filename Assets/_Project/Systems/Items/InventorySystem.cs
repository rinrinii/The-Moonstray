using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [Serializable]
    public sealed class Snapshot
    {
        public int maxSlots;
        public List<SlotRecord> slots = new();
    }

    [Serializable]
    public sealed class SlotRecord
    {
        public int itemID;
        public string itemName;
        public int amount;
    }

    public static InventorySystem Instance;
    public event Action OnInventoryChanged; 

    [System.Serializable]
    public class Slot
    {
        public ItemData item;
        public int amount;
    }

    public int maxSlots = 10;
    public List<Slot> slots = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool Add(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        if (item.stackable)
        {
            foreach (var slot in slots)
            {
                if (IsSameItem(slot.item, item))
                {
                    slot.amount += amount;
                    OnInventoryChanged?.Invoke(); 
                    return true;
                }
            }
        }
        if (slots.Count < maxSlots)
        {
            slots.Add(new Slot { item = item, amount = amount });
            OnInventoryChanged?.Invoke(); 
            return true;
        }
        return false;
    }

    public bool Remove(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        for (int i = 0; i < slots.Count; i++)
        {
            if (IsSameItem(slots[i].item, item))
            {
                slots[i].amount -= amount;
                if (slots[i].amount <= 0)
                    slots.RemoveAt(i);
                OnInventoryChanged?.Invoke(); 
                return true;
            }
        }
        return false;
    }

    private bool IsSameItem(ItemData first, ItemData second)
    {
        if (first == null || second == null)
            return false;

        if (first == second)
            return true;

        if (first.itemID != 0 && first.itemID == second.itemID)
            return true;

        return !string.IsNullOrWhiteSpace(first.itemName) &&
            first.itemName == second.itemName;
    }

    public Snapshot CaptureState()
    {
        Snapshot snapshot = new() { maxSlots = maxSlots };
        foreach (Slot slot in slots)
        {
            if (slot?.item == null || slot.amount <= 0)
                continue;

            snapshot.slots.Add(new SlotRecord
            {
                itemID = slot.item.itemID,
                itemName = slot.item.itemName,
                amount = slot.amount
            });
        }

        return snapshot;
    }

    public void RestoreState(Snapshot snapshot)
    {
        slots.Clear();
        if (snapshot != null)
        {
            maxSlots = snapshot.maxSlots > 0 ? snapshot.maxSlots : maxSlots;
            ItemData[] items = Resources.LoadAll<ItemData>("Items");
            foreach (SlotRecord record in snapshot.slots ?? new List<SlotRecord>())
            {
                ItemData item = Array.Find(items, candidate =>
                    candidate != null &&
                    ((record.itemID != 0 && candidate.itemID == record.itemID) ||
                     (!string.IsNullOrWhiteSpace(record.itemName) && candidate.itemName == record.itemName)));

                if (item == null)
                {
                    Debug.LogWarning($"Inventory restore skipped unavailable item '{record.itemName}' ({record.itemID}).");
                    continue;
                }

                if (record.amount > 0)
                    slots.Add(new Slot { item = item, amount = record.amount });
            }
        }

        OnInventoryChanged?.Invoke();
    }

    public void ResetState()
    {
        slots.Clear();
        OnInventoryChanged?.Invoke();
    }
}
