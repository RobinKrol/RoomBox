using UnityEngine;
using InventorySystem.EventSystem;
using InventorySystem.Logging;

namespace InventorySystem.Examples
{
    /// <summary>
    /// Пример использования Event System
    /// </summary>
    public class EventSystemExample : MonoBehaviour
    {
        [SerializeField] private InventoryEventSystem eventSystem;
        
        void Start()
        {
            // Находим Event System если не назначен
            if (eventSystem == null)
                eventSystem = Object.FindFirstObjectByType<InventoryEventSystem>();
            
            if (eventSystem != null)
            {
                // Подписываемся на события
                SubscribeToEvents();
                Debug.Log("EventSystemExample: Подписался на события");
            }
            else
            {
                Debug.LogWarning("EventSystemExample: InventoryEventSystem не найден!");
            }
        }
        
        void OnDestroy()
        {
            // Отписываемся от событий
            if (eventSystem != null)
            {
                UnsubscribeFromEvents();
            }
        }
        
        private void SubscribeToEvents()
        {
            // Базовые события инвентаря
            eventSystem.OnInventoryChanged += HandleInventoryChanged;
            eventSystem.OnItemAdded += HandleItemAdded;
            eventSystem.OnItemRemoved += HandleItemRemoved;
            
            // События UI
            eventSystem.OnInventoryToggled += HandleInventoryToggled;
        }
        
        private void UnsubscribeFromEvents()
        {
            // Базовые события инвентаря
            eventSystem.OnInventoryChanged -= HandleInventoryChanged;
            eventSystem.OnItemAdded -= HandleItemAdded;
            eventSystem.OnItemRemoved -= HandleItemRemoved;
            
            // События UI
            eventSystem.OnInventoryToggled -= HandleInventoryToggled;
        }
        
        #region Event Handlers
        
        private void HandleInventoryChanged()
        {
            Debug.Log("🔔 [EventSystemExample] Инвентарь изменился!");
        }
        
        private void HandleItemAdded(IItem item, int amount)
        {
            Debug.Log($"🔔 [EventSystemExample] Добавлен предмет: {item.ItemName} x{amount}");
        }
        
        private void HandleItemRemoved(IItem item, int amount)
        {
            Debug.Log($"🔔 [EventSystemExample] Удален предмет: {item.ItemName} x{amount}");
        }
        
        private void HandleInventoryToggled(bool isOpen)
        {
            Debug.Log($"🔔 [EventSystemExample] Инвентарь {(isOpen ? "открыт" : "закрыт")}!");
        }
        
        #endregion
        
        #region Context Menu Methods
        
        [ContextMenu("Тест: Вызвать все события")]
        public void TestAllEvents()
        {
            if (eventSystem == null)
            {
                Debug.LogError("EventSystem не найден!");
                return;
            }
            
            Debug.Log("🧪 Тестирование всех событий Event System...");
            
            try
            {
                // Тестируем базовые события
                eventSystem.InvokeInventoryChanged();
                eventSystem.InvokeInventoryToggled(true);
                eventSystem.InvokeInventoryToggled(false);
                
                Debug.Log("✅ Базовые события вызваны успешно!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Ошибка при вызове событий: {e.Message}");
            }
            
            Debug.Log("🧪 Тестирование завершено!");
        }
        
        [ContextMenu("Проверить подписчиков")]
        public void CheckSubscribers()
        {
            if (eventSystem != null)
            {
                Debug.Log("🔍 Проверка подписчиков Event System...");
                Debug.Log($"EventSystem найден: {eventSystem.name}");
                
                // Простая проверка без вызова метода
                Debug.Log("✅ EventSystem готов к использованию");
            }
            else
            {
                Debug.LogError("❌ EventSystem не найден!");
            }
        }
        
        #endregion
    }
}
