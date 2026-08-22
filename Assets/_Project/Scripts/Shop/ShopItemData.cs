using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public abstract class ShopItemData : ScriptableObject
{
    [Header("Shop Info")]
    public string itemId;
    public string itemName;
    public Sprite itemIcon;
    public ItemRarity rarity;
    public int basePrice;
}