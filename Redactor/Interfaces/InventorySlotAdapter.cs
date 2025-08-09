using UnityEngine;

/// <summary>
/// Адаптер для использования существующего класса InventorySlot через интерфейс IInventorySlot
/// </summary>
public class InventorySlotAdapter : IInventorySlot
{
    private readonly InventorySlot _originalSlot;
    
    public InventorySlotAdapter(InventorySlot originalSlot)
    {
        _originalSlot = originalSlot ?? throw new System.ArgumentNullException(nameof(originalSlot));
    }
    
    // Реализация интерфейса IInventorySlot
    public IItem Item => _originalSlot.Item;
    public int Quantity => _originalSlot.Quantity;
    public bool IsEmpty => _originalSlot.IsEmpty;
    public bool CanAddMore => _originalSlot.CanAddMore;
    public int MaxStackSize => _originalSlot.MaxStackSize;
    
    public int AddItems(IItem item, int amount = 1)
    {
        return _originalSlot.AddItems(item, amount);
    }
    
    public int RemoveItems(int amount = 1)
    {
        return _originalSlot.RemoveItems(amount);
    }
    
    public void Clear()
    {
        _originalSlot.Clear();
    }
    
    public int GetFreeSpace()
    {
        return _originalSlot.GetFreeSpace();
    }
    
    public bool CanAddAmount(int amount)
    {
        return _originalSlot.CanAddAmount(amount);
    }
    
    public bool CanStackWith(IInventorySlot otherSlot)
    {
        return _originalSlot.CanStackWith(otherSlot);
    }
    
    public IInventorySlot Clone()
    {
        return new InventorySlotAdapter(_originalSlot.CloneOriginal());
    }
    
    /// <summary>
    /// Получает оригинальный слот для обратной совместимости
    /// </summary>
    public InventorySlot GetOriginalSlot()
    {
        return _originalSlot;
    }
    
    public override string ToString()
    {
        return _originalSlot.ToString();
    }
} 