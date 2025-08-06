using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Logging;
using InventorySystem.Factories;

namespace InventorySystem.Adapters
{
    /// <summary>
    /// Адаптер для совместимости старого InventoryManager с новой архитектурой
    /// </summary>
    public class InventoryManagerAdapter : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private InventoryManager oldInventoryManager;
        [SerializeField] private OptimizedInventoryManager newInventoryManager;
        
        [Header("Настройки миграции")]
        [SerializeField] private bool autoMigrate = true;
        [SerializeField] private bool preserveOldManager = true;
        
        private bool isMigrated = false;
        
        void Awake()
        {
            if (autoMigrate)
            {
                MigrateToNewArchitecture();
            }
        }
        
        /// <summary>
        /// Мигрировать на новую архитектуру
        /// </summary>
        [ContextMenu("Мигрировать на новую архитектуру")]
        public void MigrateToNewArchitecture()
        {
            if (isMigrated)
            {
                UnityEngine.Debug.LogWarning("Миграция уже выполнена");
                return;
            }
            
            UnityEngine.Debug.Log("=== НАЧАЛО МИГРАЦИИ INVENTORY MANAGER ===");
            
            // Находим старый InventoryManager если не указан
            if (oldInventoryManager == null)
            {
                oldInventoryManager = Object.FindFirstObjectByType<InventoryManager>();
                if (oldInventoryManager == null)
                {
                    UnityEngine.Debug.LogError("Не найден старый InventoryManager для миграции");
                    return;
                }
            }
            
            // Создаем новый OptimizedInventoryManager
            if (newInventoryManager == null)
            {
                newInventoryManager = gameObject.AddComponent<OptimizedInventoryManager>();
            }
            
            // Копируем настройки
            CopySettings();
            
            // Копируем данные
            CopyData();
            
            // Настраиваем события
            SetupEvents();
            
            // Скрываем старый менеджер если нужно
            if (!preserveOldManager)
            {
                oldInventoryManager.gameObject.SetActive(false);
            }
            
            isMigrated = true;
            UnityEngine.Debug.Log("=== МИГРАЦИЯ INVENTORY MANAGER ЗАВЕРШЕНА ===");
        }
        
        /// <summary>
        /// Копировать настройки из старого менеджера
        /// </summary>
        private void CopySettings()
        {
            UnityEngine.Debug.Log("Копирование настроек...");
            
            // Здесь можно добавить копирование специфичных настроек
            // если они понадобятся в новой архитектуре
            
            UnityEngine.Debug.Log("Настройки скопированы");
        }
        
        /// <summary>
        /// Копировать данные из старого менеджера
        /// </summary>
        private void CopyData()
        {
            UnityEngine.Debug.Log("Копирование данных...");
            
            // Копируем предметы из старого инвентаря
            for (int i = 0; i < oldInventoryManager.SlotCount; i++)
            {
                var oldSlot = oldInventoryManager.GetSlot(i);
                if (oldSlot != null && !oldSlot.IsEmpty)
                {
                    // Получаем оригинальный Item из слота
                    var originalItem = oldSlot.Item as Item ?? (oldSlot.Item as ItemWrapper)?.GetOriginalItem();
                    if (originalItem != null)
                    {
                        var itemWrapper = InventoryFactory.CreateItemAdapter(originalItem);
                        newInventoryManager.AddItem(itemWrapper, oldSlot.Quantity);
                        UnityEngine.Debug.Log($"Скопирован предмет: {originalItem.itemName} x{oldSlot.Quantity}");
                    }
                }
            }
            
            UnityEngine.Debug.Log("Данные скопированы");
        }
        
        /// <summary>
        /// Настроить события для совместимости
        /// </summary>
        private void SetupEvents()
        {
            UnityEngine.Debug.Log("Настройка событий...");
            
            // Подписываемся на события нового менеджера
            newInventoryManager.OnInventoryChanged += () => {
                UnityEngine.Debug.Log("Событие: InventoryChanged");
                // Здесь можно добавить вызов старых событий если нужно
            };
            
            newInventoryManager.OnItemAdded += (item, amount) => {
                UnityEngine.Debug.Log($"Событие: ItemAdded - {item.ItemName} x{amount}");
                // Здесь можно добавить вызов старых событий если нужно
            };
            
            newInventoryManager.OnItemRemoved += (item, amount) => {
                UnityEngine.Debug.Log($"Событие: ItemRemoved - {item.ItemName} x{amount}");
                // Здесь можно добавить вызов старых событий если нужно
            };
            
            UnityEngine.Debug.Log("События настроены");
        }
        
        /// <summary>
        /// Получить новый менеджер инвентаря
        /// </summary>
        public OptimizedInventoryManager GetNewInventoryManager()
        {
            return newInventoryManager;
        }
        
        /// <summary>
        /// Получить старый менеджер инвентаря
        /// </summary>
        public InventoryManager GetOldInventoryManager()
        {
            return oldInventoryManager;
        }
        
        /// <summary>
        /// Проверить статус миграции
        /// </summary>
        [ContextMenu("Проверить статус миграции")]
        public void CheckMigrationStatus()
        {
            UnityEngine.Debug.Log("=== СТАТУС МИГРАЦИИ INVENTORY MANAGER ===");
            UnityEngine.Debug.Log($"Миграция выполнена: {isMigrated}");
            UnityEngine.Debug.Log($"Старый менеджер: {(oldInventoryManager != null ? "найден" : "не найден")}");
            UnityEngine.Debug.Log($"Новый менеджер: {(newInventoryManager != null ? "найден" : "не найден")}");
            UnityEngine.Debug.Log($"Автоматическая миграция: {autoMigrate}");
            UnityEngine.Debug.Log($"Сохранение старого менеджера: {preserveOldManager}");
            
            if (newInventoryManager != null)
            {
                UnityEngine.Debug.Log($"Слотов в новом менеджере: {newInventoryManager.SlotCount}");
                UnityEngine.Debug.Log($"Всего предметов: {newInventoryManager.GetTotalItemCount()}");
                UnityEngine.Debug.Log($"Свободных слотов: {newInventoryManager.GetFreeSlotCount()}");
            }
            
            UnityEngine.Debug.Log("=== ПРОВЕРКА ЗАВЕРШЕНА ===");
        }
        
        /// <summary>
        /// Откатить миграцию
        /// </summary>
        [ContextMenu("Откатить миграцию")]
        public void RollbackMigration()
        {
            if (!isMigrated)
            {
                UnityEngine.Debug.LogWarning("Миграция не была выполнена");
                return;
            }
            
            UnityEngine.Debug.Log("=== ОТКАТ МИГРАЦИИ INVENTORY MANAGER ===");
            
            // Восстанавливаем старый менеджер
            if (oldInventoryManager != null)
            {
                oldInventoryManager.gameObject.SetActive(true);
            }
            
            // Удаляем новый менеджер
            if (newInventoryManager != null)
            {
                DestroyImmediate(newInventoryManager);
                newInventoryManager = null;
            }
            
            isMigrated = false;
            UnityEngine.Debug.Log("=== ОТКАТ ЗАВЕРШЕН ===");
        }
    }
} 