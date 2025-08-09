using UnityEngine;

/// <summary>
/// Интерфейс для предметов в инвентаре
/// </summary>
public interface IItem
{
    /// <summary>
    /// Уникальный идентификатор предмета
    /// </summary>
    string ItemId { get; }
    
    /// <summary>
    /// Название предмета
    /// </summary>
    string ItemName { get; }
    
    /// <summary>
    /// Иконка предмета
    /// </summary>
    Sprite Icon { get; }
    
    /// <summary>
    /// Префаб предмета для размещения в мире
    /// </summary>
    GameObject Prefab { get; }
    
    /// <summary>
    /// Редкость предмета
    /// </summary>
    ItemRarity Rarity { get; }
    
    /// <summary>
    /// Максимальное количество предметов в одном слоте
    /// </summary>
    int MaxStackSize { get; }
    
    /// <summary>
    /// Можно ли складывать этот предмет с другим
    /// </summary>
    bool CanStackWith(IItem otherItem);
    
    /// <summary>
    /// Получить описание предмета
    /// </summary>
    string GetDescription();
}

/// <summary>
/// Перечисление редкости предметов
/// </summary>
public enum ItemRarity
{
    Common = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
} 