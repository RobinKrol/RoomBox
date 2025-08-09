using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Тестовый скрипт для отладки системы поверхностей
/// </summary>
public class SurfaceTestDebugger : MonoBehaviour
{
    [Header("Тестирование поверхностей")]
    public bool enableTesting = true;
    public Key testKey = Key.T;
    public Vector3 testPosition = Vector3.zero;
    
    [Header("Отладка")]
    public bool showDebugInfo = true;
    
    void Update()
    {
        if (!enableTesting) return;
        
        if (Keyboard.current != null && Keyboard.current[testKey].wasPressedThisFrame)
        {
            TestSurfaceSystem();
        }
    }
    
    void TestSurfaceSystem()
    {
        if (showDebugInfo)
            Debug.Log("=== ТЕСТ СИСТЕМЫ ПОВЕРХНОСТЕЙ ===");
        
        // Находим все объекты с PlacementLayerComponent
        PlacementLayerComponent[] allComponents = FindObjectsByType<PlacementLayerComponent>(FindObjectsSortMode.None);
        
        if (showDebugInfo)
            Debug.Log($"Найдено объектов с PlacementLayerComponent: {allComponents.Length}");
        
        foreach (var component in allComponents)
        {
            if (showDebugInfo)
            {
                Debug.Log($"Объект: {component.gameObject.name}");
                Debug.Log($"  - PlacementLayer: {component.PlacementLayer}");
                Debug.Log($"  - IsSurface: {component.IsSurface}");
                Debug.Log($"  - SurfaceHeight: {component.SurfaceHeight}");
                Debug.Log($"  - SurfaceSize: {component.SurfaceSize}");
                Debug.Log($"  - Layer: {component.gameObject.layer}");
                Debug.Log($"  - Position: {component.transform.position}");
                
                // Проверяем коллайдеры
                Collider[] colliders = component.GetComponents<Collider>();
                Debug.Log($"  - Colliders: {colliders.Length}");
                foreach (var col in colliders)
                {
                    Debug.Log($"    - {col.GetType().Name}: enabled={col.enabled}, isTrigger={col.isTrigger}");
                }
            }
            
            // Если это поверхность, тестируем позицию
            if (component.IsSurface)
            {
                Vector3 testPos = testPosition;
                bool onSurface = component.IsPositionOnSurface(testPos);
                
                if (showDebugInfo)
                {
                    Debug.Log($"  - Тест позиции {testPos}: {onSurface}");
                    Debug.Log($"  - SurfacePosition: {component.GetSurfacePosition()}");
                }
            }
        }
        
        if (showDebugInfo)
            Debug.Log("=== КОНЕЦ ТЕСТА ===");
    }
    
    void OnDrawGizmos()
    {
        if (!enableTesting) return;
        
        // Рисуем тестовую позицию
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(testPosition, 0.1f);
        
        // Рисуем все поверхности
        PlacementLayerComponent[] allComponents = FindObjectsByType<PlacementLayerComponent>(FindObjectsSortMode.None);
        foreach (var component in allComponents)
        {
            if (component.IsSurface)
            {
                Gizmos.color = Color.yellow;
                Vector3 surfacePos = component.GetSurfacePosition();
                Gizmos.DrawWireCube(surfacePos, new Vector3(component.SurfaceSize.x, 0.1f, component.SurfaceSize.y));
                
                // Линия от объекта до поверхности
                Gizmos.color = Color.white;
                Gizmos.DrawLine(component.transform.position, surfacePos);
            }
        }
    }
} 