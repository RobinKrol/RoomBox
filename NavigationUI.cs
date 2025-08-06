using UnityEngine;
using TMPro;

/// <summary>
/// Управляет UI текстом с подсказками навигации
/// </summary>
public class NavigationUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private TextMeshProUGUI navigationText;
    
    [Header("Тексты подсказок")]
    [SerializeField] private string validPlacementText = "ЛКМ - разместить, ПКМ - повернуть, Колесико - отменить";
    [SerializeField] private string invalidPlacementText = "Нельзя разместить здесь";
    [SerializeField] private string rotationText = "ПКМ - повернуть";
    
    [Header("Настройки")]
    [SerializeField] private bool showHints = true;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;
    
    private static NavigationUI instance;
    public static NavigationUI Instance => instance;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Автоматически находим TextMeshProUGUI если не назначен
        if (navigationText == null)
        {
            navigationText = GetComponent<TextMeshProUGUI>();
            if (navigationText == null)
            {
                navigationText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }
        
        // Скрываем текст по умолчанию
        HideHint();
    }
    
    /// <summary>
    /// Показывает подсказку для валидного размещения
    /// </summary>
    public void ShowValidPlacementHint()
    {
        if (!showHints || navigationText == null) return;
        
        navigationText.text = validPlacementText;
        navigationText.color = validColor;
        navigationText.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Показывает подсказку для невалидного размещения
    /// </summary>
    public void ShowInvalidPlacementHint()
    {
        if (!showHints || navigationText == null) return;
        
        navigationText.text = invalidPlacementText;
        navigationText.color = invalidColor;
        navigationText.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Показывает подсказку для поворота
    /// </summary>
    public void ShowRotationHint()
    {
        if (!showHints || navigationText == null) return;
        
        navigationText.text = rotationText;
        navigationText.color = validColor;
        navigationText.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Скрывает подсказку
    /// </summary>
    public void HideHint()
    {
        if (navigationText != null)
        {
            navigationText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Устанавливает текст подсказки вручную
    /// </summary>
    public void SetHintText(string text, Color color)
    {
        if (!showHints || navigationText == null) return;
        
        navigationText.text = text;
        navigationText.color = color;
        navigationText.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Включает/выключает показ подсказок
    /// </summary>
    public void SetShowHints(bool show)
    {
        showHints = show;
        if (!showHints)
        {
            HideHint();
        }
    }
} 