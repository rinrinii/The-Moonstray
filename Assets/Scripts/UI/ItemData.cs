using UnityEngine;

public enum ItemType
{
    Consumable,
    Resource,
    Fragment,
    LostItem,
    KeyItem
}

[CreateAssetMenu(menuName = "Moonstray/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int itemID;
    [TextArea] public string description;
    public Sprite icon;

    public ItemType type;
    public bool canUse;

    public bool stackable = true;
}