using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Configuration;
using InventorySystem.BaseComponents;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Помощник для настройки слоев и масок коллизий
    /// </summary>
    public class LayerSetupHelper : MonoBehaviour
    {
        [Header("Рекомендуемые слои")]
        [SerializeField] private string staticEnvironmentLayer = "StaticEnvironment";
        [SerializeField] private string furnitureLayer = "Furniture";
        [SerializeField] private string ignoreCollisionLayer = "IgnoreCollision";
        [SerializeField] private string surfaceLayer = "Surface";
        
        [Header("Текущие настройки")]
        [SerializeField] private LayerMask currentCollisionMask;
        [SerializeField] private LayerMask currentSurfaceMask;
        
        [ContextMenu("Создать рекомендуемые слои")]
        public void CreateRecommendedLayers()
        {
            Debug.Log("📋 Рекомендуемые слои для настройки в Unity:");
            Debug.Log($"  - {staticEnvironmentLayer} (для пола/стен)");
            Debug.Log($"  - {furnitureLayer} (для перемещаемых предметов)");
            Debug.Log($"  - {ignoreCollisionLayer} (для декоративных объектов)");
            Debug.Log($"  - {surfaceLayer} (для поверхностей - столов, тумб)");
            Debug.Log("");
            Debug.Log("⚠️ ВНИМАНИЕ: Слои нужно создать вручную в Unity:");
            Debug.Log("  1. Edit → Project Settings → Tags and Layers");
            Debug.Log("  2. Добавьте слои в User Layers 8-31");
            Debug.Log("  3. Назначьте объекты на соответствующие слои");
        }
        
        [ContextMenu("Настроить маски коллизий")]
        public void SetupCollisionMasks()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            // Создаем новую конфигурацию
            var config = new PlacementValidationConfig();
            
            // Настраиваем маску коллизий - проверяем только мебель (предметы, которые могут пересекаться)
            int furnitureLayerIndex = LayerMask.NameToLayer(furnitureLayer);
            
            if (furnitureLayerIndex == -1)
            {
                Debug.LogError($"Слой {furnitureLayer} не найден!");
                Debug.LogError("Создайте слои вручную в Unity: Edit → Project Settings → Tags and Layers");
                return;
            }
            
            // Проверяем только слой Furniture для предотвращения пересечений
            LayerMask collisionMask = (1 << furnitureLayerIndex);
            currentCollisionMask = collisionMask;
            
            // Настраиваем маску поверхностей - проверяем только поверхности
            int surfaceLayerIndex = LayerMask.NameToLayer(surfaceLayer);
            LayerMask surfaceMask = surfaceLayerIndex != -1 ? (1 << surfaceLayerIndex) : (LayerMask)(-1);
            currentSurfaceMask = surfaceMask;
            
            // Устанавливаем настройки через reflection
            var collisionMaskField = typeof(PlacementValidationConfig).GetField("collisionCheckMask", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            collisionMaskField?.SetValue(config, collisionMask);
            
            var surfaceMaskField = typeof(PlacementValidationConfig).GetField("surfaceCheckMask", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            surfaceMaskField?.SetValue(config, surfaceMask);
            
            // Включаем предотвращение пересечений
            var preventOverlapField = typeof(PlacementValidationConfig).GetField("preventObjectOverlap", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            preventOverlapField?.SetValue(config, true);
            
            // Включаем строгую валидацию
            var useStrictValidationField = typeof(PlacementValidationConfig).GetField("useStrictValidation", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            useStrictValidationField?.SetValue(config, true);
            
            // Включаем систему слоев
            var enableLayerSystemField = typeof(PlacementValidationConfig).GetField("enableLayerSystem", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enableLayerSystemField?.SetValue(config, true);
            
            // Устанавливаем конфигурацию
            var configField = typeof(BaseInventoryComponent<PlacementValidationConfig>).GetField("configuration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(validator, config);
            
            Debug.Log("✅ Маски коллизий настроены!");
            Debug.Log($"  CollisionCheckMask: {collisionMask} (слой: {furnitureLayer})");
            Debug.Log($"  SurfaceCheckMask: {surfaceMask} (слой: {surfaceLayer})");
            Debug.Log($"  PreventObjectOverlap: true");
            Debug.Log($"  UseStrictValidation: true");
            Debug.Log("📝 Логика: Проверяем пересечения только между объектами на слое Furniture");
        }
        
        [ContextMenu("Проверить текущие настройки")]
        public void CheckCurrentSettings()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            var config = validator.Configuration;
            Debug.Log($"📋 Текущие настройки валидатора:");
            Debug.Log($"  PreventObjectOverlap: {config.PreventObjectOverlap}");
            Debug.Log($"  UseStrictValidation: {config.UseStrictValidation}");
            Debug.Log($"  EnableLayerSystem: {config.EnableLayerSystem}");
            Debug.Log($"  CollisionCheckMask: {config.CollisionCheckMask}");
            Debug.Log($"  SurfaceCheckMask: {config.SurfaceCheckMask}");
            
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
        }
        
        [ContextMenu("Назначить слои объектам на сцене")]
        public void AssignLayersToSceneObjects()
        {
            Debug.Log("🔧 Назначение слоев объектам на сцене:");
            
            // Находим все объекты с PlacementLayerComponent
            var placementComponents = FindObjectsByType<PlacementLayerComponent>(FindObjectsSortMode.None);
            Debug.Log($"Найдено {placementComponents.Length} объектов с PlacementLayerComponent");
            
            foreach (var component in placementComponents)
            {
                string recommendedLayer = GetRecommendedLayer(component.PlacementLayer);
                string currentLayer = LayerMask.LayerToName(component.gameObject.layer);
                
                Debug.Log($"  {component.name}:");
                Debug.Log($"    - PlacementLayer: {component.PlacementLayer}");
                Debug.Log($"    - Текущий слой: {currentLayer}");
                Debug.Log($"    - Рекомендуемый слой: {recommendedLayer}");
                
                if (currentLayer != recommendedLayer)
                {
                    Debug.Log($"    ⚠️ Рекомендуется изменить слой на {recommendedLayer}");
                }
            }
            
            // Находим все объекты с коллайдерами
            var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            Debug.Log($"Найдено {colliders.Length} объектов с коллайдерами");
            
            int assignedCount = 0;
            foreach (var collider in colliders)
            {
                if (collider.GetComponent<PlacementLayerComponent>() == null)
                {
                    string currentLayer = LayerMask.LayerToName(collider.gameObject.layer);
                    if (currentLayer == "Default" || currentLayer == "")
                    {
                        Debug.Log($"  {collider.name}: слой Default - рассмотрите назначение на {furnitureLayer}");
                        assignedCount++;
                    }
                }
            }
            
            if (assignedCount > 0)
            {
                Debug.Log($"⚠️ Найдено {assignedCount} объектов с коллайдерами на слое Default");
                Debug.Log("Рекомендуется назначить им слой Furniture для правильной работы коллизий");
            }
        }
        
        private string GetRecommendedLayer(PlacementLayer placementLayer)
        {
            switch (placementLayer)
            {
                case PlacementLayer.Floor: return staticEnvironmentLayer;
                case PlacementLayer.Surface: return surfaceLayer;
                case PlacementLayer.Item: return furnitureLayer;
                case PlacementLayer.Wall: return staticEnvironmentLayer;
                default: return "Default";
            }
        }
    }
}
