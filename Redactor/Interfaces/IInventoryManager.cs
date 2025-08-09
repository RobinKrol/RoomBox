using System.Collections.Generic;

/// <summary>
/// Интерфейс для управления инвентарем
/// </summary>
public interface IInventoryManager
{
    /// <summary>
    /// Количество слотов в инвентаре
    /// </summary>
    int SlotCount { get; }
    
    /// <summary>
    /// Получить слот по индексу
    /// </summary>
    /// <param name="index">Индекс слота</param>
    /// <returns>Слот инвентаря</returns>
    IInventorySlot GetSlot(int index);
    
    /// <summary>
    /// Получить все слоты инвентаря
    /// </summary>
    /// <returns>Список всех слотов</returns>
    IReadOnlyList<IInventorySlot> GetAllSlots();
    
    /// <summary>
    /// Добавить предмет в инвентарь
    /// </summary>
    /// <param name="item">Предмет для добавления</param>
    /// <param name="amount">Количество предметов</param>
    /// <returns>true если предмет был добавлен</returns>
    bool AddItem(IItem item, int amount = 1);
    
    /// <summary>
    /// Удалить предмет из инвентаря
    /// </summary>
    /// <param name="item">Предмет для удаления</param>
    /// <param name="amount">Количество предметов</param>
    /// <returns>true если предмет был удален</returns>
    bool RemoveItem(IItem item, int amount = 1);
    
    /// <summary>
    /// Удалить предмет из конкретного слота
    /// </summary>
    /// <param name="slotIndex">Индекс слота</param>
    /// <param name="amount">Количество предметов</param>
    /// <returns>true если предмет был удален</returns>
    bool RemoveItemFromSlot(int slotIndex, int amount = 1);
    
    /// <summary>
    /// Проверить, есть ли предмет в инвентаре
    /// </summary>
    /// <param name="item">Предмет для проверки</param>
    /// <param name="amount">Минимальное количество</param>
    /// <returns>true если предмет есть в достаточном количестве</returns>
    bool HasItem(IItem item, int amount = 1);
    
    /// <summary>
    /// Получить количество конкретного предмета в инвентаре
    /// </summary>
    /// <param name="item">Предмет для подсчета</param>
    /// <returns>Количество предметов</returns>
    int GetItemCount(IItem item);
    
    /// <summary>
    /// Проверить, есть ли свободное место в инвентаре
    /// </summary>
    /// <returns>true если есть свободные слоты</returns>
    bool HasFreeSpace();
    
    /// <summary>
    /// Получить количество свободных слотов
    /// </summary>
    /// <returns>Количество свободных слотов</returns>
    int GetFreeSlotCount();
    
    /// <summary>
    /// Получить общее количество предметов в инвентаре
    /// </summary>
    /// <returns>Общее количество предметов</returns>
    int GetTotalItemCount();
    
    /// <summary>
    /// Очистить весь инвентарь
    /// </summary>
    void ClearInventory();
    
    /// <summary>
    /// Найти пустой слот
    /// </summary>
    /// <returns>Индекс пустого слота или -1 если нет свободных слотов</returns>
    int FindEmptySlot();
    
    /// <summary>
    /// Найти слот с конкретным предметом
    /// </summary>
    /// <param name="item">Предмет для поиска</param>
    /// <returns>Индекс слота или -1 если предмет не найден</returns>
    int FindSlotWithItem(IItem item);
    
    /// <summary>
    /// Обновить UI инвентаря
    /// </summary>
    void UpdateUI();
    
    /// <summary>
    /// Событие, вызываемое при изменении инвентаря
    /// </summary>
    event System.Action OnInventoryChanged;
    
    /// <summary>
    /// Событие, вызываемое при добавлении предмета
    /// </summary>
    event System.Action<IItem, int> OnItemAdded;
    
    /// <summary>
    /// Событие, вызываемое при удалении предмета
    /// </summary>
    event System.Action<IItem, int> OnItemRemoved;
} 