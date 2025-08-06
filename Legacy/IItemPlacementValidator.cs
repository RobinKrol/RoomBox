using UnityEngine;

/// <summary>
/// Интерфейс для валидации размещения предметов в мире
/// </summary>
public interface IItemPlacementValidator
{
    /// <summary>
    /// Проверить, можно ли разместить предмет в указанной позиции
    /// </summary>
    /// <param name="item">Предмет для размещения</param>
    /// <param name="position">Позиция для размещения</param>
    /// <param name="rotation">Поворот предмета</param>
    /// <returns>true если размещение возможно</returns>
    bool CanPlaceItem(IItem item, Vector3 position, Quaternion rotation);
    
    /// <summary>
    /// Получить информацию о валидности размещения
    /// </summary>
    /// <param name="item">Предмет для размещения</param>
    /// <param name="position">Позиция для размещения</param>
    /// <param name="rotation">Поворот предмета</param>
    /// <returns>Детальная информация о валидности</returns>
    PlacementValidationResult ValidatePlacement(IItem item, Vector3 position, Quaternion rotation);
    
    /// <summary>
    /// Получить подходящую позицию для размещения предмета
    /// </summary>
    /// <param name="item">Предмет для размещения</param>
    /// <param name="desiredPosition">Желаемая позиция</param>
    /// <param name="rotation">Поворот предмета</param>
    /// <returns>Скорректированная позиция или null если размещение невозможно</returns>
    Vector3? GetValidPlacementPosition(IItem item, Vector3 desiredPosition, Quaternion rotation);
    
    /// <summary>
    /// Получить визуальную обратную связь для размещения
    /// </summary>
    /// <param name="item">Предмет для размещения</param>
    /// <param name="position">Позиция для размещения</param>
    /// <param name="rotation">Поворот предмета</param>
    /// <returns>Информация для визуальной обратной связи</returns>
    PlacementVisualFeedback GetVisualFeedback(IItem item, Vector3 position, Quaternion rotation);
    
    /// <summary>
    /// Установить превью объект для исключения из проверок
    /// </summary>
    /// <param name="preview">Превью объект</param>
    void SetPreviewInstance(GameObject preview);
}

/// <summary>
/// Результат валидации размещения
/// </summary>
public struct PlacementValidationResult
{
    public bool IsValid;
    public string ErrorMessage;
    public PlacementErrorType ErrorType;
    public Vector3? SuggestedPosition;
}

/// <summary>
/// Тип ошибки размещения
/// </summary>
public enum PlacementErrorType
{
    None,
    Collision,
    OutOfBounds,
    InvalidSurface,
    Overlapping,
    InvalidLayer,
    InvalidItem,
    ObjectOverlap
}

/// <summary>
/// Визуальная обратная связь для размещения
/// </summary>
public struct PlacementVisualFeedback
{
    public bool IsValid;
    public Color Color;
    public string Message;
    public bool ShowWarning;
} 