using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Управляет UI счетчиком предметов в инвентаре
/// </summary>
public class InventoryCounterUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private TextMeshProUGUI counterText;
    
    [Header("Анимация")]
    [SerializeField] private bool enablePulseAnimation = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseScale = 1.1f;
    
    private InventoryManager inventoryManager;
    private Vector3 originalScale;
    private bool isPulsing = false;
    
    void Start()
    {
                // Находим InventoryManager
        inventoryManager = Object.FindFirstObjectByType<InventoryManager>();
        
        // Автоматически находим компоненты, если они не назначены
        if (counterText == null)
            counterText = GetComponentInChildren<TextMeshProUGUI>();
        
        originalScale = transform.localScale;
        
        // Обновляем счетчик при старте
        UpdateCounter();
    }
    
    void Update()
    {
        // Анимация пульсации при добавлении предметов
        if (enablePulseAnimation && isPulsing)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * (pulseScale - 1f) * 0.5f;
            transform.localScale = originalScale * pulse;
        }
    }
    
    /// <summary>
    /// Обновляет счетчик предметов
    /// </summary>
    public void UpdateCounter()
    {
        if (counterText == null)
            return;
        
        int totalItems = 0;
        
        // Получаем данные из InventoryManager
        if (inventoryManager != null)
        {
            // Используем новый метод для получения общего количества предметов
            totalItems = inventoryManager.GetTotalItemCount();
        }
        
        // Обновляем текст
        counterText.text = totalItems.ToString();
        
        // Запускаем анимацию при изменении количества
        if (totalItems > 0)
        {
            StartPulseAnimation();
        }
    }
    

    
    /// <summary>
    /// Запускает анимацию пульсации
    /// </summary>
    private void StartPulseAnimation()
    {
        if (!enablePulseAnimation)
            return;
        
        isPulsing = true;
        Invoke(nameof(StopPulseAnimation), 1f);
    }
    
    /// <summary>
    /// Останавливает анимацию пульсации
    /// </summary>
    private void StopPulseAnimation()
    {
        isPulsing = false;
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Публичный метод для обновления счетчика (можно вызывать из других скриптов)
    /// </summary>
    [ContextMenu("Обновить счетчик")]
    public void ForceUpdateCounter()
    {
        UpdateCounter();
    }
} 