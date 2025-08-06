/// <summary>
/// Интерфейс для системы событий инвентаря
/// </summary>
public interface IInventoryEventSystem
{
    /// <summary>
    /// Событие изменения инвентаря
    /// </summary>
    event System.Action OnInventoryChanged;
    
    /// <summary>
    /// Событие добавления предмета
    /// </summary>
    event System.Action<IItem, int> OnItemAdded;
    
    /// <summary>
    /// Событие удаления предмета
    /// </summary>
    event System.Action<IItem, int> OnItemRemoved;
    
    /// <summary>
    /// Событие изменения слота
    /// </summary>
    event System.Action<int, IInventorySlot> OnSlotChanged;
    
    /// <summary>
    /// Событие начала перетаскивания
    /// </summary>
    event System.Action<IItem, int> OnDragStarted;
    
    /// <summary>
    /// Событие окончания перетаскивания
    /// </summary>
    event System.Action<IItem, int, bool> OnDragEnded; // bool - успешно ли размещен
    
    /// <summary>
    /// Событие размещения предмета в мире
    /// </summary>
    event System.Action<IItem, UnityEngine.Vector3, UnityEngine.Quaternion> OnItemPlaced;
    
    /// <summary>
    /// Событие ошибки размещения
    /// </summary>
    event System.Action<IItem, string> OnPlacementError;
    
    /// <summary>
    /// Событие открытия/закрытия инвентаря
    /// </summary>
    event System.Action<bool> OnInventoryToggled;
    
    /// <summary>
    /// Вызвать событие изменения инвентаря
    /// </summary>
    void InvokeInventoryChanged();
    
    /// <summary>
    /// Вызвать событие добавления предмета
    /// </summary>
    void InvokeItemAdded(IItem item, int amount);
    
    /// <summary>
    /// Вызвать событие удаления предмета
    /// </summary>
    void InvokeItemRemoved(IItem item, int amount);
    
    /// <summary>
    /// Вызвать событие изменения слота
    /// </summary>
    void InvokeSlotChanged(int slotIndex, IInventorySlot slot);
    
    /// <summary>
    /// Вызвать событие начала перетаскивания
    /// </summary>
    void InvokeDragStarted(IItem item, int slotIndex);
    
    /// <summary>
    /// Вызвать событие окончания перетаскивания
    /// </summary>
    void InvokeDragEnded(IItem item, int slotIndex, bool wasPlaced);
    
    /// <summary>
    /// Вызвать событие размещения предмета
    /// </summary>
    void InvokeItemPlaced(IItem item, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation);
    
    /// <summary>
    /// Вызвать событие ошибки размещения
    /// </summary>
    void InvokePlacementError(IItem item, string errorMessage);
    
    /// <summary>
    /// Вызвать событие переключения инвентаря
    /// </summary>
    void InvokeInventoryToggled(bool isOpen);
} 