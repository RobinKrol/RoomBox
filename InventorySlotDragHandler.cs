using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using LineworkLite.FreeOutline;
using System.Collections.Generic;
using InventorySystem.OptimizedComponents;
using InventorySystem.Factories;

public class InventorySlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Основные настройки")]
    public Item item;
    public GameObject dragPrefab;
    
    [Header("Ссылки на системы")]
    [SerializeField] private InventoryManager inventoryManager; // Оставляем для обратной совместимости
    [SerializeField] private PlacedItemsCounter placedItemsCounter;
    [SerializeField] private NavigationUI navigationUI;
    [SerializeField] private InventorySlotUI slotUI; // Ссылка на UI слота для событий
    [SerializeField] private OptimizedItemPlacementValidator placementValidator; // Система валидации
    
    // Интерфейсы для работы с системой
    private IInventoryManager _inventoryManager;
    private IItemPlacementValidator _placementValidator;
    private IInventoryEventSystem _eventSystem;
    
    // Индекс текущего слота
    private int _slotIndex = -1;
    
    [Header("Сетка")]
    public GridPlacement gridPlacement; // Ссылка на GridPlacement
    
    // Приватные переменные
    private GameObject previewInstance;
    private int outlineLayer;
    private Quaternion previewRotation;
    private bool isRotated90 = false;
    private bool wasRightButtonPressed = false;
    private bool wasMiddleButtonPressed = false;
    private bool isDragging = false;
    private bool canPlace = true; // Можно ли разместить объект
    private Renderer[] previewRenderers; // Рендереры превью для изменения цвета
    private Material[] originalMaterials; // Оригинальные материалы
    private bool wasValidLastFrame = true; // Для отслеживания изменения валидности
    private Camera mainCamera; // Кэшированная камера для оптимизации

    void Awake()
    {
        // Получаем настройки из InventoryManager
        if (inventoryManager == null)
        {
            inventoryManager = Object.FindFirstObjectByType<InventoryManager>();
            if (inventoryManager == null)
            {
                Debug.LogError("[InventorySlotDragHandler] InventoryManager не найден в сцене!");
            }
            else
            {
                Debug.Log($"[InventorySlotDragHandler] InventoryManager найден: {inventoryManager.name}");
            }
        }
        
        // Устанавливаем слой обводки из настроек InventoryManager
        if (inventoryManager != null)
        {
            outlineLayer = LayerMask.NameToLayer(inventoryManager.OutlineLayerName);
        }
        else
        {
            outlineLayer = LayerMask.NameToLayer("OutlinePreview");
        }
        
        mainCamera = Camera.main; // Кэшируем камеру
            
        if (placedItemsCounter == null)
        {
            placedItemsCounter = PlacedItemsCounter.GetInstance();
            if (placedItemsCounter == null)
            {
                Debug.LogError("[InventorySlotDragHandler] PlacedItemsCounter не найден!");
            }
        }
        
        if (navigationUI == null)
        {
            navigationUI = NavigationUI.Instance;
            if (navigationUI == null)
            {
                Debug.LogWarning("[InventorySlotDragHandler] NavigationUI не найден");
            }
        }
        
        // Автоматически находим InventorySlotUI на том же объекте
        if (slotUI == null)
        {
            slotUI = GetComponent<InventorySlotUI>();
            if (slotUI == null)
            {
                Debug.LogWarning("[InventorySlotDragHandler] InventorySlotUI не найден на том же объекте");
            }
        }
        
        // Автоматически находим OptimizedItemPlacementValidator
        if (placementValidator == null)
        {
            placementValidator = Object.FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (placementValidator == null)
            {
                Debug.LogWarning("[InventorySlotDragHandler] OptimizedItemPlacementValidator не найден в сцене");
            }
        }
        
        // Инициализируем интерфейсы
        InitializeInterfaces();
        
        // Отладочная информация о настройках
        if (inventoryManager != null && inventoryManager.DebugCollisions)
        {
            Debug.Log($"[InventorySlotDragHandler] Инициализация завершена:");
            Debug.Log($"[InventorySlotDragHandler]   - item: {item}");
            Debug.Log($"[InventorySlotDragHandler]   - dragPrefab: {dragPrefab}");
            Debug.Log($"[InventorySlotDragHandler]   - inventoryManager: {inventoryManager}");
            Debug.Log($"[InventorySlotDragHandler]   - placedItemsCounter: {placedItemsCounter}");
            Debug.Log($"[InventorySlotDragHandler]   - outlineLayer: {outlineLayer} ({inventoryManager.OutlineLayerName})");
        }
    }
    
    /// <summary>
    /// Инициализация интерфейсов для работы с системой
    /// </summary>
    private void InitializeInterfaces()
    {
        // Инициализируем IInventoryManager
        _inventoryManager = inventoryManager as IInventoryManager;
        if (_inventoryManager == null && inventoryManager != null)
        {
            Debug.LogWarning("[InventorySlotDragHandler] InventoryManager не реализует IInventoryManager");
        }
        
        // Инициализируем IItemPlacementValidator
        _placementValidator = placementValidator as IItemPlacementValidator;
        if (_placementValidator == null && placementValidator != null)
        {
            Debug.LogWarning("[InventorySlotDragHandler] OptimizedItemPlacementValidator не реализует IItemPlacementValidator");
        }
        
        // Инициализируем IInventoryEventSystem
        _eventSystem = InventoryEventSystem.GetInstance();
        if (_eventSystem == null)
        {
            Debug.LogWarning("[InventorySlotDragHandler] InventoryEventSystem не найден");
        }
        
        if (inventoryManager?.DebugCollisions == true)
        {
            Debug.Log($"[InventorySlotDragHandler] Интерфейсы инициализированы:");
            Debug.Log($"  - IInventoryManager: {_inventoryManager != null}");
            Debug.Log($"  - IItemPlacementValidator: {_placementValidator != null}");
            Debug.Log($"  - IInventoryEventSystem: {_eventSystem != null}");
                }
    }
    
    /// <summary>
    /// Получить индекс текущего слота
    /// </summary>
    private int GetSlotIndex()
    {
        if (_slotIndex >= 0) return _slotIndex;
        
        // Пытаемся получить индекс из UI слота
        if (slotUI != null)
        {
            _slotIndex = slotUI.SlotIndex;
            return _slotIndex;
        }
        
        // Fallback: ищем индекс по позиции в родительском объекте
        Transform parent = transform.parent;
        if (parent != null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) == transform)
                {
                    _slotIndex = i;
                    return _slotIndex;
                }
            }
        }
        
        return -1;
    }
    
    void Update()
    {
        // Проверяем и восстанавливаем ссылки, если они потерялись
        if (inventoryManager == null)
        {
            inventoryManager = Object.FindFirstObjectByType<InventoryManager>();
            if (inventoryManager == null && InventoryManager.Instance != null)
            {
                inventoryManager = InventoryManager.Instance;
                Debug.Log("[Update] Восстановлена ссылка на InventoryManager через Instance");
            }
        }
        
        if (placedItemsCounter == null)
        {
            placedItemsCounter = PlacedItemsCounter.GetInstance();
        }
        
        if (navigationUI == null)
        {
            navigationUI = NavigationUI.Instance;
        }
        
        // Проверяем, что перетаскивание действительно активно
        if (isDragging && previewInstance == null)
        {
            Debug.LogWarning("[Update] isDragging=true, но previewInstance=null. Сбрасываем состояние.");
            // Сбрасываем только флаг, не вызываем CancelDrag() чтобы избежать рекурсии
            isDragging = false;
            canPlace = true;
            isRotated90 = false;
            wasRightButtonPressed = false;
            wasMiddleButtonPressed = false;
            return;
        }
        
        if (isDragging && previewInstance != null)
        {
            HandleRotation();
        }
    }

    private void HandleRotation()
    {
        bool isRightButtonPressed = Mouse.current.rightButton.isPressed;
        bool isMiddleButtonPressed = Mouse.current.middleButton.isPressed;
        
        // Поворот предмета правой кнопкой мыши
        if (isRightButtonPressed && !wasRightButtonPressed)
        {
            isRotated90 = !isRotated90;
            if (isRotated90)
            {
                previewInstance.transform.rotation = Quaternion.Euler(0, 90, 0);
            }
            else
            {
                previewInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            Debug.Log($"Предмет повернут на {(isRotated90 ? "90" : "0")} градусов");
            
            // Воспроизводим звук поворота
            PlaySound(inventoryManager?.RotationSound);
        }
        
        // Отмена перетаскивания колесиком мыши
        if (isMiddleButtonPressed && !wasMiddleButtonPressed)
        {
            Debug.Log("Перетаскивание отменено колесиком мыши");
            CancelDrag();
            return;
        }
        
        wasRightButtonPressed = isRightButtonPressed;
        wasMiddleButtonPressed = isMiddleButtonPressed;
    }

    private bool CheckPlacementValidity(Vector3 position)
    {
        // Используем интерфейс валидации если доступен
        if (_placementValidator != null && item != null)
        {
            if (inventoryManager?.DebugSurfaceSystem == true)
                Debug.Log($"[CheckPlacementValidity] Используем интерфейс валидации для {item.ItemName}");
            
            // Передаем превью объект в валидатор для исключения из проверок
            if (previewInstance != null)
            {
                _placementValidator.SetPreviewInstance(previewInstance);
            }
            
            return _placementValidator.CanPlaceItem(item?.ToIItem(), position, previewRotation);
        }
        
        // Fallback к прямой ссылке если интерфейс недоступен
        if (placementValidator != null && item != null)
        {
            if (inventoryManager?.DebugSurfaceSystem == true)
                Debug.Log($"[CheckPlacementValidity] Используем прямую ссылку валидации для {item.ItemName}");
            
            // Передаем превью объект в валидатор для исключения из проверок
            if (previewInstance != null)
            {
                placementValidator.SetPreviewInstance(previewInstance);
            }
            
            return placementValidator.CanPlaceItem(item?.ToIItem(), position, previewRotation);
        }
        
        // Fallback к старой логике если валидатор не найден
        if (inventoryManager == null) return true;
        
        if (!inventoryManager.EnableValidation) 
        {
            if (inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[CheckPlacementValidity] Валидация отключена, возвращаю true");
            return true;
        }
        
        if (inventoryManager.DebugSurfaceSystem)
            Debug.Log($"[CheckPlacementValidity] Используем старую логику валидации для позиции: {position}");
        
        bool result;
        if (inventoryManager.AllowTouchingWalls)
        {
            result = CheckPlacementValidityTouching(position);
            if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"   🔍 Проверка коллизий (касание): {(result ? "✅ ОК" : "❌ НЕ ПРОШЛА")}");
            }
        }
        else
        {
            result = CheckPlacementValidityStrict(position);
            if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"   🔍 Проверка коллизий (строгая): {(result ? "✅ ОК" : "❌ НЕ ПРОШЛА")}");
            }
        }
        
        // Дополнительная проверка наложения объектов
        if (result && inventoryManager.PreventObjectOverlap)
        {
            if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"   🔍 Запускаем проверку наложения объектов...");
            }
            
            bool hasOverlap = CheckObjectOverlap(position);
            if (hasOverlap)
            {
                if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
                {
                    Debug.Log($"   🔍 Проверка наложения: ❌ НЕ ПРОШЛА (объекты накладываются)");
                }
                return false;
            }
            else if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"   🔍 Проверка наложения: ✅ ОК");
            }
        }
        else if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
        {
            if (!result)
            {
                Debug.Log($"   🔍 Проверка наложения: ПРОПУЩЕНА (основная проверка не прошла)");
            }
            if (!inventoryManager.PreventObjectOverlap)
            {
                Debug.Log($"   🔍 Проверка наложения: ОТКЛЮЧЕНА (preventObjectOverlap = false)");
            }
        }
        
        if (inventoryManager.DebugSurfaceSystem)
            Debug.Log($"[CheckPlacementValidity] Финальный результат: {result}");
        
        return result;
    }

    private bool CheckPlacementValidityTouching(Vector3 position)
    {
        if (inventoryManager == null) return true;
        
        if (inventoryManager.DebugSurfaceSystem)
            Debug.Log($"[CheckPlacementValidityTouching] Начинаем проверку для позиции: {position}");
        
        // Получаем слой размещения для текущего предмета
        PlacementLayer itemLayer = GetPlacementLayer(previewInstance);
        
        if (inventoryManager.DebugSurfaceSystem)
            Debug.Log($"[CheckPlacementValidityTouching] Слой предмета: {itemLayer}");
        
        // Проверяем коллизии с учетом слоев
        bool hasLayerCollisions = CheckLayerCollisions(position, itemLayer);
        
        if (inventoryManager.DebugSurfaceSystem)
            Debug.Log($"[CheckPlacementValidityTouching] hasLayerCollisions: {hasLayerCollisions}");
        
        // Получаем размер объекта для более точной проверки
        float checkRadius = GetCollisionCheckRadius();
        
        // Проверяем коллизии в центре (только если нет коллизий слоев)
        Collider[] centerColliders = new Collider[0];
        if (!hasLayerCollisions)
        {
            centerColliders = Physics.OverlapSphere(position, checkRadius, inventoryManager.CollisionCheckMask);
            
            if (inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"[CheckPlacementValidityTouching] Проверка коллизий:");
                Debug.Log($"[CheckPlacementValidityTouching]   - Позиция: {position}");
                Debug.Log($"[CheckPlacementValidityTouching]   - Радиус: {checkRadius}");
                Debug.Log($"[CheckPlacementValidityTouching]   - collisionCheckMask: {inventoryManager.CollisionCheckMask.value}");
                Debug.Log($"[CheckPlacementValidityTouching]   - hasLayerCollisions: {hasLayerCollisions}");
                Debug.Log($"[CheckPlacementValidityTouching]   - Найдено коллайдеров: {centerColliders.Length}");
                
                // Выводим информацию о каждом найденном коллайдере
                for (int i = 0; i < centerColliders.Length; i++)
                {
                    Collider col = centerColliders[i];
                    Debug.Log($"[CheckPlacementValidityTouching]   - Коллайдер {i}: {col.gameObject.name} (Layer: {LayerMask.LayerToName(col.gameObject.layer)})");
                }
            }
        }
        else
        {
            if (inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"[CheckPlacementValidityTouching] Пропускаем проверку коллизий в центре (есть коллизии слоев)");
            }
        }
        
        // Дополнительная проверка: если есть коллизии, проверяем, не являются ли они поверхностями
        if (centerColliders.Length > 0 && inventoryManager.EnableLayerSystem)
        {
            bool hasValidSurface = false;
            
            foreach (Collider col in centerColliders)
            {
                // Исключаем превью объект
                if (col.gameObject == previewInstance || 
                    col.transform.IsChildOf(previewInstance.transform) ||
                    col.gameObject.layer == outlineLayer)
                {
                    continue;
                }
                
                // Проверяем, является ли это поверхностью
                PlacementLayerComponent surfaceComponent = col.GetComponent<PlacementLayerComponent>();
                if (surfaceComponent != null && surfaceComponent.IsSurface)
                {
                    PlacementLayer surfaceLayer = surfaceComponent.PlacementLayer;
                    bool canPlaceOnSurface = itemLayer.CanPlaceOn(surfaceLayer);
                    bool onSurface = surfaceComponent.IsPositionOnSurface(position);
                    
                    if (inventoryManager.DebugSurfaceSystem)
                    {
                        Debug.Log($"[CheckPlacementValidityTouching] Найдена поверхность: {col.gameObject.name}");
                        Debug.Log($"[CheckPlacementValidityTouching]   - Слой поверхности: {surfaceLayer}");
                        Debug.Log($"[CheckPlacementValidityTouching]   - CanPlaceOn: {canPlaceOnSurface}");
                        Debug.Log($"[CheckPlacementValidityTouching]   - OnSurface: {onSurface}");
                    }
                    
                    if (canPlaceOnSurface && onSurface)
                    {
                        hasValidSurface = true;
                        if (inventoryManager.DebugSurfaceSystem)
                        {
                            Debug.Log($"[CheckPlacementValidityTouching] ✅ Разрешаем размещение на поверхности: {col.gameObject.name}");
                        }
                        break;
                    }
                }
            }
            
            if (hasValidSurface)
            {
                if (inventoryManager.DebugSurfaceSystem)
                {
                    Debug.Log($"[CheckPlacementValidityTouching] ✅ Найдена валидная поверхность, разрешаем размещение");
                }
                return true; // Разрешаем размещение на поверхности
            }
        }
        
        // Проверяем коллизии в углах объекта (для крупных объектов)
        bool hasCornerCollisions = CheckCornerCollisions(position);
        
        if (inventoryManager.DebugCollisions && inventoryManager.DetailedDebugCollisions)
        {
            Debug.Log($"🔍 Проверка коллизий в позиции {position}");
            Debug.Log($"   Слой предмета: {itemLayer.GetDisplayName()}");
            Debug.Log($"   Радиус проверки: {checkRadius}");
            Debug.Log($"   Коллизии слоев: {hasLayerCollisions}");
            Debug.Log($"   Найдено коллайдеров в центре: {centerColliders.Length}");
            Debug.Log($"   Коллизии в углах: {hasCornerCollisions}");
        }
        
        // Если есть коллизии слоев, размещение невозможно
        if (hasLayerCollisions)
        {
            if (inventoryManager.DebugCollisions)
            {
                Debug.Log("❌ Обнаружены коллизии слоев!");
            }
            return false;
        }
        
        // Исключаем сам превью объект и его дочерние элементы из проверки
        foreach (Collider col in centerColliders)
        {
            // Проверяем, не является ли это превью объектом или его дочерним элементом
            if (col.gameObject != previewInstance && 
                !col.transform.IsChildOf(previewInstance.transform) &&
                col.gameObject.layer != outlineLayer && // Исключаем объекты на слое превью
                !col.isTrigger && // Исключаем триггер-коллайдеры
                !IsTagIgnored(col.gameObject.tag) && // Исключаем объекты с игнорируемыми тегами
                !IsLayerIgnored(col.gameObject.layer)) // Исключаем объекты с игнорируемыми слоями
            {
                if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
                {
                    Debug.Log($"⚠️ Коллизия в центре с объектом: {col.gameObject.name}");
                    Debug.Log($"   Слой: {LayerMask.LayerToName(col.gameObject.layer)}");
                    Debug.Log($"   Тег: {col.gameObject.tag}");
                    Debug.Log($"   Позиция: {col.transform.position}");
                    
                    // Проверяем, есть ли PlacementLayerComponent
                    PlacementLayerComponent layerComponent = col.GetComponent<PlacementLayerComponent>();
                    if (layerComponent != null)
                    {
                        Debug.Log($"   PlacementLayerComponent: {layerComponent.PlacementLayer}, IsSurface: {layerComponent.IsSurface}");
                    }
                    else
                    {
                        Debug.Log($"   PlacementLayerComponent: НЕ НАЙДЕН");
                    }
                }
                return false; // Есть коллизия
            }
        }
        
        // Если есть коллизии в углах, размещение невозможно
        if (hasCornerCollisions)
        {
            if (inventoryManager.DebugCollisions)
            {
                Debug.Log("❌ Обнаружены коллизии в углах объекта!");
            }
            return false;
        }
        
        if (inventoryManager.DebugCollisions)
        {
            Debug.Log("✅ Проверка коллизий пройдена успешно!");
        }
        
        return true; // Размещение возможно
    }

    private bool CheckPlacementValidityStrict(Vector3 position)
    {
        if (inventoryManager == null) return true;
        
        // Более строгая проверка - используем больший радиус для предотвращения касания
        float checkRadius = GetCollisionCheckRadius() * 1.5f; // Увеличиваем радиус на 50%
        
        // Проверяем коллизии в центре
        Collider[] centerColliders = Physics.OverlapSphere(position, checkRadius, inventoryManager.CollisionCheckMask);
        
        // Проверяем коллизии в углах объекта (для крупных объектов)
        bool hasCornerCollisions = CheckCornerCollisions(position);
        
        if (inventoryManager.DebugCollisions)
        {
            Debug.Log($"Строгая проверка коллизий в позиции {position}, радиус: {checkRadius}, найдено коллайдеров в центре: {centerColliders.Length}, коллизии в углах: {hasCornerCollisions}");
        }
        
        // Исключаем сам превью объект и его дочерние элементы из проверки
        foreach (Collider col in centerColliders)
        {
            // Проверяем, не является ли это превью объектом или его дочерним элементом
            if (col.gameObject != previewInstance && 
                !col.transform.IsChildOf(previewInstance.transform) &&
                col.gameObject.layer == outlineLayer && // Исключаем объекты на слое превью
                !col.isTrigger && // Исключаем триггер-коллайдеры
                !IsTagIgnored(col.gameObject.tag) && // Исключаем объекты с игнорируемыми тегами
                !IsLayerIgnored(col.gameObject.layer)) // Исключаем объекты с игнорируемыми слоями
            {
                if (inventoryManager.DebugCollisions)
                {
                    Debug.Log($"Строгая коллизия в центре с объектом: {col.gameObject.name} (слой: {LayerMask.LayerToName(col.gameObject.layer)}, тег: {col.gameObject.tag})");
                }
                return false; // Есть коллизия
            }
        }
        
        // Если есть коллизии в углах, размещение невозможно
        if (hasCornerCollisions)
        {
            if (inventoryManager.DebugCollisions)
            {
                Debug.Log("❌ Обнаружены строгие коллизии в углах объекта!");
            }
            return false;
        }
        
        if (inventoryManager.DebugCollisions)
        {
            Debug.Log("✅ Строгая проверка коллизий пройдена успешно!");
        }
        
        return true; // Размещение возможно
    }

    private float GetCollisionCheckRadius()
    {
        if (previewInstance == null || inventoryManager == null) 
            return inventoryManager != null ? inventoryManager.CollisionCheckRadius : 0.3f;
        
        // Получаем размер объекта через коллайдер
        Collider col = previewInstance.GetComponent<Collider>();
        if (col != null)
        {
            Vector3 size = col.bounds.size;
            float maxSize = Mathf.Max(size.x, size.y, size.z);
            
            // Более точный радиус проверки - учитываем размер объекта
            float calculatedRadius = maxSize * inventoryManager.ObjectSizeMultiplier; // Используем настраиваемый множитель
            float finalRadius = Mathf.Max(inventoryManager.CollisionCheckRadius, calculatedRadius);
            
            // Минимальный радиус для надежного обнаружения поверхностей
            finalRadius = Mathf.Max(finalRadius, 0.3f);
            
            if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"[GetCollisionCheckRadius] Размер объекта: {size}, максимальный размер: {maxSize}, рассчитанный радиус: {calculatedRadius}, финальный радиус: {finalRadius}");
            }
            
            return finalRadius;
        }
        
        return inventoryManager.CollisionCheckRadius;
    }

    private bool IsTagIgnored(string tag)
    {
        if (inventoryManager == null) return false;
        
        string[] ignoredTags = inventoryManager.IgnoredTags;
        if (ignoredTags == null || ignoredTags.Length == 0) return false;
        
        foreach (string ignoredTag in ignoredTags)
        {
            if (tag == ignoredTag) return true;
        }
        
        return false;
    }

    private bool IsLayerIgnored(int layer)
    {
        if (inventoryManager == null) return false;
        return (inventoryManager.IgnoredLayers.value & (1 << layer)) != 0;
    }
    
    /// <summary>
    /// Проверка наложения объектов друг на друга
    /// </summary>
    private bool CheckObjectOverlap(Vector3 position)
    {
        if (previewInstance == null || inventoryManager == null) return false;
        
        // Получаем размеры и границы текущего объекта с учетом поворота
        Vector3 objectSize = GetObjectSize();
        
        // Создаем границы с учетом поворота объекта
        Bounds currentBounds = new Bounds(position, objectSize);
        
        // Расширяем границы для более точной проверки
        // Для предметов на поверхностях используем меньший отступ
        float margin = inventoryManager.OverlapCheckMargin;
        if (inventoryManager.EnableLayerSystem && previewInstance != null)
        {
            PlacementLayer itemLayer = GetPlacementLayer(previewInstance);
            if (itemLayer == PlacementLayer.Item)
            {
                // Для предметов на поверхностях используем меньший отступ
                margin = inventoryManager.OverlapCheckMargin * 0.5f;
            }
        }
        currentBounds.Expand(margin);
        
        if (inventoryManager.DebugCollisions)
        {
            Debug.Log($"[OverlapCheck] ===== НАЧАЛО ПРОВЕРКИ НАЛОЖЕНИЯ =====");
            Debug.Log($"[OverlapCheck] Объект: {previewInstance.name}");
            Debug.Log($"[OverlapCheck] Размер объекта: {objectSize}");
            Debug.Log($"[OverlapCheck] Позиция: {position}");
            Debug.Log($"[OverlapCheck] Поворот объекта: {previewInstance.transform.rotation.eulerAngles}");
            Debug.Log($"[OverlapCheck] Границы проверки: {currentBounds}");
            Debug.Log($"[OverlapCheck] collisionCheckMask: {inventoryManager.CollisionCheckMask.value}");
        }
        
        // Получаем все коллайдеры в области с учетом поворота
        Collider[] nearbyColliders = Physics.OverlapBox(
            currentBounds.center, 
            currentBounds.extents, 
            previewInstance.transform.rotation, 
            inventoryManager.CollisionCheckMask
        );
        
        if (inventoryManager.DebugCollisions)
        {
            Debug.Log($"[OverlapCheck] Найдено коллайдеров в области: {nearbyColliders.Length}");
            
            // Выводим информацию о каждом найденном коллайдере
            for (int i = 0; i < nearbyColliders.Length; i++)
            {
                Collider col = nearbyColliders[i];
                Debug.Log($"[OverlapCheck] Коллайдер {i}: {col.gameObject.name} (Layer: {LayerMask.LayerToName(col.gameObject.layer)}, Tag: {col.gameObject.tag})");
            }
            
            // Дополнительная отладка слоев
            if (inventoryManager.ShowLayerDebugInfo)
            {
                Debug.Log($"[OverlapCheck] Анализ слоев:");
                Debug.Log($"[OverlapCheck]   - collisionCheckMask включает слой Default: {(inventoryManager.CollisionCheckMask.value & (1 << LayerMask.NameToLayer("Default"))) != 0}");
                Debug.Log($"[OverlapCheck]   - collisionCheckMask включает слой OutlinePreview: {(inventoryManager.CollisionCheckMask.value & (1 << LayerMask.NameToLayer("OutlinePreview"))) != 0}");
                
                // Найдем все объекты в сцене с коллайдерами
                Collider[] allColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
                Debug.Log($"[OverlapCheck] Всего коллайдеров в сцене: {allColliders.Length}");
                
                // Группируем по слоям
                Dictionary<int, int> layerCounts = new Dictionary<int, int>();
                foreach (var col in allColliders)
                {
                    int layer = col.gameObject.layer;
                    if (!layerCounts.ContainsKey(layer))
                        layerCounts[layer] = 0;
                    layerCounts[layer]++;
                }
                
                foreach (var kvp in layerCounts)
                {
                    string layerName = LayerMask.LayerToName(kvp.Key);
                    Debug.Log($"[OverlapCheck]   - Слой {layerName} ({kvp.Key}): {kvp.Value} объектов");
                }
            }
        }
        
        foreach (Collider col in nearbyColliders)
        {
            // Исключаем сам превью объект и его дочерние элементы
            if (col.gameObject == previewInstance || 
                col.transform.IsChildOf(previewInstance.transform) ||
                col.gameObject.layer == outlineLayer ||
                col.isTrigger ||
                IsTagIgnored(col.gameObject.tag) ||
                IsLayerIgnored(col.gameObject.layer))
            {
                continue;
            }
            
            // Получаем компонент слоя размещения
            PlacementLayerComponent otherLayerComponent = col.GetComponent<PlacementLayerComponent>();
            PlacementLayer currentItemLayer = GetPlacementLayer(previewInstance);
            
            // Проверяем, можно ли размещать на этом объекте
            bool canPlaceOnThis = false;
            if (otherLayerComponent != null && inventoryManager.AllowStackingOnSurfaces)
            {
                canPlaceOnThis = otherLayerComponent.IsSurface && 
                                currentItemLayer.CanPlaceOn(otherLayerComponent.PlacementLayer);
            }
            
            // Проверяем пересечение границ с учетом поворота
            Bounds otherBounds = col.bounds;
            
            // Более точная проверка пересечения с учетом поворота
            bool hasOverlap = CheckRotatedBoundsIntersection(currentBounds, otherBounds, previewInstance.transform.rotation, col.transform.rotation);
            
            if (inventoryManager.DebugCollisions)
            {
                Debug.Log($"[OverlapCheck] --- Проверяем объект: {col.gameObject.name} ---");
                Debug.Log($"[OverlapCheck]   - Границы: {otherBounds}");
                Debug.Log($"[OverlapCheck]   - Пересечение: {hasOverlap}");
                Debug.Log($"[OverlapCheck]   - Можно размещать на нем: {canPlaceOnThis}");
                Debug.Log($"[OverlapCheck]   - PlacementLayerComponent: {otherLayerComponent != null}");
                if (otherLayerComponent != null)
                {
                    Debug.Log($"[OverlapCheck]   - IsSurface: {otherLayerComponent.IsSurface}");
                    Debug.Log($"[OverlapCheck]   - PlacementLayer: {otherLayerComponent.PlacementLayer}");
                }
            }
            
            // Если можно размещать на этом объекте, проверяем, что позиция находится на поверхности
            if (hasOverlap && canPlaceOnThis)
            {
                if (otherLayerComponent.IsPositionOnSurface(position))
                {
                    if (inventoryManager.DebugCollisions)
                    {
                        Debug.Log($"[OverlapCheck] ✅ Размещение на поверхности разрешено: {col.gameObject.name}");
                    }
                    continue; // Разрешаем размещение на поверхности
                }
                else
                {
                    if (inventoryManager.DebugCollisions)
                    {
                        Debug.Log($"[OverlapCheck] ❌ Позиция не на поверхности объекта: {col.gameObject.name}");
                    }
                    return true; // Позиция не на поверхности, но есть пересечение
                }
            }
            
            // Если есть пересечение и нельзя размещать на этом объекте
            if (hasOverlap && !canPlaceOnThis)
            {
                // Дополнительная проверка: возможно, объекты находятся на разных высотах
                if (inventoryManager.CheckHeightDifference)
                {
                    float heightDifference = Mathf.Abs(position.y - col.bounds.center.y);
                    float maxHeightDiff = Mathf.Max(objectSize.y, col.bounds.size.y) * 0.3f + inventoryManager.OverlapTolerance;
                    
                    if (heightDifference > maxHeightDiff)
                    {
                        if (inventoryManager.DebugCollisions)
                        {
                            Debug.Log($"[OverlapCheck] ✅ Объекты на разной высоте: {heightDifference:F3} > {maxHeightDiff:F3}");
                        }
                        continue; // Объекты на разной высоте, наложения нет
                    }
                }
                
                if (inventoryManager.DebugCollisions)
                {
                    Debug.Log($"[OverlapCheck] ❌ Обнаружено наложение с объектом: {col.gameObject.name}");
                }
                return true; // Есть наложение
            }
        }
        
        if (inventoryManager.DebugCollisions)
        {
            Debug.Log($"[OverlapCheck] ✅ Наложений не обнаружено");
            Debug.Log($"[OverlapCheck] ===== КОНЕЦ ПРОВЕРКИ НАЛОЖЕНИЯ =====");
        }
        
        return false; // Наложений нет
    }
    
    /// <summary>
    /// Проверка пересечения границ с учетом поворота объектов
    /// </summary>
    private bool CheckRotatedBoundsIntersection(Bounds bounds1, Bounds bounds2, Quaternion rotation1, Quaternion rotation2)
    {
        // Простая проверка пересечения границ
        bool simpleIntersection = bounds1.Intersects(bounds2);
        
        if (!simpleIntersection) return false;
        
        // Более точная проверка с учетом поворота
        // Проверяем пересечение по осям с учетом поворота
        Vector3 center1 = bounds1.center;
        Vector3 center2 = bounds2.center;
        Vector3 size1 = bounds1.size;
        Vector3 size2 = bounds2.size;
        
        // Применяем поворот к размерам
        Vector3 rotatedSize1 = rotation1 * size1;
        Vector3 rotatedSize2 = rotation2 * size2;
        
        // Вычисляем минимальное расстояние между центрами
        float minDistanceX = (Mathf.Abs(rotatedSize1.x) + Mathf.Abs(rotatedSize2.x)) * 0.5f;
        float minDistanceZ = (Mathf.Abs(rotatedSize1.z) + Mathf.Abs(rotatedSize2.z)) * 0.5f;
        
        // Проверяем расстояние по X и Z осям
        float distanceX = Mathf.Abs(center1.x - center2.x);
        float distanceZ = Mathf.Abs(center1.z - center2.z);
        
        bool hasOverlap = distanceX < minDistanceX && distanceZ < minDistanceZ;
        
        if (inventoryManager.DebugCollisions)
        {
            Debug.Log($"[RotatedBounds] Центр1: {center1}, Центр2: {center2}");
            Debug.Log($"[RotatedBounds] Размер1: {size1} -> {rotatedSize1}");
            Debug.Log($"[RotatedBounds] Размер2: {size2} -> {rotatedSize2}");
            Debug.Log($"[RotatedBounds] Расстояние X: {distanceX:F3} < {minDistanceX:F3} = {distanceX < minDistanceX}");
            Debug.Log($"[RotatedBounds] Расстояние Z: {distanceZ:F3} < {minDistanceZ:F3} = {distanceZ < minDistanceZ}");
            Debug.Log($"[RotatedBounds] Наложение: {hasOverlap}");
        }
        
        return hasOverlap;
    }

    private bool CheckFloorBounds(Vector3 position)
    {
        if (inventoryManager == null) return true;
        
        if (!inventoryManager.CheckFloorBounds || inventoryManager.DisableFloorBoundsCheck)
        {
            if (inventoryManager.DebugCollisions && inventoryManager.DisableFloorBoundsCheck)
            {
                Debug.Log($"   ✅ Границы пола: ОТКЛЮЧЕНЫ (пропускаем)");
            }
            return true;
        }
        
        // Находим пол в сцене
        GameObject floor = GameObject.FindGameObjectWithTag("RoomBoxFloor");
        if (floor == null) return true; // Если пол не найден, разрешаем размещение
        
        // Получаем коллайдер пола
        Collider floorCollider = floor.GetComponent<Collider>();
        if (floorCollider == null) return true;
        
        // Получаем размеры пола
        Bounds floorBounds = floorCollider.bounds;
        
        // Получаем размеры объекта
        Vector3 objectSize = GetObjectSize();
        
        // Получаем поворот объекта
        Quaternion rotation = previewInstance != null ? previewInstance.transform.rotation : Quaternion.identity;
        
        // Вычисляем углы объекта с учетом поворота
        float halfWidth = objectSize.x * 0.5f;
        float halfLength = objectSize.z * 0.5f;
        
        Vector3[] corners = new Vector3[]
        {
            position + rotation * new Vector3(halfWidth, 0, halfLength),   // Правый верхний
            position + rotation * new Vector3(-halfWidth, 0, halfLength),  // Левый верхний
            position + rotation * new Vector3(halfWidth, 0, -halfLength),  // Правый нижний
            position + rotation * new Vector3(-halfWidth, 0, -halfLength)  // Левый нижний
        };
        
        // Проверяем, не выходит ли какой-либо угол за границы пола
        bool withinBounds = true;
        int cornerIndex = 0;
        foreach (Vector3 corner in corners)
        {
            bool cornerInBounds = corner.x >= floorBounds.min.x + inventoryManager.FloorBoundsMargin &&
                                 corner.x <= floorBounds.max.x - inventoryManager.FloorBoundsMargin &&
                                 corner.z >= floorBounds.min.z + inventoryManager.FloorBoundsMargin &&
                                 corner.z <= floorBounds.max.z - inventoryManager.FloorBoundsMargin;
            
            if (inventoryManager.ShowDetailedDebug)
            {
                Debug.Log($"   Угол {cornerIndex}: {corner}");
                Debug.Log($"     Границы пола X: {floorBounds.min.x + inventoryManager.FloorBoundsMargin} до {floorBounds.max.x - inventoryManager.FloorBoundsMargin}");
                Debug.Log($"     Границы пола Z: {floorBounds.min.z + inventoryManager.FloorBoundsMargin} до {floorBounds.max.z - inventoryManager.FloorBoundsMargin}");
                Debug.Log($"     Угол в границах: {(cornerInBounds ? "✅ ДА" : "❌ НЕТ")}");
            }
            
            if (!cornerInBounds)
            {
                withinBounds = false;
                if (inventoryManager.ShowDetailedDebug)
                {
                    Debug.Log($"   ❌ Угол {cornerIndex} выходит за границы!");
                }
                break;
            }
            cornerIndex++;
        }
        
        if (inventoryManager.DebugCollisions)
        {
            if (!withinBounds)
            {
                Debug.Log($"❌ Объект выходит за границы пола!");
                Debug.Log($"   Позиция: {position}");
                Debug.Log($"   Поворот: {rotation.eulerAngles}");
                Debug.Log($"   Размеры объекта: {objectSize}");
                Debug.Log($"   Половина ширины: {halfWidth}, половина длины: {halfLength}");
                Debug.Log($"   Границы пола: {floorBounds.min} - {floorBounds.max}");
                Debug.Log($"   Размер пола: {floorBounds.size}");
                Debug.Log($"   Отступ: {inventoryManager.FloorBoundsMargin}");
                Debug.Log($"   Углы объекта:");
                for (int i = 0; i < corners.Length; i++)
                {
                    Debug.Log($"     Угол {i}: {corners[i]}");
                }
            }
            else
            {
                Debug.Log($"✅ Объект в пределах пола. Позиция: {position}, поворот: {rotation.eulerAngles}, размеры: {objectSize}");
            }
        }
        
        return withinBounds;
    }

    private bool CheckCornerCollisions(Vector3 position)
    {
        if (previewInstance == null || inventoryManager == null) return false;
        
        // Получаем размеры объекта
        Vector3 objectSize = GetObjectSize();
        float halfWidth = objectSize.x * 0.5f;
        float halfLength = objectSize.z * 0.5f;
        
        // Получаем поворот объекта
        Quaternion rotation = previewInstance.transform.rotation;
        
        // Проверяем 4 угла объекта с учетом поворота
        Vector3[] corners = new Vector3[]
        {
            position + rotation * new Vector3(halfWidth, 0, halfLength),   // Правый верхний
            position + rotation * new Vector3(-halfWidth, 0, halfLength),  // Левый верхний
            position + rotation * new Vector3(halfWidth, 0, -halfLength),  // Правый нижний
            position + rotation * new Vector3(-halfWidth, 0, -halfLength)  // Левый нижний
        };
        
        if (inventoryManager.ShowDetailedDebug)
        {
            Debug.Log($"🔍 Проверка угловых коллизий:");
            Debug.Log($"   Позиция: {position}");
            Debug.Log($"   Поворот: {rotation.eulerAngles}");
            Debug.Log($"   Размеры: {objectSize}, половина ширины: {halfWidth}, половина длины: {halfLength}");
            for (int i = 0; i < corners.Length; i++)
            {
                Debug.Log($"   Угол {i}: {corners[i]}");
            }
        }
        
        float cornerCheckRadius = 0.1f; // Небольшой радиус для проверки углов
        
        foreach (Vector3 corner in corners)
        {
            Collider[] cornerColliders = Physics.OverlapSphere(corner, cornerCheckRadius, inventoryManager.CollisionCheckMask);
            
            foreach (Collider col in cornerColliders)
            {
                // Проверяем, не является ли это превью объектом или его дочерним элементом
                if (col.gameObject != previewInstance && 
                    !col.transform.IsChildOf(previewInstance.transform) &&
                    col.gameObject.layer == outlineLayer && // Исключаем объекты на слое превью
                    !col.isTrigger && // Исключаем триггер-коллайдеры
                    !IsTagIgnored(col.gameObject.tag) && // Исключаем объекты с игнорируемыми тегами
                    !IsLayerIgnored(col.gameObject.layer)) // Исключаем объекты с игнорируемыми слоями
                {
                    if (inventoryManager.DebugCollisions)
                    {
                        Debug.Log($"⚠️ Коллизия в углу {corner} с объектом: {col.gameObject.name}");
                        Debug.Log($"   Поворот объекта: {rotation.eulerAngles}");
                        Debug.Log($"   Слой: {LayerMask.LayerToName(col.gameObject.layer)}");
                        Debug.Log($"   Тег: {col.gameObject.tag}");
                    }
                    return true; // Есть коллизия в углу
                }
            }
        }
        
        return false; // Нет коллизий в углах
    }

    private Vector3 GetObjectSize()
    {
        if (previewInstance == null) return Vector3.one;
        
        // Получаем коллайдер объекта
        Collider objectCollider = previewInstance.GetComponent<Collider>();
        if (objectCollider != null)
        {
            // Получаем размеры с учетом поворота
            Vector3 size;
            
            if (objectCollider is BoxCollider boxCollider)
            {
                // Для BoxCollider используем локальные размеры
                size = boxCollider.size;
                
                if (inventoryManager != null && inventoryManager.DebugCollisions)
                {
                    Debug.Log($"[GetObjectSize] BoxCollider - Локальный размер: {boxCollider.size}");
                }
            }
            else if (objectCollider is CapsuleCollider capsuleCollider)
            {
                // Для CapsuleCollider используем локальные размеры
                size = new Vector3(capsuleCollider.radius * 2, capsuleCollider.height, capsuleCollider.radius * 2);
            }
            else if (objectCollider is SphereCollider sphereCollider)
            {
                // Для SphereCollider размер не зависит от поворота
                float diameter = sphereCollider.radius * 2;
                size = new Vector3(diameter, diameter, diameter);
            }
            else
            {
                // Для других типов коллайдеров используем bounds с учетом поворота
                size = objectCollider.bounds.size;
            }
            
            if (inventoryManager != null && inventoryManager.DebugCollisions)
            {
                Debug.Log($"[GetObjectSize] Коллайдер: {objectCollider.GetType().Name}, Размер: {size}, Поворот: {previewInstance.transform.rotation.eulerAngles}");
            }
            
            return size;
        }
        
        // Fallback - используем размер рендерера
        Renderer objectRenderer = previewInstance.GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            // Получаем размеры с учетом поворота
            Vector3 size = objectRenderer.bounds.size;
            
            if (inventoryManager != null && inventoryManager.DebugCollisions)
            {
                Debug.Log($"[GetObjectSize] Рендерер: {objectRenderer.GetType().Name}, Размер: {size}");
            }
            
            return size;
        }
        
        // Если ничего не найдено, возвращаем размер по умолчанию
        Vector3 defaultSize = Vector3.one;
        
        if (inventoryManager != null && inventoryManager.DebugCollisions)
        {
            Debug.Log($"[GetObjectSize] Используется размер по умолчанию: {defaultSize}");
        }
        
        return defaultSize;
    }
    
    /// <summary>
    /// Получить размеры объекта с учетом поворота (только для визуализации)
    /// </summary>
    private Vector3 GetObjectSizeWithRotation()
    {
        if (previewInstance == null) return Vector3.one;
        
        // Получаем базовые размеры
        Vector3 baseSize = GetObjectSize();
        
        // Применяем поворот к размерам для визуализации
        Vector3 rotatedSize = previewInstance.transform.rotation * baseSize;
        Vector3 finalSize = new Vector3(Mathf.Abs(rotatedSize.x), Mathf.Abs(rotatedSize.y), Mathf.Abs(rotatedSize.z));
        
        if (inventoryManager != null && inventoryManager.DebugCollisions)
        {
            Debug.Log($"[GetObjectSizeWithRotation] Базовый размер: {baseSize}, Повернутый: {finalSize}");
        }
        
        return finalSize;
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (inventoryManager == null || !inventoryManager.ShowFloorBounds) return;
        
        // Находим пол в сцене
        GameObject floor = GameObject.FindGameObjectWithTag("RoomBoxFloor");
        if (floor == null) return;
        
        // Получаем коллайдер пола
        Collider floorCollider = floor.GetComponent<Collider>();
        if (floorCollider == null) return;
        
        // Получаем размеры пола
        Bounds floorBounds = floorCollider.bounds;
        
        // Рисуем границы пола
        Gizmos.color = Color.green;
        Vector3 center = floorBounds.center;
        Vector3 size = floorBounds.size;
        size.y = 0.1f; // Тонкая линия
        Gizmos.DrawWireCube(center, size);
        
        // Рисуем границы с отступом
        Gizmos.color = Color.red;
        Vector3 marginSize = size;
        marginSize.x -= inventoryManager.FloorBoundsMargin * 2;
        marginSize.z -= inventoryManager.FloorBoundsMargin * 2;
        Gizmos.DrawWireCube(center, marginSize);
        
        // Если есть превью объект, рисуем его границы с учетом поворота
        if (previewInstance != null)
        {
            Vector3 objectSize = GetObjectSize();
            Vector3 rotatedSize = GetObjectSizeWithRotation();
            
            // Рисуем границы объекта с учетом поворота
            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(previewInstance.transform.position, previewInstance.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, objectSize);
            Gizmos.matrix = Matrix4x4.identity; // Сбрасываем матрицу
            
            // Рисуем границы проверки наложения с учетом поворота
            if (inventoryManager.PreventObjectOverlap)
            {
                // Создаем расширенные размеры с учетом отступа
                Vector3 expandedSize = objectSize + Vector3.one * inventoryManager.OverlapCheckMargin * 2f;
                
                Gizmos.color = inventoryManager.OverlapWarningColor;
                Gizmos.matrix = Matrix4x4.TRS(previewInstance.transform.position, previewInstance.transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, expandedSize);
                Gizmos.matrix = Matrix4x4.identity; // Сбрасываем матрицу
                
                // Рисуем дополнительную информацию
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(previewInstance.transform.position + Vector3.up * 2f, 
                    $"Overlap Check\nBase: {objectSize}\nRotated: {rotatedSize}\nExpanded: {expandedSize}\nRotation: {previewInstance.transform.rotation.eulerAngles}");
                #endif
            }
        }
    }
    #endif

    private void UpdateVisualFeedback(bool isValid)
    {
        if (previewRenderers == null || inventoryManager == null) return;
        
        Color targetColor;
        
        // Проверяем наложение объектов для специального цвета предупреждения
        bool hasOverlap = false;
        if (inventoryManager.PreventObjectOverlap && previewInstance != null)
        {
            hasOverlap = CheckObjectOverlap(previewInstance.transform.position);
        }
        
        // Используем систему слоев для визуальной обратной связи
        if (inventoryManager.EnableLayerSystem && previewInstance != null)
        {
            PlacementLayer itemLayer = GetPlacementLayer(previewInstance);
            UpdateLayerVisualFeedback(isValid, itemLayer);
        }
        else
        {
            // Стандартная визуальная обратная связь с учетом наложения
            if (hasOverlap)
            {
                targetColor = inventoryManager.OverlapWarningColor; // Оранжевый для предупреждения о наложении
            }
            else
            {
                targetColor = isValid ? inventoryManager.ValidPlacementColor : inventoryManager.InvalidPlacementColor;
            }
        
            // Обновляем цвет основного объекта
            for (int i = 0; i < previewRenderers.Length; i++)
            {
                if (previewRenderers[i] != null)
                {
                    Material[] materials = previewRenderers[i].materials;
                    for (int j = 0; j < materials.Length; j++)
                    {
                        if (materials[j].HasProperty("_Color"))
                        {
                            materials[j].color = targetColor;
                        }
                    }
                }
            }
        }
        
        // Отслеживаем изменение валидности для эффектов
        if (wasValidLastFrame != isValid)
        {
            wasValidLastFrame = isValid;
            
            if (inventoryManager.DebugCollisions)
            {
                Debug.Log($"Валидность размещения изменилась: {(isValid ? "✅ Валидно" : "❌ Невалидно")}");
            }
        }
    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[OnBeginDrag] Начало перетаскивания: item={item}, dragPrefab={dragPrefab}");
        
        if (item == null || dragPrefab == null) 
        {
            Debug.LogWarning($"[OnBeginDrag] Перетаскивание отменено: item={item}, dragPrefab={dragPrefab}");
            return;
        }
        
        previewInstance = Instantiate(dragPrefab);
        SetLayerRecursively(previewInstance, outlineLayer);
        
        // Сохраняем оригинальный слой префаба для превью объекта
        int originalLayer = dragPrefab.layer;
        if (originalLayer != outlineLayer)
        {
            SetLayerRecursively(previewInstance, originalLayer);
            Debug.Log($"✅ Сохранен оригинальный слой {LayerMask.LayerToName(originalLayer)} для превью объекта: {previewInstance.name}");
        }
        
        // Настраиваем коллайдеры превью объекта
        SetupPreviewColliders(previewInstance);
        
        // Сохраняем рендереры и материалы для визуальной обратной связи
        previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[previewRenderers.Length];
        for (int i = 0; i < previewRenderers.Length; i++)
        {
            if (previewRenderers[i] != null)
            {
                originalMaterials[i] = previewRenderers[i].material;
            }
        }
        
        // --- Добавлено: лог размера объекта только при начале перетаскивания ---
        Vector3 size = GetObjectSize();
        Debug.Log($"📏 Вычисление размеров объекта: {previewInstance.name}, размер: {size}");
        // --- Конец добавления ---
        
        previewRotation = previewInstance.transform.rotation;
        isRotated90 = false;
        wasRightButtonPressed = false;
        wasMiddleButtonPressed = false;
        isDragging = true;
        canPlace = true;
        
        Debug.Log($"[OnBeginDrag] Перетаскивание начато успешно для предмета: {item.itemName}");
        
        // Вызываем событие начала перетаскивания
        slotUI?.TriggerOnDragStarted();
        
        // Вызываем событие через интерфейс системы событий
        _eventSystem?.InvokeDragStarted(item?.ToIItem(), GetSlotIndex());
        
        // Воспроизводим звук начала перетаскивания
        PlaySound(inventoryManager?.DragStartSound);
        
        // Показываем подсказку
        if (navigationUI != null)
        {
            navigationUI.ShowValidPlacementHint();
        }
    }
    
    /// <summary>
    /// Воспроизводит звуковой эффект
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (inventoryManager == null || !inventoryManager.EnableSoundEffects || clip == null) return;
        
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.PlayOneShot(clip);
    }

    private void SetupPreviewColliders(GameObject previewObject)
    {
        // Получаем все коллайдеры на превью объекте
        Collider[] colliders = previewObject.GetComponentsInChildren<Collider>();
        
        foreach (Collider col in colliders)
        {
            // Делаем коллайдер триггером для превью
            col.isTrigger = true;
            
            // Отключаем физические взаимодействия
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Проверяем состояние перетаскивания
        if (!isDragging)
        {
            Debug.LogWarning("[OnDrag] Перетаскивание не активно!");
            return;
        }
        
        if (previewInstance == null) 
        {
            Debug.LogWarning("[OnDrag] previewInstance is null! Пытаемся восстановить...");
            
            // Пытаемся восстановить previewInstance
            if (item != null && dragPrefab != null)
            {
                previewInstance = Instantiate(dragPrefab);
                SetLayerRecursively(previewInstance, outlineLayer);
                SetupPreviewColliders(previewInstance);
                
                // Восстанавливаем рендереры и материалы
                previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();
                originalMaterials = new Material[previewRenderers.Length];
                for (int i = 0; i < previewRenderers.Length; i++)
                {
                    if (previewRenderers[i] != null)
                    {
                        originalMaterials[i] = previewRenderers[i].material;
                    }
                }
                
                Debug.Log("[OnDrag] previewInstance восстановлен");
            }
            else
            {
                Debug.LogError("[OnDrag] Не удалось восстановить previewInstance - item или dragPrefab null");
                CancelDrag();
                return;
            }
        }
        
        if (item == null)
        {
            Debug.LogError("[OnDrag] item is null during drag!");
            CancelDrag();
            return;
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[OnDrag] mainCamera is null!");
                return;
            }
        }
        
        Vector3 targetPosition;
        
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        
        if (inventoryManager != null && inventoryManager.UseRaycastPositioning)
        {
            // Используем Raycast для более точного позиционирования
            Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("RoomBoxFloor"))
            {
                targetPosition = hit.point;
            }
            else
            {
                // Fallback на плоскость если Raycast не попал в пол
                Plane floorPlane = new Plane(Vector3.up, new Vector3(0, inventoryManager != null ? inventoryManager.FloorHeight : 0f, 0));
                
                if (floorPlane.Raycast(ray, out float distance))
                {
                    targetPosition = ray.GetPoint(distance);
                }
                else
                {
                    // Двойной fallback на ScreenToWorldPoint
                    float distanceToFloor = Mathf.Abs(mainCamera.transform.position.y) + (inventoryManager != null ? inventoryManager.CameraDistanceOffset : 0f);
                    targetPosition = mainCamera.ScreenToWorldPoint(new Vector3(
                        mouseScreenPos.x, mouseScreenPos.y, distanceToFloor
                    ));
                    targetPosition.y = inventoryManager != null ? inventoryManager.FloorHeight : 0f;
                }
            }
        }
        else
        {
            // Используем ScreenPointToRay с фиксированной плоскостью для точного позиционирования
            Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);
            Plane floorPlane = new Plane(Vector3.up, new Vector3(0, inventoryManager != null ? inventoryManager.FloorHeight : 0f, 0));
            
            if (floorPlane.Raycast(ray, out float distance))
            {
                targetPosition = ray.GetPoint(distance);
            }
            else
            {
                // Fallback если луч не пересекает плоскость
                float distanceToFloor = Mathf.Abs(mainCamera.transform.position.y) + (inventoryManager != null ? inventoryManager.CameraDistanceOffset : 0f);
                targetPosition = mainCamera.ScreenToWorldPoint(new Vector3(
                    mouseScreenPos.x, mouseScreenPos.y, distanceToFloor
                ));
                targetPosition.y = inventoryManager != null ? inventoryManager.FloorHeight : 0f;
            }
        }

        // --- ДОБАВЛЕНО: поддержка прилипания к поверхности для Item ---
        if (inventoryManager != null && inventoryManager.EnableLayerSystem && previewInstance != null)
        {
            PlacementLayer itemLayer = GetPlacementLayer(previewInstance);
            if (inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[SurfaceDebug] enableLayerSystem={inventoryManager.EnableLayerSystem}, previewInstance={previewInstance.name}, itemLayer={itemLayer}");
            
            if (itemLayer == PlacementLayer.Item)
            {
                if (inventoryManager.DebugSurfaceSystem)
                    Debug.Log($"[SurfaceDebug] Предмет имеет слой Item, ищем поверхность...");
                
                var surface = FindSuitableSurface(targetPosition, itemLayer);
                if (surface != null)
                {
                    Vector3 surfacePos = surface.GetSurfacePosition();
                    targetPosition.y = surfacePos.y;
                    if (inventoryManager.DebugSurfaceSystem)
                        Debug.Log($"[SurfaceDebug] Найдена поверхность {surface.gameObject.name}, установлена Y={surfacePos.y}");
                }
                else
                {
                    if (inventoryManager.DebugSurfaceSystem)
                        Debug.Log($"[SurfaceDebug] Поверхность не найдена, оставляем Y={targetPosition.y}");
                }
            }
            else
            {
                if (inventoryManager.DebugSurfaceSystem)
                    Debug.Log($"[SurfaceDebug] Предмет имеет слой {itemLayer}, поиск поверхности пропущен");
            }
        }
        else
        {
            if (inventoryManager != null && inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[SurfaceDebug] enableLayerSystem={inventoryManager?.EnableLayerSystem}, previewInstance={previewInstance}");
        }
        // --- КОНЕЦ ДОБАВЛЕНИЯ ---

        if (inventoryManager != null && inventoryManager.DebugPositioning)
        {
            Debug.Log($"Mouse Screen: {mouseScreenPos}, Target World: {targetPosition}, Camera Pos: {mainCamera.transform.position}");
        }
        
        HandleValidDrag(targetPosition);
    }

    private void HandleValidDrag(Vector3 hitPoint)
    {
        Vector3 targetPosition = hitPoint;
        
        // Привязка к сетке
        if (gridPlacement != null)
        {
            targetPosition = gridPlacement.SnapToGrid(targetPosition);
        }

        // --- ДОБАВЛЕНО: поддержка прилипания к поверхности для Item после SnapToGrid ---
        if (inventoryManager != null && inventoryManager.EnableLayerSystem && previewInstance != null)
        {
            PlacementLayer itemLayer = GetPlacementLayer(previewInstance);
            if (inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[SurfaceDebug2] enableLayerSystem={inventoryManager.EnableLayerSystem}, previewInstance={previewInstance.name}, itemLayer={itemLayer}");
            
            if (itemLayer == PlacementLayer.Item)
            {
                if (inventoryManager.DebugSurfaceSystem)
                    Debug.Log($"[SurfaceDebug2] Предмет имеет слой Item, ищем поверхность...");
                
                var surface = FindSuitableSurface(targetPosition, itemLayer);
                if (surface != null)
                {
                    Vector3 surfacePos = surface.GetSurfacePosition();
                    targetPosition.y = surfacePos.y;
                    if (inventoryManager.DebugSurfaceSystem)
                        Debug.Log($"[SurfaceDebug2] Найдена поверхность {surface.gameObject.name}, установлена Y={surfacePos.y}");
                }
                else
                {
                    if (inventoryManager.DebugSurfaceSystem)
                        Debug.Log($"[SurfaceDebug2] Поверхность не найдена, оставляем Y={targetPosition.y}");
                }
            }
            else
            {
                if (inventoryManager.DebugSurfaceSystem)
                    Debug.Log($"[SurfaceDebug2] Предмет имеет слой {itemLayer}, поиск поверхности пропущен");
            }
        }
        else
        {
            if (inventoryManager != null && inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[SurfaceDebug2] enableLayerSystem={inventoryManager?.EnableLayerSystem}, previewInstance={previewInstance}");
        }
        // --- КОНЕЦ ДОБАВЛЕНИЯ ---
        
        // Проверка валидности позиции
        bool isValid = CheckPlacementValidityWithGrid(targetPosition);
        canPlace = isValid;
        
        if (inventoryManager != null && inventoryManager.DebugSurfaceSystem)
        {
            Debug.Log($"[HandleValidDrag] Проверка валидности завершена:");
            Debug.Log($"[HandleValidDrag]   - isValid: {isValid}");
            Debug.Log($"[HandleValidDrag]   - canPlace: {canPlace}");
            Debug.Log($"[HandleValidDrag]   - Позиция: {targetPosition}");
        }
        
        UpdateVisualFeedback(isValid);

        // Обновляем подсказку
        if (navigationUI != null)
        {
            if (isValid)
            {
                navigationUI.ShowValidPlacementHint();
            }
            else
            {
                navigationUI.ShowInvalidPlacementHint();
            }
        }

        // Мгновенное движение без задержек
        previewInstance.transform.position = targetPosition;
                }



    private void HandleInvalidDrag()
    {
        canPlace = inventoryManager == null || !inventoryManager.EnableValidation; // true если валидация отключена, false если включена
        UpdateVisualFeedback(canPlace);
        
        // Обновляем подсказку
        if (navigationUI != null)
        {
            navigationUI.ShowInvalidPlacementHint();
        }
    }

    private bool CheckPlacementValidityWithGrid(Vector3 position)
    {
        if (inventoryManager == null) return true;
        
        if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
        {
            Debug.Log($"🔍 Проверка валидности размещения в позиции: {position}");
            Debug.Log($"   Поворот объекта: {previewInstance?.transform.rotation.eulerAngles}");
        }
        
        // Проверка границ пола
        bool floorBoundsValid = CheckFloorBounds(position);
        if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
        {
            Debug.Log($"   ✅ Границы пола: {(floorBoundsValid ? "ОК" : "❌ НЕ ПРОШЛА")}");
        }
        if (!floorBoundsValid)
        {
            if (inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[CheckPlacementValidityWithGrid] ❌ Границы пола не прошли проверку");
            return false;
        }
        
        // Проверка сетки
        bool gridValid = true;
        if (gridPlacement != null)
        {
            gridValid = gridPlacement.IsOnGrid(position);
            if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"   ✅ Сетка: {(gridValid ? "ОК" : "❌ НЕ ПРОШЛА")}");
            }
            if (!gridValid)
            {
                if (inventoryManager.DebugSurfaceSystem)
                    Debug.Log($"[CheckPlacementValidityWithGrid] ❌ Сетка не прошла проверку");
                return false;
            }
        }
        else
        {
            if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"   ✅ Сетка: НЕ НАСТРОЕНА (пропускаем)");
            }
        }
        
        // Проверка коллизий (если включена валидация)
        if (inventoryManager.EnableValidation && !inventoryManager.DisableCollisionCheck)
        {
            bool collisionValid = CheckPlacementValidity(position);
            if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"   ✅ Коллизии: {(collisionValid ? "ОК" : "❌ НЕ ПРОШЛА")}");
            }
            if (!collisionValid && inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[CheckPlacementValidityWithGrid] ❌ Коллизии не прошли проверку");
            return collisionValid;
        }
        else if (inventoryManager.DisableCollisionCheck)
        {
            if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"   ✅ Коллизии: ОТКЛЮЧЕНЫ (пропускаем)");
            }
        }
        else
        {
            if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"   ✅ Коллизии: ВАЛИДАЦИЯ ОТКЛЮЧЕНА (пропускаем)");
            }
        }
        
        if (inventoryManager.DebugSurfaceSystem)
            Debug.Log($"[CheckPlacementValidityWithGrid] ✅ Все проверки пройдены успешно");
        
        return true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"OnEndDrag: isDragging={isDragging}, previewInstance={previewInstance}, canPlace={canPlace}, item={item}");
        
        // Проверяем, что перетаскивание действительно активно
        if (!isDragging)
        {
            Debug.LogWarning("[OnEndDrag] Перетаскивание не активно, игнорируем");
            return;
        }
        
        if (previewInstance != null && canPlace && item != null)
        {
            Debug.Log("Условие размещения выполнено, размещаю предмет!");
            
            // Сохраняем копию item перед удалением из инвентаря
            Item itemToPlace = item;
            
            // Используем позицию превью вместо повторного raycast
            Vector3 placementPosition = previewInstance.transform.position;
            
            // Размещаем объект
            GameObject placedObject = Instantiate(itemToPlace.prefab, placementPosition, previewInstance.transform.rotation);
            Debug.Log("Instantiate выполнен!");
            Debug.Log($"Instantiate: {placedObject.name} at {placedObject.transform.position}");
            Debug.Log("Предмет успешно размещен на полу.");
            
            // Сохраняем оригинальный слой префаба для размещенного объекта
            int originalLayer = itemToPlace.prefab.layer;
            SetLayerRecursively(placedObject, originalLayer);
            Debug.Log($"✅ Сохранен оригинальный слой {LayerMask.LayerToName(originalLayer)} для размещенного объекта: {placedObject.name}");
                    
            // Настраиваем коллайдеры размещенного объекта
            SetupPlacedObjectColliders(placedObject);
            
            // Добавляем компонент PlacedItem для возможности удаления
            PlacedItem placedItemComponent = placedObject.GetComponent<PlacedItem>();
            if (placedItemComponent == null)
            {
                placedItemComponent = placedObject.AddComponent<PlacedItem>();
            }
            placedItemComponent.SetItemData(itemToPlace);
            
            // Убираем предмет из инвентаря - РАБОТАЕМ С НОВОЙ АРХИТЕКТУРОЙ
            bool itemRemoved = false;
            
            // 1. Пробуем использовать OptimizedInventoryManager
            var optimizedManager = FindFirstObjectByType<OptimizedInventoryManager>();
            if (optimizedManager != null)
            {
                // Конвертируем Item в IItem
                var itemWrapper = new ItemWrapper(itemToPlace);
                if (optimizedManager.HasItem(itemWrapper, 1))
                {
                    optimizedManager.RemoveItem(itemWrapper, 1);
                    itemRemoved = true;
                    Debug.Log($"Предмет {itemToPlace.itemName} убран из OptimizedInventoryManager");
                }
                else
                {
                    Debug.LogWarning($"Предмет {itemToPlace.itemName} не найден в OptimizedInventoryManager!");
                }
            }
            // 2. Fallback на старый InventoryManager
            else if (inventoryManager != null)
            {
                if (inventoryManager.HasItem(itemToPlace, 1))
                {
                    inventoryManager.RemoveItem(itemToPlace, 1);
                    itemRemoved = true;
                    Debug.Log($"Предмет {itemToPlace.itemName} убран из старого InventoryManager");
                }
                else
                {
                    Debug.LogWarning($"Предмет {itemToPlace.itemName} не найден в старом InventoryManager!");
                }
            }
            // 3. Fallback на InventoryManager.Instance
            else if (InventoryManager.Instance != null)
            {
                if (InventoryManager.Instance.HasItem(itemToPlace, 1))
                {
                    InventoryManager.Instance.RemoveItem(itemToPlace, 1);
                    itemRemoved = true;
                    Debug.Log($"Предмет {itemToPlace.itemName} убран из InventoryManager.Instance");
                }
            }
            else
            {
                Debug.LogError("Не найден ни один InventoryManager!");
            }

            
            // Проверяем результат удаления предмета
            if (!itemRemoved)
            {
                Debug.LogWarning($"Предмет {itemToPlace.itemName} не был удален из инвентаря, но размещение продолжается");
            }
            
            // Добавляем в счетчик размещенных предметов
            if (placedItemsCounter != null)
            {
                placedItemsCounter.AddPlacedItem(itemToPlace);
            }
            else
            {
                // Если ссылка потеряна, получаем экземпляр заново
                PlacedItemsCounter.GetInstance().AddPlacedItem(itemToPlace);
            }
            
            // Эффекты размещения
            if (inventoryManager != null && inventoryManager.EnableVisualEffects)
            {
                PlacementEffects effects = placedObject.GetComponent<PlacementEffects>();
                if (effects != null)
                {
                    effects.PlayPlacementEffect();
                }
            }
            
            // Воспроизводим звук размещения
            PlaySound(inventoryManager?.PlacementSound);
        }
        else
        {
            Debug.Log($"Условие размещения НЕ выполнено! previewInstance={previewInstance}, canPlace={canPlace}, item={item}");
            
            // Воспроизводим звук неудачного размещения
            PlaySound(inventoryManager?.InvalidPlacementSound);
        }
        
        // Вызываем событие окончания перетаскивания
        slotUI?.TriggerOnDragEnded();
        
        // Вызываем событие через интерфейс системы событий (успешное размещение)
        _eventSystem?.InvokeDragEnded(item?.ToIItem(), GetSlotIndex(), true);
        
        // Очистка после перетаскивания
        CleanupAfterDrag();
        
        // Скрываем подсказку
        if (navigationUI != null)
        {
            navigationUI.HideHint();
        }
    }

    private void CleanupAfterDrag()
    {
        Debug.Log("[CleanupAfterDrag] Начинаем очистку");
        
        // Восстанавливаем оригинальные материалы
        RestoreOriginalMaterials();
        
        // Отключаем обводку
        DisableOutline();
        
        // Уничтожаем превью и очищаем переменные
        if (previewInstance != null)
        {
            Debug.Log("[CleanupAfterDrag] Уничтожаем previewInstance");
            Destroy(previewInstance);
            previewInstance = null;
        }
        
        // Очищаем ссылки
        previewRenderers = null;
        originalMaterials = null;
        
        Debug.Log("[CleanupAfterDrag] Очистка завершена");
    }
    
    /// <summary>
    /// Отменяет перетаскивание и возвращает предмет в исходное состояние
    /// </summary>
    private void CancelDrag()
    {
        Debug.Log("Отмена перетаскивания");
        
        // Воспроизводим звук отмены
        PlaySound(inventoryManager?.CancelSound);
        
        // Очистка после перетаскивания
        CleanupAfterDrag();
        
        // Сбрасываем состояние перетаскивания
        isDragging = false;
        canPlace = true;
        isRotated90 = false;
        wasRightButtonPressed = false;
        wasMiddleButtonPressed = false;
        
        // Вызываем событие окончания перетаскивания (отмена)
        slotUI?.TriggerOnDragEnded();
        
        // Вызываем событие через интерфейс системы событий (отмена)
        _eventSystem?.InvokeDragEnded(item?.ToIItem(), GetSlotIndex(), false);
        
        // Восстанавливаем состояние слота
        if (item != null)
        {
            Debug.Log($"Предмет {item.itemName} возвращен в слот");
        }
        
        // Скрываем подсказку
        if (navigationUI != null)
        {
            navigationUI.HideHint();
        }
    }
    


    private void RestoreOriginalMaterials()
    {
        if (previewRenderers != null && originalMaterials != null)
        {
            for (int i = 0; i < previewRenderers.Length; i++)
            {
                if (previewRenderers[i] != null && originalMaterials[i] != null)
                {
                    previewRenderers[i].material = originalMaterials[i];
                }
            }
            }
        }
        
    private void DisableOutline()
    {
        // Отключаем обводку напрямую через FreeOutlineSettings
        var outlineSettings = Resources.FindObjectsOfTypeAll<FreeOutlineSettings>();
        if (outlineSettings.Length > 0)
        {
            foreach (var outline in outlineSettings[0].Outlines)
            {
                outline.SetActive(false);
            }
            Debug.Log("Обводка отключена");
        }
    }

    private void SetupPlacedObjectColliders(GameObject placedObject)
    {
        // Получаем все коллайдеры на размещенном объекте
        Collider[] colliders = placedObject.GetComponentsInChildren<Collider>();
        
        foreach (Collider col in colliders)
        {
            // Убираем триггер для размещенных объектов (нужна физическая коллизия)
            col.isTrigger = false;
            
            // Настраиваем Rigidbody если есть
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Фиксируем объект на месте
                rb.useGravity = false; // Отключаем гравитацию
            }
        }
        
        // Сохраняем оригинальный слой объекта (не изменяем его)
        string currentLayer = LayerMask.LayerToName(placedObject.layer);
        if (inventoryManager != null && inventoryManager.DebugCollisions)
        {
            Debug.Log($"[SetupPlacedObjectColliders] Объект {placedObject.name} остается на слое {currentLayer}");
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    
    #region Система слоев размещения
    
    /// <summary>
    /// Получить слой размещения для объекта
    /// </summary>
    private PlacementLayer GetPlacementLayer(GameObject obj)
    {
        if (inventoryManager == null || !inventoryManager.EnableLayerSystem) 
        {
            if (inventoryManager != null && inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[GetPlacementLayer] enableLayerSystem=false, возвращаю {inventoryManager.DefaultPlacementLayer}");
            return inventoryManager != null ? inventoryManager.DefaultPlacementLayer : PlacementLayer.Floor;
        }
        
        PlacementLayerComponent layerComponent = obj.GetComponent<PlacementLayerComponent>();
        if (layerComponent != null)
        {
            if (inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[GetPlacementLayer] Найден PlacementLayerComponent для {obj.name}, слой: {layerComponent.PlacementLayer}");
            return layerComponent.PlacementLayer;
        }
        
        // Если компонент не найден, используем слой по умолчанию
        if (inventoryManager.DebugSurfaceSystem)
            Debug.Log($"[GetPlacementLayer] PlacementLayerComponent не найден для {obj.name}, возвращаю {inventoryManager.DefaultPlacementLayer}");
        return inventoryManager.DefaultPlacementLayer;
    }
    
    /// <summary>
    /// Проверить коллизии с учетом слоев размещения
    /// </summary>
    private bool CheckLayerCollisions(Vector3 position, PlacementLayer itemLayer)
    {
        if (inventoryManager == null || !inventoryManager.EnableLayerSystem) 
        {
            if (inventoryManager != null && inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[CheckLayerCollisions] Система слоев отключена, возвращаю false");
            return false;
        }
        
        float checkRadius = GetCollisionCheckRadius();
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius, inventoryManager.CollisionCheckMask);
        
        if (inventoryManager.DebugSurfaceSystem)
        {
            Debug.Log($"[CheckLayerCollisions] Проверка коллизий слоев:");
            Debug.Log($"[CheckLayerCollisions]   - Позиция: {position}");
            Debug.Log($"[CheckLayerCollisions]   - Радиус: {checkRadius}");
            Debug.Log($"[CheckLayerCollisions]   - collisionCheckMask: {inventoryManager.CollisionCheckMask.value}");
            Debug.Log($"[CheckLayerCollisions]   - Слой предмета: {itemLayer}");
            Debug.Log($"[CheckLayerCollisions]   - Найдено коллайдеров: {colliders.Length}");
        }
        
        foreach (Collider col in colliders)
        {
            // Исключаем сам превью объект и его дочерние элементы
            if (col.gameObject == previewInstance || 
                col.transform.IsChildOf(previewInstance.transform) ||
                col.gameObject.layer == outlineLayer ||
                col.isTrigger ||
                IsTagIgnored(col.gameObject.tag) ||
                IsLayerIgnored(col.gameObject.layer))
            {
                if (inventoryManager.DebugSurfaceSystem)
                    Debug.Log($"[CheckLayerCollisions] Исключаем коллайдер: {col.gameObject.name} (причина: превью/слой/тег)");
                continue;
            }
            
            // Получаем слой объекта, с которым проверяем коллизию
            PlacementLayer objectLayer = GetPlacementLayer(col.gameObject);
            
            if (inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[CheckLayerCollisions] Проверяем объект: {col.gameObject.name}, слой: {objectLayer}");
            
            // Проверяем, может ли предмет размещаться на этом объекте
            if (!itemLayer.CanPlaceOn(objectLayer))
            {
                if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
                {
                    Debug.Log($"⚠️ Коллизия слоев: {itemLayer.GetDisplayName()} не может размещаться на {objectLayer.GetDisplayName()}");
                    Debug.Log($"   Объект: {col.gameObject.name}");
                }
                return true; // Есть коллизия
            }
            
            // Если это предметы одного слоя, проверяем обычную коллизию
            if (itemLayer == objectLayer)
            {
                if (inventoryManager.DebugCollisions || inventoryManager.DebugSurfaceSystem)
                {
                    Debug.Log($"⚠️ Коллизия в одном слое: {itemLayer.GetDisplayName()}");
                    Debug.Log($"   Объект: {col.gameObject.name}");
                }
                return true; // Есть коллизия
            }
            
            if (inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[CheckLayerCollisions] ✅ Объект {col.gameObject.name} прошел проверку слоев");
        }
        
        if (inventoryManager.DebugSurfaceSystem)
            Debug.Log($"[CheckLayerCollisions] ✅ Нет коллизий слоев");
        
        return false; // Нет коллизий
    }
    
    /// <summary>
    /// Найти подходящую поверхность для размещения
    /// </summary>
    private PlacementLayerComponent FindSuitableSurface(Vector3 position, PlacementLayer itemLayer)
    {
        if (inventoryManager == null || !inventoryManager.EnableLayerSystem) return null;
        
        float checkRadius = 20f; // Увеличил радиус для поиска поверхностей
        
        // Используем отдельную маску для поиска поверхностей
        LayerMask searchMask = inventoryManager.SurfaceCheckMask.value != 0 ? inventoryManager.SurfaceCheckMask : inventoryManager.CollisionCheckMask;
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius, searchMask);
        
        if (inventoryManager.DebugSurfaceSystem)
        {
            Debug.Log($"[SurfaceCheck] Ищем поверхность в радиусе {checkRadius} от позиции {position}");
            Debug.Log($"[SurfaceCheck] Найдено коллайдеров: {colliders.Length}");
            Debug.Log($"[SurfaceCheck] Используемая маска: {searchMask.value}");
        }
        
        // Специальная проверка для отладки - ищем все объекты с PlacementLayerComponent
        if (inventoryManager.DebugSurfaceSystem)
        {
            PlacementLayerComponent[] debugPlacementComponents = FindObjectsByType<PlacementLayerComponent>(FindObjectsSortMode.None);
            Debug.Log($"[SurfaceCheck] Найдено объектов с PlacementLayerComponent: {debugPlacementComponents.Length}");
            
            foreach (var component in debugPlacementComponents)
            {
                float distance = Vector3.Distance(position, component.transform.position);
                Collider col = component.GetComponent<Collider>();
                Debug.Log($"[SurfaceCheck] Объект: {component.gameObject.name}, Слой: {component.PlacementLayer}, IsSurface: {component.IsSurface}, Расстояние: {distance:F2}, Collider: {col != null}, Layer: {component.gameObject.layer}");
            }
            
            Debug.Log($"[SurfaceCheck] collisionCheckMask: {inventoryManager.CollisionCheckMask.value}, surfaceCheckMask: {inventoryManager.SurfaceCheckMask.value}");
            
            // Проверяем все объекты с PlacementLayerComponent на наличие коллайдеров
            foreach (var component in debugPlacementComponents)
            {
                if (component.IsSurface)
                {
                    Collider[] allColliders = component.GetComponents<Collider>();
                    Collider[] allCollidersInChildren = component.GetComponentsInChildren<Collider>();
                    Debug.Log($"[SurfaceCheck] Поверхность {component.gameObject.name}: Colliders={allColliders.Length}, CollidersInChildren={allCollidersInChildren.Length}");
                    
                    foreach (var col in allColliders)
                    {
                        Debug.Log($"[SurfaceCheck]   - {col.GetType().Name}: enabled={col.enabled}, isTrigger={col.isTrigger}, layer={col.gameObject.layer}");
                    }
                }
            }
        }
        
        // Сначала проверяем коллайдеры, найденные через OverlapSphere
        foreach (Collider col in colliders)
        {
            // Исключаем сам превью объект
            if (col.gameObject == previewInstance || 
                col.transform.IsChildOf(previewInstance.transform))
            {
                continue;
            }
            
            if (inventoryManager.DebugSurfaceSystem)
                Debug.Log($"[SurfaceCheck] Проверяю коллайдер: {col.gameObject.name}, Layer: {col.gameObject.layer}, Tag: {col.gameObject.tag}, Position: {col.transform.position}");
            
            PlacementLayerComponent surfaceComponent = col.GetComponent<PlacementLayerComponent>();
            if (surfaceComponent != null && surfaceComponent.IsSurface)
            {
                PlacementLayer surfaceLayer = surfaceComponent.PlacementLayer;
                bool canPlace = itemLayer.CanPlaceOn(surfaceLayer);
                bool onSurface = surfaceComponent.IsPositionOnSurface(position);
                
                if (inventoryManager.DebugSurfaceSystem)
                {
                    Debug.Log($"[SurfaceCheck] Найден PlacementLayerComponent: IsSurface={surfaceComponent.IsSurface}, PlacementLayer={surfaceLayer}, CanPlaceOn={canPlace}");
                    Debug.Log($"[SurfaceCheck] Проверка поверхности: {col.gameObject.name}");
                    Debug.Log($"[SurfaceCheck]   - SurfaceHeight: {surfaceComponent.SurfaceHeight}");
                    Debug.Log($"[SurfaceCheck]   - SurfaceSize: {surfaceComponent.SurfaceSize}");
                    Debug.Log($"[SurfaceCheck]   - Позиция предмета: {position}");
                    Debug.Log($"[SurfaceCheck]   - CanPlaceOn: {canPlace}");
                    Debug.Log($"[SurfaceCheck]   - OnSurface: {onSurface}");
                }
                
                // Проверяем, может ли предмет размещаться на этой поверхности
                if (canPlace)
                {
                    // Проверяем, находится ли позиция в пределах поверхности
                    if (onSurface)
                    {
                        if (inventoryManager.DebugSurfaceSystem)
                            Debug.Log($"✅ Найдена подходящая поверхность: {col.gameObject.name} (Layer: {surfaceLayer})");
                        return surfaceComponent;
                    }
                    else
                    {
                        if (inventoryManager.DebugSurfaceSystem)
                            Debug.Log($"[SurfaceCheck] ❌ Позиция {position} НЕ на поверхности {col.gameObject.name}");
                    }
                }
                else
                {
                    if (inventoryManager.DebugSurfaceSystem)
                        Debug.Log($"[SurfaceCheck] ❌ {itemLayer} не может размещаться на {surfaceLayer}");
                }
            }
            else
            {
                if (inventoryManager.DebugSurfaceSystem)
                    Debug.Log($"[SurfaceCheck] Объект {col.gameObject.name} не является поверхностью (PlacementLayerComponent={surfaceComponent != null}, IsSurface={surfaceComponent?.IsSurface})");
            }
        }
        
        // Если через OverlapSphere не нашли, попробуем найти все поверхности в радиусе
        if (inventoryManager.DebugSurfaceSystem)
            Debug.Log($"[SurfaceCheck] OverlapSphere не нашел поверхностей, ищем все поверхности в радиусе...");
        
        PlacementLayerComponent[] allPlacementComponents = FindObjectsByType<PlacementLayerComponent>(FindObjectsSortMode.None);
        foreach (var component in allPlacementComponents)
        {
            if (!component.IsSurface) continue;
            
            float distance = Vector3.Distance(position, component.transform.position);
            if (distance > checkRadius) continue;
            
            PlacementLayer surfaceLayer = component.PlacementLayer;
            bool canPlace = itemLayer.CanPlaceOn(surfaceLayer);
            bool onSurface = component.IsPositionOnSurface(position);
            
            if (inventoryManager.DebugSurfaceSystem)
            {
                Debug.Log($"[SurfaceCheck] Проверяю поверхность напрямую: {component.gameObject.name}");
                Debug.Log($"[SurfaceCheck]   - Расстояние: {distance:F2}");
                Debug.Log($"[SurfaceCheck]   - CanPlaceOn: {canPlace}");
                Debug.Log($"[SurfaceCheck]   - OnSurface: {onSurface}");
            }
            
            if (canPlace && onSurface)
            {
                if (inventoryManager.DebugSurfaceSystem)
                    Debug.Log($"✅ Найдена подходящая поверхность (прямой поиск): {component.gameObject.name} (Layer: {surfaceLayer})");
                return component;
            }
        }
        
        if (inventoryManager.DebugSurfaceSystem)
            Debug.Log($"[SurfaceCheck] Подходящая поверхность не найдена");
        
        return null;
    }
    
    /// <summary>
    /// Обновить визуальную обратную связь с учетом слоев
    /// </summary>
    private void UpdateLayerVisualFeedback(bool isValid, PlacementLayer itemLayer)
    {
        if (inventoryManager == null || !inventoryManager.EnableLayerSystem) return;
        
        Color targetColor;
        
        if (isValid)
        {
            // Используем цвет слоя для валидного размещения
            targetColor = itemLayer.GetLayerColor();
        }
        else
        {
            // Красный для невалидного размещения
            targetColor = inventoryManager.InvalidPlacementColor;
        }
        
        // Обновляем цвет объекта
        if (previewRenderers != null)
        {
            for (int i = 0; i < previewRenderers.Length; i++)
            {
                if (previewRenderers[i] != null)
                {
                    Material[] materials = previewRenderers[i].materials;
                    for (int j = 0; j < materials.Length; j++)
                    {
                        if (materials[j].HasProperty("_Color"))
                        {
                            materials[j].color = targetColor;
                        }
                    }
                }
            }
        }
    }
    
    #endregion
}
