using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Тестовый скрипт для проверки системы складывания предметов
/// </summary>
public class StackingTest : MonoBehaviour
{
    [Header("Тестирование")]
    [SerializeField] private InventorySystem.OptimizedComponents.OptimizedInventoryManager inventoryManager;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private Key testKey = Key.T;
    [SerializeField] private Key clearKey = Key.C;
    
    void Update()
    {
        if (Keyboard.current == null) return;
        
        // Тест добавления случайного предмета
        if (Keyboard.current[testKey].wasPressedThisFrame)
        {
            TestAddRandomItem();
        }
        
        // Очистка инвентаря
        if (Keyboard.current[clearKey].wasPressedThisFrame)
        {
            TestClearInventory();
        }
    }
    
    /// <summary>
    /// Тестирует добавление случайного предмета
    /// </summary>
    private void TestAddRandomItem()
    {
        if (inventoryManager == null || itemDatabase == null || itemDatabase.items.Count == 0)
        {
            Debug.LogWarning("Не настроены компоненты для тестирования!");
            return;
        }
        
        // Выбираем случайный предмет
        Item randomItem = itemDatabase.items[Random.Range(0, itemDatabase.items.Count)];
        int amount = Random.Range(1, 5); // Случайное количество 1-4
        
        Debug.Log($"Тест: Добавляем {randomItem.itemName} x{amount}");
        inventoryManager.AddItem(new ItemWrapper(randomItem), amount);
    }
    
    /// <summary>
    /// Тестирует очистку инвентаря
    /// </summary>
    private void TestClearInventory()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager не настроен!");
            return;
        }
        
        Debug.Log("Тест: Очищаем инвентарь");
        inventoryManager.ClearInventory();
    }
    
    /// <summary>
    /// Тестирует добавление конкретного предмета
    /// </summary>
    public void TestAddSpecificItem(Item item, int amount = 1)
    {
        if (inventoryManager == null || item == null)
        {
            Debug.LogWarning("Не настроены компоненты для тестирования!");
            return;
        }
        
        Debug.Log($"Тест: Добавляем {item.itemName} x{amount}");
        inventoryManager.AddItem(new ItemWrapper(item), amount);
    }
} 