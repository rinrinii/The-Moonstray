using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
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
        if (item.stackable)
        {
            foreach (var slot in slots)
            {
                if (slot.item == item)
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
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
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
}