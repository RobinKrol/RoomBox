using UnityEngine;
using System.Collections.Generic;
using InventorySystem.OptimizedComponents;

namespace InventorySystem.Factories
{
    /// <summary>
    /// Фабрика для создания объектов инвентаря
    /// </summary>
    public static class InventoryFactory
    {
        /// <summary>
        /// Создает новый слот инвентаря
        /// </summary>
        public static InventorySlot CreateInventorySlot(Item item = null, int quantity = 0)
        {
            return new InventorySlot(item, quantity);
        }
        
        /// <summary>
        /// Создает адаптер для слота инвентаря
        /// </summary>
        public static InventorySlotAdapter CreateInventorySlotAdapter(InventorySlot originalSlot)
        {
            return new InventorySlotAdapter(originalSlot);
        }
        
        /// <summary>
        /// Создает адаптер для предмета
        /// </summary>
        public static ItemWrapper CreateItemAdapter(Item originalItem)
        {
            return originalItem != null ? new ItemWrapper(originalItem) : null;
        }
        
        /// <summary>
        /// Создает список слотов инвентаря
        /// </summary>
        public static List<InventorySlot> CreateInventorySlots(int count)
        {
            var slots = new List<InventorySlot>();
            for (int i = 0; i < count; i++)
            {
                slots.Add(CreateInventorySlot());
            }
            return slots;
        }
        
        /// <summary>
        /// Создает тестовый предмет
        /// </summary>
        public static Item CreateTestItem(string itemName = "TestItem", int maxStackSize = 10)
        {
            var item = ScriptableObject.CreateInstance<Item>();
            item.itemName = itemName;
            // Устанавливаем MaxStackSize через reflection, так как это read-only свойство
            var maxStackSizeField = typeof(Item).GetField("maxStackSize", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maxStackSizeField?.SetValue(item, maxStackSize);
            return item;
        }
        
        /// <summary>
        /// Создает предмет с указанными параметрами
        /// </summary>
        public static Item CreateItem(string itemName, int maxStackSize, Sprite icon = null, GameObject prefab = null)
        {
            var item = ScriptableObject.CreateInstance<Item>();
            item.itemName = itemName;
            // Устанавливаем MaxStackSize через reflection, так как это read-only свойство
            var maxStackSizeField = typeof(Item).GetField("maxStackSize", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maxStackSizeField?.SetValue(item, maxStackSize);
            item.icon = icon;
            item.prefab = prefab;
            return item;
        }
    }
    
    /// <summary>
    /// Фабрика для создания UI компонентов
    /// </summary>
    public static class UIFactory
    {
        /// <summary>
        /// Создает UI слот инвентаря
        /// </summary>
        public static InventorySlotUI CreateInventorySlotUI(Transform parent = null)
        {
            var go = new GameObject("InventorySlotUI");
            if (parent != null)
            {
                go.transform.SetParent(parent);
            }
            
            var slotUI = go.AddComponent<InventorySlotUI>();
            return slotUI;
        }
        
        /// <summary>
        /// Создает drag handler для слота
        /// </summary>
        public static InventorySlotDragHandler CreateDragHandler(InventorySlotUI slotUI)
        {
            var dragHandler = slotUI.gameObject.AddComponent<InventorySlotDragHandler>();
            return dragHandler;
        }
    }
    
    /// <summary>
    /// Фабрика для создания систем валидации
    /// </summary>
    public static class ValidationFactory
    {
        /// <summary>
        /// Создает систему валидации размещения
        /// </summary>
        public static OptimizedItemPlacementValidator CreatePlacementValidator(InventoryManager inventoryManager = null)
        {
            var go = new GameObject("OptimizedItemPlacementValidator");
            var validator = go.AddComponent<OptimizedItemPlacementValidator>();
            
            if (inventoryManager != null)
            {
                // Устанавливаем ссылку через reflection для тестирования
                var field = typeof(OptimizedItemPlacementValidator).GetField("inventoryManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(validator, inventoryManager);
            }
            
            return validator;
        }
    }
    
    /// <summary>
    /// Фабрика для создания систем событий
    /// </summary>
    public static class EventSystemFactory
    {
        /// <summary>
        /// Создает систему событий инвентаря
        /// </summary>
        public static InventoryEventSystem CreateInventoryEventSystem()
        {
            var go = new GameObject("InventoryEventSystem");
            return go.AddComponent<InventoryEventSystem>();
        }
    }
} 