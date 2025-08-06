using UnityEngine;
using InventorySystem.BaseComponents;
using InventorySystem.Configuration;
using InventorySystem.Logging;
using System.Collections.Generic;

namespace InventorySystem.OptimizedComponents
{
    /// <summary>
    /// Оптимизированная система валидации размещения предметов
    /// </summary>
    public partial class OptimizedItemPlacementValidator : BaseInventoryComponent<PlacementValidationConfig>, IItemPlacementValidator
    {
        [Header("Ссылки")]
        [SerializeField] private InventoryManager inventoryManager;
        
        // Кэшированные компоненты для оптимизации
        private readonly Dictionary<Collider, bool> ignoredCollidersCache = new Dictionary<Collider, bool>();
        private readonly List<Collider> tempColliders = new List<Collider>();
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Автоматически находим InventoryManager если не назначен
            if (inventoryManager == null)
            {
                inventoryManager = FindRequiredComponent<InventoryManager>();
            }
            
            // Применяем конфигурацию
            ApplyConfiguration(configuration);
            
            LogDebug("Оптимизированная система валидации инициализирована");
        }
        
        protected override PlacementValidationConfig CreateDefaultConfiguration()
        {
            return new PlacementValidationConfig();
        }
        
        public bool CanPlaceItem(IItem item, Vector3 position, Quaternion rotation)
        {
            return SafeExecute(() =>
            {
                if (!configuration.EnableValidation)
                {
                    LogDebug("Валидация отключена, возвращаю true");
                    return true;
                }
                
                if (item == null)
                {
                    LogWarning("Предмет null, возвращаю false");
                    return false;
                }
                
                LogDebug($"Проверяем размещение {item.ItemName} в позиции {position}");
                
                bool result = configuration.UseStrictValidation 
                    ? CheckPlacementValidityStrict(item, position, rotation)
                    : CheckPlacementValidityTouching(item, position, rotation);
                
                LogDebug($"Результат валидации: {result}");
                return result;
            }, false, "валидации размещения");
        }
        
        public PlacementValidationResult ValidatePlacement(IItem item, Vector3 position, Quaternion rotation)
        {
            return SafeExecute(() =>
            {
                var result = new PlacementValidationResult
                {
                    IsValid = false,
                    ErrorType = PlacementErrorType.None,
                    ErrorMessage = "Неизвестная ошибка"
                };
                
                if (!configuration.EnableValidation)
                {
                    result.IsValid = true;
                    result.ErrorType = PlacementErrorType.None;
                    result.ErrorMessage = "Валидация отключена";
                    return result;
                }
                
                if (item == null)
                {
                    result.ErrorType = PlacementErrorType.InvalidItem;
                    result.ErrorMessage = "Предмет не может быть null";
                    return result;
                }
                
                // Проверяем границы пола
                if (configuration.CheckFloorBounds && !CheckFloorBounds(item, position, rotation))
                {
                    result.ErrorType = PlacementErrorType.OutOfBounds;
                    result.ErrorMessage = "Предмет выходит за границы пола";
                    return result;
                }
                
                // Проверяем коллизии
                if (!CheckCollisions(item, position, rotation))
                {
                    result.ErrorType = PlacementErrorType.Collision;
                    result.ErrorMessage = "Обнаружена коллизия с другими объектами";
                    return result;
                }
                
                // Проверяем валидность поверхности
                if (!CheckSurfaceValidity(item, position, rotation))
                {
                    result.ErrorType = PlacementErrorType.InvalidSurface;
                    result.ErrorMessage = "Невалидная поверхность для размещения";
                    return result;
                }
                
                // Проверяем наложение объектов
                if (configuration.PreventObjectOverlap && !CheckObjectOverlap(item, position, rotation))
                {
                    result.ErrorType = PlacementErrorType.ObjectOverlap;
                    result.ErrorMessage = "Обнаружено наложение с другими объектами";
                    return result;
                }
                
                result.IsValid = true;
                result.ErrorType = PlacementErrorType.None;
                result.ErrorMessage = "Размещение валидно";
                
                return result;
            }, new PlacementValidationResult { IsValid = false, ErrorType = PlacementErrorType.None, ErrorMessage = "Ошибка валидации" }, "валидации размещения");
        }
        
        public Vector3? GetValidPlacementPosition(IItem item, Vector3 desiredPosition, Quaternion rotation)
        {
            return SafeExecute(() =>
            {
                if (CanPlaceItem(item, desiredPosition, rotation))
                {
                    return desiredPosition;
                }
                
                // Попытка найти ближайшую валидную позицию
                var testPositions = GenerateTestPositions(desiredPosition, 1f, 8);
                foreach (var testPos in testPositions)
                {
                    if (CanPlaceItem(item, testPos, rotation))
                    {
                        return testPos;
                    }
                }
                
                return (Vector3?)null;
            }, null, "поиска валидной позиции");
        }
        
        public PlacementVisualFeedback GetVisualFeedback(IItem item, Vector3 position, Quaternion rotation)
        {
            return SafeExecute(() =>
            {
                var feedback = new PlacementVisualFeedback
                {
                    IsValid = false,
                    Color = configuration.InvalidPlacementColor,
                    Message = "Неизвестное состояние"
                };
                
                if (!configuration.EnableValidation)
                {
                    feedback.IsValid = true;
                    feedback.Color = configuration.ValidPlacementColor;
                    feedback.Message = "Валидация отключена";
                    return feedback;
                }
                
                if (CanPlaceItem(item, position, rotation))
                {
                    feedback.IsValid = true;
                    feedback.Color = configuration.ValidPlacementColor;
                    feedback.Message = "Размещение возможно";
                }
                else
                {
                    feedback.IsValid = false;
                    feedback.Color = configuration.InvalidPlacementColor;
                    feedback.Message = "Размещение невозможно";
                }
                
                return feedback;
            }, new PlacementVisualFeedback { IsValid = false, Color = Color.red, Message = "Ошибка получения обратной связи" }, "получения визуальной обратной связи");
        }
        
        // Продолжение в следующем файле...
    }
} 