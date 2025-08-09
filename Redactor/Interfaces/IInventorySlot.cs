/// <summary>
/// Интерфейс для слота инвентаря
/// </summary>
public interface IInventorySlot
{
    /// <summary>
    /// Предмет в слоте
    /// </summary>
    IItem Item { get; }
    
    /// <summary>
    /// Количество предметов в слоте
    /// </summary>
    int Quantity { get; }
    
    /// <summary>
    /// Проверяет, пуст ли слот
    /// </summary>
    bool IsEmpty { get; }
    
    /// <summary>
    /// Проверяет, можно ли добавить еще предметы в этот слот
    /// </summary>
    bool CanAddMore { get; }
    
    /// <summary>
    /// Максимальное количество предметов в слоте
    /// </summary>
    int MaxStackSize { get; }
    
    /// <summary>
    /// Добавляет предметы в слот
    /// </summary>
    /// <param name="item">Предмет для добавления</param>
    /// <param name="amount">Количество для добавления</param>
    /// <returns>Количество предметов, которые не поместились</returns>
    int AddItems(IItem item, int amount = 1);
    
    /// <summary>
    /// Удаляет предметы из слота
    /// </summary>
    /// <param name="amount">Количество для удаления</param>
    /// <returns>Количество фактически удаленных предметов</returns>
    int RemoveItems(int amount = 1);
    
    /// <summary>
    /// Очищает слот
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Получает свободное место в слоте
    /// </summary>
    /// <returns>Количество предметов, которые можно добавить</returns>
    int GetFreeSpace();
    
    /// <summary>
    /// Проверяет, можно ли добавить указанное количество предметов
    /// </summary>
    /// <param name="amount">Количество для проверки</param>
    /// <returns>true если можно добавить</returns>
    bool CanAddAmount(int amount);
    
    /// <summary>
    /// Проверяет, можно ли сложить предметы с другим слотом
    /// </summary>
    /// <param name="otherSlot">Другой слот для проверки</param>
    /// <returns>true если можно сложить</returns>
    bool CanStackWith(IInventorySlot otherSlot);
    
    /// <summary>
    /// Создает копию слота
    /// </summary>
    /// <returns>Новый слот с теми же данными</returns>
    IInventorySlot Clone();
} 