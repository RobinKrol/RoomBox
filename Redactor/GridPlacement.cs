using UnityEngine;

public class GridPlacement : MonoBehaviour
{
    [Header("Настройки сетки")]
    public float gridSize = 1f; // Размер ячейки сетки
    public bool snapToGrid = true; // Привязка к сетке
    public Vector3 gridOffset = Vector3.zero; // Смещение сетки
    
    [Header("Визуализация сетки")]
    public bool showGrid = true; // Показывать ли сетку
    public Material gridMaterial; // Материал для отображения сетки
    public Color gridColor = new Color(1f, 1f, 1f, 0.3f); // Цвет сетки
    
    private GameObject gridVisualization;
    
    void Start()
    {
       
    }
    
    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        if (!snapToGrid) return worldPosition;
        
        Vector3 snappedPosition = worldPosition + gridOffset;
        
        // Привязка к сетке
        snappedPosition.x = Mathf.Round(snappedPosition.x / gridSize) * gridSize;
        snappedPosition.z = Mathf.Round(snappedPosition.z / gridSize) * gridSize;
        
        return snappedPosition - gridOffset;
    }
    
    public bool IsOnGrid(Vector3 worldPosition)
    {
        Vector3 snappedPosition = SnapToGrid(worldPosition);
        return Vector3.Distance(worldPosition, snappedPosition) < gridSize * 0.1f;
    }
    
    private void CreateGridVisualization()
    {
        if (gridVisualization != null) return;
        
        gridVisualization = new GameObject("Grid Visualization");
        gridVisualization.transform.SetParent(transform);
        
        // Создаем линии сетки
        int gridLines = 20; // Количество линий сетки
        float gridExtent = gridLines * gridSize * 0.5f;
        
        for (int i = 0; i <= gridLines; i++)
        {
            float pos = i * gridSize - gridExtent;
            
            // Вертикальные линии
            CreateGridLine(new Vector3(pos, 0, -gridExtent), new Vector3(pos, 0, gridExtent));
            
            // Горизонтальные линии
            CreateGridLine(new Vector3(-gridExtent, 0, pos), new Vector3(gridExtent, 0, pos));
        }
    }
    
    private void CreateGridLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("GridLine");
        line.transform.SetParent(gridVisualization.transform);
        
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = gridMaterial != null ? gridMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.startColor = gridColor;
        lr.endColor = gridColor;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
    
    void OnDrawGizmos()
    {
        if (!showGrid) return;
        
        Gizmos.color = gridColor;
        int gridLines = 10;
        float gridExtent = gridLines * gridSize * 0.5f;
        
        for (int i = 0; i <= gridLines; i++)
        {
            float pos = i * gridSize - gridExtent;
            
            // Вертикальные линии
            Gizmos.DrawLine(
                new Vector3(pos, 0, -gridExtent) + gridOffset,
                new Vector3(pos, 0, gridExtent) + gridOffset
            );
            
            // Горизонтальные линии
            Gizmos.DrawLine(
                new Vector3(-gridExtent, 0, pos) + gridOffset,
                new Vector3(gridExtent, 0, pos) + gridOffset
            );
        }
    }
} 