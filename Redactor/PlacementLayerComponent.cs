using UnityEngine;

/// <summary>
/// Компонент для определения слоя размещения объекта
/// </summary>
public class PlacementLayerComponent : MonoBehaviour
{
    [Header("Настройки слоя размещения")]
    [SerializeField] private PlacementLayer placementLayer = PlacementLayer.Floor;
    
    [Header("Дополнительные настройки")]
    [SerializeField] private bool isSurface = false; // Является ли этот объект поверхностью для размещения
    [SerializeField] private float surfaceHeight = 0f; // Высота поверхности от пола
    [SerializeField] private Vector2 surfaceSize = Vector2.one; // Размер поверхности
    
    [Header("Отладка")]
    [SerializeField] private bool showDebugInfo = false;
    
    /// <summary>
    /// Слой размещения объекта
    /// </summary>
    public PlacementLayer PlacementLayer => placementLayer;
    
    /// <summary>
    /// Является ли этот объект поверхностью для размещения
    /// </summary>
    public bool IsSurface => isSurface;
    
    /// <summary>
    /// Высота поверхности от пола
    /// </summary>
    public float SurfaceHeight => surfaceHeight;
    
    /// <summary>
    /// Размер поверхности
    /// </summary>
    public Vector2 SurfaceSize => surfaceSize;
    
    /// <summary>
    /// Получить позицию поверхности для размещения предметов
    /// </summary>
    public Vector3 GetSurfacePosition()
    {
        return transform.position + Vector3.up * surfaceHeight;
    }
    
    /// <summary>
    /// Проверить, находится ли позиция в пределах поверхности
    /// </summary>
    public bool IsPositionOnSurface(Vector3 position)
    {
        if (!isSurface) return false;
        
        Vector3 surfacePos = GetSurfacePosition();
        
        // Преобразуем позицию в локальные координаты поверхности с учетом поворота
        Vector3 localPos = transform.InverseTransformPoint(position);
        localPos.y = 0; // Игнорируем Y координату для проверки поверхности
        
        // Проверяем, находится ли позиция в пределах поверхности
        bool onSurface = Mathf.Abs(localPos.x) <= surfaceSize.x * 0.5f && Mathf.Abs(localPos.z) <= surfaceSize.y * 0.5f;
        
        if (showDebugInfo)
        {
            Debug.Log($"[IsPositionOnSurface] Позиция: {position}, Центр поверхности: {surfacePos}, Локальная: {localPos}, Размер: {surfaceSize}, Поворот: {transform.rotation.eulerAngles}, Результат: {onSurface}");
        }
        return onSurface;
    }
    
    /// <summary>
    /// Получить границы поверхности с учетом поворота
    /// </summary>
    public Bounds GetSurfaceBounds()
    {
        Vector3 surfacePos = GetSurfacePosition();
        Vector3 rotatedSize = transform.rotation * new Vector3(surfaceSize.x, 0.1f, surfaceSize.y);
        return new Bounds(surfacePos, rotatedSize);
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;
        
        // Рисуем цвет в зависимости от слоя
        Gizmos.color = placementLayer.GetLayerColor();
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            // Используем центр и размер коллайдера для точного отображения
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
        else
        {
            // Если коллайдера нет, используем позицию объекта
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
        
        // Если это поверхность, рисуем её границы с учетом поворота
        if (isSurface)
        {
            Gizmos.color = Color.yellow;
            Vector3 surfacePos = GetSurfacePosition();
            
            // Рисуем границы поверхности с учетом поворота
            Gizmos.matrix = Matrix4x4.TRS(surfacePos, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(surfaceSize.x, 0.1f, surfaceSize.y));
            Gizmos.matrix = Matrix4x4.identity; // Сбрасываем матрицу
            
            // Рисуем линию от объекта до поверхности
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, surfacePos);
        }
        else
        {
            // Если это не поверхность, но включена отладка, показываем информацию
            if (showDebugInfo)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireCube(transform.position, Vector3.one);
                
                // Показываем текст с информацией
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, 
                    $"Layer: {placementLayer}\nIsSurface: {isSurface}");
                #endif
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // Показываем информацию о слое
        Vector3 labelPos = transform.position + Vector3.up * 2f;
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPos, $"{placementLayer.GetDisplayName()}\n{(isSurface ? "Поверхность" : "Обычный")}");
        #endif
    }
    #endif
} 