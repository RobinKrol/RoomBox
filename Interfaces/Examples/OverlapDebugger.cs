using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Configuration;
using InventorySystem.BaseComponents;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Диагностический скрипт для отладки наложений предметов
    /// </summary>
    public class OverlapDebugger : MonoBehaviour
    {
        [Header("Настройки диагностики")]
        [SerializeField] private Vector3 testPosition = Vector3.zero;
        [SerializeField] private float testRadius = 1f;
        [SerializeField] private bool showGizmos = true;
        
        [ContextMenu("🔍 ДИАГНОСТИКА НАЛОЖЕНИЙ")]
        public void DebugOverlaps()
        {
            Debug.Log("🔍 ДИАГНОСТИКА НАЛОЖЕНИЙ ПРЕДМЕТОВ");
            Debug.Log("=====================================");
            
            // 1. Проверяем настройки валидатора
            CheckValidatorSettings();
            
            // 2. Проверяем объекты на сцене
            CheckSceneObjects();
            
            // 3. Тестируем наложения в указанной позиции
            TestOverlapAtPosition();
            
            // 4. Проверяем реальную работу валидатора
            TestValidatorWork();
            
            Debug.Log("✅ ДИАГНОСТИКА ЗАВЕРШЕНА");
        }
        
        private void CheckValidatorSettings()
        {
            Debug.Log("📋 ПРОВЕРКА НАСТРОЕК ВАЛИДАТОРА:");
            
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("❌ OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            var config = validator.Configuration;
            Debug.Log($"  - PreventObjectOverlap: {config.PreventObjectOverlap}");
            Debug.Log($"  - UseStrictValidation: {config.UseStrictValidation}");
            Debug.Log($"  - EnableLayerSystem: {config.EnableLayerSystem}");
            Debug.Log($"  - CollisionCheckMask: {config.CollisionCheckMask}");
            Debug.Log($"  - SurfaceCheckMask: {config.SurfaceCheckMask}");
            Debug.Log($"  - CollisionCheckRadius: {config.CollisionCheckRadius}");
            Debug.Log($"  - OverlapCheckMargin: {config.OverlapCheckMargin}");
            
            // Показываем какие слои включены в маски
            Debug.Log($"  Слои в CollisionCheckMask:");
            for (int i = 0; i < 32; i++)
            {
                if (((1 << i) & config.CollisionCheckMask) != 0)
                {
                    string layerName = LayerMask.LayerToName(i);
                    Debug.Log($"    - {layerName} (индекс: {i})");
                }
            }
            
            Debug.Log($"  Слои в SurfaceCheckMask:");
            for (int i = 0; i < 32; i++)
            {
                if (((1 << i) & config.SurfaceCheckMask) != 0)
                {
                    string layerName = LayerMask.LayerToName(i);
                    Debug.Log($"    - {layerName} (индекс: {i})");
                }
            }
            
            // КРИТИЧЕСКАЯ ПРОВЕРКА
            if (!config.PreventObjectOverlap)
            {
                Debug.LogError("❌ КРИТИЧЕСКАЯ ОШИБКА: PreventObjectOverlap = false!");
                Debug.LogError("Это означает, что наложения НЕ ПРЕДОТВРАЩАЮТСЯ!");
            }
            
            if (!config.UseStrictValidation)
            {
                Debug.LogError("❌ КРИТИЧЕСКАЯ ОШИБКА: UseStrictValidation = false!");
                Debug.LogError("Это означает, что строгая валидация ОТКЛЮЧЕНА!");
            }
        }
        
        private void CheckSceneObjects()
        {
            Debug.Log("📋 ПРОВЕРКА ОБЪЕКТОВ НА СЦЕНЕ:");
            
            // Проверяем все объекты с коллайдерами
            var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            Debug.Log($"Найдено {colliders.Length} объектов с коллайдерами");
            
            int furnitureCount = 0;
            int surfaceCount = 0;
            int staticEnvCount = 0;
            int defaultCount = 0;
            int otherCount = 0;
            
            foreach (var col in colliders)
            {
                string layerName = LayerMask.LayerToName(col.gameObject.layer);
                
                switch (layerName)
                {
                    case "Furniture":
                        furnitureCount++;
                        Debug.Log($"  🪑 {col.name} (Furniture)");
                        break;
                    case "Surface":
                        surfaceCount++;
                        Debug.Log($"  🏠 {col.name} (Surface)");
                        break;
                    case "StaticEnvironment":
                        staticEnvCount++;
                        Debug.Log($"  🏢 {col.name} (StaticEnvironment)");
                        break;
                    case "Default":
                        defaultCount++;
                        Debug.Log($"  ⚠️ {col.name} (Default) - РЕКОМЕНДУЕТСЯ ИЗМЕНИТЬ СЛОЙ!");
                        break;
                    default:
                        otherCount++;
                        Debug.Log($"  ❓ {col.name} ({layerName})");
                        break;
                }
            }
            
            Debug.Log($"ИТОГО:");
            Debug.Log($"  - Furniture: {furnitureCount}");
            Debug.Log($"  - Surface: {surfaceCount}");
            Debug.Log($"  - StaticEnvironment: {staticEnvCount}");
            Debug.Log($"  - Default: {defaultCount} ⚠️");
            Debug.Log($"  - Другие: {otherCount}");
            
            if (defaultCount > 0)
            {
                Debug.LogWarning($"⚠️ Найдено {defaultCount} объектов на слое Default!");
                Debug.LogWarning("Объекты на слое Default могут мешать размещению предметов.");
                Debug.LogWarning("Рекомендуется назначить им соответствующие слои.");
            }
        }
        
        private void TestOverlapAtPosition()
        {
            Debug.Log($"🧪 ТЕСТ НАЛОЖЕНИЙ В ПОЗИЦИИ {testPosition}:");
            
            // Проверяем коллизии в указанной позиции
            var colliders = Physics.OverlapSphere(testPosition, testRadius);
            Debug.Log($"Найдено {colliders.Length} объектов в радиусе {testRadius}");
            
            foreach (var col in colliders)
            {
                string layerName = LayerMask.LayerToName(col.gameObject.layer);
                float distance = Vector3.Distance(testPosition, col.bounds.center);
                
                Debug.Log($"  📍 {col.name} (слой: {layerName}, расстояние: {distance:F2})");
                
                // Проверяем, будет ли это наложение
                if (layerName == "Furniture")
                {
                    Debug.Log($"    ❌ ПОТЕНЦИАЛЬНОЕ НАЛОЖЕНИЕ с предметом на слое Furniture!");
                }
                else if (layerName == "Surface")
                {
                    Debug.Log($"    ❌ ПОТЕНЦИАЛЬНОЕ НАЛОЖЕНИЕ с предметом на слое Surface!");
                }
                else if (layerName == "StaticEnvironment")
                {
                    Debug.Log($"    ✅ Поверхность - не мешает размещению");
                }
                else
                {
                    Debug.Log($"    ⚠️ Неизвестный слой - может мешать");
                }
            }
        }
        
        private void TestValidatorWork()
        {
            Debug.Log("🧪 ТЕСТ РАБОТЫ ВАЛИДАТОРА:");
            
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("❌ Валидатор не найден!");
                return;
            }
            
            // Создаем тестовый предмет
            var testItem = InventorySystem.Factories.InventoryFactory.CreateTestItem();
            
            // Тестируем в позиции, где есть наложения
            var result = validator.CanPlaceItem(testItem, testPosition, Quaternion.identity);
            
            Debug.Log($"Результат валидации в позиции {testPosition}: {result}");
            
            if (result)
            {
                Debug.LogWarning("⚠️ ВАЛИДАТОР РАЗРЕШАЕТ РАЗМЕЩЕНИЕ В ЗОНЕ НАЛОЖЕНИЙ!");
                Debug.LogWarning("Это означает, что PreventObjectOverlap не работает!");
            }
            else
            {
                Debug.Log("✅ Валидатор правильно запрещает размещение в зоне наложений");
            }
        }
        
        [ContextMenu("🔧 БЫСТРОЕ ИСПРАВЛЕНИЕ")]
        public void QuickFix()
        {
            var quickFix = FindFirstObjectByType<QuickFixOverlap>();
            if (quickFix != null)
            {
                quickFix.QuickFixOverlapIssue();
            }
            else
            {
                Debug.LogError("❌ QuickFixOverlap не найден! Добавьте его на сцену.");
            }
        }
        
        [ContextMenu("📊 ПРОВЕРИТЬ НАСТРОЙКИ")]
        public void CheckSettings()
        {
            var quickFix = FindFirstObjectByType<QuickFixOverlap>();
            if (quickFix != null)
            {
                quickFix.CheckCurrentSettings();
            }
            else
            {
                Debug.LogError("❌ QuickFixOverlap не найден!");
            }
        }
        
        [ContextMenu("🚨 ПРИНУДИТЕЛЬНОЕ ИСПРАВЛЕНИЕ")]
        public void ForceFix()
        {
            Debug.Log("🚨 ПРИНУДИТЕЛЬНОЕ ИСПРАВЛЕНИЕ НАСТРОЕК");
            
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("❌ Валидатор не найден!");
                return;
            }
            
            // Принудительно устанавливаем настройки через reflection
            var configType = typeof(PlacementValidationConfig);
            var config = new PlacementValidationConfig();
            
            // Устанавливаем критические настройки
            SetField(config, "preventObjectOverlap", true);
            SetField(config, "useStrictValidation", true);
            SetField(config, "enableLayerSystem", true);
            
            // Устанавливаем маски
            int furnitureLayer = LayerMask.NameToLayer("Furniture");
            int surfaceLayer = LayerMask.NameToLayer("Surface");
            int staticEnvLayer = LayerMask.NameToLayer("StaticEnvironment");
            
            LayerMask collisionMask = (1 << furnitureLayer) | (1 << surfaceLayer);
            LayerMask surfaceMask = (1 << surfaceLayer) | (1 << staticEnvLayer);
            
            SetField(config, "collisionCheckMask", collisionMask);
            SetField(config, "surfaceCheckMask", surfaceMask);
            
            // Устанавливаем конфигурацию в валидатор
            var configField = typeof(BaseInventoryComponent<PlacementValidationConfig>).GetField("configuration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(validator, config);
            
            Debug.Log("✅ Принудительное исправление применено!");
            Debug.Log($"  - PreventObjectOverlap: true");
            Debug.Log($"  - UseStrictValidation: true");
            Debug.Log($"  - CollisionCheckMask: {collisionMask}");
            Debug.Log($"  - SurfaceCheckMask: {surfaceMask}");
        }
        
        private void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public);
            field?.SetValue(obj, value);
        }
        
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;
            
            // Рисуем сферу тестирования
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(testPosition, testRadius);
            
            // Рисуем все объекты с коллайдерами
            var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            foreach (var col in colliders)
            {
                string layerName = LayerMask.LayerToName(col.gameObject.layer);
                
                switch (layerName)
                {
                    case "Furniture":
                        Gizmos.color = Color.red;
                        break;
                    case "Surface":
                        Gizmos.color = Color.blue;
                        break;
                    case "StaticEnvironment":
                        Gizmos.color = Color.green;
                        break;
                    case "Default":
                        Gizmos.color = Color.magenta;
                        break;
                    default:
                        Gizmos.color = Color.white;
                        break;
                }
                
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
    }
}
