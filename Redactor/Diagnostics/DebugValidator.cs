using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Factories;

namespace InventorySystem.Diagnostics
{
    public class DebugValidator : MonoBehaviour
    {
        [ContextMenu("Быстрый тест валидации")]
        public void QuickValidationTest()
        {
            UnityEngine.Debug.Log("=== БЫСТРЫЙ ТЕСТ ВАЛИДАЦИИ ===");
            
            var validator = Object.FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                UnityEngine.Debug.LogError("❌ OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            // Создаем тестовый предмет
            var testItem = InventoryFactory.CreateTestItem();
            if (testItem == null)
            {
                UnityEngine.Debug.LogError("❌ Не удалось создать тестовый предмет!");
                return;
            }
            
            // Тестируем размещение на полу
            Vector3 floorPosition = Vector3.up * 0.5f;
            bool canPlaceOnFloor = validator.CanPlaceItem(testItem, floorPosition, Quaternion.identity);
            UnityEngine.Debug.Log($"Размещение на полу (0, 0.5, 0): {(canPlaceOnFloor ? "✅ РАЗРЕШЕНО" : "❌ ЗАПРЕЩЕНО")}");
            
            // Тестируем размещение в воздухе
            Vector3 airPosition = Vector3.up * 2f;
            bool canPlaceInAir = validator.CanPlaceItem(testItem, airPosition, Quaternion.identity);
            UnityEngine.Debug.Log($"Размещение в воздухе (0, 2, 0): {(canPlaceInAir ? "✅ РАЗРЕШЕНО" : "❌ ЗАПРЕЩЕНО")}");
            
            // Тестируем размещение на поверхности
            var surfaceComponents = Object.FindObjectsByType<PlacementLayerComponent>(FindObjectsSortMode.None);
            if (surfaceComponents.Length > 0)
            {
                var surfaceComponent = surfaceComponents[0];
                Vector3 surfacePosition = surfaceComponent.GetSurfacePosition() + Vector3.up * 0.1f;
                bool canPlaceOnSurface = validator.CanPlaceItem(testItem, surfacePosition, Quaternion.identity);
                UnityEngine.Debug.Log($"Размещение на поверхности {surfaceComponent.name}: {(canPlaceOnSurface ? "✅ РАЗРЕШЕНО" : "❌ ЗАПРЕЩЕНО")}");
                UnityEngine.Debug.Log($"  - Позиция поверхности: {surfaceComponent.GetSurfacePosition()}");
                UnityEngine.Debug.Log($"  - Тестовая позиция: {surfacePosition}");
                UnityEngine.Debug.Log($"  - IsSurface: {surfaceComponent.IsSurface}");
                UnityEngine.Debug.Log($"  - PlacementLayer: {surfaceComponent.PlacementLayer}");
            }
            
            // Проверяем настройки
            UnityEngine.Debug.Log($"Enable Layer System: {validator.Configuration.EnableLayerSystem}");
            UnityEngine.Debug.Log($"Surface Check Mask: {validator.Configuration.SurfaceCheckMask}");
            UnityEngine.Debug.Log($"Default Placement Layer: {validator.Configuration.DefaultPlacementLayer}");
            
            UnityEngine.Debug.Log("=== ТЕСТ ЗАВЕРШЕН ===");
        }
        
        [ContextMenu("Проверить настройки валидатора")]
        public void CheckValidatorSettings()
        {
            UnityEngine.Debug.Log("=== ДИАГНОСТИКА OPTIMIZED ITEM PLACEMENT VALIDATOR ===");
            
            var validator = Object.FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                UnityEngine.Debug.LogError("❌ OptimizedItemPlacementValidator не найден в сцене!");
                return;
            }
            
            UnityEngine.Debug.Log($"✅ OptimizedItemPlacementValidator найден: {validator.name}");
            
            // Проверяем настройки
            var config = validator.Configuration;
            UnityEngine.Debug.Log($"Enable Validation: {config.EnableValidation}");
            UnityEngine.Debug.Log($"Enable Layer System: {config.EnableLayerSystem}");
            UnityEngine.Debug.Log($"Surface Check Mask: {config.SurfaceCheckMask}");
            UnityEngine.Debug.Log($"Collision Check Mask: {config.CollisionCheckMask}");
            UnityEngine.Debug.Log($"Default Placement Layer: {config.DefaultPlacementLayer}");
            UnityEngine.Debug.Log($"Check Floor Bounds: {config.CheckFloorBounds}");
            UnityEngine.Debug.Log($"Prevent Object Overlap: {config.PreventObjectOverlap}");
            
            // Проверяем другие компоненты
            var dragHandlers = Object.FindObjectsByType<InventorySlotDragHandler>(FindObjectsSortMode.None);
            UnityEngine.Debug.Log($"📋 Найдено InventorySlotDragHandler: {dragHandlers.Length}");
            
            var placementComponents = Object.FindObjectsByType<PlacementLayerComponent>(FindObjectsSortMode.None);
            UnityEngine.Debug.Log($"🏷️ Найдено PlacementLayerComponent: {placementComponents.Length}");
            
            foreach (var component in placementComponents)
            {
                UnityEngine.Debug.Log($"  - {component.name}: Layer={component.PlacementLayer}, IsSurface={component.IsSurface}");
            }
            
            UnityEngine.Debug.Log("=== ДИАГНОСТИКА ЗАВЕРШЕНА ===");
        }
    }
} 