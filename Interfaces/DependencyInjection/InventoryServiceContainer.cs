using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using InventorySystem.Logging;
using InventorySystem.Factories;
using static UnityEngine.Object;

namespace InventorySystem.DependencyInjection
{
    /// <summary>
    /// Простой контейнер для dependency injection
    /// </summary>
    public class InventoryServiceContainer
    {
        private static InventoryServiceContainer instance;
        public static InventoryServiceContainer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InventoryServiceContainer();
                }
                return instance;
            }
        }
        
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<object>> factories = new Dictionary<Type, Func<object>>();
        
        private InventoryServiceContainer() { }
        
        /// <summary>
        /// Регистрация сервиса как синглтон
        /// </summary>
        public void RegisterSingleton<T>(T instance) where T : class
        {
            services[typeof(T)] = instance;
        }
        
        /// <summary>
        /// Регистрация фабрики для создания сервиса
        /// </summary>
        public void RegisterFactory<T>(Func<T> factory) where T : class
        {
            factories[typeof(T)] = () => factory();
        }
        
        /// <summary>
        /// Регистрация типа как синглтон
        /// </summary>
        public void RegisterSingleton<T>() where T : class, new()
        {
            RegisterFactory<T>(() => new T());
        }
        
        /// <summary>
        /// Получение сервиса
        /// </summary>
        public T Resolve<T>() where T : class
        {
            var type = typeof(T);
            
            // Проверяем, есть ли уже созданный экземпляр
            if (services.TryGetValue(type, out var existingService))
            {
                return (T)existingService;
            }
            
            // Проверяем, есть ли фабрика
            if (factories.TryGetValue(type, out var factory))
            {
                var instance = factory();
                services[type] = instance; // Кэшируем как синглтон
                return (T)instance;
            }
            
            throw new InvalidOperationException($"Сервис типа {type.Name} не зарегистрирован");
        }
        
        /// <summary>
        /// Попытка получить сервис
        /// </summary>
        public bool TryResolve<T>(out T service) where T : class
        {
            try
            {
                service = Resolve<T>();
                return true;
            }
            catch
            {
                service = null;
                return false;
            }
        }
        
        /// <summary>
        /// Попытка получить сервис по типу
        /// </summary>
        public bool TryResolve(Type type, out object service)
        {
            try
            {
                service = Resolve(type);
                return true;
            }
            catch
            {
                service = null;
                return false;
            }
        }
        
        /// <summary>
        /// Получение сервиса по типу
        /// </summary>
        public object Resolve(Type type)
        {
            // Проверяем, есть ли уже созданный экземпляр
            if (services.TryGetValue(type, out var existingService))
            {
                return existingService;
            }
            
            // Проверяем, есть ли фабрика
            if (factories.TryGetValue(type, out var factory))
            {
                var instance = factory();
                services[type] = instance; // Кэшируем как синглтон
                return instance;
            }
            
            throw new InvalidOperationException($"Сервис типа {type.Name} не зарегистрирован");
        }
        
        /// <summary>
        /// Очистка всех сервисов
        /// </summary>
        public void Clear()
        {
            services.Clear();
            factories.Clear();
        }
        
        /// <summary>
        /// Удаление конкретного сервиса
        /// </summary>
        public void Unregister<T>()
        {
            var type = typeof(T);
            services.Remove(type);
            factories.Remove(type);
        }
    }
    
    /// <summary>
    /// Атрибут для автоматической инъекции зависимостей
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
    }
    
    /// <summary>
    /// Базовый класс для компонентов с автоматической инъекцией зависимостей
    /// </summary>
    public abstract class InjectableMonoBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            InjectDependencies();
        }
        
        /// <summary>
        /// Автоматическая инъекция зависимостей
        /// </summary>
        protected virtual void InjectDependencies()
        {
            var type = GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<InjectAttribute>() != null)
                {
                    var fieldType = field.FieldType;
                    if (InventoryServiceContainer.Instance.TryResolve(fieldType, out object service))
                    {
                        field.SetValue(this, service);
                    }
                    else
                    {
                        Debug.LogWarning($"Не удалось разрешить зависимость {fieldType.Name} для поля {field.Name} в {type.Name}");
                    }
                }
            }
            
            var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<InjectAttribute>() != null && property.CanWrite)
                {
                    var propertyType = property.PropertyType;
                    if (InventoryServiceContainer.Instance.TryResolve(propertyType, out object service))
                    {
                        property.SetValue(this, service);
                    }
                    else
                    {
                        Debug.LogWarning($"Не удалось разрешить зависимость {propertyType.Name} для свойства {property.Name} в {type.Name}");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Регистратор сервисов по умолчанию
    /// </summary>
    public static class DefaultServiceRegistrar
    {
        /// <summary>
        /// Регистрация всех сервисов по умолчанию
        /// </summary>
        public static void RegisterDefaultServices()
        {
            var container = InventoryServiceContainer.Instance;
            
            // Регистрируем системы логирования
            if (!container.TryResolve<InventorySystem.Logging.ILogHandler>(out _))
            {
                container.RegisterSingleton<InventorySystem.Logging.ILogHandler>(new UnityLogHandler());
            }
            
            // Фабрики - это статические классы, поэтому не регистрируем их как сервисы
            // Они доступны напрямую через свои статические методы
        }
    }
    
    /// <summary>
    /// Расширения для упрощения работы с контейнером
    /// </summary>
    public static class ServiceContainerExtensions
    {
        /// <summary>
        /// Регистрация MonoBehaviour как сервиса
        /// </summary>
        public static void RegisterMonoBehaviour<T>(this InventoryServiceContainer container, T instance) where T : MonoBehaviour
        {
            container.RegisterSingleton<T>(instance);
        }
        
        /// <summary>
        /// Поиск и регистрация MonoBehaviour в сцене
        /// </summary>
        public static void RegisterMonoBehaviourInScene<T>(this InventoryServiceContainer container) where T : MonoBehaviour
        {
            var instance = FindFirstObjectByType<T>();
            if (instance != null)
            {
                container.RegisterMonoBehaviour(instance);
            }
            else
            {
                Debug.LogWarning($"MonoBehaviour типа {typeof(T).Name} не найден в сцене");
            }
        }
    }
} 