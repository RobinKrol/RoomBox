using UnityEngine;

namespace InventorySystem.Configuration
{
    /// <summary>
    /// Базовый класс для конфигурации компонентов системы инвентаря
    /// </summary>
    [System.Serializable]
    public abstract class BaseConfiguration
    {
        [Header("Общие настройки")]
        [SerializeField] protected bool enableDebugLogging = true;
        [SerializeField] protected bool enableValidation = true;
        
        [Header("Визуальная обратная связь")]
        [SerializeField] protected Color validPlacementColor = Color.green;
        [SerializeField] protected Color invalidPlacementColor = Color.red;
        [SerializeField] protected Color overlapWarningColor = new Color(1f, 0.5f, 0f, 1f);
        
        [Header("Отладка")]
        [SerializeField] protected bool showDetailedDebug = true;
        [SerializeField] protected bool showDebugGizmos = true;
        
        // Публичные свойства
        public bool EnableDebugLogging => enableDebugLogging;
        public bool EnableValidation => enableValidation;
        public Color ValidPlacementColor => validPlacementColor;
        public Color InvalidPlacementColor => invalidPlacementColor;
        public Color OverlapWarningColor => overlapWarningColor;
        public bool ShowDetailedDebug => showDetailedDebug;
        public bool ShowDebugGizmos => showDebugGizmos;
        
        /// <summary>
        /// Логирование с проверкой включения отладки
        /// </summary>
        protected void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                UnityEngine.Debug.Log($"[{GetType().Name}] {message}");
            }
        }
        
        /// <summary>
        /// Логирование предупреждений
        /// </summary>
        protected void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning($"[{GetType().Name}] {message}");
        }
        
        /// <summary>
        /// Логирование ошибок
        /// </summary>
        protected void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[{GetType().Name}] {message}");
        }
    }
    
    /// <summary>
    /// Конфигурация для системы валидации размещения
    /// </summary>
    [System.Serializable]
    public class PlacementValidationConfig : BaseConfiguration
    {
        [Header("Настройки коллизий")]
        [SerializeField] private float collisionCheckRadius = 0.3f;
        [SerializeField] private LayerMask collisionCheckMask = -1;
        [SerializeField] private LayerMask surfaceCheckMask = -1;
        [SerializeField] private bool useStrictValidation = false;
        
        [Header("Настройки границ")]
        [SerializeField] private bool checkFloorBounds = true;
        [SerializeField] private float floorBoundsMargin = 0.01f;
        
        [Header("Настройки наложения")]
        [SerializeField] private bool preventObjectOverlap = true;
        [SerializeField] private float overlapCheckMargin = 0.02f;
        [SerializeField] private float overlapTolerance = 0.05f;
        [SerializeField] private bool checkHeightDifference = true;
        
        [Header("Система слоев")]
        [SerializeField] private bool enableLayerSystem = false;
        [SerializeField] private PlacementLayer defaultPlacementLayer = PlacementLayer.Floor;
        [SerializeField] private string[] ignoredTags = { "RoomBoxFloor" };
        [SerializeField] private LayerMask ignoredLayers = 0;
        
        // Публичные свойства
        public float CollisionCheckRadius => collisionCheckRadius;
        public LayerMask CollisionCheckMask => collisionCheckMask;
        public LayerMask SurfaceCheckMask => surfaceCheckMask;
        public bool UseStrictValidation => useStrictValidation;
        public bool CheckFloorBounds => checkFloorBounds;
        public float FloorBoundsMargin => floorBoundsMargin;
        public bool PreventObjectOverlap => preventObjectOverlap;
        public float OverlapCheckMargin => overlapCheckMargin;
        public float OverlapTolerance => overlapTolerance;
        public bool CheckHeightDifference => checkHeightDifference;
        public bool EnableLayerSystem => enableLayerSystem;
        public PlacementLayer DefaultPlacementLayer => defaultPlacementLayer;
        public string[] IgnoredTags => ignoredTags;
        public LayerMask IgnoredLayers => ignoredLayers;
    }
    
    /// <summary>
    /// Конфигурация для системы перетаскивания
    /// </summary>
    [System.Serializable]
    public class DragHandlerConfig : BaseConfiguration
    {
        [Header("Настройки перетаскивания")]
        [SerializeField] private string outlineLayerName = "OutlinePreview";
        [SerializeField] private float objectSizeMultiplier = 0.3f;
        [SerializeField] private bool allowTouchingWalls = true;
        [SerializeField] private float invalidAreaMultiplier = 1.1f;
        
        [Header("Звуковые эффекты")]
        [SerializeField] private bool enableSoundEffects = true;
        [SerializeField] private AudioClip dragStartSound;
        [SerializeField] private AudioClip placementSound;
        [SerializeField] private AudioClip cancelSound;
        [SerializeField] private AudioClip invalidPlacementSound;
        [SerializeField] private AudioClip rotationSound;
        
        [Header("Настройки позиционирования")]
        [SerializeField] private float floorHeight = 0f;
        [SerializeField] private bool useRaycastPositioning = false;
        [SerializeField] private float cameraDistanceOffset = 0f;
        
        // Публичные свойства
        public string OutlineLayerName => outlineLayerName;
        public float ObjectSizeMultiplier => objectSizeMultiplier;
        public bool AllowTouchingWalls => allowTouchingWalls;
        public float InvalidAreaMultiplier => invalidAreaMultiplier;
        public bool EnableSoundEffects => enableSoundEffects;
        public AudioClip DragStartSound => dragStartSound;
        public AudioClip PlacementSound => placementSound;
        public AudioClip CancelSound => cancelSound;
        public AudioClip InvalidPlacementSound => invalidPlacementSound;
        public AudioClip RotationSound => rotationSound;
        public float FloorHeight => floorHeight;
        public bool UseRaycastPositioning => useRaycastPositioning;
        public float CameraDistanceOffset => cameraDistanceOffset;
    }
} 