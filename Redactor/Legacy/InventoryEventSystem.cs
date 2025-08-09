using UnityEngine;

/// <summary>
/// Централизованная система событий для инвентаря
/// </summary>
// LEGACY: оставлено для обратной совместимости. Новая система в InventorySystem.EventSystem.InventoryEventSystem
public class InventoryEventSystem : MonoBehaviour, IInventoryEventSystem
{
    public static InventoryEventSystem Instance { get; private set; }
    
    // События интерфейса IInventoryEventSystem
    public event System.Action OnInventoryChanged;
    public event System.Action<IItem, int> OnItemAdded;
    public event System.Action<IItem, int> OnItemRemoved;
    public event System.Action<int, IInventorySlot> OnSlotChanged;
    public event System.Action<IItem, int> OnDragStarted;
    public event System.Action<IItem, int, bool> OnDragEnded;
    public event System.Action<IItem, Vector3, Quaternion> OnItemPlaced;
    public event System.Action<IItem, string> OnPlacementError;
    public event System.Action<bool> OnInventoryToggled;
    
    [Header("Настройки отладки")]
    [SerializeField] private bool enableEventLogging = true;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[InventoryEventSystem] Инициализирована централизованная система событий");
        }
        else
        {
            Debug.LogWarning("[InventoryEventSystem] Дублирующий экземпляр уничтожен");
            Destroy(gameObject);
        }
    }
    
    // Реализация методов интерфейса IInventoryEventSystem
    
    public void InvokeInventoryChanged()
    {
        if (enableEventLogging)
            Debug.Log("[InventoryEventSystem] InvokeInventoryChanged");
        OnInventoryChanged?.Invoke();
    }
    
    public void InvokeItemAdded(IItem item, int amount)
    {
        if (enableEventLogging)
            Debug.Log($"[InventoryEventSystem] InvokeItemAdded: {item?.ItemName} x{amount}");
        OnItemAdded?.Invoke(item, amount);
    }
    
    public void InvokeItemRemoved(IItem item, int amount)
    {
        if (enableEventLogging)
            Debug.Log($"[InventoryEventSystem] InvokeItemRemoved: {item?.ItemName} x{amount}");
        OnItemRemoved?.Invoke(item, amount);
    }
    
    public void InvokeSlotChanged(int slotIndex, IInventorySlot slot)
    {
        if (enableEventLogging)
            Debug.Log($"[InventoryEventSystem] InvokeSlotChanged: слот {slotIndex}, предмет: {slot?.Item?.ItemName}");
        OnSlotChanged?.Invoke(slotIndex, slot);
    }
    
    public void InvokeDragStarted(IItem item, int slotIndex)
    {
        if (enableEventLogging)
            Debug.Log($"[InventoryEventSystem] InvokeDragStarted: {item?.ItemName} из слота {slotIndex}");
        OnDragStarted?.Invoke(item, slotIndex);
    }
    
    public void InvokeDragEnded(IItem item, int slotIndex, bool wasPlaced)
    {
        if (enableEventLogging)
            Debug.Log($"[InventoryEventSystem] InvokeDragEnded: {item?.ItemName} из слота {slotIndex}, размещен: {wasPlaced}");
        OnDragEnded?.Invoke(item, slotIndex, wasPlaced);
    }
    
    public void InvokeItemPlaced(IItem item, Vector3 position, Quaternion rotation)
    {
        if (enableEventLogging)
            Debug.Log($"[InventoryEventSystem] InvokeItemPlaced: {item?.ItemName} в позиции {position}");
        OnItemPlaced?.Invoke(item, position, rotation);
    }
    
    public void InvokePlacementError(IItem item, string errorMessage)
    {
        if (enableEventLogging)
            Debug.LogWarning($"[InventoryEventSystem] InvokePlacementError: {item?.ItemName} - {errorMessage}");
        OnPlacementError?.Invoke(item, errorMessage);
    }
    
    public void InvokeInventoryToggled(bool isOpen)
    {
        if (enableEventLogging)
            Debug.Log($"[InventoryEventSystem] InvokeInventoryToggled: {(isOpen ? "открыт" : "закрыт")}");
        OnInventoryToggled?.Invoke(isOpen);
    }
    
    /// <summary>
    /// Получить экземпляр системы событий (создает если не существует)
    /// </summary>
    public static InventoryEventSystem GetInstance()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("InventoryEventSystem");
            Instance = go.AddComponent<InventoryEventSystem>();
            Debug.Log("[InventoryEventSystem] Создан новый экземпляр системы событий");
        }
        return Instance;
    }
    
    /// <summary>
    /// Очистить все подписки на события (для отладки)
    /// </summary>
    [ContextMenu("Очистить все события")]
    public void ClearAllEvents()
    {
        OnInventoryChanged = null;
        OnItemAdded = null;
        OnItemRemoved = null;
        OnSlotChanged = null;
        OnDragStarted = null;
        OnDragEnded = null;
        OnItemPlaced = null;
        OnPlacementError = null;
        OnInventoryToggled = null;
        Debug.Log("[InventoryEventSystem] Все события очищены");
    }
    
    /// <summary>
    /// Получить количество подписчиков на каждое событие (для отладки)
    /// </summary>
    [ContextMenu("Показать количество подписчиков")]
    public void LogSubscriberCounts()
    {
        Debug.Log($"[InventoryEventSystem] Количество подписчиков:");
        Debug.Log($"  OnInventoryChanged: {OnInventoryChanged?.GetInvocationList().Length ?? 0}");
        Debug.Log($"  OnItemAdded: {OnItemAdded?.GetInvocationList().Length ?? 0}");
        Debug.Log($"  OnItemRemoved: {OnItemRemoved?.GetInvocationList().Length ?? 0}");
        Debug.Log($"  OnSlotChanged: {OnSlotChanged?.GetInvocationList().Length ?? 0}");
        Debug.Log($"  OnDragStarted: {OnDragStarted?.GetInvocationList().Length ?? 0}");
        Debug.Log($"  OnDragEnded: {OnDragEnded?.GetInvocationList().Length ?? 0}");
        Debug.Log($"  OnItemPlaced: {OnItemPlaced?.GetInvocationList().Length ?? 0}");
        Debug.Log($"  OnPlacementError: {OnPlacementError?.GetInvocationList().Length ?? 0}");
        Debug.Log($"  OnInventoryToggled: {OnInventoryToggled?.GetInvocationList().Length ?? 0}");
    }
} 