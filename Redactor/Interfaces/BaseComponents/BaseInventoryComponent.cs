using UnityEngine;
using InventorySystem.Configuration;

namespace InventorySystem.BaseComponents
{
    /// <summary>
    /// Базовый класс для всех компонентов системы инвентаря
    /// </summary>
    public abstract class BaseInventoryComponent : MonoBehaviour
    {
        [Header("Базовые настройки")]
        [SerializeField] protected bool enableDebugLogging = true;
        [SerializeField] protected bool enableValidation = true;
        [SerializeField] protected bool showDebugGizmos = true;
        
        protected bool isInitialized = false;
        protected Camera mainCamera;
        
        // Публичные свойства
        public bool EnableDebugLogging => enableDebugLogging;
        public bool EnableValidation => enableValidation;
        public bool ShowDebugGizmos => showDebugGizmos;
        public bool IsInitialized => isInitialized;
        
        protected virtual void Awake()
        {
            InitializeComponent();
        }
        
        protected virtual void Start()
        {
            if (!isInitialized)
            {
                InitializeComponent();
            }
        }
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        public virtual void InitializeComponent()
        {
            if (isInitialized) return;
            
            mainCamera = Camera.main;
            
            LogDebug($"Инициализация компонента {GetType().Name}");
            
            OnInitialize();
            
            isInitialized = true;
            LogDebug($"Компонент {GetType().Name} успешно инициализирован");
        }
        
        /// <summary>
        /// Переопределяемый метод для дополнительной инициализации
        /// </summary>
        protected virtual void OnInitialize()
        {
            // Переопределяется в наследниках
        }
        
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
        
        /// <summary>
        /// Проверка инициализации компонента
        /// </summary>
        protected void EnsureInitialized()
        {
            if (!isInitialized)
            {
                LogWarning("Компонент не инициализирован, выполняем инициализацию");
                InitializeComponent();
            }
        }
        
        /// <summary>
        /// Получение компонента с проверкой
        /// </summary>
        protected T GetRequiredComponent<T>() where T : Component
        {
            var component = GetComponent<T>();
            if (component == null)
            {
                LogError($"Требуемый компонент {typeof(T).Name} не найден на объекте {gameObject.name}");
            }
            return component;
        }
        
        /// <summary>
        /// Получение компонента в сцене с проверкой
        /// </summary>
        protected T FindRequiredComponent<T>() where T : Component
        {
            var component = Object.FindFirstObjectByType<T>();
            if (component == null)
            {
                LogError($"Требуемый компонент {typeof(T).Name} не найден в сцене");
            }
            return component;
        }
        
        /// <summary>
        /// Безопасное выполнение действия с проверкой инициализации
        /// </summary>
        protected void SafeExecute(System.Action action, string actionName = "действие")
        {
            try
            {
                EnsureInitialized();
                action?.Invoke();
            }
            catch (System.Exception e)
            {
                LogError($"Ошибка при выполнении {actionName}: {e.Message}");
            }
        }
        
        /// <summary>
        /// Безопасное выполнение функции с проверкой инициализации
        /// </summary>
        protected T SafeExecute<T>(System.Func<T> func, T defaultValue = default(T), string actionName = "действие")
        {
            try
            {
                EnsureInitialized();
                return func != null ? func() : defaultValue;
            }
            catch (System.Exception e)
            {
                LogError($"Ошибка при выполнении {actionName}: {e.Message}");
                return defaultValue;
            }
        }
        
        protected virtual void OnValidate()
        {
            // Валидация настроек в редакторе
            if (Application.isPlaying && isInitialized)
            {
                LogDebug("Настройки изменены в редакторе");
            }
        }
        
        protected virtual void OnDestroy()
        {
            LogDebug($"Компонент {GetType().Name} уничтожается");
            OnCleanup();
        }
        
        /// <summary>
        /// Переопределяемый метод для очистки ресурсов
        /// </summary>
        public virtual void OnCleanup()
        {
            // Переопределяется в наследниках
        }
    }
    
    /// <summary>
    /// Базовый класс для компонентов с конфигурацией
    /// </summary>
    public abstract class BaseInventoryComponent<TConfig> : BaseInventoryComponent 
        where TConfig : BaseConfiguration
    {
        [Header("Конфигурация")]
        [SerializeField] protected TConfig configuration;
        
        public TConfig Configuration => configuration;
        
        protected override void OnInitialize()
        {
            if (configuration == null)
            {
                LogWarning("Конфигурация не назначена, используем настройки по умолчанию");
                configuration = CreateDefaultConfiguration();
            }
            
            base.OnInitialize();
        }
        
        /// <summary>
        /// Создание конфигурации по умолчанию
        /// </summary>
        protected abstract TConfig CreateDefaultConfiguration();
        
        /// <summary>
        /// Применение конфигурации
        /// </summary>
        protected virtual void ApplyConfiguration(TConfig config)
        {
            if (config == null) return;
            
            enableDebugLogging = config.EnableDebugLogging;
            enableValidation = config.EnableValidation;
            showDebugGizmos = config.ShowDebugGizmos;
            
            LogDebug("Конфигурация применена");
        }
    }
} 