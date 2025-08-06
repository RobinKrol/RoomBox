using UnityEngine;
using InventorySystem.BaseComponents;
using InventorySystem.Logging;

namespace InventorySystem.EventSystem
{
    /// <summary>
    /// Централизованная система событий для инвентаря
    /// Версия 1.0 - все события и методы доступны
    /// </summary>
    public class InventoryEventSystem : BaseInventoryComponent
    {
        // События инвентаря
        public event System.Action OnInventoryChanged;
        public event System.Action<IItem, int> OnItemAdded;
        public event System.Action<IItem, int> OnItemRemoved;
        public event System.Action<int> OnSlotClicked;
        public event System.Action<int> OnSlotDragStarted;
        public event System.Action<int> OnSlotDragEnded;
        
        // События UI
        public event System.Action OnInventoryOpened;
        public event System.Action OnInventoryClosed;
        public event System.Action<bool> OnInventoryToggled;
        
        // События валидации
        public event System.Action<IItem, Vector3, bool> OnItemPlacementValidated;
        public event System.Action<IItem, Vector3, string> OnItemPlacementFailed;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            LogDebug("InventoryEventSystem инициализирован");
        }
        
        #region Inventory Events
        
        /// <summary>
        /// Вызвать событие изменения инвентаря
        /// </summary>
        public void InvokeInventoryChanged()
        {
            LogDebug("Событие: InventoryChanged");
            OnInventoryChanged?.Invoke();
        }
        
        /// <summary>
        /// Вызвать событие добавления предмета
        /// </summary>
        public void InvokeItemAdded(IItem item, int amount)
        {
            LogDebug($"Событие: ItemAdded - {item.ItemName} x{amount}");
            OnItemAdded?.Invoke(item, amount);
        }
        
        /// <summary>
        /// Вызвать событие удаления предмета
        /// </summary>
        public void InvokeItemRemoved(IItem item, int amount)
        {
            LogDebug($"Событие: ItemRemoved - {item.ItemName} x{amount}");
            OnItemRemoved?.Invoke(item, amount);
        }
        
        #endregion
        
        #region Slot Events
        
        /// <summary>
        /// Вызвать событие клика по слоту
        /// </summary>
        public void InvokeSlotClicked(int slotIndex)
        {
            LogDebug($"Событие: SlotClicked - слот {slotIndex}");
            OnSlotClicked?.Invoke(slotIndex);
        }
        
        /// <summary>
        /// Вызвать событие начала перетаскивания слота
        /// </summary>
        public void InvokeSlotDragStarted(int slotIndex)
        {
            LogDebug($"Событие: SlotDragStarted - слот {slotIndex}");
            OnSlotDragStarted?.Invoke(slotIndex);
        }
        
        /// <summary>
        /// Вызвать событие окончания перетаскивания слота
        /// </summary>
        public void InvokeSlotDragEnded(int slotIndex)
        {
            LogDebug($"Событие: SlotDragEnded - слот {slotIndex}");
            OnSlotDragEnded?.Invoke(slotIndex);
        }
        
        #endregion
        
        #region UI Events
        
        /// <summary>
        /// Вызвать событие открытия инвентаря
        /// </summary>
        public void InvokeInventoryOpened()
        {
            LogDebug("Событие: InventoryOpened");
            OnInventoryOpened?.Invoke();
        }
        
        /// <summary>
        /// Вызвать событие закрытия инвентаря
        /// </summary>
        public void InvokeInventoryClosed()
        {
            LogDebug("Событие: InventoryClosed");
            OnInventoryClosed?.Invoke();
        }
        
        /// <summary>
        /// Вызвать событие переключения инвентаря
        /// </summary>
        public void InvokeInventoryToggled(bool isOpen)
        {
            LogDebug($"Событие: InventoryToggled - {(isOpen ? "открыт" : "закрыт")}");
            OnInventoryToggled?.Invoke(isOpen);
        }
        
        #endregion
        
        #region Validation Events
        
        /// <summary>
        /// Вызвать событие успешной валидации размещения
        /// </summary>
        public void InvokeItemPlacementValidated(IItem item, Vector3 position, bool isValid)
        {
            LogDebug($"Событие: ItemPlacementValidated - {item.ItemName} в {position} = {(isValid ? "валидно" : "невалидно")}");
            OnItemPlacementValidated?.Invoke(item, position, isValid);
        }
        
        /// <summary>
        /// Вызвать событие неудачной валидации размещения
        /// </summary>
        public void InvokeItemPlacementFailed(IItem item, Vector3 position, string reason)
        {
            LogDebug($"Событие: ItemPlacementFailed - {item.ItemName} в {position}: {reason}");
            OnItemPlacementFailed?.Invoke(item, position, reason);
        }
        
        #endregion
        
        #region Context Menu Methods
        
        [ContextMenu("Проверить подписчиков событий")]
        public void CheckEventSubscribers()
        {
            LogDebug("=== ПРОВЕРКА ПОДПИСЧИКОВ СОБЫТИЙ ===");
            LogDebug("Метод CheckEventSubscribers доступен и работает");
            
            var inventoryChangedCount = OnInventoryChanged?.GetInvocationList().Length ?? 0;
            var itemAddedCount = OnItemAdded?.GetInvocationList().Length ?? 0;
            var itemRemovedCount = OnItemRemoved?.GetInvocationList().Length ?? 0;
            var slotClickedCount = OnSlotClicked?.GetInvocationList().Length ?? 0;
            var slotDragStartedCount = OnSlotDragStarted?.GetInvocationList().Length ?? 0;
            var slotDragEndedCount = OnSlotDragEnded?.GetInvocationList().Length ?? 0;
            var inventoryOpenedCount = OnInventoryOpened?.GetInvocationList().Length ?? 0;
            var inventoryClosedCount = OnInventoryClosed?.GetInvocationList().Length ?? 0;
            var inventoryToggledCount = OnInventoryToggled?.GetInvocationList().Length ?? 0;
            var placementValidatedCount = OnItemPlacementValidated?.GetInvocationList().Length ?? 0;
            var placementFailedCount = OnItemPlacementFailed?.GetInvocationList().Length ?? 0;
            
            LogDebug($"OnInventoryChanged: {inventoryChangedCount} подписчиков");
            LogDebug($"OnItemAdded: {itemAddedCount} подписчиков");
            LogDebug($"OnItemRemoved: {itemRemovedCount} подписчиков");
            LogDebug($"OnSlotClicked: {slotClickedCount} подписчиков");
            LogDebug($"OnSlotDragStarted: {slotDragStartedCount} подписчиков");
            LogDebug($"OnSlotDragEnded: {slotDragEndedCount} подписчиков");
            LogDebug($"OnInventoryOpened: {inventoryOpenedCount} подписчиков");
            LogDebug($"OnInventoryClosed: {inventoryClosedCount} подписчиков");
            LogDebug($"OnInventoryToggled: {inventoryToggledCount} подписчиков");
            LogDebug($"OnItemPlacementValidated: {placementValidatedCount} подписчиков");
            LogDebug($"OnItemPlacementFailed: {placementFailedCount} подписчиков");
            
            LogDebug("=== ПРОВЕРКА ЗАВЕРШЕНА ===");
        }
        
        [ContextMenu("Очистить все события")]
        public void ClearAllEvents()
        {
            LogDebug("Очистка всех событий");
            
            OnInventoryChanged = null;
            OnItemAdded = null;
            OnItemRemoved = null;
            OnSlotClicked = null;
            OnSlotDragStarted = null;
            OnSlotDragEnded = null;
            OnInventoryOpened = null;
            OnInventoryClosed = null;
            OnInventoryToggled = null;
            OnItemPlacementValidated = null;
            OnItemPlacementFailed = null;
            
            LogDebug("Все события очищены");
        }
        
        #endregion
    }
} 