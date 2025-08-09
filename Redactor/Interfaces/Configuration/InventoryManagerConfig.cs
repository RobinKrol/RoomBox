using UnityEngine;
using InventorySystem.Configuration;

namespace InventorySystem.Configuration
{
    /// <summary>
    /// Конфигурация для OptimizedInventoryManager
    /// </summary>
    [System.Serializable]
    public class InventoryManagerConfig : BaseConfiguration
    {
        [Header("Основные настройки")]
        [SerializeField] private int defaultSlotCount = 8;
        [SerializeField] private bool autoCreateSlots = true;
        [SerializeField] private bool autoCreateUI = true;
        
        [Header("Настройки стака")]
        [SerializeField] private bool enableStacking = true;
        [SerializeField] private int maxStackSize = 99;
        
        [Header("Настройки UI")]
        [SerializeField] private bool showItemCount = true;
        [SerializeField] private bool showItemIcons = true;
        [SerializeField] private Color emptySlotColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private Color filledSlotColor = Color.white;
        
        [Header("Настройки событий")]
        [SerializeField] private bool enableEvents = true;
        [SerializeField] private bool logEvents = true;
        
        // Публичные свойства
        public int DefaultSlotCount => defaultSlotCount;
        public bool AutoCreateSlots => autoCreateSlots;
        public bool AutoCreateUI => autoCreateUI;
        public bool EnableStacking => enableStacking;
        public int MaxStackSize => maxStackSize;
        public bool ShowItemCount => showItemCount;
        public bool ShowItemIcons => showItemIcons;
        public Color EmptySlotColor => emptySlotColor;
        public Color FilledSlotColor => filledSlotColor;
        public bool EnableEvents => enableEvents;
        public bool LogEvents => logEvents;
    }
} 