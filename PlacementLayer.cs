using UnityEngine;

/// <summary>
/// Слои размещения объектов в комнате
/// </summary>
public enum PlacementLayer
{
    /// <summary>
    /// Пол - ковры, большая мебель, предметы на полу
    /// </summary>
    Floor = 0,
    
    /// <summary>
    /// Поверхности - столы, тумбы, поверхности для размещения предметов
    /// </summary>
    Surface = 1,
    
    /// <summary>
    /// Предметы - мелкие предметы (бутылки, книги, посуда)
    /// </summary>
    Item = 2,
    
    /// <summary>
    /// Стены - предметы на стенах (картины, полки)
    /// </summary>
    Wall = 3
}

/// <summary>
/// Расширения для работы с PlacementLayer
/// </summary>
public static class PlacementLayerExtensions
{
    /// <summary>
    /// Получить название слоя для отображения
    /// </summary>
    public static string GetDisplayName(this PlacementLayer layer)
    {
        switch (layer)
        {
            case PlacementLayer.Floor: return "Пол";
            case PlacementLayer.Surface: return "Поверхность";
            case PlacementLayer.Item: return "Предмет";
            case PlacementLayer.Wall: return "Стена";
            default: return "Неизвестно";
        }
    }
    
    /// <summary>
    /// Получить цвет для визуализации слоя
    /// </summary>
    public static Color GetLayerColor(this PlacementLayer layer)
    {
        switch (layer)
        {
            case PlacementLayer.Floor: return Color.green;
            case PlacementLayer.Surface: return Color.yellow;
            case PlacementLayer.Item: return Color.blue;
            case PlacementLayer.Wall: return Color.magenta;
            default: return Color.white;
        }
    }
    
    /// <summary>
    /// Проверить, может ли слой размещаться на другом слое
    /// </summary>
    public static bool CanPlaceOn(this PlacementLayer thisLayer, PlacementLayer otherLayer)
    {
        switch (thisLayer)
        {
            case PlacementLayer.Floor:
                // Пол можно размещать на полу и поверхностях
                return otherLayer == PlacementLayer.Floor || otherLayer == PlacementLayer.Surface;
                
            case PlacementLayer.Surface:
                // Поверхности можно размещать на полу
                return otherLayer == PlacementLayer.Floor;
                
            case PlacementLayer.Item:
                // Предметы можно размещать на полу и поверхностях
                return otherLayer == PlacementLayer.Floor || otherLayer == PlacementLayer.Surface;
                
            case PlacementLayer.Wall:
                // Стенные предметы можно размещать только на стенах
                return otherLayer == PlacementLayer.Wall;
                
            default:
                return false;
        }
    }
} 