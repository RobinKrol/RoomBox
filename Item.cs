using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "RoomBox/Item")]
public class Item : ScriptableObject, IItem
{
    public enum ItemRarityType
    {
        Обычный = 1,
        Редкий = 2,
        Эпический = 3,
        Легендарный = 4
    }

    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public ItemRarityType rarity;

    // Реализация интерфейса IItem
    public string ItemId => itemName;
    public string ItemName => itemName;
    public Sprite Icon => icon;
    public GameObject Prefab => prefab;
    public ItemRarity Rarity => ConvertRarity(rarity);
    public int MaxStackSize => 99;

    public bool CanStackWith(IItem otherItem)
    {
        return otherItem != null && 
               ItemId == otherItem.ItemId && 
               Rarity == otherItem.Rarity;
    }

    public string GetDescription()
    {
        return $"Предмет: {ItemName}\nРедкость: {Rarity}\nМаксимальный стек: {MaxStackSize}";
    }

    private static ItemRarity ConvertRarity(ItemRarityType oldRarity)
    {
        return oldRarity switch
        {
            ItemRarityType.Обычный => ItemRarity.Common,
            ItemRarityType.Редкий => ItemRarity.Rare,
            ItemRarityType.Эпический => ItemRarity.Epic,
            ItemRarityType.Легендарный => ItemRarity.Legendary,
            _ => ItemRarity.Common
        };
    }
}
