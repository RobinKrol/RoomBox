using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Factories;

namespace InventorySystem.Tests
{
    /// <summary>
    /// Быстрый тест для проверки работы новой системы
    /// </summary>
    public class QuickTest : MonoBehaviour
    {
        [ContextMenu("Тест новой системы")]
        public void TestNewSystem()
        {
            Debug.Log("=== Тест новой системы инвентаря ===");
            
            // 1. Создаем предмет через фабрику
            var item = InventoryFactory.CreateTestItem("Тестовый предмет", 5);
            Debug.Log($"✅ Создан предмет: {item.itemName}, MaxStack: {item.MaxStackSize}");
            
            // 2. Создаем валидатор через фабрику
            var validator = ValidationFactory.CreatePlacementValidator();
            Debug.Log($"✅ Создан валидатор: {validator.name}");
            
            // 3. Тестируем валидацию
            var canPlace = validator.CanPlaceItem(item, Vector3.zero, Quaternion.identity);
            Debug.Log($"✅ Валидация: {canPlace}");
            
            // 4. Тестируем детальную валидацию
            var result = validator.ValidatePlacement(item, Vector3.zero, Quaternion.identity);
            Debug.Log($"✅ Детальная валидация: {result.IsValid}, Ошибка: {result.ErrorMessage}");
            
            // 5. Тестируем визуальную обратную связь
            var feedback = validator.GetVisualFeedback(item, Vector3.zero, Quaternion.identity);
            Debug.Log($"✅ Визуальная обратная связь: {feedback.IsValid}, Цвет: {feedback.Color}, Сообщение: {feedback.Message}");
            
            // 6. Тестируем поиск валидной позиции
            var validPosition = validator.GetValidPlacementPosition(item, Vector3.zero, Quaternion.identity);
            Debug.Log($"✅ Валидная позиция: {validPosition}");
            
            Debug.Log("=== Тест завершен ===");
        }
        
        [ContextMenu("Тест производительности")]
        public void TestPerformance()
        {
            Debug.Log("=== Тест производительности ===");
            
            var validator = Object.FindFirstObjectByType<OptimizedItemPlacementValidator>();
            if (validator == null)
            {
                Debug.LogError("❌ OptimizedItemPlacementValidator не найден в сцене!");
                return;
            }
            
            var item = InventoryFactory.CreateTestItem("Тест производительности", 1);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Выполняем 1000 проверок
            for (int i = 0; i < 1000; i++)
            {
                var position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                validator.CanPlaceItem(item, position, Quaternion.identity);
            }
            
            stopwatch.Stop();
            Debug.Log($"✅ 1000 проверок выполнено за {stopwatch.ElapsedMilliseconds}ms");
            Debug.Log($"✅ Среднее время: {stopwatch.ElapsedMilliseconds / 1000f}ms на проверку");
            
            Debug.Log("=== Тест производительности завершен ===");
        }
        
        [ContextMenu("Сравнение со старой системой")]
        public void CompareWithOldSystem()
        {
            Debug.Log("=== Сравнение систем ===");
            
            var newValidator = Object.FindFirstObjectByType<OptimizedItemPlacementValidator>();
            var oldValidator = Object.FindFirstObjectByType<ItemPlacementValidator>();
            
            if (newValidator == null)
            {
                Debug.LogError("❌ OptimizedItemPlacementValidator не найден!");
                return;
            }
            
            if (oldValidator == null)
            {
                Debug.LogWarning("⚠️ ItemPlacementValidator не найден (возможно, уже заменен)");
            }
            
            var item = InventoryFactory.CreateTestItem("Сравнительный тест", 1);
            var testPosition = Vector3.zero;
            
            // Тест новой системы
            var newResult = newValidator.CanPlaceItem(item, testPosition, Quaternion.identity);
            Debug.Log($"✅ Новая система: {newResult}");
            
                         // Тест старой системы (если доступна)
             if (oldValidator != null)
             {
                 var oldResult = oldValidator.CanPlaceItem(item, testPosition, Quaternion.identity);
                 Debug.Log($"✅ Старая система: {oldResult}");
                 Debug.Log($"✅ Результаты совпадают: {newResult == oldResult}");
             }
            
            Debug.Log("=== Сравнение завершено ===");
        }
    }
} 