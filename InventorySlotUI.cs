using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Управляет UI отдельного слота инвентаря
/// </summary>
public class InventorySlotUI : MonoBehaviour, IInventorySlotUI
{
    [Header("UI элементы")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private InventorySlotDragHandler dragHandler;
    
    private InventorySlot currentSlot;
    private int slotIndex = -1;
    
    // События интерфейса IInventorySlotUI
    public event System.Action<int> OnSlotClicked;
    public event System.Action<int> OnDragStarted;
    public event System.Action<int> OnDragEnded;
    
    public int SlotIndex 
    { 
        get => slotIndex; 
        set => slotIndex = value; 
    }
    
    void Awake()
    {
        // Автоматически находим компоненты, если они не назначены
        if (itemIcon == null)
            itemIcon = GetComponent<Image>();
        
        if (dragHandler == null)
            dragHandler = GetComponent<InventorySlotDragHandler>();
        
        // Автоматически находим TextMeshProUGUI компонент в дочерних объектах
        if (quantityText == null)
        {
            quantityText = GetComponentInChildren<TextMeshProUGUI>();
            if (quantityText != null)
            {
                Debug.Log($"Автоматически найден TextMeshProUGUI в слоте {gameObject.name}: {quantityText.name}");
            }
            else
            {
                Debug.LogWarning($"Не найден TextMeshProUGUI в слоте {gameObject.name}");
            }
        }
        ClearSlot(); // <-- инициализация UI как пустого
    }
    
    /// <summary>
    /// Обновляет UI слота (для обратной совместимости)
    /// </summary>
    public void UpdateSlotUI(InventorySlot slot)
    {
        currentSlot = slot;
        
        if (slot == null || slot.IsEmpty)
        {
            ClearSlot();
            return;
        }
        
        // Проверяем, что у слота есть предмет
        if (slot.Item == null)
        {
            Debug.LogWarning($"Слот содержит null предмет: {gameObject.name}");
            ClearSlot();
            return;
        }
        
        // Обновляем иконку
        if (itemIcon != null)
        {
            itemIcon.sprite = slot.Item.Icon;
            itemIcon.enabled = true;
            // Восстанавливаем непрозрачность для слота с предметом
            itemIcon.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"itemIcon не найден в слоте {gameObject.name}");
        }
        
        // Обновляем drag handler
        if (dragHandler != null)
        {
            // Конвертируем IItem в Item для dragHandler
            Item originalItem = slot.Item as Item ?? (slot.Item as ItemWrapper)?.GetOriginalItem();
            if (originalItem != null)
            {
                dragHandler.item = originalItem;
                dragHandler.dragPrefab = originalItem.prefab;
            }
            else
            {
                Debug.LogWarning($"Не удалось получить оригинальный Item для dragHandler в слоте {gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"dragHandler не найден в слоте {gameObject.name}");
        }
        
        // Обновляем текст количества
        UpdateQuantityText(slot.Quantity);
    }
    
    /// <summary>
    /// Обновляет UI слота (реализация интерфейса IInventorySlotUI)
    /// </summary>
    public void UpdateSlotUI(IInventorySlot slot)
    {
        if (slot == null || slot.IsEmpty)
        {
            ClearSlot();
            return;
        }
        
        // Конвертируем IInventorySlot в InventorySlot если нужно
        InventorySlot originalSlot = slot as InventorySlot ?? (slot as InventorySlotAdapter)?.GetOriginalSlot();
        if (originalSlot != null)
        {
            UpdateSlotUI(originalSlot);
        }
        else
        {
            // Обновляем UI напрямую из IInventorySlot
            if (itemIcon != null && slot.Item != null)
            {
                itemIcon.sprite = slot.Item.Icon;
                itemIcon.enabled = true;
                // Восстанавливаем непрозрачность для слота с предметом
                itemIcon.color = Color.white;
            }
            
            UpdateQuantityText(slot.Quantity);
        }
    }
    
    /// <summary>
    /// Обновляет текст количества
    /// </summary>
    private void UpdateQuantityText(int quantity)
    {
        if (quantityText == null)
        {
            // Попробуем найти компонент еще раз
            quantityText = GetComponentInChildren<TextMeshProUGUI>();
            if (quantityText == null)
            {
                Debug.LogWarning($"Не удалось найти TextMeshProUGUI в слоте {gameObject.name}");
                return;
            }
        }
        
        if (quantity > 1)
        {
            quantityText.text = quantity.ToString();
            quantityText.enabled = true;
        }
        else
        {
            quantityText.text = "";
            quantityText.enabled = false;
        }
    }
    
    /// <summary>
    /// Принудительно обновляет отображение количества
    /// </summary>
    public void ForceUpdateQuantity()
    {
        if (currentSlot != null && !currentSlot.IsEmpty)
        {
            UpdateQuantityText(currentSlot.Quantity);
        }
    }
    
    /// <summary>
    /// Очищает слот
    /// </summary>
    public void ClearSlot()
    {
        currentSlot = null;
        
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            // Делаем фон прозрачным для пустого слота
            itemIcon.color = new Color(1f, 1f, 1f, 0f);
        }
        
        if (quantityText != null)
        {
            quantityText.enabled = false;
            quantityText.text = "";
        }
        
        if (dragHandler != null)
        {
            dragHandler.item = null;
            dragHandler.dragPrefab = null;
        }
    }
    
    /// <summary>
    /// Получает текущий слот (для обратной совместимости)
    /// </summary>
    public InventorySlot GetCurrentSlot()
    {
        return currentSlot;
    }
    
    /// <summary>
    /// Проверяет, пуст ли слот (для обратной совместимости)
    /// </summary>
    public bool IsEmpty()
    {
        return currentSlot == null || currentSlot.IsEmpty;
    }
    
    // Реализация интерфейса IInventorySlotUI
    
    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
    
    public void SetDraggable(bool isDraggable)
    {
        if (dragHandler != null)
        {
            dragHandler.enabled = isDraggable;
        }
    }
    
    public void SetHighlight(Color color)
    {
        if (itemIcon != null)
        {
            itemIcon.color = color;
        }
    }
    
    public void ClearHighlight()
    {
        if (itemIcon != null)
        {
            itemIcon.color = Color.white;
        }
    }
    
    public void PlayAddItemAnimation()
    {
        // Простая анимация добавления предмета
        if (itemIcon != null)
        {
            itemIcon.transform.localScale = Vector3.zero;
            StartCoroutine(AnimateScale(itemIcon.transform, Vector3.one, 0.3f));
        }
    }
    
    public void PlayRemoveItemAnimation()
    {
        // Простая анимация удаления предмета
        if (itemIcon != null)
        {
            StartCoroutine(AnimateScale(itemIcon.transform, Vector3.zero, 0.2f));
        }
    }
    
    /// <summary>
    /// Вызывает событие начала перетаскивания (для использования из InventorySlotDragHandler)
    /// </summary>
    public void TriggerOnDragStarted()
    {
        OnDragStarted?.Invoke(SlotIndex);
    }
    
    /// <summary>
    /// Вызывает событие окончания перетаскивания (для использования из InventorySlotDragHandler)
    /// </summary>
    public void TriggerOnDragEnded()
    {
        OnDragEnded?.Invoke(SlotIndex);
    }
    
    /// <summary>
    /// Вызывает событие клика по слоту (для использования из других компонентов)
    /// </summary>
    public void TriggerOnSlotClicked()
    {
        OnSlotClicked?.Invoke(SlotIndex);
    }
    
    private System.Collections.IEnumerator AnimateScale(Transform target, Vector3 endScale, float duration)
    {
        Vector3 startScale = target.localScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            target.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }
        
        target.localScale = endScale;
    }
} 