using UnityEngine;
using InventorySystem.BaseComponents;

namespace InventorySystem.OptimizedComponents
{
    /// <summary>
    /// Оптимизированный слот инвентаря для новой архитектуры
    /// </summary>
    public class OptimizedInventorySlot : BaseInventoryComponent, IInventorySlot
    {
        [SerializeField] private int maxStackSize = 99;
        [SerializeField] private int quantity = 0;
        [SerializeField] private ItemWrapper itemWrapper;

        public IItem Item => itemWrapper;
        public int Quantity => quantity;
        public bool IsEmpty => itemWrapper == null || quantity <= 0;
        public bool CanAddMore => !IsEmpty && quantity < MaxStackSize;
        public int MaxStackSize => itemWrapper != null ? itemWrapper.MaxStackSize : maxStackSize;

        // Не используем конструкторы у MonoBehaviour. Инициализация происходит в Awake/Start при необходимости

        public int AddItems(IItem item, int amount = 1)
        {
            UnityEngine.Debug.Log($"[OptimizedInventorySlot] AddItems: item={item?.ItemName}, amount={amount}, IsEmpty={IsEmpty}, itemWrapper={(itemWrapper == null ? "null" : itemWrapper.ToString())}, quantity={quantity}");
            if (item == null || amount <= 0)
                return amount;

            if (IsEmpty)
            {
                UnityEngine.Debug.Log("[OptimizedInventorySlot] AddItems: слот пустой, добавляем новый itemWrapper и quantity");
                itemWrapper = item as ItemWrapper ?? new ItemWrapper(item as Item);
                quantity = Mathf.Min(amount, MaxStackSize);
                UnityEngine.Debug.Log($"[OptimizedInventorySlot] AddItems: после добавления в пустой слот: itemWrapper={(itemWrapper == null ? "null" : itemWrapper.ToString())}, quantity={quantity}");
                return amount - quantity;
            }

            if (!Item.CanStackWith(item))
            {
                UnityEngine.Debug.Log("[OptimizedInventorySlot] AddItems: предметы не стакаются");
                return amount;
            }

            int free = GetFreeSpace();
            int toAdd = Mathf.Min(amount, free);
            quantity += toAdd;
            UnityEngine.Debug.Log($"[OptimizedInventorySlot] AddItems: после добавления в существующий слот: quantity={quantity}");
            return amount - toAdd;
        }

        public int RemoveItems(int amount = 1)
        {
            if (IsEmpty || amount <= 0)
                return 0;
            int removed = Mathf.Min(amount, quantity);
            quantity -= removed;
            if (quantity <= 0)
                Clear();
            return removed;
        }

        public void Clear()
        {
            UnityEngine.Debug.Log($"[OptimizedInventorySlot] Clear() called: itemWrapper={(itemWrapper == null ? "null" : itemWrapper.ToString())}, quantity={quantity} -> itemWrapper=null, quantity=0");
            itemWrapper = null;
            quantity = 0;
        }

        public int GetFreeSpace()
        {
            return MaxStackSize - quantity;
        }

        public bool CanAddAmount(int amount)
        {
            if (IsEmpty) return amount <= MaxStackSize;
            return (quantity + amount) <= MaxStackSize;
        }

        public bool CanStackWith(IInventorySlot otherSlot)
        {
            if (otherSlot == null || otherSlot.IsEmpty || this.IsEmpty)
                return false;
            return Item.CanStackWith(otherSlot.Item);
        }

        public IInventorySlot Clone()
        {
            var clone = new GameObject("OptimizedInventorySlot_Clone").AddComponent<OptimizedInventorySlot>();
            if (!IsEmpty)
            {
                var originalItem = (itemWrapper as ItemWrapper)?.GetOriginalItem();
                if (originalItem != null)
                    clone.itemWrapper = new ItemWrapper(originalItem);
                else
                    clone.itemWrapper = null;
                clone.quantity = quantity;
            }
            clone.maxStackSize = maxStackSize;
            return clone;
        }
    }
}