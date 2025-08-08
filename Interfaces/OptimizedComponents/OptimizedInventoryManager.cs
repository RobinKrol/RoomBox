using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InventorySystem.BaseComponents;
using InventorySystem.Logging;
using InventorySystem.DependencyInjection;
using InventorySystem.Factories;
using InventorySystem.Configuration;
using InventorySystem.EventSystem;

namespace InventorySystem.OptimizedComponents
{
    /// <summary>
    /// Оптимизированный менеджер инвентаря на базе новой архитектуры
    /// </summary>
    public class OptimizedInventoryManager : BaseInventoryComponent<InventoryManagerConfig>, IInventoryManager
    {
        [Header("Ссылки на компоненты")]
        [SerializeField] private InventoryEventSystem eventSystem;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Button redactorButton;
        [SerializeField] private TextMeshProUGUI inventoryCounterText;
        
        [Header("Слоты")]
        [SerializeField] private List<IInventorySlot> inventorySlots = new List<IInventorySlot>();
        [SerializeField] private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
        
        // Состояние
        private bool isInventoryOpen = false;
        
        // События интерфейса IInventoryManager
        public event System.Action OnInventoryChanged;
        public event System.Action<IItem, int> OnItemAdded;
        public event System.Action<IItem, int> OnItemRemoved;
        
        // Публичные свойства
        public int SlotCount => inventorySlots.Count;
        public bool IsInventoryOpen => isInventoryOpen;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            LogDebug("Инициализация OptimizedInventoryManager");
            
            // Находим необходимые компоненты
            if (eventSystem == null)
            {
                // Сначала ищем в текущем GameObject
                eventSystem = GetComponent<InventoryEventSystem>();
                
                // Если не найден, ищем в сцене
                if (eventSystem == null)
                {
                    eventSystem = Object.FindFirstObjectByType<InventoryEventSystem>();
                    LogDebug($"Найден InventoryEventSystem в сцене: {(eventSystem != null ? eventSystem.name : "не найден")}");
                }
                
                // Если все еще не найден, создаем новый
                if (eventSystem == null)
                {
                    LogWarning("InventoryEventSystem не найден, создаем новый...");
                    GameObject eventSystemGO = new GameObject("InventoryEventSystem");
                    eventSystem = eventSystemGO.AddComponent<InventoryEventSystem>();
                }
            }
            
            // Создаем слоты если их нет
            if (inventorySlots.Count == 0)
            {
                CreateDefaultSlots();
            }
            
            // Создаем UI если его нет
            if (slotUIs.Count == 0)
            {
                CreateSlotUIs();
            }
            
            // Подписываемся на события
            if (eventSystem != null)
            {
                eventSystem.OnInventoryChanged += HandleInventoryChanged;
                eventSystem.OnItemAdded += HandleItemAdded;
                eventSystem.OnItemRemoved += HandleItemRemoved;
                LogDebug($"Подписались на события InventoryEventSystem: {eventSystem.name}");
            }
            
            LogDebug($"OptimizedInventoryManager инициализирован: {inventorySlots.Count} слотов");
            UpdateInventoryUI(); // <-- вызвать после инициализации
        }
        
        protected override InventoryManagerConfig CreateDefaultConfiguration()
        {
            return new InventoryManagerConfig();
        }
        
        public override void OnCleanup()
        {
            // Отписываемся от событий
            if (eventSystem != null)
            {
                eventSystem.OnInventoryChanged -= HandleInventoryChanged;
                eventSystem.OnItemAdded -= HandleItemAdded;
                eventSystem.OnItemRemoved -= HandleItemRemoved;
            }
            
            base.OnCleanup();
        }
        
        #region IInventoryManager Implementation
        
        public IInventorySlot GetSlot(int index)
        {
            if (index >= 0 && index < inventorySlots.Count)
            {
                return inventorySlots[index];
            }
            LogWarning($"Попытка получить слот с неверным индексом: {index}");
            return null;
        }
        
        public IReadOnlyList<IInventorySlot> GetAllSlots()
        {
            return inventorySlots.AsReadOnly();
        }
        
        public bool AddItem(IItem item, int amount = 1)
        {
            // Гарантируем, что менеджер инициализирован и слоты созданы
            EnsureInitialized();
            if (inventorySlots == null || inventorySlots.Count == 0)
            {
                LogWarning("Слоты ещё не созданы. Создаём слоты по умолчанию перед добавлением предмета.");
                CreateDefaultSlots();
                if (slotUIs != null && slotUIs.Count == 0)
                {
                    CreateSlotUIs();
                }
            }

            if (item == null)
            {
                LogWarning("Попытка добавить null предмет");
                return false;
            }
            
            LogDebug($"Добавление предмета: {item.ItemName} x{amount}");
            
            int originalAmount = amount;
            // Ищем слот с таким же предметом для стака
            int existingSlotIndex = FindSlotWithItem(item);
            LogDebug($"Найден существующий слот с предметом: {existingSlotIndex}");
            
            if (existingSlotIndex >= 0 && item.MaxStackSize > 1)
            {
                var existingSlot = inventorySlots[existingSlotIndex];
                int spaceInStack = item.MaxStackSize - existingSlot.Quantity;
                int amountToAdd = Mathf.Min(amount, spaceInStack);
                
                if (amountToAdd > 0)
                {
                    existingSlot.AddItems(item, amountToAdd);
                    LogDebug($"Добавлено в существующий слот {existingSlotIndex}: {amountToAdd}");
                    amount -= amountToAdd;
                }
            }
            
            // Если остались предметы, ищем пустые слоты
            while (amount > 0)
            {
                int emptySlotIndex = FindEmptySlot();
                LogDebug($"Найден пустой слот: {emptySlotIndex}");
                
                if (emptySlotIndex < 0)
                {
                    LogWarning("Нет свободного места в инвентаре");
                    return false;
                }
                
                int amountToAdd = Mathf.Min(amount, item.MaxStackSize);
                var result = inventorySlots[emptySlotIndex].AddItems(item, amountToAdd);
                LogDebug($"Попытка добавить в слот {emptySlotIndex}: {amountToAdd}, результат: {result} осталось");
                amount = result;
                
                LogDebug($"Добавлено в новый слот {emptySlotIndex}: {amountToAdd}");
            }
            
            UpdateInventoryUI();
            // Логирование состояния всех слотов после добавления
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                var slot = inventorySlots[i];
                LogDebug($"[DIAG] Слот {i}: IsEmpty={slot.IsEmpty}, Quantity={slot.Quantity}, Item={slot.Item?.ItemName ?? "null"}");
            }
            return true;
        }
        
        public bool RemoveItem(IItem item, int amount = 1)
        {
            if (item == null)
            {
                LogWarning("Попытка удалить null предмет");
                return false;
            }
            
            LogDebug($"Удаление предмета: {item.ItemName} x{amount}");
            
            int remainingToRemove = amount;
            int slotIndex = FindSlotWithItem(item);
            
            while (remainingToRemove > 0 && slotIndex >= 0)
            {
                var slot = inventorySlots[slotIndex];
                int amountInSlot = slot.Quantity;
                int amountToRemove = Mathf.Min(remainingToRemove, amountInSlot);
                
                slot.RemoveItems(amountToRemove);
                remainingToRemove -= amountToRemove;
                
                if (slot.IsEmpty)
                {
                    slot.Clear();
                }
                
                slotIndex = FindSlotWithItem(item);
            }
            
            if (remainingToRemove > 0)
            {
                LogWarning($"Не удалось удалить {remainingToRemove} предметов {item.ItemName}");
                return false;
            }
            
            UpdateInventoryUI();
            return true;
        }
        
        public bool RemoveItemFromSlot(int slotIndex, int amount = 1)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
            {
                LogWarning($"Попытка удалить из неверного слота: {slotIndex}");
                return false;
            }
            
            var slot = inventorySlots[slotIndex];
            if (slot.IsEmpty)
            {
                LogWarning($"Слот {slotIndex} пустой");
                return false;
            }
            
            LogDebug($"Удаление из слота {slotIndex}: {slot.Item.ItemName} x{amount}");
            
            int removed = slot.RemoveItems(amount);
            if (removed > 0 && slot.IsEmpty)
            {
                slot.Clear();
            }
            
            UpdateInventoryUI();
            return removed > 0;
        }
        
        public bool HasItem(IItem item, int amount = 1)
        {
            if (item == null) return false;
            
            int totalCount = 0;
            foreach (var slot in inventorySlots)
            {
                if (!slot.IsEmpty && slot.Item.ItemName == item.ItemName)
                {
                    totalCount += slot.Quantity;
                    if (totalCount >= amount)
                        return true;
                }
            }
            
            return false;
        }
        
        public int GetItemCount(IItem item)
        {
            if (item == null) return 0;
            
            int totalCount = 0;
            foreach (var slot in inventorySlots)
            {
                if (!slot.IsEmpty && slot.Item.ItemName == item.ItemName)
                {
                    totalCount += slot.Quantity;
                }
            }
            
            return totalCount;
        }
        
        public int FindEmptySlot()
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                LogDebug($"Проверка слота {i}: IsEmpty = {inventorySlots[i].IsEmpty}");
                if (inventorySlots[i].IsEmpty)
                    return i;
            }
            return -1;
        }
        
        public int FindSlotWithItem(IItem item)
        {
            if (item == null) return -1;
            
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (!inventorySlots[i].IsEmpty && inventorySlots[i].Item.ItemName == item.ItemName)
                    return i;
            }
            return -1;
        }
        
        public bool HasFreeSpace()
        {
            return FindEmptySlot() >= 0;
        }
        
        public int GetFreeSlotCount()
        {
            int count = 0;
            foreach (var slot in inventorySlots)
            {
                if (slot.IsEmpty)
                    count++;
            }
            return count;
        }
        
        public int GetTotalItemCount()
        {
            int total = 0;
            foreach (var slot in inventorySlots)
            {
                if (!slot.IsEmpty)
                    total += slot.Quantity;
            }
            return total;
        }
        
        public void ClearInventory()
        {
            LogDebug("Очистка инвентаря");
            foreach (var slot in inventorySlots)
            {
                slot.Clear();
            }
            UpdateInventoryUI();
        }
        
        public void UpdateUI()
        {
            UpdateInventoryUI();
        }
        
        #endregion
        
        #region UI Management
        
        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(isInventoryOpen);
            }
            UpdateInventoryUI();
            
            LogDebug($"Инвентарь {(isInventoryOpen ? "открыт" : "закрыт")}");
        }
        
        /// <summary>
        /// Открыть панель инвентаря
        /// </summary>
        public void OpenInventory()
        {
            isInventoryOpen = true;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
            }
            
            LogDebug("Инвентарь открыт");
        }
        
        /// <summary>
        /// Закрыть панель инвентаря
        /// </summary>
        public void CloseInventory()
        {
            isInventoryOpen = false;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            
            LogDebug("Инвентарь закрыт");
        }
        
                private void UpdateInventoryUI()
        {
            LogDebug($"Обновление UI: {slotUIs.Count} UI слотов, {inventorySlots.Count} слотов данных");
            
            // Если UI ещё не создан, создадим его на лету, чтобы изменения были видны сразу
            if ((slotUIs == null || slotUIs.Count == 0) && inventoryPanel != null)
            {
                LogDebug("UI слотов нет — создаём их перед обновлением UI");
                CreateSlotUIs();
            }
            
            for (int i = 0; i < slotUIs.Count && i < inventorySlots.Count; i++)
            {
                var slot = inventorySlots[i];
                var slotUI = slotUIs[i];
                
                LogDebug($"Обновление UI слота {i}: IsEmpty = {slot.IsEmpty}");
                
                if (!slot.IsEmpty)
                {
                    slotUI.UpdateSlotUI(slot);
                    LogDebug($"Слот {i + 1} содержит {slot.Quantity}x {slot.Item.ItemName}");
                    
                    // Дополнительная проверка dragHandler
                    var dragHandler = slotUI.GetComponent<InventorySlotDragHandler>();
                    if (dragHandler != null)
                    {
                        LogDebug($"Проверка dragHandler слота {i}: item={dragHandler.item?.itemName ?? "null"}, prefab={dragHandler.dragPrefab?.name ?? "null"}");
                    }
                }
                else
                {
                    slotUI.UpdateSlotUI(null);
                    LogDebug($"Слот {i + 1} очищен (пустой)");
                }
            }
            
            UpdateInventoryCounter();
            LogDebug("Обновление завершено");
        }
        
        private void UpdateInventoryCounter()
        {
            if (inventoryCounterText != null)
            {
                int totalItems = GetTotalItemCount();
                int totalSlots = inventorySlots.Count;
                inventoryCounterText.text = $"{totalItems}/{totalSlots}";
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleInventoryChanged()
        {
            LogDebug("[InventoryEventSystem] InvokeInventoryChanged");
            OnInventoryChanged?.Invoke();
        }
        
        private void HandleItemAdded(IItem item, int amount)
        {
            LogDebug($"[InventoryEventSystem] InvokeItemAdded: {item.ItemName} x{amount}");
            OnItemAdded?.Invoke(item, amount);
        }
        
        private void HandleItemRemoved(IItem item, int amount)
        {
            LogDebug($"[InventoryEventSystem] InvokeItemRemoved: {item.ItemName} x{amount}");
            OnItemRemoved?.Invoke(item, amount);
        }
        
        #endregion
        
        #region Private Methods
        
        private void CreateDefaultSlots()
        {
            LogDebug("Создание слотов по умолчанию");
            inventorySlots.Clear(); // Очищаем список перед созданием
            // Создаем 8 оптимизированных слотов
            for (int i = 0; i < 8; i++)
            {
                var slotGo = new GameObject($"InventorySlot_{i + 1}");
                slotGo.transform.SetParent(transform, false);
                var slot = slotGo.AddComponent<OptimizedInventorySlot>();
                inventorySlots.Add(slot);
            }
            LogDebug($"Создано {inventorySlots.Count} слотов");
        }
        
        private void CreateSlotUIs()
        {
            LogDebug("Создание UI слотов");
            
            // Находим родительский объект для UI слотов
            Transform slotsParent = inventoryPanel?.transform;
            if (slotsParent == null)
            {
                LogWarning("Не найден родительский объект для UI слотов");
                return;
            }
            
            // 1) Пробуем использовать уже существующие слоты UI в панели
            var existingSlotUIs = slotsParent.GetComponentsInChildren<InventorySlotUI>(true);
            if (existingSlotUIs != null && existingSlotUIs.Length > 0)
            {
                LogDebug($"Найдено существующих UI слотов: {existingSlotUIs.Length}");
                slotUIs.Clear();
                slotUIs.AddRange(existingSlotUIs);
            }
            
            // 2) Если слоты UI отсутствуют или их меньше, чем слотов данных — создаем недостающие
            for (int i = slotUIs.Count; i < inventorySlots.Count; i++)
            {
                var slotUI = UIFactory.CreateInventorySlotUI(slotsParent);
                slotUIs.Add(slotUI);
            }
            
            // 3) Инициализируем каждый UI слот как пустой до первой синхронизации данных
            for (int i = 0; i < slotUIs.Count; i++)
            {
                slotUIs[i].UpdateSlotUI(null);
            }
            
            LogDebug($"Создано {slotUIs.Count} UI слотов");
        }
        
        #endregion
        
        #region Context Menu Methods
        
        [ContextMenu("Создать слоты инвентаря")]
        public void ForceCreateSlots()
        {
            CreateDefaultSlots();
            CreateSlotUIs();
            UpdateInventoryUI();
        }
        
        [ContextMenu("Обновить отображение количества")]
        public void ForceUpdateAllSlotQuantities()
        {
            UpdateInventoryUI();
        }
        
        [ContextMenu("Проверить состояние инвентаря")]
        public void CheckInventoryState()
        {
            LogDebug("=== ПРОВЕРКА СОСТОЯНИЯ ИНВЕНТАРЯ ===");
            LogDebug($"Всего слотов: {inventorySlots.Count}");
            LogDebug($"UI слотов: {slotUIs.Count}");
            LogDebug($"Свободных слотов: {GetFreeSlotCount()}");
            LogDebug($"Всего предметов: {GetTotalItemCount()}");
            LogDebug($"Инвентарь открыт: {isInventoryOpen}");
            
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                var slot = inventorySlots[i];
                if (!slot.IsEmpty)
                {
                    LogDebug($"Слот {i + 1}: {slot.Quantity}x {slot.Item.ItemName}");
                }
                else
                {
                    LogDebug($"Слот {i + 1}: пустой");
                }
            }
            
            LogDebug("=== ПРОВЕРКА ЗАВЕРШЕНА ===");
        }
        
        [ContextMenu("Найти Event System")]
        public void FindEventSystem()
        {
            LogDebug("=== ПОИСК EVENT SYSTEM ===");
            
            // Ищем в текущем GameObject
            var localEventSystem = GetComponent<InventoryEventSystem>();
            if (localEventSystem != null)
            {
                LogDebug($"✅ Найден InventoryEventSystem на текущем GameObject: {localEventSystem.name}");
                eventSystem = localEventSystem;
                return;
            }
            
            // Ищем в сцене
            var sceneEventSystem = Object.FindFirstObjectByType<InventoryEventSystem>();
            if (sceneEventSystem != null)
            {
                LogDebug($"✅ Найден InventoryEventSystem в сцене: {sceneEventSystem.name}");
                eventSystem = sceneEventSystem;
                return;
            }
            
            // Создаем новый
            LogWarning("❌ InventoryEventSystem не найден, создаем новый...");
            GameObject eventSystemGO = new GameObject("InventoryEventSystem");
            eventSystem = eventSystemGO.AddComponent<InventoryEventSystem>();
            LogDebug($"✅ Создан новый InventoryEventSystem: {eventSystem.name}");
            
            LogDebug("=== ПОИСК ЗАВЕРШЕН ===");
        }
        
        #endregion
    }
} 