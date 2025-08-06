using UnityEngine;
using System.Collections.Generic;
using static InventoryManagerTests; // Для доступа к MockItem

/// <summary>
/// Система валидации размещения предметов в мире
/// </summary>
public class ItemPlacementValidator : MonoBehaviour, IItemPlacementValidator
{
    [Header("Ссылки")]
    [SerializeField] private InventoryManager inventoryManager;
    
    [Header("Настройки валидации")]
    [SerializeField] private bool enableValidation = true;
    [SerializeField] private bool useStrictValidation = false;
    [SerializeField] private bool enableDebugLogging = true;
    
    [Header("Настройки коллизий")]
    [SerializeField] private float collisionCheckRadius = 0.3f;
    [SerializeField] private LayerMask collisionCheckMask = -1;
    [SerializeField] private LayerMask surfaceCheckMask = -1;
    
    [Header("Настройки границ")]
    [SerializeField] private bool checkFloorBounds = true;
    [SerializeField] private float floorBoundsMargin = 0.01f;
    
    [Header("Настройки наложения")]
    [SerializeField] private bool preventObjectOverlap = true;
    [SerializeField] private float overlapCheckMargin = 0.02f;
    
    [Header("Визуальная обратная связь")]
    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;
    [SerializeField] private Color overlapWarningColor = new Color(1f, 0.5f, 0f, 1f);
    
    private Camera mainCamera;
    private GameObject previewInstance; // Ссылка на превью объект для исключения из проверок
    
    void Awake()
    {
        // Автоматически находим InventoryManager если не назначен
        if (inventoryManager == null)
        {
            inventoryManager = Object.FindFirstObjectByType<InventoryManager>();
            if (inventoryManager == null)
            {
                Debug.LogError("[ItemPlacementValidator] InventoryManager не найден!");
            }
        }
        
        mainCamera = Camera.main;
        
        if (enableDebugLogging)
        {
            Debug.Log("[ItemPlacementValidator] Система валидации инициализирована");
        }
    }
    
    /// <summary>
    /// Установить превью объект для исключения из проверок
    /// </summary>
    public void SetPreviewInstance(GameObject preview)
    {
        previewInstance = preview;
    }
    
    public bool CanPlaceItem(IItem item, Vector3 position, Quaternion rotation)
    {
        if (!enableValidation)
        {
            if (enableDebugLogging)
                Debug.Log("[ItemPlacementValidator] Валидация отключена, возвращаю true");
            return true;
        }
        
        if (item == null)
        {
            if (enableDebugLogging)
                Debug.LogWarning("[ItemPlacementValidator] Предмет null, возвращаю false");
            return false;
        }
        
        if (enableDebugLogging)
            Debug.Log($"[ItemPlacementValidator] Проверяем размещение {item.ItemName} в позиции {position}");
        
        bool result;
        if (useStrictValidation)
        {
            result = CheckPlacementValidityStrict(item, position, rotation);
        }
        else
        {
            result = CheckPlacementValidityTouching(item, position, rotation);
        }
        
        if (enableDebugLogging)
            Debug.Log($"[ItemPlacementValidator] Результат валидации: {result}");
        
        return result;
    }
    
    public PlacementValidationResult ValidatePlacement(IItem item, Vector3 position, Quaternion rotation)
    {
        var result = new PlacementValidationResult
        {
            IsValid = false,
            ErrorType = PlacementErrorType.None,
            ErrorMessage = "Неизвестная ошибка"
        };
        
        if (!enableValidation)
        {
            result.IsValid = true;
            result.ErrorType = PlacementErrorType.None;
            result.ErrorMessage = "";
            return result;
        }
        
        if (item == null)
        {
            result.ErrorType = PlacementErrorType.None;
            result.ErrorMessage = "Предмет не может быть null";
            return result;
        }
        
        // Проверяем границы пола
        if (checkFloorBounds && !CheckFloorBounds(item, position, rotation))
        {
            result.ErrorType = PlacementErrorType.OutOfBounds;
            result.ErrorMessage = "Предмет выходит за границы пола";
            return result;
        }
        
        // Проверяем коллизии
        if (!CheckCollisions(item, position, rotation))
        {
            result.ErrorType = PlacementErrorType.Collision;
            result.ErrorMessage = "Обнаружены коллизии";
            return result;
        }
        
        // Проверяем наложение объектов
        if (preventObjectOverlap && CheckObjectOverlap(item, position, rotation))
        {
            result.ErrorType = PlacementErrorType.Overlapping;
            result.ErrorMessage = "Объекты накладываются друг на друга";
            return result;
        }
        
        // Проверяем поверхность
        if (!CheckSurfaceValidity(item, position, rotation))
        {
            result.ErrorType = PlacementErrorType.InvalidSurface;
            result.ErrorMessage = "Нельзя разместить на этой поверхности";
            return result;
        }
        
        result.IsValid = true;
        result.ErrorType = PlacementErrorType.None;
        result.ErrorMessage = "";
        return result;
    }
    
    public Vector3? GetValidPlacementPosition(IItem item, Vector3 desiredPosition, Quaternion rotation)
    {
        if (CanPlaceItem(item, desiredPosition, rotation))
        {
            return desiredPosition;
        }
        
        // Попытка найти ближайшую валидную позицию
        Vector3[] testPositions = GenerateTestPositions(desiredPosition, 1f, 8);
        
        foreach (Vector3 testPos in testPositions)
        {
            if (CanPlaceItem(item, testPos, rotation))
            {
                if (enableDebugLogging)
                    Debug.Log($"[ItemPlacementValidator] Найдена валидная позиция: {testPos}");
                return testPos;
            }
        }
        
        if (enableDebugLogging)
            Debug.LogWarning("[ItemPlacementValidator] Не найдена валидная позиция");
        return null;
    }
    
    public PlacementVisualFeedback GetVisualFeedback(IItem item, Vector3 position, Quaternion rotation)
    {
        var validationResult = ValidatePlacement(item, position, rotation);
        
        var feedback = new PlacementVisualFeedback
        {
            IsValid = validationResult.IsValid,
            Message = validationResult.ErrorMessage,
            ShowWarning = false
        };
        
        if (validationResult.IsValid)
        {
            feedback.Color = validPlacementColor;
        }
        else
        {
            switch (validationResult.ErrorType)
            {
                case PlacementErrorType.Overlapping:
                    feedback.Color = overlapWarningColor;
                    feedback.ShowWarning = true;
                    break;
                default:
                    feedback.Color = invalidPlacementColor;
                    break;
            }
        }
        
        return feedback;
    }
    
    // Приватные методы валидации
    
    private bool CheckPlacementValidityTouching(IItem item, Vector3 position, Quaternion rotation)
    {
        if (enableDebugLogging)
            Debug.Log($"[ItemPlacementValidator] CheckPlacementValidityTouching для {item.ItemName} в позиции {position}");
        
        // Проверяем границы пола
        if (checkFloorBounds && !CheckFloorBounds(item, position, rotation))
        {
            if (enableDebugLogging)
                Debug.Log("[ItemPlacementValidator] ❌ Границы пола не прошли проверку");
            return false;
        }
        
        // Проверяем коллизии
        if (!CheckCollisions(item, position, rotation))
        {
            if (enableDebugLogging)
                Debug.Log("[ItemPlacementValidator] ❌ Коллизии не прошли проверку");
            return false;
        }
        
        // Проверяем поверхность
        if (!CheckSurfaceValidity(item, position, rotation))
        {
            if (enableDebugLogging)
                Debug.Log("[ItemPlacementValidator] ❌ Поверхность не прошла проверку");
            return false;
        }
        
        if (enableDebugLogging)
            Debug.Log("[ItemPlacementValidator] ✅ Все проверки пройдены успешно");
        return true;
    }
    
    private bool CheckPlacementValidityStrict(IItem item, Vector3 position, Quaternion rotation)
    {
        if (enableDebugLogging)
            Debug.Log($"[ItemPlacementValidator] CheckPlacementValidityStrict для {item.ItemName} в позиции {position}");
        
        // Строгая проверка - все условия должны быть выполнены
        bool floorBoundsValid = !checkFloorBounds || CheckFloorBounds(item, position, rotation);
        bool collisionsValid = CheckCollisions(item, position, rotation);
        bool surfaceValid = CheckSurfaceValidity(item, position, rotation);
        bool overlapValid = !preventObjectOverlap || !CheckObjectOverlap(item, position, rotation);
        
        bool result = floorBoundsValid && collisionsValid && surfaceValid && overlapValid;
        
        if (enableDebugLogging)
        {
            Debug.Log($"[ItemPlacementValidator] Результаты строгой проверки:");
            Debug.Log($"  - Границы пола: {floorBoundsValid}");
            Debug.Log($"  - Коллизии: {collisionsValid}");
            Debug.Log($"  - Поверхность: {surfaceValid}");
            Debug.Log($"  - Наложение: {overlapValid}");
            Debug.Log($"  - Итоговый результат: {result}");
        }
        
        return result;
    }
    
    private bool CheckFloorBounds(IItem item, Vector3 position, Quaternion rotation)
    {
        if (!checkFloorBounds) return true;
        
        // Получаем размеры предмета
        Vector3 itemSize = GetItemSize(item, rotation);
        
        // Проверяем, что предмет находится в пределах границ пола с учетом отступа
        // Упрощенная проверка - можно расширить для реальных границ
        if (enableDebugLogging)
        {
            Debug.Log($"[ItemPlacementValidator] Проверка границ пола: позиция={position}, размер={itemSize}, отступ={floorBoundsMargin}");
        }
        
        // Здесь можно добавить более сложную логику проверки границ
        // Например, проверка выхода за границы игровой области
        return true; // Упрощенная проверка
    }
    
    private bool CheckCollisions(IItem item, Vector3 position, Quaternion rotation)
    {
        if (inventoryManager == null) return true;
        
        float checkRadius = GetCollisionCheckRadius(item);
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius, collisionCheckMask);
        
        if (enableDebugLogging)
            Debug.Log($"[ItemPlacementValidator] Найдено {colliders.Length} коллайдеров в радиусе {checkRadius}");
        
        foreach (Collider col in colliders)
        {
            // Пропускаем игнорируемые объекты
            if (ShouldIgnoreCollider(col))
                continue;
            
            if (enableDebugLogging)
                Debug.Log($"[ItemPlacementValidator] ❌ Обнаружена коллизия с {col.gameObject.name}");
            return false;
        }
        
        return true;
    }
    
    private bool CheckSurfaceValidity(IItem item, Vector3 position, Quaternion rotation)
    {
        if (inventoryManager == null) return true;
        
        // Проверяем, есть ли подходящая поверхность под предметом
        Vector3 checkPosition = position + Vector3.down * 0.1f;
        Collider[] surfaceColliders = Physics.OverlapSphere(checkPosition, 0.1f, surfaceCheckMask);
        
        if (enableDebugLogging)
            Debug.Log($"[ItemPlacementValidator] Найдено {surfaceColliders.Length} поверхностей");
        
        // Исключаем превью объект из проверки
        foreach (Collider col in surfaceColliders)
        {
            if (previewInstance != null && (col.gameObject == previewInstance || 
                col.transform.IsChildOf(previewInstance.transform)))
            {
                continue;
            }
            
            // Если нашли хотя бы одну валидную поверхность, возвращаем true
            return true;
        }
        
        return false;
    }
    
    private bool CheckObjectOverlap(IItem item, Vector3 position, Quaternion rotation)
    {
        if (!preventObjectOverlap) return false;
        
        Vector3 itemSize = GetItemSize(item, rotation);
        Vector3 checkSize = itemSize + Vector3.one * overlapCheckMargin;
        
        Collider[] overlappingColliders = Physics.OverlapBox(position, checkSize / 2f, rotation);
        
        if (enableDebugLogging)
            Debug.Log($"[ItemPlacementValidator] Найдено {overlappingColliders.Length} перекрывающихся объектов");
        
        foreach (Collider col in overlappingColliders)
        {
            if (ShouldIgnoreCollider(col))
                continue;
            
            if (enableDebugLogging)
                Debug.Log($"[ItemPlacementValidator] ❌ Обнаружено наложение с {col.gameObject.name}");
            return true;
        }
        
        return false;
    }
    
    private bool ShouldIgnoreCollider(Collider col)
    {
        if (col == null) return true;
        
        // Игнорируем триггеры
        if (col.isTrigger) return true;
        
        // Игнорируем объекты с определенными тегами
        if (col.CompareTag("RoomBoxFloor")) return true;
        
        // Игнорируем объекты на определенных слоях
        if (inventoryManager != null && (inventoryManager.IgnoredLayers.value & (1 << col.gameObject.layer)) != 0)
            return true;
        
        return false;
    }
    
    private float GetCollisionCheckRadius(IItem item)
    {
        if (inventoryManager != null)
        {
            return inventoryManager.CollisionCheckRadius;
        }
        return collisionCheckRadius;
    }
    
    private Vector3 GetItemSize(IItem item, Quaternion rotation)
    {
        // Упрощенная реализация - можно расширить для получения реальных размеров
        return Vector3.one * 0.5f;
    }
    
    private Vector3[] GenerateTestPositions(Vector3 center, float radius, int count)
    {
        Vector3[] positions = new Vector3[count];
        float angleStep = 360f / count;
        
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            positions[i] = center + offset;
        }
        
        return positions;
    }
    
    // Методы для отладки
    
    [ContextMenu("Тест валидации")]
    public void TestValidation()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[ItemPlacementValidator] Камера не найдена для теста");
            return;
        }
        
        Vector3 testPosition = mainCamera.transform.position + mainCamera.transform.forward * 5f;
        testPosition.y = 0.5f;
        
        // Создаем тестовый предмет
        var testItem = new MockItem("test_item", "TestItem", 1);
        
        bool canPlace = CanPlaceItem(testItem, testPosition, Quaternion.identity);
        var validationResult = ValidatePlacement(testItem, testPosition, Quaternion.identity);
        var visualFeedback = GetVisualFeedback(testItem, testPosition, Quaternion.identity);
        
        Debug.Log($"[ItemPlacementValidator] Тест валидации:");
        Debug.Log($"  - Позиция: {testPosition}");
        Debug.Log($"  - Можно разместить: {canPlace}");
        Debug.Log($"  - Валидность: {validationResult.IsValid}");
        Debug.Log($"  - Ошибка: {validationResult.ErrorMessage}");
        Debug.Log($"  - Цвет обратной связи: {visualFeedback.Color}");
    }
} 