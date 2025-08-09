using UnityEngine;

/// <summary>
/// Интерфейс для UI компонента слота инвентаря
/// </summary>
public interface IInventorySlotUI
{
    /// <summary>
    /// Индекс слота в инвентаре
    /// </summary>
    int SlotIndex { get; set; }
    
    /// <summary>
    /// Обновить отображение слота
    /// </summary>
    /// <param name="slot">Данные слота для отображения</param>
    void UpdateSlotUI(IInventorySlot slot);
    
    /// <summary>
    /// Показать слот как активный (выбранный)
    /// </summary>
    void SetActive(bool isActive);
    
    /// <summary>
    /// Показать слот как доступный для перетаскивания
    /// </summary>
    /// <param name="isDraggable">Можно ли перетаскивать</param>
    void SetDraggable(bool isDraggable);
    
    /// <summary>
    /// Показать подсветку слота
    /// </summary>
    /// <param name="color">Цвет подсветки</param>
    void SetHighlight(Color color);
    
    /// <summary>
    /// Убрать подсветку слота
    /// </summary>
    void ClearHighlight();
    
    /// <summary>
    /// Показать анимацию при добавлении предмета
    /// </summary>
    void PlayAddItemAnimation();
    
    /// <summary>
    /// Показать анимацию при удалении предмета
    /// </summary>
    void PlayRemoveItemAnimation();
    
    /// <summary>
    /// Событие, вызываемое при клике на слот
    /// </summary>
    event System.Action<int> OnSlotClicked;
    
    /// <summary>
    /// Событие, вызываемое при начале перетаскивания
    /// </summary>
    event System.Action<int> OnDragStarted;
    
    /// <summary>
    /// Событие, вызываемое при окончании перетаскивания
    /// </summary>
    event System.Action<int> OnDragEnded;
} 