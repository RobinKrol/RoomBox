using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Factories;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Тестовый скрипт для проверки работы OptimizedInventoryManager
    /// </summary>
    public class InventoryTest : MonoBehaviour
    {
        [Header("Тестирование")]
        [SerializeField] private OptimizedInventoryManager inventoryManager;
        
        [ContextMenu("Тест: Добавить предмет")]
        public void TestAddItem()
        {
            if (inventoryManager == null)
            {
                inventoryManager = FindFirstObjectByType<OptimizedInventoryManager>();
                if (inventoryManager == null)
                {
                    Debug.LogError("OptimizedInventoryManager не найден на сцене!");
                    return;
                }
            }
            
            var testItem = InventoryFactory.CreateTestItem();
            bool success = inventoryManager.AddItem(testItem, 1);
            Debug.Log($"Тест добавления предмета: {success}");
        }
        
        [ContextMenu("Тест: Добавить 3 предмета")]
        public void TestAddMultipleItems()
        {
            if (inventoryManager == null)
            {
                inventoryManager = FindFirstObjectByType<OptimizedInventoryManager>();
                if (inventoryManager == null)
                {
                    Debug.LogError("OptimizedInventoryManager не найден на сцене!");
                    return;
                }
            }
            
            var testItem = InventoryFactory.CreateTestItem();
            bool success = inventoryManager.AddItem(testItem, 3);
            Debug.Log($"Тест добавления 3 предметов: {success}");
        }
        
        [ContextMenu("Тест: Проверить состояние инвентаря")]
        public void TestCheckInventoryState()
        {
            if (inventoryManager == null)
            {
                inventoryManager = FindFirstObjectByType<OptimizedInventoryManager>();
                if (inventoryManager == null)
                {
                    Debug.LogError("OptimizedInventoryManager не найден на сцене!");
                    return;
                }
            }
            
            Debug.Log($"=== Состояние инвентаря ===");
            Debug.Log($"Всего слотов: {inventoryManager.SlotCount}");
            Debug.Log($"Свободных слотов: {inventoryManager.GetFreeSlotCount()}");
            Debug.Log($"Всего предметов: {inventoryManager.GetTotalItemCount()}");
            
            for (int i = 0; i < inventoryManager.SlotCount; i++)
            {
                var slot = inventoryManager.GetSlot(i);
                Debug.Log($"Слот {i}: IsEmpty={slot.IsEmpty}, Quantity={slot.Quantity}, Item={slot.Item?.ItemName ?? "null"}");
            }
        }
        
        [ContextMenu("Тест: Очистить инвентарь")]
        public void TestClearInventory()
        {
            if (inventoryManager == null)
            {
                inventoryManager = FindFirstObjectByType<OptimizedInventoryManager>();
                if (inventoryManager == null)
                {
                    Debug.LogError("OptimizedInventoryManager не найден на сцене!");
                    return;
                }
            }
            
            inventoryManager.ClearInventory();
            Debug.Log("Инвентарь очищен");
        }
    }
} 