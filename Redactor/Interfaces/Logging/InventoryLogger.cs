using UnityEngine;
using System;
using System.Collections.Generic;

namespace InventorySystem.Logging
{
    /// <summary>
    /// Уровни логирования
    /// </summary>
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4
    }
    
    /// <summary>
    /// Категории логирования
    /// </summary>
    public enum LogCategory
    {
        General,
        Inventory,
        Validation,
        UI,
        Events,
        DragHandler,
        Placement
    }
    
    /// <summary>
    /// Централизованная система логирования для инвентаря
    /// </summary>
    public static class InventoryLogger
    {
        private static LogLevel currentLogLevel = LogLevel.Info;
        private static readonly Dictionary<LogCategory, bool> enabledCategories = new Dictionary<LogCategory, bool>();
        private static readonly List<ILogHandler> logHandlers = new List<ILogHandler>();
        
        static InventoryLogger()
        {
            // Включаем все категории по умолчанию
            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                enabledCategories[category] = true;
            }
            
            // Добавляем стандартный обработчик
            logHandlers.Add(new UnityLogHandler());
        }
        
        /// <summary>
        /// Установка уровня логирования
        /// </summary>
        public static void SetLogLevel(LogLevel level)
        {
            currentLogLevel = level;
            Log(LogCategory.General, LogLevel.Info, $"Уровень логирования установлен: {level}");
        }
        
        /// <summary>
        /// Включение/выключение категории логирования
        /// </summary>
        public static void SetCategoryEnabled(LogCategory category, bool enabled)
        {
            enabledCategories[category] = enabled;
            Log(LogCategory.General, LogLevel.Info, $"Категория {category} {(enabled ? "включена" : "выключена")}");
        }
        
        /// <summary>
        /// Добавление обработчика логирования
        /// </summary>
        public static void AddLogHandler(ILogHandler handler)
        {
            if (handler != null && !logHandlers.Contains(handler))
            {
                logHandlers.Add(handler);
            }
        }
        
        /// <summary>
        /// Удаление обработчика логирования
        /// </summary>
        public static void RemoveLogHandler(ILogHandler handler)
        {
            logHandlers.Remove(handler);
        }
        
        /// <summary>
        /// Основной метод логирования
        /// </summary>
        public static void Log(LogCategory category, LogLevel level, string message, UnityEngine.Object context = null)
        {
            if (level > currentLogLevel || !enabledCategories[category])
                return;
                
            var logEntry = new LogEntry
            {
                Category = category,
                Level = level,
                Message = message,
                Context = context,
                Timestamp = DateTime.Now
            };
            
            foreach (var handler in logHandlers)
            {
                handler.HandleLog(logEntry);
            }
        }
        
        /// <summary>
        /// Логирование ошибки
        /// </summary>
        public static void LogError(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Log(category, LogLevel.Error, message, context);
        }
        
        /// <summary>
        /// Логирование предупреждения
        /// </summary>
        public static void LogWarning(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Log(category, LogLevel.Warning, message, context);
        }
        
        /// <summary>
        /// Логирование информации
        /// </summary>
        public static void LogInfo(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Log(category, LogLevel.Info, message, context);
        }
        
        /// <summary>
        /// Логирование отладки
        /// </summary>
        public static void LogDebug(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Log(category, LogLevel.Debug, message, context);
        }
        
        /// <summary>
        /// Логирование с форматированием
        /// </summary>
        public static void LogFormat(LogCategory category, LogLevel level, string format, params object[] args)
        {
            Log(category, level, string.Format(format, args));
        }
    }
    
    /// <summary>
    /// Запись лога
    /// </summary>
    public struct LogEntry
    {
        public LogCategory Category;
        public LogLevel Level;
        public string Message;
        public UnityEngine.Object Context;
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Интерфейс обработчика логирования
    /// </summary>
    public interface ILogHandler
    {
        void HandleLog(LogEntry entry);
    }
    
    /// <summary>
    /// Обработчик логирования Unity
    /// </summary>
    public class UnityLogHandler : ILogHandler
    {
        public void HandleLog(LogEntry entry)
        {
            var formattedMessage = $"[{entry.Category}] {entry.Message}";
            
            switch (entry.Level)
            {
                case LogLevel.Error:
                    Debug.LogError(formattedMessage, entry.Context);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formattedMessage, entry.Context);
                    break;
                case LogLevel.Info:
                case LogLevel.Debug:
                    Debug.Log(formattedMessage, entry.Context);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Обработчик логирования в файл
    /// </summary>
    public class FileLogHandler : ILogHandler
    {
        private readonly string logFilePath;
        private readonly System.IO.StreamWriter writer;
        
        public FileLogHandler(string filePath = null)
        {
            logFilePath = filePath ?? $"{Application.persistentDataPath}/inventory_log.txt";
            writer = new System.IO.StreamWriter(logFilePath, true);
            writer.AutoFlush = true;
        }
        
        public void HandleLog(LogEntry entry)
        {
            var formattedMessage = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Category}] [{entry.Level}] {entry.Message}";
            writer.WriteLine(formattedMessage);
        }
        
        public void Dispose()
        {
            writer?.Dispose();
        }
    }
    
    /// <summary>
    /// Расширения для упрощения логирования
    /// </summary>
    public static class LoggerExtensions
    {
        public static void LogError(this MonoBehaviour component, LogCategory category, string message)
        {
            InventoryLogger.LogError(category, message, component);
        }
        
        public static void LogWarning(this MonoBehaviour component, LogCategory category, string message)
        {
            InventoryLogger.LogWarning(category, message, component);
        }
        
        public static void LogInfo(this MonoBehaviour component, LogCategory category, string message)
        {
            InventoryLogger.LogInfo(category, message, component);
        }
        
        public static void LogDebug(this MonoBehaviour component, LogCategory category, string message)
        {
            InventoryLogger.LogDebug(category, message, component);
        }
    }
} 