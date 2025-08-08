using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Configuration;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Диагностический скрипт для тестирования пересечений предметов
    /// </summary>
    public class OverlapTest : MonoBehaviour
    {
        [Header("Тестовые настройки")]
        [SerializeField] private Vector3 testPosition = Vector3.zero;
        [SerializeField] private float testRadius = 1f;
        [SerializeField] private LayerMask testMask = -1;
        
        [ContextMenu("Тест пересечений в позиции")]
        public void TestOverlapAtPosition()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            Debug.Log($"🔍 Тестируем пересечения в позиции {testPosition}");
            
            // Проверяем коллизии сферой
            var sphereColliders = Physics.OverlapSphere(testPosition, testRadius, testMask);
            Debug.Log($"Найдено коллайдеров сферой: {sphereColliders.Length}");
            foreach (var col in sphereColliders)
            {
                Debug.Log($"  - {col.name} (слой: {LayerMask.LayerToName(col.gameObject.layer)})");
            }
            
            // Проверяем коллизии боксом
            var boxColliders = Physics.OverlapBox(testPosition, Vector3.one * 0.5f, Quaternion.identity, testMask);
            Debug.Log($"Найдено коллайдеров боксом: {boxColliders.Length}");
            foreach (var col in boxColliders)
            {
                Debug.Log($"  - {col.name} (слой: {LayerMask.LayerToName(col.gameObject.layer)})");
            }
        }
        
        [ContextMenu("Проверить настройки валидатора")]
        public void CheckValidatorSettings()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            var config = validator.Configuration;
            Debug.Log($"📋 Настройки валидатора:");
            Debug.Log($"  PreventObjectOverlap: {config.PreventObjectOverlap}");
            Debug.Log($"  CollisionCheckRadius: {config.CollisionCheckRadius}");
            Debug.Log($"  CollisionCheckMask: {config.CollisionCheckMask}");
            Debug.Log($"  EnableValidation: {config.EnableValidation}");
            Debug.Log($"  UseStrictValidation: {config.UseStrictValidation}");
        }
        
        [ContextMenu("Тест размещения предмета")]
        public void TestItemPlacement()
        {
            var validator = FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            // Создаем тестовый предмет
            var testItem = ScriptableObject.CreateInstance<Item>();
            testItem.itemName = "TestItem";
            testItem.prefab = new GameObject("TestPrefab");
            
            var wrapper = new ItemWrapper(testItem);
            
            Debug.Log($"🧪 Тестируем размещение предмета в позиции {testPosition}");
            bool canPlace = validator.CanPlaceItem(wrapper, testPosition, Quaternion.identity);
            Debug.Log($"Результат: {(canPlace ? "✅ Можно разместить" : "❌ Нельзя разместить")}");
            
            // Очищаем тестовые объекты
            DestroyImmediate(testItem.prefab);
            DestroyImmediate(testItem);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Рисуем сферу тестирования
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(testPosition, testRadius);
            
            // Рисуем бокс тестирования
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(testPosition, Vector3.one);
        }
    }
}
