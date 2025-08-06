using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Скрипт для размещенных предметов с возможностью удаления
/// </summary>
public class PlacedItem : MonoBehaviour
{
    [Header("Настройки предмета")]
    [SerializeField] private Item itemData;
    [SerializeField] private bool canBeRemoved = true;
    [SerializeField] private Key removeKey = Key.Delete;
    
    [Header("UI")]
    [SerializeField] private bool showRemovePrompt = true;
    [SerializeField] private string removePromptText = "Нажмите Delete для удаления";
    
    private bool isHighlighted = false;
    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Color highlightColor = Color.yellow;
    
    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalMaterials[i] = renderers[i].material;
            }
        }
    }
    
    void Update()
    {
        if (canBeRemoved && Keyboard.current != null && Keyboard.current[removeKey].wasPressedThisFrame)
        {
            RemoveItem();
        }
    }
    
    void OnMouseEnter()
    {
        if (canBeRemoved)
        {
            HighlightItem(true);
        }
    }
    
    void OnMouseExit()
    {
        if (canBeRemoved)
        {
            HighlightItem(false);
        }
    }
    
    /// <summary>
    /// Устанавливает данные предмета
    /// </summary>
    public void SetItemData(Item item)
    {
        itemData = item;
    }
    
    /// <summary>
    /// Получает данные предмета
    /// </summary>
    public Item GetItemData()
    {
        return itemData;
    }
    
    /// <summary>
    /// Подсвечивает предмет
    /// </summary>
    private void HighlightItem(bool highlight)
    {
        if (isHighlighted == highlight) return;
        
        isHighlighted = highlight;
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                if (highlight)
                {
                    // Создаем подсвеченный материал
                    Material highlightMaterial = new Material(originalMaterials[i]);
                    highlightMaterial.color = highlightColor;
                    renderers[i].material = highlightMaterial;
                }
                else
                {
                    // Возвращаем оригинальный материал
                    renderers[i].material = originalMaterials[i];
                }
            }
        }
    }
    
    /// <summary>
    /// Удаляет предмет
    /// </summary>
    public void RemoveItem()
    {
        if (!canBeRemoved) return;
        
        Debug.Log($"Удаление предмета: {itemData?.itemName ?? "Неизвестный предмет"}");
        
        // Убираем из счетчика размещенных предметов
        if (PlacedItemsCounter.GetInstance() != null && itemData != null)
        {
            PlacedItemsCounter.GetInstance().RemovePlacedItem(itemData);
        }
        
        // Возвращаем предмет в инвентарь (опционально)
        if (InventoryManager.Instance != null && itemData != null)
        {
            InventoryManager.Instance.AddItem(itemData, 1);
            Debug.Log($"Предмет {itemData.itemName} возвращен в инвентарь");
        }
        
        // Уничтожаем объект
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Устанавливает возможность удаления
    /// </summary>
    public void SetCanBeRemoved(bool canRemove)
    {
        canBeRemoved = canRemove;
    }
    
    /// <summary>
    /// Устанавливает клавишу удаления
    /// </summary>
    public void SetRemoveKey(Key key)
    {
        removeKey = key;
    }
    
    void OnGUI()
    {
        if (showRemovePrompt && canBeRemoved && isHighlighted)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50, 100, 20), removePromptText);
        }
    }
} 