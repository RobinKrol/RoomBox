using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Configuration;
using InventorySystem.BaseComponents;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Быстрое исправление настроек для предотвращения наложения предметов
    /// </summary>
    public class QuickFixOverlap : MonoBehaviour
    {
        [ContextMenu("🚨 БЫСТРОЕ ИСПРАВЛЕНИЕ НАЛОЖЕНИЙ")]
        public void QuickFixOverlapIssue()
        {
            Debug.Log("🚨 БЫСТРОЕ ИСПРАВЛЕНИЕ НАЛОЖЕНИЙ ПРЕДМЕТОВ");
            Debug.Log("==========================================");
            
            // 1. Исправляем OptimizedItemPlacementValidator
            FixOptimizedValidator();
            
            // 2. Исправляем старый InventoryManager
            FixOldInventoryManager();
            
            // 3. Проверяем слои
            CheckLayers();
            
            Debug.Log("✅ ИСПРАВЛЕНИЕ ЗАВЕРШЕНО!");
            Debug.Log("Теперь предметы не должны накладываться друг на друга.");
        }
        
        private void FixOptimizedValidator()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("❌ OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            Debug.Log("🔧 Исправляем OptimizedItemPlacementValidator...");
            
            // Создаем новую конфигурацию
            var config = new PlacementValidationConfig();
            
            // Настраиваем маску коллизий - Furniture + Surface (предметы, которые не должны пересекаться)
            int furnitureLayer = LayerMask.NameToLayer("Furniture");
            int surfaceLayer = LayerMask.NameToLayer("Surface");
            
            if (furnitureLayer == -1)
            {
                Debug.LogError("❌ Слой Furniture не найден! Создайте его в Unity.");
                return;
            }
            
            LayerMask collisionMask = (1 << furnitureLayer);
            if (surfaceLayer != -1)
                collisionMask |= (1 << surfaceLayer);
            
            // Настраиваем маску поверхностей - Surface + StaticEnvironment (для пола)
            int staticEnvLayer = LayerMask.NameToLayer("StaticEnvironment");
            LayerMask surfaceMask = 0;
            
            if (surfaceLayer != -1)
                surfaceMask |= (1 << surfaceLayer);
            if (staticEnvLayer != -1)
                surfaceMask |= (1 << staticEnvLayer);
                
            if (surfaceMask == 0)
                surfaceMask = (LayerMask)(-1); // Fallback на все слои
            
            // Устанавливаем настройки через reflection
            SetConfigField(config, "collisionCheckMask", collisionMask);
            SetConfigField(config, "surfaceCheckMask", surfaceMask);
            SetConfigField(config, "preventObjectOverlap", true);
            SetConfigField(config, "useStrictValidation", true);
            SetConfigField(config, "enableLayerSystem", true);
            SetConfigField(config, "collisionCheckRadius", 0.3f);
            SetConfigField(config, "overlapCheckMargin", 0.05f);
            
            // Устанавливаем конфигурацию
            SetValidatorConfig(validator, config);
            
            Debug.Log($"✅ OptimizedItemPlacementValidator исправлен:");
            Debug.Log($"  - CollisionCheckMask: {collisionMask} (Furniture + Surface)");
            Debug.Log($"  - SurfaceCheckMask: {surfaceMask} (Surface + StaticEnvironment)");
            Debug.Log($"  - PreventObjectOverlap: true");
            Debug.Log($"  - UseStrictValidation: true");
        }
        
        private void FixOldInventoryManager()
        {
            var oldManager = FindFirstObjectByType<InventoryManager>();
            if (oldManager == null)
            {
                Debug.Log("ℹ️ Старый InventoryManager не найден (это хорошо)");
                return;
            }
            
            Debug.Log("🔧 Исправляем старый InventoryManager...");
            
            // Настраиваем маску коллизий - Furniture + Surface (предметы, которые не должны пересекаться)
            int furnitureLayer = LayerMask.NameToLayer("Furniture");
            int surfaceLayer = LayerMask.NameToLayer("Surface");
            
            if (furnitureLayer == -1)
            {
                Debug.LogError("❌ Слой Furniture не найден!");
                return;
            }
            
            LayerMask collisionMask = (1 << furnitureLayer);
            if (surfaceLayer != -1)
                collisionMask |= (1 << surfaceLayer);
            
            // Настраиваем маску поверхностей - Surface + StaticEnvironment (для пола)
            int staticEnvLayer = LayerMask.NameToLayer("StaticEnvironment");
            LayerMask surfaceMask = 0;
            
            if (surfaceLayer != -1)
                surfaceMask |= (1 << surfaceLayer);
            if (staticEnvLayer != -1)
                surfaceMask |= (1 << staticEnvLayer);
                
            if (surfaceMask == 0)
                surfaceMask = (LayerMask)(-1); // Fallback на все слои
            
            // Устанавливаем настройки через reflection
            SetField(oldManager, "collisionCheckMask", collisionMask);
            SetField(oldManager, "surfaceCheckMask", surfaceMask);
            SetField(oldManager, "preventObjectOverlap", true);
            SetField(oldManager, "enableLayerSystem", true);
            SetField(oldManager, "collisionCheckRadius", 0.3f);
            SetField(oldManager, "overlapCheckMargin", 0.05f);
            
            Debug.Log($"✅ Старый InventoryManager исправлен:");
            Debug.Log($"  - CollisionCheckMask: {collisionMask} (Furniture + Surface)");
            Debug.Log($"  - SurfaceCheckMask: {surfaceMask} (Surface + StaticEnvironment)");
            Debug.Log($"  - PreventObjectOverlap: true");
        }
        
        private void CheckLayers()
        {
            Debug.Log("🔍 Проверяем слои...");
            
            int furnitureLayer = LayerMask.NameToLayer("Furniture");
            int surfaceLayer = LayerMask.NameToLayer("Surface");
            int staticEnvLayer = LayerMask.NameToLayer("StaticEnvironment");
            
            Debug.Log($"  - Furniture: {(furnitureLayer != -1 ? $"✅ {furnitureLayer}" : "❌ не найден")}");
            Debug.Log($"  - Surface: {(surfaceLayer != -1 ? $"✅ {surfaceLayer}" : "❌ не найден")}");
            Debug.Log($"  - StaticEnvironment: {(staticEnvLayer != -1 ? $"✅ {staticEnvLayer}" : "❌ не найден")}");
            
            if (furnitureLayer == -1)
            {
                Debug.LogError("❌ Создайте слой Furniture в Unity: Edit → Project Settings → Tags and Layers");
            }
        }
        
        private void SetConfigField(PlacementValidationConfig config, string fieldName, object value)
        {
            var field = typeof(PlacementValidationConfig).GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(config, value);
        }
        
        private void SetValidatorConfig(OptimizedItemPlacementValidator validator, PlacementValidationConfig config)
        {
            var configField = typeof(BaseInventoryComponent<PlacementValidationConfig>).GetField("configuration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(validator, config);
        }
        
        private void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public);
            field?.SetValue(obj, value);
        }
        
        [ContextMenu("📋 Проверить текущие настройки")]
        public void CheckCurrentSettings()
        {
            Debug.Log("📋 ТЕКУЩИЕ НАСТРОЙКИ:");
            
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator != null)
            {
                var config = validator.Configuration;
                Debug.Log($"OptimizedItemPlacementValidator:");
                Debug.Log($"  - PreventObjectOverlap: {config.PreventObjectOverlap}");
                Debug.Log($"  - UseStrictValidation: {config.UseStrictValidation}");
                Debug.Log($"  - CollisionCheckMask: {config.CollisionCheckMask}");
                Debug.Log($"  - SurfaceCheckMask: {config.SurfaceCheckMask}");
            }
            
            var oldManager = FindFirstObjectByType<InventoryManager>();
            if (oldManager != null)
            {
                Debug.Log($"Старый InventoryManager:");
                Debug.Log($"  - PreventObjectOverlap: {oldManager.PreventObjectOverlap}");
                Debug.Log($"  - CollisionCheckMask: {oldManager.CollisionCheckMask}");
                Debug.Log($"  - SurfaceCheckMask: {oldManager.SurfaceCheckMask}");
            }
        }
    }
}
