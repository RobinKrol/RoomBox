using UnityEngine;

/// <summary>
/// Адаптер для существующего класса Item, реализующий интерфейс IItem
/// </summary>
public static class ItemAdapter
{
    /// <summary>
    /// Создает IItem из существующего Item
    /// </summary>
    /// <param name="item">Существующий предмет</param>
    /// <returns>IItem обертка или null если item равен null</returns>
    public static IItem ToIItem(this Item item)
    {
        return item != null ? new ItemWrapper(item) : null;
    }
}

/// <summary>
/// Обертка для существующего класса Item
/// </summary>
public class ItemWrapper : IItem
{
    private readonly Item _item;

    public ItemWrapper(Item item)
    {
        _item = item ?? throw new System.ArgumentNullException(nameof(item));
    }

    public string ItemId => _item.itemName; // Используем itemName как ID
    public string ItemName => _item.itemName;
    public Sprite Icon => _item.icon;
    public GameObject Prefab => _item.prefab;
    public ItemRarity Rarity => ConvertRarity(_item.rarity);
    public int MaxStackSize => 99; // Стандартное значение

    public bool CanStackWith(IItem otherItem)
    {
        if (otherItem == null) return false;
        return ItemId == otherItem.ItemId && Rarity == otherItem.Rarity;
    }

    public string GetDescription()
    {
        return $"Предмет: {ItemName}, Редкость: {Rarity}";
    }

    /// <summary>
    /// Конвертирует старый enum Rarity в новый ItemRarity
    /// </summary>
    private static ItemRarity ConvertRarity(Item.ItemRarityType oldRarity)
    {
        return oldRarity switch
        {
            Item.ItemRarityType.Обычный => ItemRarity.Common,
            Item.ItemRarityType.Редкий => ItemRarity.Rare,
            Item.ItemRarityType.Эпический => ItemRarity.Epic,
            Item.ItemRarityType.Легендарный => ItemRarity.Legendary,
            _ => ItemRarity.Common
        };
    }

    /// <summary>
    /// Получить оригинальный Item
    /// </summary>
    public Item GetOriginalItem() => _item;
} 