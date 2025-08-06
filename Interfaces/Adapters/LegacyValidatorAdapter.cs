using UnityEngine;
using InventorySystem.OptimizedComponents;

namespace InventorySystem.Adapters
{
    /// <summary>
    /// Адаптер для совместимости со старым ItemPlacementValidator
    /// </summary>
    public class LegacyValidatorAdapter : MonoBehaviour, IItemPlacementValidator
    {
        [SerializeField] private OptimizedItemPlacementValidator optimizedValidator;
        
        private void Awake()
        {
            if (optimizedValidator == null)
            {
                optimizedValidator = Object.FindFirstObjectByType<OptimizedItemPlacementValidator>();
            }
        }
        
        public bool CanPlaceItem(IItem item, Vector3 position, Quaternion rotation)
        {
            return optimizedValidator?.CanPlaceItem(item, position, rotation) ?? false;
        }
        
        public PlacementValidationResult ValidatePlacement(IItem item, Vector3 position, Quaternion rotation)
        {
            return optimizedValidator?.ValidatePlacement(item, position, rotation) 
                ?? new PlacementValidationResult { IsValid = false, ErrorMessage = "Validator not found" };
        }
        
        public Vector3? GetValidPlacementPosition(IItem item, Vector3 desiredPosition, Quaternion rotation)
        {
            return optimizedValidator?.GetValidPlacementPosition(item, desiredPosition, rotation);
        }
        
        public PlacementVisualFeedback GetVisualFeedback(IItem item, Vector3 position, Quaternion rotation)
        {
            return optimizedValidator?.GetVisualFeedback(item, position, rotation) 
                ?? new PlacementVisualFeedback { IsValid = false, Color = Color.red, Message = "Validator not found" };
        }
        
        public void SetPreviewInstance(GameObject preview)
        {
            optimizedValidator?.SetPreviewInstance(preview);
        }
        
        // Старый интерфейс для совместимости
        public bool CanPlaceItem(IItem item, Vector3 position)
        {
            return CanPlaceItem(item, position, Quaternion.identity);
        }
    }
} 