using UnityEngine;

/// <summary>
/// Представляет слот инвентаря с предметом и его количеством
/// </summary>
[System.Serializable]
public class InventorySlot : IInventorySlot
{
    [SerializeField] private Item item;
    [SerializeField] private int quantity;

    public const int MAX_STACK_SIZE = 99;
    
    // Реализация интерфейса IInventorySlot
    public IItem Item => item;
    public int Quantity => quantity;
    public bool IsEmpty => item == null || quantity <= 0;
    public bool CanAddMore => item != null && quantity < MAX_STACK_SIZE;
    public int MaxStackSize => MAX_STACK_SIZE;

    // Оставляем для обратной совместимости
    public Item OriginalItem => item;

    public bool CanStackWith(Item otherItem)
    {
        return item != null && otherItem != null && 
               item.itemName == otherItem.itemName && 
               item.rarity == otherItem.rarity;
    }
    
    /// <summary>
    /// Создает пустой слот
    /// </summary>
    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }
    
    /// <summary>
    /// Создает слот с предметом
    /// </summary>
    public InventorySlot(Item item, int quantity = 1)
    {
        this.item = item;
        this.quantity = Mathf.Clamp(quantity, 0, MAX_STACK_SIZE);
    }
    
    /// <summary>
    /// Добавляет предметы в слот (только если это тот же предмет)
    /// </summary>
    /// <param name="itemToAdd">Предмет для добавления</param>
    /// <param name="amount">Количество для добавления</param>
    /// <returns>Количество предметов, которые не поместились</returns>
    public int AddItems(IItem itemToAdd, int amount = 1)
    {
        if (itemToAdd == null || amount <= 0)
            return amount;
        
        // Конвертируем IItem в Item если нужно
        Item originalItem = itemToAdd as Item ?? (itemToAdd as ItemWrapper)?.GetOriginalItem();
        if (originalItem == null)
        {
            Debug.LogWarning($"AddItems: не удалось конвертировать IItem в Item: {itemToAdd?.ItemName}");
            return amount;
        }
        
        return AddItemsInternal(originalItem, amount);
    }
    
    /// <summary>
    /// Внутренний метод для добавления предметов (для обратной совместимости)
    /// </summary>
    public int AddItems(Item itemToAdd, int amount = 1)
    {
        return AddItemsInternal(itemToAdd, amount);
    }
    
    private int AddItemsInternal(Item itemToAdd, int amount)
    {
        if (itemToAdd == null || amount <= 0)
            return amount;
        
        // Если слот пустой, создаем новый стек
        if (IsEmpty)
        {
            item = itemToAdd;
            quantity = Mathf.Min(amount, MAX_STACK_SIZE);
            return Mathf.Max(0, amount - MAX_STACK_SIZE);
        }
        
        // Если это тот же предмет, добавляем к существующему стеку
        if (CanStackWith(itemToAdd))
        {
            int spaceLeft = MAX_STACK_SIZE - quantity;
            int canAdd = Mathf.Min(amount, spaceLeft);
            quantity += canAdd;
            return amount - canAdd;
        }
        
        // Если это другой предмет, возвращаем все количество
        return amount;
    }
    
    /// <summary>
    /// Удаляет предметы из слота
    /// </summary>
    /// <param name="amount">Количество для удаления</param>
    /// <returns>Количество предметов, которые были удалены</returns>
    public int RemoveItems(int amount = 1)
    {
        if (IsEmpty || amount <= 0)
        {
            Debug.Log($"RemoveItems: слот пуст или amount <= 0. IsEmpty={IsEmpty}, amount={amount}");
            return 0;
        }
        
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        
        Debug.Log($"RemoveItems: удалено {removed} из {item?.itemName}, осталось: {quantity}");
        
        // Если слот стал пустым, очищаем его
        if (quantity <= 0)
        {
            Debug.Log($"RemoveItems: слот стал пустым, очищаем");
            Clear();
        }
        
        return removed;
    }
    
    /// <summary>
    /// Очищает слот
    /// </summary>
    public void Clear()
    {
        item = null;
        quantity = 0;
    }
    
    /// <summary>
    /// Получает количество свободного места в стеке
    /// </summary>
    public int GetFreeSpace()
    {
        if (IsEmpty)
            return MAX_STACK_SIZE;
        
        return MAX_STACK_SIZE - quantity;
    }
    
    /// <summary>
    /// Проверяет, можно ли добавить указанное количество предметов
    /// </summary>
    public bool CanAddAmount(int amount)
    {
        if (IsEmpty)
            return amount <= MAX_STACK_SIZE;
        
        return quantity + amount <= MAX_STACK_SIZE;
    }
    
    /// <summary>
    /// Проверяет, можно ли сложить предметы с другим слотом
    /// </summary>
    public bool CanStackWith(IInventorySlot otherSlot)
    {
        return otherSlot != null && !otherSlot.IsEmpty && 
               Item != null && otherSlot.Item != null && 
               Item.CanStackWith(otherSlot.Item);
    }
    
    /// <summary>
    /// Создает копию слота
    /// </summary>
    public IInventorySlot Clone()
    {
        return new InventorySlot(item, quantity);
    }
    
    /// <summary>
    /// Создает копию слота (для обратной совместимости)
    /// </summary>
    public InventorySlot CloneOriginal()
    {
        return new InventorySlot(item, quantity);
    }
    
    public override string ToString()
    {
        if (IsEmpty)
            return "Empty Slot";
        
        return $"{item.itemName} x{quantity}";
    }
} 