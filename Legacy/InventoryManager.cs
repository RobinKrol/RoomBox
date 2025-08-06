using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Управляет инвентарём игрока, отображением и добавлением/удалением предметов.
/// </summary>
public class InventoryManager : MonoBehaviour, IInventoryManager
{
    public static InventoryManager Instance { get; private set; }
    
    // Ссылка на централизованную систему событий
    [SerializeField] private InventoryEventSystem eventSystem;
    
    // События интерфейса IInventoryManager (для обратной совместимости)
    public event System.Action OnInventoryChanged;
    public event System.Action<IItem, int> OnItemAdded;
    public event System.Action<IItem, int> OnItemRemoved;
    
    [Header("Основные настройки инвентаря")]
    [SerializeField] private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    [SerializeField] private List<InventorySlotUI> slotUIs; // UI компоненты слотов
    [SerializeField] private GameObject inventoryPanel; 
    [SerializeField] private Button redactorButton;
    [SerializeField] private TextMeshProUGUI inventoryCounterText; // Текст для отображения количества предметов
    private bool isInventoryOpen = false;

    [Header("Общие настройки перетаскивания")]
    [SerializeField] private string outlineLayerName = "OutlinePreview";
    
    [Header("Валидация размещения")]
    [SerializeField] private bool enableValidation = true;
    [SerializeField] private LayerMask collisionCheckMask = -1;
    [SerializeField] private LayerMask surfaceCheckMask = -1;
    [SerializeField] private float collisionCheckRadius = 0.3f;
    [SerializeField] private float objectSizeMultiplier = 0.3f;
    [SerializeField] private bool allowTouchingWalls = true;
    [SerializeField] private bool checkFloorBounds = true;
    [SerializeField] private float floorBoundsMargin = 0.01f;
    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;
    
    [Header("Предотвращение наложения предметов")]
    [SerializeField] private bool preventObjectOverlap = true;
    [SerializeField] private float overlapCheckMargin = 0.02f;
    [SerializeField] private bool allowStackingOnSurfaces = true;
    [SerializeField] private Color overlapWarningColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private float overlapTolerance = 0.05f;
    [SerializeField] private bool checkHeightDifference = true;
    
    [Header("Система слоев размещения")]
    [SerializeField] private bool enableLayerSystem = true;
    [SerializeField] private PlacementLayer defaultPlacementLayer = PlacementLayer.Floor;
    [SerializeField] private string[] ignoredTags = { "RoomBoxFloor" };
    [SerializeField] private LayerMask ignoredLayers = 0;
    
    [Header("Отладка")]
    [SerializeField] private bool showLayerDebugInfo = true;
    [SerializeField] private bool disableFloorBoundsCheck = false;
    [SerializeField] private bool disableCollisionCheck = false;
    [SerializeField] private bool showDetailedDebug = true;
    [SerializeField] private bool detailedDebugCollisions = false;
    [SerializeField] private bool debugSurfaceSystem = true;
    [SerializeField] private bool debugCollisions = true;
    [SerializeField] private bool showFloorBounds = true;
    [SerializeField] private bool showInvalidArea = true;
    [SerializeField] private float invalidAreaMultiplier = 1.1f;
    
    [Header("Эффекты")]
    [SerializeField] private bool enableVisualEffects = true;
    [SerializeField] private bool enableSoundEffects = true;
    
    [Header("Звуковые эффекты")]
    [SerializeField] private AudioClip dragStartSound;
    [SerializeField] private AudioClip placementSound;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip invalidPlacementSound;
    [SerializeField] private AudioClip rotationSound;
    
    [Header("Настройки позиционирования")]
    [SerializeField] private float floorHeight = 0f;
    [SerializeField] private bool useRaycastPositioning = false;
    [SerializeField] private float cameraDistanceOffset = 0f;
    [SerializeField] private bool debugPositioning = false;

    // Публичные свойства для доступа к настройкам
    public string OutlineLayerName => outlineLayerName;
    public bool EnableValidation => enableValidation;
    public LayerMask CollisionCheckMask => collisionCheckMask;
    public LayerMask SurfaceCheckMask => surfaceCheckMask;
    public float CollisionCheckRadius => collisionCheckRadius;
    public float ObjectSizeMultiplier => objectSizeMultiplier;
    public bool AllowTouchingWalls => allowTouchingWalls;
    public bool CheckFloorBounds => checkFloorBounds;
    public float FloorBoundsMargin => floorBoundsMargin;
    public Color ValidPlacementColor => validPlacementColor;
    public Color InvalidPlacementColor => invalidPlacementColor;
    public bool PreventObjectOverlap => preventObjectOverlap;
    public float OverlapCheckMargin => overlapCheckMargin;
    public bool AllowStackingOnSurfaces => allowStackingOnSurfaces;
    public Color OverlapWarningColor => overlapWarningColor;
    public float OverlapTolerance => overlapTolerance;
    public bool CheckHeightDifference => checkHeightDifference;
    public bool EnableLayerSystem => enableLayerSystem;
    public PlacementLayer DefaultPlacementLayer => defaultPlacementLayer;
    public string[] IgnoredTags => ignoredTags;
    public LayerMask IgnoredLayers => ignoredLayers;
    public bool ShowLayerDebugInfo => showLayerDebugInfo;
    public bool DisableFloorBoundsCheck => disableFloorBoundsCheck;
    public bool DisableCollisionCheck => disableCollisionCheck;
    public bool ShowDetailedDebug => showDetailedDebug;
    public bool DetailedDebugCollisions => detailedDebugCollisions;
    public bool DebugSurfaceSystem => debugSurfaceSystem;
    public bool DebugCollisions => debugCollisions;
    public bool ShowFloorBounds => showFloorBounds;
    public bool ShowInvalidArea => showInvalidArea;
    public float InvalidAreaMultiplier => invalidAreaMultiplier;
    public bool EnableVisualEffects => enableVisualEffects;
    public bool EnableSoundEffects => enableSoundEffects;
    public AudioClip DragStartSound => dragStartSound;
    public AudioClip PlacementSound => placementSound;
    public AudioClip CancelSound => cancelSound;
    public AudioClip InvalidPlacementSound => invalidPlacementSound;
    public AudioClip RotationSound => rotationSound;
    public float FloorHeight => floorHeight;
    public bool UseRaycastPositioning => useRaycastPositioning;
    public float CameraDistanceOffset => cameraDistanceOffset;
    public bool DebugPositioning => debugPositioning;

    // Реализация интерфейса IInventoryManager
    public int SlotCount => inventorySlots.Count;
    
    public IInventorySlot GetSlot(int index)
    {
        return index >= 0 && index < inventorySlots.Count ? inventorySlots[index] : null;
    }
    
    public IReadOnlyList<IInventorySlot> GetAllSlots()
    {
        return inventorySlots.AsReadOnly();
    }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Инициализация системы событий
        if (eventSystem == null)
        {
            eventSystem = InventoryEventSystem.GetInstance();
            Debug.Log("[InventoryManager] Подключена централизованная система событий");
        }
    }

    void Start()
    {
        redactorButton.onClick.AddListener(ToggleInventory);
        inventoryPanel.SetActive(isInventoryOpen);
        
        // Автоматически создаем UI слоты, если они не настроены
        if (slotUIs == null || slotUIs.Count == 0)
        {
            CreateSlotUIs();
        }
        
        // Обновляем UI для создания слотов данных
        UpdateInventoryUI();
    }
    
    /// <summary>
    /// Автоматически создает UI слоты из существующих Image слотов
    /// </summary>
    private void CreateSlotUIs()
    {
        slotUIs = new List<InventorySlotUI>();
        
        // Сначала проверяем, есть ли уже настроенные слоты
        if (slotUIs != null && slotUIs.Count > 0)
        {
            Debug.Log($"Найдено {slotUIs.Count} уже настроенных UI слотов");
            return;
        }
        
        // Ищем все Image компоненты в дочерних объектах (старые слоты)
        Image[] oldSlots = GetComponentsInChildren<Image>();
        Debug.Log($"Найдено {oldSlots.Length} Image компонентов в дочерних объектах");
        
        foreach (Image oldSlot in oldSlots)
        {
            // Проверяем, что это не UI элемент количества
            if (oldSlot.name.Contains("Quantity") || oldSlot.name.Contains("Background"))
            {
                Debug.Log($"Пропускаем {oldSlot.name} - это элемент количества");
                continue;
            }
            
            // Проверяем, что это не фон или другой UI элемент
            if (oldSlot.name.Contains("Background") || oldSlot.name.Contains("Panel") || oldSlot.name.Contains("Button"))
            {
                Debug.Log($"Пропускаем {oldSlot.name} - это UI элемент");
                continue;
            }
            
            // Добавляем компонент InventorySlotUI
            InventorySlotUI slotUI = oldSlot.gameObject.GetComponent<InventorySlotUI>();
            if (slotUI == null)
            {
                slotUI = oldSlot.gameObject.AddComponent<InventorySlotUI>();
                Debug.Log($"Добавлен InventorySlotUI на {oldSlot.name}");
            }
            
            slotUIs.Add(slotUI);
        }
        
        // Если слоты не найдены, создаем базовые слоты
        if (slotUIs.Count == 0)
        {
            CreateDefaultSlots();
        }
        
        Debug.Log($"Создано {slotUIs.Count} UI слотов инвентаря");
    }
    
    /// <summary>
    /// Создает базовые слоты инвентаря, если они не найдены
    /// </summary>
    private void CreateDefaultSlots()
    {
        Debug.Log("Создаем базовые слоты инвентаря...");
        
        // Создаем контейнер для слотов, если его нет
        Transform slotsContainer = transform.Find("SlotsContainer");
        if (slotsContainer == null)
        {
            GameObject containerGO = new GameObject("SlotsContainer");
            containerGO.transform.SetParent(transform);
            slotsContainer = containerGO.transform;
            
            // Добавляем Grid Layout Group для автоматического расположения
            var gridLayout = containerGO.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(60, 60);
            gridLayout.spacing = new Vector2(5, 5);
            gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 5; // 5 слотов в ряду
        }
        
        // Создаем 10 базовых слотов
        for (int i = 0; i < 10; i++)
        {
            GameObject slotGO = new GameObject($"Slot_{i}");
            slotGO.transform.SetParent(slotsContainer);
            
            // Добавляем Image компонент
            var image = slotGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Темно-серый цвет
            
            // Добавляем InventorySlotUI
            var slotUI = slotGO.AddComponent<InventorySlotUI>();
            slotUIs.Add(slotUI);
            
            Debug.Log($"Создан базовый слот {i}");
        }
    }

    /// <summary>
    /// Открывает или закрывает инвентарь.
    /// </summary>
    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
        
        // Вызываем событие через централизованную систему
        eventSystem?.InvokeInventoryToggled(isInventoryOpen);
    }

    /// <summary>
    /// Обновляет UI инвентаря.
    /// </summary>
    public void UpdateInventoryUI()
    {
        // Проверяем, что у нас есть UI слоты
        if (slotUIs == null || slotUIs.Count == 0)
        {
            Debug.LogWarning("UpdateInventoryUI: slotUIs пуст или null, пытаемся создать слоты");
            CreateSlotUIs();
        }
        
        // Убеждаемся, что у нас есть достаточно слотов данных
        while (inventorySlots.Count < slotUIs.Count)
        {
            inventorySlots.Add(new InventorySlot());
        }
        
        Debug.Log($"UpdateInventoryUI: обновляем {slotUIs.Count} UI слотов, {inventorySlots.Count} слотов данных");
        
        for (int i = 0; i < slotUIs.Count; i++)
        {
            var inventorySlot = inventorySlots[i];
            var slotUI = slotUIs[i];
            
            if (slotUI == null)
            {
                Debug.LogError($"UpdateInventoryUI: slotUI[{i}] is null!");
                continue;
            }
            
            // Обновляем UI слота
            slotUI.UpdateSlotUI(inventorySlot);
            
            // Отладочная информация о слоте
            if (inventorySlot != null && !inventorySlot.IsEmpty)
            {
                Debug.Log($"UpdateInventoryUI: слот {i} содержит {inventorySlot.Quantity}x {inventorySlot.Item?.ItemName}");
            }
        }
        
        // Обновляем счетчик предметов
        UpdateInventoryCounter();
        
        Debug.Log($"UpdateInventoryUI: обновление завершено");
    }
    
    /// <summary>
    /// Обновляет счетчик предметов в инвентаре
    /// </summary>
    private void UpdateInventoryCounter()
    {
        if (inventoryCounterText == null)
            return;
        
        int totalItems = 0;
        int totalSlots = 0;
        
        foreach (var slot in inventorySlots)
        {
            if (!slot.IsEmpty)
            {
                totalItems += slot.Quantity;
                totalSlots++;
            }
        }
        
        // Отображаем количество предметов и занятых слотов
        inventoryCounterText.text = $"{totalItems}";
        
        // Можно также показать занятые слоты: $"{totalItems} ({totalSlots}/{slotUIs.Count})"
    }

    /// <summary>
    /// Добавляет предмет в инвентарь с автоматическим складыванием (для обратной совместимости).
    /// </summary>
    public void AddItem(Item newItem, int amount = 1)
    {
        if (newItem == null || amount <= 0)
            return;
        
        int remainingAmount = amount;
        
        // Сначала пытаемся добавить к существующим стекам
        for (int i = 0; i < inventorySlots.Count && remainingAmount > 0; i++)
        {
            if (inventorySlots[i].CanStackWith(newItem))
            {
                remainingAmount = inventorySlots[i].AddItems(newItem, remainingAmount);
            }
        }
        
        // Если остались предметы, ищем пустые слоты
        while (remainingAmount > 0)
        {
            // Ищем пустой слот
            int emptySlotIndex = FindEmptySlotInternal();
            if (emptySlotIndex == -1)
            {
                Debug.LogWarning($"Инвентарь полон! Не удалось добавить {remainingAmount} предметов {newItem.ItemName}");
                break;
            }
            
            // Добавляем в пустой слот
            remainingAmount = inventorySlots[emptySlotIndex].AddItems(newItem, remainingAmount);
        }
        
        UpdateInventoryUI();
        // Принудительно обновляем отображение количества во всех слотах
        ForceUpdateAllSlotQuantities();
        
        // Вызываем события через централизованную систему
        eventSystem?.InvokeInventoryChanged();
        eventSystem?.InvokeItemAdded(newItem.ToIItem(), amount);
    }
    
    /// <summary>
    /// Добавляет предмет в инвентарь (реализация интерфейса IInventoryManager).
    /// </summary>
    public bool AddItem(IItem item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        
        // Конвертируем IItem в Item если нужно
        Item originalItem = item as Item ?? (item as ItemWrapper)?.GetOriginalItem();
        if (originalItem == null)
        {
            Debug.LogWarning($"AddItem: не удалось конвертировать IItem в Item: {item?.ItemName}");
            return false;
        }
        
        int originalAmount = amount;
        AddItem(originalItem, amount);
        
        // Вызываем события
        OnItemAdded?.Invoke(item, originalAmount);
        OnInventoryChanged?.Invoke();
        
        return true;
    }
    
    /// <summary>
    /// Находит первый пустой слот в инвентаре (внутренний метод).
    /// </summary>
    private int FindEmptySlotInternal()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                return i;
            }
        }
        return -1; // Нет пустых слотов
    }

    /// <summary>
    /// Удаляет предмет из инвентаря (для обратной совместимости).
    /// </summary>
    public void RemoveItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            Debug.LogWarning($"RemoveItem: неверные параметры - item={item}, amount={amount}");
            return;
        }
        
        Debug.Log($"RemoveItem: удаляем {amount} предметов {item.ItemName}");
        
        int remainingToRemove = amount;
        int totalRemoved = 0;
        
        // Ищем слоты с этим предметом и удаляем из них
        for (int i = inventorySlots.Count - 1; i >= 0 && remainingToRemove > 0; i--)
        {
            if (inventorySlots[i].Item != null && inventorySlots[i].Item.ItemId == item.ItemId)
            {
                int removed = inventorySlots[i].RemoveItems(remainingToRemove);
                remainingToRemove -= removed;
                totalRemoved += removed;
                
                Debug.Log($"RemoveItem: удалено {removed} из слота {i}, осталось удалить: {remainingToRemove}");
            }
        }
        
        if (remainingToRemove > 0)
        {
            Debug.LogWarning($"Не удалось удалить {remainingToRemove} предметов {item.ItemName} - недостаточно в инвентаре. Удалено: {totalRemoved}");
        }
        else
        {
            Debug.Log($"RemoveItem: успешно удалено {totalRemoved} предметов {item.ItemName}");
        }
        
        // Обновляем UI
        UpdateInventoryUI();
        // Принудительно обновляем отображение количества во всех слотах
        ForceUpdateAllSlotQuantities();
        
        // Вызываем события через централизованную систему
        eventSystem?.InvokeInventoryChanged();
        eventSystem?.InvokeItemRemoved(item.ToIItem(), totalRemoved);
    }
    
    /// <summary>
    /// Удаляет предмет из инвентаря (реализация интерфейса IInventoryManager).
    /// </summary>
    public bool RemoveItem(IItem item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        
        // Конвертируем IItem в Item если нужно
        Item originalItem = item as Item ?? (item as ItemWrapper)?.GetOriginalItem();
        if (originalItem == null)
        {
            Debug.LogWarning($"RemoveItem: не удалось конвертировать IItem в Item: {item?.ItemName}");
            return false;
        }
        
        int originalAmount = amount;
        RemoveItem(originalItem, amount);
        
        // Вызываем события
        OnItemRemoved?.Invoke(item, originalAmount);
        OnInventoryChanged?.Invoke();
        
        return true;
    }
    
    /// <summary>
    /// Удаляет предмет из конкретного слота (для обратной совместимости).
    /// </summary>
    public void RemoveItemFromSlotInternal(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
            return;
        
        inventorySlots[slotIndex].RemoveItems(amount);
        UpdateInventoryUI();
        // Принудительно обновляем отображение количества во всех слотах
        ForceUpdateAllSlotQuantities();
    }
    
    /// <summary>
    /// Проверяет, есть ли предмет в инвентаре.
    /// </summary>
    public bool HasItem(Item item, int amount = 1)
    {
        if (item == null)
            return false;
        
        int totalQuantity = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.Item != null && slot.Item.ItemId == item.ItemId)
            {
                totalQuantity += slot.Quantity;
                if (totalQuantity >= amount)
                    return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Получает общее количество предмета в инвентаре.
    /// </summary>
    public int GetItemCount(Item item)
    {
        if (item == null)
            return 0;
        
        int totalQuantity = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.Item != null && slot.Item.ItemId == item.ItemId)
            {
                totalQuantity += slot.Quantity;
            }
        }
        
        return totalQuantity;
    }
    
    /// <summary>
    /// Проверяет, есть ли свободное место в инвентаре.
    /// </summary>
    public bool HasFreeSpace()
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.IsEmpty)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Получает количество свободных слотов.
    /// </summary>
    public int GetFreeSlotCount()
    {
        int count = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.IsEmpty)
                count++;
        }
        return count;
    }
    
    /// <summary>
    /// Получает общее количество предметов в инвентаре.
    /// </summary>
    public int GetTotalItemCount()
    {
        int totalItems = 0;
        foreach (var slot in inventorySlots)
        {
            if (!slot.IsEmpty)
            {
                totalItems += slot.Quantity;
            }
        }
        return totalItems;
    }
    
    /// <summary>
    /// Очищает весь инвентарь.
    /// </summary>
    public void ClearInventory()
    {
        foreach (var slot in inventorySlots)
        {
            slot.Clear();
        }
        UpdateInventoryUI();
        // Принудительно обновляем отображение количества во всех слотах
        ForceUpdateAllSlotQuantities();
    }
    
    /// <summary>
    /// Принудительно создает слоты инвентаря (для отладки)
    /// </summary>
    [ContextMenu("Создать слоты инвентаря")]
    public void ForceCreateSlots()
    {
        slotUIs = new List<InventorySlotUI>();
        CreateDefaultSlots();
        UpdateInventoryUI();
        Debug.Log($"Принудительно создано {slotUIs.Count} слотов инвентаря");
    }
    
    /// <summary>
    /// Принудительно обновляет отображение количества во всех слотах
    /// </summary>
    [ContextMenu("Обновить отображение количества")]
    public void ForceUpdateAllSlotQuantities()
    {
        foreach (var slotUI in slotUIs)
        {
            if (slotUI != null)
            {
                slotUI.ForceUpdateQuantity();
            }
        }
        Debug.Log("Обновлено отображение количества во всех слотах");
    }
    
    /// <summary>
    /// Проверяет состояние инвентаря (для отладки)
    /// </summary>
    [ContextMenu("Проверить состояние инвентаря")]
    public void CheckInventoryState()
    {
        Debug.Log($"=== Состояние инвентаря ===");
        Debug.Log($"UI слотов: {slotUIs?.Count ?? 0}");
        Debug.Log($"Слотов данных: {inventorySlots.Count}");
        Debug.Log($"Свободных слотов: {GetFreeSlotCount()}");
        Debug.Log($"Есть свободное место: {HasFreeSpace()}");
        
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            if (!slot.IsEmpty)
            {
                Debug.Log($"Слот {i}: {slot.Item.ItemName} x{slot.Quantity}");
            }
        }
    }
    
    // Дополнительные методы интерфейса IInventoryManager
    
    /// <summary>
    /// Удаляет предмет из конкретного слота (реализация интерфейса).
    /// </summary>
    public bool RemoveItemFromSlot(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
            return false;
        
        var slot = inventorySlots[slotIndex];
        if (slot.IsEmpty)
            return false;
        
        int removed = slot.RemoveItems(amount);
        if (removed > 0)
        {
            UpdateInventoryUI();
            ForceUpdateAllSlotQuantities();
            OnItemRemoved?.Invoke(slot.Item, removed);
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Проверяет, есть ли предмет в инвентаре (реализация интерфейса).
    /// </summary>
    public bool HasItem(IItem item, int amount = 1)
    {
        if (item == null) return false;
        
        // Конвертируем IItem в Item если нужно
        Item originalItem = item as Item ?? (item as ItemWrapper)?.GetOriginalItem();
        if (originalItem == null)
        {
            Debug.LogWarning($"HasItem: не удалось конвертировать IItem в Item: {item?.ItemName}");
            return false;
        }
        
        return HasItem(originalItem, amount);
    }
    
    /// <summary>
    /// Получает общее количество предмета в инвентаре (реализация интерфейса).
    /// </summary>
    public int GetItemCount(IItem item)
    {
        if (item == null) return 0;
        
        // Конвертируем IItem в Item если нужно
        Item originalItem = item as Item ?? (item as ItemWrapper)?.GetOriginalItem();
        if (originalItem == null)
        {
            Debug.LogWarning($"GetItemCount: не удалось конвертировать IItem в Item: {item?.ItemName}");
            return 0;
        }
        
        return GetItemCount(originalItem);
    }
    
    /// <summary>
    /// Находит первый пустой слот в инвентаре (реализация интерфейса).
    /// </summary>
    public int FindEmptySlot()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                return i;
            }
        }
        return -1; // Нет пустых слотов
    }
    
    /// <summary>
    /// Находит слот с указанным предметом (реализация интерфейса).
    /// </summary>
    public int FindSlotWithItem(IItem item)
    {
        if (item == null) return -1;
        
        // Конвертируем IItem в Item если нужно
        Item originalItem = item as Item ?? (item as ItemWrapper)?.GetOriginalItem();
        if (originalItem == null)
        {
            Debug.LogWarning($"FindSlotWithItem: не удалось конвертировать IItem в Item: {item?.ItemName}");
            return -1;
        }
        
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (!inventorySlots[i].IsEmpty && inventorySlots[i].Item.CanStackWith(item))
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Обновляет UI инвентаря (реализация интерфейса).
    /// </summary>
    public void UpdateUI()
    {
        UpdateInventoryUI();
    }
}
