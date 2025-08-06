using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет счетчиком размещенных предметов (только для отладки)
/// </summary>
public class PlacedItemsCounter : MonoBehaviour
{
    [Header("Отладка")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Dictionary<string, int> placedItemsCount = new Dictionary<string, int>();
    private int totalPlacedItems = 0;
    
    public static PlacedItemsCounter Instance { get; private set; }
    
    /// <summary>
    /// Безопасно получает экземпляр PlacedItemsCounter, создавая его при необходимости
    /// </summary>
    public static PlacedItemsCounter GetInstance()
    {
        if (Instance == null)
        {
            // Ищем существующий экземпляр в сцене
            Instance = Object.FindFirstObjectByType<PlacedItemsCounter>();
            
            if (Instance == null)
            {
                // Создаем новый GameObject с PlacedItemsCounter
                GameObject go = new GameObject("PlacedItemsCounter");
                Instance = go.AddComponent<PlacedItemsCounter>();
                Debug.Log("PlacedItemsCounter: Создан новый экземпляр");
            }
        }
        
        return Instance;
    }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            
            // Делаем GameObject корневым перед DontDestroyOnLoad
            if (transform.parent != null)
            {
                Debug.Log($"PlacedItemsCounter: Делаем GameObject корневым (был дочерним от {transform.parent.name})");
                transform.SetParent(null);
            }
            
            DontDestroyOnLoad(gameObject);
            Debug.Log($"PlacedItemsCounter: Singleton создан и установлен DontDestroyOnLoad для {gameObject.name}");
        }
        else
        {
            Debug.Log($"PlacedItemsCounter: Дубликат обнаружен, уничтожаем {gameObject.name}");
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Добавляет размещенный предмет в счетчик
    /// </summary>
    public void AddPlacedItem(Item item)
    {
        if (item == null) return;
        
        string itemName = item.itemName;
        
        // Увеличиваем счетчик для конкретного предмета
        if (placedItemsCount.ContainsKey(itemName))
        {
            placedItemsCount[itemName]++;
        }
        else
        {
            placedItemsCount[itemName] = 1;
        }
        
        // Увеличиваем общий счетчик
        totalPlacedItems++;
        
        if (showDebugLogs)
        {
            Debug.Log($"Размещен предмет: {itemName}. Всего размещено: {totalPlacedItems}");
        }
    }
    
    /// <summary>
    /// Убирает предмет из счетчика (если предмет удаляется)
    /// </summary>
    public void RemovePlacedItem(Item item)
    {
        if (item == null) return;
        
        string itemName = item.itemName;
        
        if (placedItemsCount.ContainsKey(itemName) && placedItemsCount[itemName] > 0)
        {
            placedItemsCount[itemName]--;
            totalPlacedItems--;
            
            if (placedItemsCount[itemName] <= 0)
            {
                placedItemsCount.Remove(itemName);
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"Удален предмет: {itemName}. Всего размещено: {totalPlacedItems}");
            }
        }
    }
    
    /// <summary>
    /// Получает количество размещенных предметов определенного типа
    /// </summary>
    public int GetItemCount(string itemName)
    {
        return placedItemsCount.ContainsKey(itemName) ? placedItemsCount[itemName] : 0;
    }
    
    /// <summary>
    /// Получает общее количество размещенных предметов
    /// </summary>
    public int GetTotalCount()
    {
        return totalPlacedItems;
    }
    
    /// <summary>
    /// Получает словарь всех размещенных предметов
    /// </summary>
    public Dictionary<string, int> GetAllPlacedItems()
    {
        return new Dictionary<string, int>(placedItemsCount);
    }
    
    /// <summary>
    /// Сбрасывает счетчик
    /// </summary>
    [ContextMenu("Сбросить счетчик")]
    public void ResetCounter()
    {
        placedItemsCount.Clear();
        totalPlacedItems = 0;
        
        if (showDebugLogs)
        {
            Debug.Log("Счетчик размещенных предметов сброшен");
        }
    }
    
    /// <summary>
    /// Показывает детальную статистику в консоли
    /// </summary>
    [ContextMenu("Показать статистику")]
    public void ShowStatistics()
    {
        Debug.Log("=== Статистика размещенных предметов ===");
        Debug.Log($"Общее количество: {totalPlacedItems}");
        
        foreach (var kvp in placedItemsCount)
        {
            Debug.Log($"- {kvp.Key}: {kvp.Value}");
        }
    }
} 