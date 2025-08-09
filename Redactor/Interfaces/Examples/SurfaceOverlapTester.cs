using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Configuration;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Специальный тестер для проверки наложений между предметами на слое Surface
    /// </summary>
    public class SurfaceOverlapTester : MonoBehaviour
    {
        [Header("Настройки тестирования")]
        [SerializeField] private Vector3 testPosition = Vector3.zero;
        [SerializeField] private float testRadius = 2f;
        
        [ContextMenu("🧪 ТЕСТ НАЛОЖЕНИЙ ПОВЕРХНОСТЕЙ")]
        public void TestSurfaceOverlaps()
        {
            Debug.Log("🧪 ТЕСТ НАЛОЖЕНИЙ ПОВЕРХНОСТЕЙ");
            Debug.Log("================================");
            
            // 1. Проверяем настройки валидатора
            CheckValidatorSettings();
            
            // 2. Находим все объекты на слое Surface
            FindSurfaceObjects();
            
            // 3. Тестируем наложения в указанной позиции
            TestOverlapsAtPosition();
            
            Debug.Log("✅ ТЕСТ ЗАВЕРШЕН");
        }
        
        private void CheckValidatorSettings()
        {
            Debug.Log("📋 НАСТРОЙКИ ВАЛИДАТОРА:");
            
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("❌ OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            var config = validator.Configuration;
            Debug.Log($"  - CollisionCheckMask: {config.CollisionCheckMask}");
            Debug.Log($"  - SurfaceCheckMask: {config.SurfaceCheckMask}");
            Debug.Log($"  - PreventObjectOverlap: {config.PreventObjectOverlap}");
            Debug.Log($"  - UseStrictValidation: {config.UseStrictValidation}");
            
            // Проверяем, включен ли слой Surface в CollisionCheckMask
            int surfaceLayer = LayerMask.NameToLayer("Surface");
            bool surfaceInCollisionMask = ((1 << surfaceLayer) & config.CollisionCheckMask) != 0;
            
            Debug.Log($"  - Слой Surface в CollisionCheckMask: {(surfaceInCollisionMask ? "✅ ВКЛЮЧЕН" : "❌ НЕ ВКЛЮЧЕН")}");
            
            if (!surfaceInCollisionMask)
            {
                Debug.LogError("❌ Слой Surface НЕ включен в CollisionCheckMask!");
                Debug.LogError("Предметы на слое Surface не будут проверяться на наложения!");
            }
        }
        
        private void FindSurfaceObjects()
        {
            Debug.Log("🔍 ПОИСК ОБЪЕКТОВ НА СЛОЕ SURFACE:");
            
            var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            var surfaceObjects = new System.Collections.Generic.List<Collider>();
            
            foreach (var col in colliders)
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("Surface"))
                {
                    surfaceObjects.Add(col);
                }
            }
            
            Debug.Log($"Найдено {surfaceObjects.Count} объектов на слое Surface:");
            
            foreach (var obj in surfaceObjects)
            {
                Debug.Log($"  🏠 {obj.name} (позиция: {obj.transform.position})");
                
                // Проверяем, есть ли PlacementLayerComponent
                var placementComponent = obj.GetComponent<PlacementLayerComponent>();
                if (placementComponent != null)
                {
                    Debug.Log($"    - PlacementLayer: {placementComponent.PlacementLayer}");
                    Debug.Log($"    - IsSurface: {placementComponent.IsSurface}");
                }
                else
                {
                    Debug.Log($"    - PlacementLayerComponent: НЕТ");
                }
            }
        }
        
        private void TestOverlapsAtPosition()
        {
            Debug.Log($"🧪 ТЕСТ НАЛОЖЕНИЙ В ПОЗИЦИИ {testPosition}:");
            
            // Проверяем коллизии с маской Furniture + Surface
            int furnitureLayer = LayerMask.NameToLayer("Furniture");
            int surfaceLayer = LayerMask.NameToLayer("Surface");
            LayerMask testMask = (1 << furnitureLayer) | (1 << surfaceLayer);
            
            var colliders = Physics.OverlapSphere(testPosition, testRadius, testMask);
            Debug.Log($"Найдено {colliders.Length} объектов в радиусе {testRadius} (маска: Furniture + Surface)");
            
            int furnitureCount = 0;
            int surfaceCount = 0;
            
            foreach (var col in colliders)
            {
                string layerName = LayerMask.LayerToName(col.gameObject.layer);
                float distance = Vector3.Distance(testPosition, col.bounds.center);
                
                Debug.Log($"  📍 {col.name} (слой: {layerName}, расстояние: {distance:F2})");
                
                if (layerName == "Furniture")
                {
                    furnitureCount++;
                    Debug.Log($"    ❌ ПОТЕНЦИАЛЬНОЕ НАЛОЖЕНИЕ с предметом Furniture!");
                }
                else if (layerName == "Surface")
                {
                    surfaceCount++;
                    Debug.Log($"    ❌ ПОТЕНЦИАЛЬНОЕ НАЛОЖЕНИЕ с предметом Surface!");
                }
            }
            
            Debug.Log($"ИТОГО:");
            Debug.Log($"  - Предметы Furniture: {furnitureCount}");
            Debug.Log($"  - Предметы Surface: {surfaceCount}");
            
            if (furnitureCount + surfaceCount > 1)
            {
                Debug.LogWarning($"⚠️ ОБНАРУЖЕНО ПОТЕНЦИАЛЬНОЕ НАЛОЖЕНИЕ!");
                Debug.LogWarning($"В радиусе {testRadius} найдено {furnitureCount + surfaceCount} предметов, которые не должны пересекаться.");
            }
            else
            {
                Debug.Log("✅ Наложений не обнаружено");
            }
        }
        
        [ContextMenu("🔧 БЫСТРОЕ ИСПРАВЛЕНИЕ")]
        public void QuickFix()
        {
            var quickFix = FindFirstObjectByType<QuickFixOverlap>();
            if (quickFix != null)
            {
                quickFix.QuickFixOverlapIssue();
                Debug.Log("✅ Быстрое исправление применено!");
                Debug.Log("Теперь слой Surface включен в CollisionCheckMask");
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
        
        private void OnDrawGizmos()
        {
            // Рисуем сферу тестирования
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(testPosition, testRadius);
            
            // Рисуем объекты на слое Surface
            var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            foreach (var col in colliders)
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("Surface"))
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                }
                else if (col.gameObject.layer == LayerMask.NameToLayer("Furniture"))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                }
            }
        }
    }
}
