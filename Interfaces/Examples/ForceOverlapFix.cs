using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Configuration;
using InventorySystem.BaseComponents;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Принудительное исправление настроек валидатора для предотвращения наложений
    /// </summary>
    public class ForceOverlapFix : MonoBehaviour
    {
        [ContextMenu("🚨 ПРИНУДИТЕЛЬНОЕ ИСПРАВЛЕНИЕ")]
        public void ForceFixOverlap()
        {
            Debug.Log("🚨 ПРИНУДИТЕЛЬНОЕ ИСПРАВЛЕНИЕ НАЛОЖЕНИЙ");
            Debug.Log("=====================================");
            
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("❌ OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            Debug.Log("🔧 Принудительно исправляем настройки...");
            
            // Создаем новую конфигурацию с правильными настройками
            var config = new PlacementValidationConfig();
            
            // КРИТИЧЕСКИЕ НАСТРОЙКИ
            SetConfigField(config, "preventObjectOverlap", true);
            SetConfigField(config, "useStrictValidation", true);
            SetConfigField(config, "enableLayerSystem", true);
            SetConfigField(config, "collisionCheckRadius", 0.3f);
            SetConfigField(config, "overlapCheckMargin", 0.05f);
            
            // НАСТРОЙКИ МАСОК
            int furnitureLayer = LayerMask.NameToLayer("Furniture");
            int surfaceLayer = LayerMask.NameToLayer("Surface");
            int staticEnvLayer = LayerMask.NameToLayer("StaticEnvironment");
            
            if (furnitureLayer == -1 || surfaceLayer == -1)
            {
                Debug.LogError("❌ Критические слои не найдены!");
                return;
            }
            
            // CollisionCheckMask = Furniture + Surface (предметы, которые не должны пересекаться)
            LayerMask collisionMask = (1 << furnitureLayer) | (1 << surfaceLayer);
            SetConfigField(config, "collisionCheckMask", collisionMask);
            
            // SurfaceCheckMask = Surface + StaticEnvironment (для пола)
            LayerMask surfaceMask = (1 << surfaceLayer) | (1 << staticEnvLayer);
            SetConfigField(config, "surfaceCheckMask", surfaceMask);
            
            // ПРИНУДИТЕЛЬНО УСТАНАВЛИВАЕМ КОНФИГУРАЦИЮ
            SetValidatorConfig(validator, config);
            
            // ПРОВЕРЯЕМ, ЧТО НАСТРОЙКИ ПРИМЕНИЛИСЬ
            var appliedConfig = validator.Configuration;
            Debug.Log("📋 ПРОВЕРКА ПРИМЕНЕННЫХ НАСТРОЕК:");
            Debug.Log($"  - PreventObjectOverlap: {appliedConfig.PreventObjectOverlap}");
            Debug.Log($"  - UseStrictValidation: {appliedConfig.UseStrictValidation}");
            Debug.Log($"  - EnableLayerSystem: {appliedConfig.EnableLayerSystem}");
            Debug.Log($"  - CollisionCheckMask: {appliedConfig.CollisionCheckMask}");
            Debug.Log($"  - SurfaceCheckMask: {appliedConfig.SurfaceCheckMask}");
            
            // КРИТИЧЕСКАЯ ПРОВЕРКА
            if (!appliedConfig.PreventObjectOverlap)
            {
                Debug.LogError("❌ КРИТИЧЕСКАЯ ОШИБКА: PreventObjectOverlap все еще false!");
                Debug.LogError("Попробуйте перезапустить игру или пересоздать валидатор!");
            }
            else
            {
                Debug.Log("✅ PreventObjectOverlap успешно установлен в true!");
            }
            
            if (!appliedConfig.UseStrictValidation)
            {
                Debug.LogError("❌ КРИТИЧЕСКАЯ ОШИБКА: UseStrictValidation все еще false!");
            }
            else
            {
                Debug.Log("✅ UseStrictValidation успешно установлен в true!");
            }
            
            Debug.Log("✅ ПРИНУДИТЕЛЬНОЕ ИСПРАВЛЕНИЕ ЗАВЕРШЕНО!");
            Debug.Log("Теперь предметы не должны накладываться друг на друга.");
        }
        
        [ContextMenu("🧪 ТЕСТ ВАЛИДАЦИИ")]
        public void TestValidation()
        {
            Debug.Log("🧪 ТЕСТ ВАЛИДАЦИИ ПОСЛЕ ИСПРАВЛЕНИЯ");
            
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("❌ Валидатор не найден!");
                return;
            }
            
            // Создаем тестовый предмет
            var testItem = InventorySystem.Factories.InventoryFactory.CreateTestItem();
            
            // Тестируем в нескольких позициях
            Vector3[] testPositions = {
                Vector3.zero,
                new Vector3(1, 0, 1),
                new Vector3(-1, 0, -1)
            };
            
            foreach (var pos in testPositions)
            {
                var result = validator.CanPlaceItem(testItem, pos, Quaternion.identity);
                Debug.Log($"Позиция {pos}: {result}");
            }
        }
        
        [ContextMenu("📊 ДЕТАЛЬНАЯ ДИАГНОСТИКА")]
        public void DetailedDiagnostics()
        {
            Debug.Log("📊 ДЕТАЛЬНАЯ ДИАГНОСТИКА ВАЛИДАТОРА");
            
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("❌ Валидатор не найден!");
                return;
            }
            
            var config = validator.Configuration;
            
            // Проверяем все поля конфигурации через reflection
            var fields = typeof(PlacementValidationConfig).GetFields(
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public);
            
            Debug.Log("📋 ВСЕ ПОЛЯ КОНФИГУРАЦИИ:");
            foreach (var field in fields)
            {
                var value = field.GetValue(config);
                Debug.Log($"  - {field.Name}: {value}");
            }
            
            // Проверяем, что валидатор действительно использует эту конфигурацию
            var configField = typeof(BaseInventoryComponent<PlacementValidationConfig>).GetField("configuration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (configField != null)
            {
                var actualConfig = configField.GetValue(validator);
                Debug.Log($"Активная конфигурация: {actualConfig}");
                
                if (actualConfig != config)
                {
                    Debug.LogWarning("⚠️ Валидатор использует другую конфигурацию!");
                }
            }
        }
        
        private void SetConfigField(PlacementValidationConfig config, string fieldName, object value)
        {
            var field = typeof(PlacementValidationConfig).GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public);
            
            if (field != null)
            {
                field.SetValue(config, value);
                Debug.Log($"✅ Установлено {fieldName} = {value}");
            }
            else
            {
                Debug.LogError($"❌ Не удалось найти поле {fieldName}");
            }
        }
        
        private void SetValidatorConfig(OptimizedItemPlacementValidator validator, PlacementValidationConfig config)
        {
            var configField = typeof(BaseInventoryComponent<PlacementValidationConfig>).GetField("configuration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (configField != null)
            {
                configField.SetValue(validator, config);
                Debug.Log("✅ Конфигурация установлена в валидатор");
            }
            else
            {
                Debug.LogError("❌ Не удалось установить конфигурацию в валидатор");
            }
        }
    }
}
