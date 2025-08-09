using UnityEngine;
using InventorySystem.EventSystem;

namespace InventorySystem.Adapters
{
    /// <summary>
    /// Адаптер, приводящий новую систему событий InventoryEventSystem к интерфейсу IInventoryEventSystem
    /// </summary>
    public class EventSystemAdapter : MonoBehaviour, IInventoryEventSystem
    {
        private InventorySystem.EventSystem.InventoryEventSystem _newEventSystem;

        // Реализация интерфейсных событий через ретрансляцию событий новой системы
        public event System.Action OnInventoryChanged;
        public event System.Action<IItem, int> OnItemAdded;
        public event System.Action<IItem, int> OnItemRemoved;
        public event System.Action<int, IInventorySlot> OnSlotChanged; // В новой системе прямого аналога нет
        public event System.Action<IItem, int> OnDragStarted;
        public event System.Action<IItem, int, bool> OnDragEnded;
        public event System.Action<IItem, Vector3, Quaternion> OnItemPlaced; // Прямого аналога нет
        public event System.Action<IItem, string> OnPlacementError; // Будет транслироваться из новой системы без позиции
        public event System.Action<bool> OnInventoryToggled;

        private void Awake()
        {
            // Ищем или создаем новую систему событий
            _newEventSystem = FindFirstObjectByType<InventorySystem.EventSystem.InventoryEventSystem>();
            if (_newEventSystem == null)
            {
                GameObject go = new GameObject("InventoryEventSystem");
                _newEventSystem = go.AddComponent<InventorySystem.EventSystem.InventoryEventSystem>();
            }

            // Подписываемся на события новой системы и ретранслируем их наружу
            _newEventSystem.OnInventoryChanged += () => OnInventoryChanged?.Invoke();
            _newEventSystem.OnItemAdded += (item, amount) => OnItemAdded?.Invoke(item, amount);
            _newEventSystem.OnItemRemoved += (item, amount) => OnItemRemoved?.Invoke(item, amount);
            _newEventSystem.OnInventoryToggled += isOpen => OnInventoryToggled?.Invoke(isOpen);

            // События перетаскивания в новой системе идут без IItem. Пробрасываем без item.
            _newEventSystem.OnSlotDragStarted += slotIndex => OnDragStarted?.Invoke(null, slotIndex);
            _newEventSystem.OnSlotDragEnded += slotIndex => OnDragEnded?.Invoke(null, slotIndex, false);

            // Ошибки валидации: позицию опускаем, так как её нет в интерфейсе
            _newEventSystem.OnItemPlacementFailed += (item, position, reason) => OnPlacementError?.Invoke(item, reason);
        }

        /// <summary>
        /// Утилита для получения (или создания) адаптера на сцене
        /// </summary>
        public static EventSystemAdapter GetOrCreate()
        {
            var adapter = FindFirstObjectByType<EventSystemAdapter>();
            if (adapter != null) return adapter;

            GameObject go = new GameObject("EventSystemAdapter");
            return go.AddComponent<EventSystemAdapter>();
        }

        #region IInventoryEventSystem methods
        public void InvokeInventoryChanged() => _newEventSystem?.InvokeInventoryChanged();

        public void InvokeItemAdded(IItem item, int amount) => _newEventSystem?.InvokeItemAdded(item, amount);

        public void InvokeItemRemoved(IItem item, int amount) => _newEventSystem?.InvokeItemRemoved(item, amount);

        public void InvokeSlotChanged(int slotIndex, IInventorySlot slot)
        {
            // В новой системе прямого аналога нет. Вызываем событие локально.
            OnSlotChanged?.Invoke(slotIndex, slot);
        }

        public void InvokeDragStarted(IItem item, int slotIndex)
        {
            _newEventSystem?.InvokeSlotDragStarted(slotIndex);
            OnDragStarted?.Invoke(item, slotIndex);
        }

        public void InvokeDragEnded(IItem item, int slotIndex, bool wasPlaced)
        {
            _newEventSystem?.InvokeSlotDragEnded(slotIndex);
            OnDragEnded?.Invoke(item, slotIndex, wasPlaced);
        }

        public void InvokeItemPlaced(IItem item, Vector3 position, Quaternion rotation)
        {
            // В новой системе нет отдельного метода, можно транслировать как успешную валидацию или добавить при необходимости
            OnItemPlaced?.Invoke(item, position, rotation);
        }

        public void InvokePlacementError(IItem item, string errorMessage)
        {
            // В новой системе есть метод с позицией, опускаем её
            _newEventSystem?.InvokeItemPlacementFailed(item, Vector3.zero, errorMessage);
            OnPlacementError?.Invoke(item, errorMessage);
        }

        public void InvokeInventoryToggled(bool isOpen)
        {
            _newEventSystem?.InvokeInventoryToggled(isOpen);
            OnInventoryToggled?.Invoke(isOpen);
        }
        #endregion
    }
}


