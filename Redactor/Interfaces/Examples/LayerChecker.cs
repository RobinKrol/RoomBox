using UnityEngine;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Скрипт для проверки и настройки слоев объектов
    /// </summary>
    public class LayerChecker : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private string furnitureLayerName = "Furniture";
        [SerializeField] private string surfaceLayerName = "Surface";
        [SerializeField] private string staticEnvironmentLayerName = "StaticEnvironment";
        
        [ContextMenu("Проверить слои на сцене")]
        public void CheckSceneLayers()
        {
            Debug.Log("🔍 Проверка слоев на сцене:");
            
            // Проверяем существование слоев
            int furnitureLayer = LayerMask.NameToLayer(furnitureLayerName);
            int surfaceLayer = LayerMask.NameToLayer(surfaceLayerName);
            int staticEnvLayer = LayerMask.NameToLayer(staticEnvironmentLayerName);
            
            Debug.Log($"Слой {furnitureLayerName}: {(furnitureLayer != -1 ? $"✅ Найден (индекс: {furnitureLayer})" : "❌ Не найден")}");
            Debug.Log($"Слой {surfaceLayerName}: {(surfaceLayer != -1 ? $"✅ Найден (индекс: {surfaceLayer})" : "❌ Не найден")}");
            Debug.Log($"Слой {staticEnvironmentLayerName}: {(staticEnvLayer != -1 ? $"✅ Найден (индекс: {staticEnvLayer})" : "❌ Не найден")}");
            
            // Проверяем объекты с коллайдерами
            var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            Debug.Log($"\n📋 Найдено {colliders.Length} объектов с коллайдерами:");
            
            int defaultLayerCount = 0;
            int furnitureLayerCount = 0;
            int surfaceLayerCount = 0;
            int otherLayerCount = 0;
            
            foreach (var col in colliders)
            {
                string layerName = LayerMask.LayerToName(col.gameObject.layer);
                
                if (layerName == "Default")
                    defaultLayerCount++;
                else if (layerName == furnitureLayerName)
                    furnitureLayerCount++;
                else if (layerName == surfaceLayerName)
                    surfaceLayerCount++;
                else
                    otherLayerCount++;
            }
            
            Debug.Log($"  - Default: {defaultLayerCount}");
            Debug.Log($"  - {furnitureLayerName}: {furnitureLayerCount}");
            Debug.Log($"  - {surfaceLayerName}: {surfaceLayerCount}");
            Debug.Log($"  - Другие: {otherLayerCount}");
            
            if (defaultLayerCount > 0)
            {
                Debug.LogWarning($"⚠️ Найдено {defaultLayerCount} объектов на слое Default!");
                Debug.LogWarning("Рекомендуется назначить им соответствующие слои для правильной работы коллизий.");
            }
        }
        
        [ContextMenu("Настроить слои для объектов")]
        public void SetupObjectLayers()
        {
            Debug.Log("🔧 Настройка слоев для объектов:");
            
            int furnitureLayer = LayerMask.NameToLayer(furnitureLayerName);
            int surfaceLayer = LayerMask.NameToLayer(surfaceLayerName);
            
            if (furnitureLayer == -1)
            {
                Debug.LogError($"❌ Слой {furnitureLayerName} не найден! Создайте его в Unity: Edit → Project Settings → Tags and Layers");
                return;
            }
            
            // Находим все объекты с PlacementLayerComponent
            var placementComponents = FindObjectsByType<PlacementLayerComponent>(FindObjectsSortMode.None);
            Debug.Log($"Найдено {placementComponents.Length} объектов с PlacementLayerComponent");
            
            int configuredCount = 0;
            foreach (var component in placementComponents)
            {
                string currentLayer = LayerMask.LayerToName(component.gameObject.layer);
                string recommendedLayer = GetRecommendedLayer(component.PlacementLayer);
                
                if (currentLayer != recommendedLayer)
                {
                    int targetLayer = LayerMask.NameToLayer(recommendedLayer);
                    if (targetLayer != -1)
                    {
                        SetLayerRecursively(component.gameObject, targetLayer);
                        Debug.Log($"✅ {component.name}: {currentLayer} → {recommendedLayer}");
                        configuredCount++;
                    }
                }
            }
            
            Debug.Log($"Настроено {configuredCount} объектов");
        }
        
        [ContextMenu("Настроить слои для префабов")]
        public void SetupPrefabLayers()
        {
            Debug.Log("🔧 Настройка слоев для префабов:");
            Debug.Log("⚠️ ВНИМАНИЕ: Слои префабов нужно настраивать вручную в Unity Editor!");
            Debug.Log("1. Выберите префаб в Project window");
            Debug.Log("2. В Inspector установите Layer:");
            Debug.Log($"   - Префабы предметов → {furnitureLayerName}");
            Debug.Log($"   - Префабы поверхностей → {surfaceLayerName}");
            Debug.Log("3. Нажмите 'Apply' для сохранения изменений");
        }
        
        private string GetRecommendedLayer(PlacementLayer placementLayer)
        {
            switch (placementLayer)
            {
                case PlacementLayer.Floor: return staticEnvironmentLayerName;
                case PlacementLayer.Surface: return surfaceLayerName;
                case PlacementLayer.Item: return furnitureLayerName;
                case PlacementLayer.Wall: return staticEnvironmentLayerName;
                default: return "Default";
            }
        }
        
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            
            obj.layer = newLayer;
            
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Рисуем информацию о слоях в Scene view
            var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            
            foreach (var col in colliders)
            {
                string layerName = LayerMask.LayerToName(col.gameObject.layer);
                Color gizmoColor = GetLayerColor(layerName);
                
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
        
        private Color GetLayerColor(string layerName)
        {
            switch (layerName)
            {
                case "Furniture": return Color.blue;
                case "Surface": return Color.yellow;
                case "StaticEnvironment": return Color.green;
                case "Default": return Color.red;
                default: return Color.white;
            }
        }
    }
}

