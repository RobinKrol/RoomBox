using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

/// <summary>
/// Пример unit-тестов для демонстрации тестируемости с интерфейсами
/// </summary>
[TestFixture]
public class InventoryManagerTests
{
    private MockInventoryManager _inventoryManager;
    private MockItem _testItem;

    [SetUp]
    public void Setup()
    {
        _inventoryManager = new MockInventoryManager();
        _testItem = new MockItem("TestItem", "Test Item", 99);
    }

    [Test]
    public void AddItem_ValidItem_ReturnsTrue()
    {
        // Act
        bool result = _inventoryManager.AddItem(_testItem, 1);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, _inventoryManager.GetItemCount(_testItem));
    }

    [Test]
    public void AddItem_InvalidItem_ReturnsFalse()
    {
        // Act
        bool result = _inventoryManager.AddItem(null, 1);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(0, _inventoryManager.GetTotalItemCount());
    }

    [Test]
    public void RemoveItem_ExistingItem_ReturnsTrue()
    {
        // Arrange
        _inventoryManager.AddItem(_testItem, 5);

        // Act
        bool result = _inventoryManager.RemoveItem(_testItem, 2);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(3, _inventoryManager.GetItemCount(_testItem));
    }

    [Test]
    public void RemoveItem_NonExistingItem_ReturnsFalse()
    {
        // Act
        bool result = _inventoryManager.RemoveItem(_testItem, 1);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(0, _inventoryManager.GetItemCount(_testItem));
    }

    [Test]
    public void HasItem_ExistingItem_ReturnsTrue()
    {
        // Arrange
        _inventoryManager.AddItem(_testItem, 3);

        // Act
        bool result = _inventoryManager.HasItem(_testItem, 2);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void HasItem_InsufficientQuantity_ReturnsFalse()
    {
        // Arrange
        _inventoryManager.AddItem(_testItem, 1);

        // Act
        bool result = _inventoryManager.HasItem(_testItem, 2);

        // Assert
        Assert.IsFalse(result);
    }
}

/// <summary>
/// Мок для IItem для тестирования
/// </summary>
public class MockItem : IItem
{
    public string ItemId { get; }
    public string ItemName { get; }
    public Sprite Icon => null;
    public GameObject Prefab => null;
    public ItemRarity Rarity => ItemRarity.Common;
    public int MaxStackSize { get; }

    public MockItem(string itemId, string itemName, int maxStackSize = 99)
    {
        ItemId = itemId;
        ItemName = itemName;
        MaxStackSize = maxStackSize;
    }

    public bool CanStackWith(IItem otherItem)
    {
        return otherItem != null && ItemId == otherItem.ItemId;
    }

    public string GetDescription()
    {
        return $"Mock Item: {ItemName}";
    }
}

/// <summary>
/// Мок для IInventorySlot для тестирования
/// </summary>
public class MockInventorySlot : IInventorySlot
{
    public IItem Item { get; private set; }
    public int Quantity { get; private set; }
    public bool IsEmpty => Item == null || Quantity <= 0;
    public bool CanAddMore => Item != null && Quantity < MaxStackSize;
    public int MaxStackSize { get; }

    public MockInventorySlot(int maxStackSize = 99)
    {
        MaxStackSize = maxStackSize;
    }

    public int AddItems(IItem item, int amount = 1)
    {
        if (item == null || amount <= 0) return amount;

        if (IsEmpty)
        {
            Item = item;
            Quantity = Mathf.Min(amount, MaxStackSize);
            return Mathf.Max(0, amount - MaxStackSize);
        }

        if (Item != null && Item.CanStackWith(item))
        {
            int spaceLeft = MaxStackSize - Quantity;
            int canAdd = Mathf.Min(amount, spaceLeft);
            Quantity += canAdd;
            return amount - canAdd;
        }

        return amount;
    }

    public int RemoveItems(int amount = 1)
    {
        if (IsEmpty || amount <= 0) return 0;

        int removed = Mathf.Min(amount, Quantity);
        Quantity -= removed;

        if (Quantity <= 0)
        {
            Clear();
        }

        return removed;
    }

    public void Clear()
    {
        Item = null;
        Quantity = 0;
    }

    public int GetFreeSpace()
    {
        return IsEmpty ? MaxStackSize : MaxStackSize - Quantity;
    }

    public bool CanAddAmount(int amount)
    {
        return amount > 0 && GetFreeSpace() >= amount;
    }

    public bool CanStackWith(IInventorySlot otherSlot)
    {
        return otherSlot != null && !otherSlot.IsEmpty && 
               Item != null && otherSlot.Item != null && Item.CanStackWith(otherSlot.Item);
    }

    public IInventorySlot Clone()
    {
        var clone = new MockInventorySlot(MaxStackSize);
        if (!IsEmpty)
        {
            clone.AddItems(Item, Quantity);
        }
        return clone;
    }
}

/// <summary>
/// Мок для IInventoryManager для тестирования
/// </summary>
public class MockInventoryManager : IInventoryManager
{
    private readonly List<IInventorySlot> _slots = new List<IInventorySlot>();
    private readonly Dictionary<string, int> _itemCounts = new Dictionary<string, int>();

    public int SlotCount => _slots.Count;

    public event System.Action OnInventoryChanged;
    public event System.Action<IItem, int> OnItemAdded;
    public event System.Action<IItem, int> OnItemRemoved;

    public MockInventoryManager(int slotCount = 8)
    {
        for (int i = 0; i < slotCount; i++)
        {
            _slots.Add(new MockInventorySlot());
        }
    }

    public IInventorySlot GetSlot(int index)
    {
        return index >= 0 && index < _slots.Count ? _slots[index] : null;
    }

    public IReadOnlyList<IInventorySlot> GetAllSlots()
    {
        return _slots.AsReadOnly();
    }

    public bool AddItem(IItem item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        int remainingAmount = amount;
        int emptySlotIndex = FindEmptySlot();

        // Сначала пытаемся добавить в существующие стеки
        for (int i = 0; i < _slots.Count && remainingAmount > 0; i++)
        {
            var slot = _slots[i];
            if (!slot.IsEmpty && slot.Item.CanStackWith(item))
            {
                remainingAmount = slot.AddItems(item, remainingAmount);
            }
        }

        // Затем добавляем в пустые слоты
        while (remainingAmount > 0 && emptySlotIndex != -1)
        {
            remainingAmount = _slots[emptySlotIndex].AddItems(item, remainingAmount);
            emptySlotIndex = FindEmptySlot();
        }

        if (amount - remainingAmount > 0)
        {
            UpdateItemCount(item, amount - remainingAmount);
            OnItemAdded?.Invoke(item, amount - remainingAmount);
            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    public bool RemoveItem(IItem item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        int totalRemoved = 0;
        int remainingToRemove = amount;

        for (int i = 0; i < _slots.Count && remainingToRemove > 0; i++)
        {
            var slot = _slots[i];
            if (!slot.IsEmpty && slot.Item.CanStackWith(item))
            {
                int removed = slot.RemoveItems(remainingToRemove);
                totalRemoved += removed;
                remainingToRemove -= removed;
            }
        }

        if (totalRemoved > 0)
        {
            UpdateItemCount(item, -totalRemoved);
            OnItemRemoved?.Invoke(item, totalRemoved);
            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    public bool RemoveItemFromSlot(int slotIndex, int amount = 1)
    {
        var slot = GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty) return false;

        int removed = slot.RemoveItems(amount);
        if (removed > 0)
        {
            UpdateItemCount(slot.Item, -removed);
            OnItemRemoved?.Invoke(slot.Item, removed);
            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    public bool HasItem(IItem item, int amount = 1)
    {
        if (item == null) return false;
        return _itemCounts.TryGetValue(item.ItemId, out int count) && count >= amount;
    }

    public int GetItemCount(IItem item)
    {
        if (item == null) return 0;
        return _itemCounts.TryGetValue(item.ItemId, out int count) ? count : 0;
    }

    public bool HasFreeSpace()
    {
        return FindEmptySlot() != -1;
    }

    public int GetFreeSlotCount()
    {
        int count = 0;
        foreach (var slot in _slots)
        {
            if (slot.IsEmpty) count++;
        }
        return count;
    }

    public int GetTotalItemCount()
    {
        int total = 0;
        foreach (var count in _itemCounts.Values)
        {
            total += count;
        }
        return total;
    }

    public void ClearInventory()
    {
        foreach (var slot in _slots)
        {
            slot.Clear();
        }
        _itemCounts.Clear();
        OnInventoryChanged?.Invoke();
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].IsEmpty) return i;
        }
        return -1;
    }

    public int FindSlotWithItem(IItem item)
    {
        if (item == null) return -1;

        for (int i = 0; i < _slots.Count; i++)
        {
            if (!_slots[i].IsEmpty && _slots[i].Item.CanStackWith(item))
            {
                return i;
            }
        }
        return -1;
    }

    public void UpdateUI()
    {
        // В моке ничего не делаем
    }

    private void UpdateItemCount(IItem item, int delta)
    {
        if (item == null) return;

        if (_itemCounts.ContainsKey(item.ItemId))
        {
            _itemCounts[item.ItemId] += delta;
            if (_itemCounts[item.ItemId] <= 0)
            {
                _itemCounts.Remove(item.ItemId);
            }
        }
        else if (delta > 0)
        {
            _itemCounts[item.ItemId] = delta;
        }
    }
} 