using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Configuration;
using InventorySystem.BaseComponents;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Быстрая настройка валидации для предотвращения пересечений предметов
    /// </summary>
    public class QuickValidatorSetup : MonoBehaviour
    {
        [ContextMenu("Включить предотвращение пересечений")]
        public void EnableOverlapPrevention()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            // Создаем новую конфигурацию с включенным предотвращением пересечений
            var config = new PlacementValidationConfig();
            
            // Устанавливаем настройки через reflection
            var preventOverlapField = typeof(PlacementValidationConfig).GetField("preventObjectOverlap", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            preventOverlapField?.SetValue(config, true);
            
            var collisionRadiusField = typeof(PlacementValidationConfig).GetField("collisionCheckRadius", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            collisionRadiusField?.SetValue(config, 0.5f);
            
            // Включаем систему слоев для поверхностей
            var enableLayerSystemField = typeof(PlacementValidationConfig).GetField("enableLayerSystem", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enableLayerSystemField?.SetValue(config, true);
            
            // Настраиваем маску для проверки поверхностей
            var surfaceCheckMaskField = typeof(PlacementValidationConfig).GetField("surfaceCheckMask", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            surfaceCheckMaskField?.SetValue(config, (LayerMask)(-1)); // Все слои
            
            // Включаем строгую валидацию для предотвращения пересечений
            var useStrictValidationField = typeof(PlacementValidationConfig).GetField("useStrictValidation", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            useStrictValidationField?.SetValue(config, true);
            
            var configField = typeof(BaseInventoryComponent<PlacementValidationConfig>).GetField("configuration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(validator, config);
            
            Debug.Log("✅ Предотвращение пересечений включено!");
            Debug.Log("✅ Система поверхностей включена!");
        }
        
        [ContextMenu("Проверить настройки")]
        public void CheckSettings()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            var config = validator.Configuration;
            Debug.Log($"📋 Настройки валидации:");
            Debug.Log($"   PreventObjectOverlap: {config.PreventObjectOverlap}");
            Debug.Log($"   CollisionCheckRadius: {config.CollisionCheckRadius}");
            Debug.Log($"   EnableValidation: {config.EnableValidation}");
            Debug.Log($"   UseStrictValidation: {config.UseStrictValidation}");
            Debug.Log($"   EnableLayerSystem: {config.EnableLayerSystem}");
            Debug.Log($"   SurfaceCheckMask: {config.SurfaceCheckMask}");
        }
        
        [ContextMenu("Восстановить систему поверхностей")]
        public void RestoreSurfaceSystem()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            // Создаем новую конфигурацию с полной поддержкой поверхностей
            var config = new PlacementValidationConfig();
            
            // Включаем все необходимые настройки
            var enableLayerSystemField = typeof(PlacementValidationConfig).GetField("enableLayerSystem", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enableLayerSystemField?.SetValue(config, true);
            
            var surfaceCheckMaskField = typeof(PlacementValidationConfig).GetField("surfaceCheckMask", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            surfaceCheckMaskField?.SetValue(config, (LayerMask)(-1));
            
            var preventOverlapField = typeof(PlacementValidationConfig).GetField("preventObjectOverlap", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            preventOverlapField?.SetValue(config, true);
            
            // Включаем строгую валидацию для предотвращения пересечений
            var useStrictValidationField = typeof(PlacementValidationConfig).GetField("useStrictValidation", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            useStrictValidationField?.SetValue(config, true);
            
            var configField = typeof(BaseInventoryComponent<PlacementValidationConfig>).GetField("configuration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(validator, config);
            
            Debug.Log("✅ Система поверхностей восстановлена!");
            Debug.Log("✅ Предотвращение пересечений включено!");
        }
    }
}
