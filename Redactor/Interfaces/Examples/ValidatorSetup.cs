using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Configuration;
using InventorySystem.BaseComponents;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Скрипт для настройки валидации размещения предметов
    /// </summary>
    public class ValidatorSetup : MonoBehaviour
    {
        [Header("Настройки валидации")]
        [SerializeField] private bool preventObjectOverlap = true;
        [SerializeField] private float collisionCheckRadius = 0.5f;
        [SerializeField] private LayerMask collisionCheckMask = -1;
        
        [ContextMenu("Настроить валидацию")]
        public void SetupValidator()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("OptimizedItemPlacementValidator не найден на сцене!");
                return;
            }
            
            // Создаем новую конфигурацию
            var config = new PlacementValidationConfig();
            
            // Настраиваем предотвращение пересечений
            var preventOverlapField = typeof(PlacementValidationConfig).GetField("preventObjectOverlap", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            preventOverlapField?.SetValue(config, preventObjectOverlap);
            
            // Настраиваем радиус проверки коллизий
            var collisionRadiusField = typeof(PlacementValidationConfig).GetField("collisionCheckRadius", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            collisionRadiusField?.SetValue(config, collisionCheckRadius);
            
            // Настраиваем маску коллизий
            var collisionMaskField = typeof(PlacementValidationConfig).GetField("collisionCheckMask", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            collisionMaskField?.SetValue(config, collisionCheckMask);
            
            // Настраиваем маску для проверки поверхностей
            var surfaceCheckMaskField = typeof(PlacementValidationConfig).GetField("surfaceCheckMask", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            surfaceCheckMaskField?.SetValue(config, (LayerMask)(-1));
            
            // Устанавливаем конфигурацию через reflection
            var configField = typeof(BaseInventoryComponent<PlacementValidationConfig>).GetField("configuration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(validator, config);
            
            Debug.Log($"Валидация настроена: PreventOverlap={preventObjectOverlap}, Radius={collisionCheckRadius}");
        }
        
        [ContextMenu("Проверить настройки валидации")]
        public void CheckValidatorSettings()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("OptimizedItemPlacementValidator не найден на сцене!");
                return;
            }
            
            var config = validator.Configuration;
            Debug.Log($"Текущие настройки валидации:");
            Debug.Log($"  - PreventObjectOverlap: {config.PreventObjectOverlap}");
            Debug.Log($"  - CollisionCheckRadius: {config.CollisionCheckRadius}");
            Debug.Log($"  - CollisionCheckMask: {config.CollisionCheckMask}");
            Debug.Log($"  - EnableValidation: {config.EnableValidation}");
        }
    }
}
