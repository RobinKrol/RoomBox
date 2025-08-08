using UnityEngine;
using InventorySystem.OptimizedComponents;
using InventorySystem.Logging;
using System.Collections.Generic;

namespace InventorySystem.OptimizedComponents
{
    /// <summary>
    /// Продолжение OptimizedItemPlacementValidator с приватными методами
    /// </summary>
    public partial class OptimizedItemPlacementValidator
    {
        // Ссылка на превью объект для исключения из проверок
        private GameObject previewInstance;
        
        /// <summary>
        /// Установить превью объект для исключения из проверок
        /// </summary>
        public void SetPreviewInstance(GameObject preview)
        {
            previewInstance = preview;
        }
        
        private bool CheckPlacementValidityTouching(IItem item, Vector3 position, Quaternion rotation)
        {
            // Проверяем границы пола
            if (configuration.CheckFloorBounds && !CheckFloorBounds(item, position, rotation))
            {
                return false;
            }
            
            // Проверяем коллизии
            if (!CheckCollisions(item, position, rotation))
            {
                return false;
            }
            
            // Проверяем валидность поверхности
            if (!CheckSurfaceValidity(item, position, rotation))
            {
                return false;
            }
            
            return true;
        }
        
        private bool CheckPlacementValidityStrict(IItem item, Vector3 position, Quaternion rotation)
        {
            // Строгая валидация включает все проверки
            if (!CheckPlacementValidityTouching(item, position, rotation))
            {
                return false;
            }
            
            // Дополнительные строгие проверки
            if (configuration.PreventObjectOverlap && !CheckObjectOverlap(item, position, rotation))
            {
                return false;
            }
            
            return true;
        }
        
        private bool CheckFloorBounds(IItem item, Vector3 position, Quaternion rotation)
        {
            if (!configuration.CheckFloorBounds) return true;
            
            var itemSize = GetItemSize(item, rotation);
            var bounds = new Bounds(position, itemSize);
            
            // Проверяем, что объект находится в пределах границ пола
            // Здесь можно добавить логику проверки границ пола
            return true; // Упрощенная проверка
        }
        
        private bool CheckCollisions(IItem item, Vector3 position, Quaternion rotation)
        {
            var collisionRadius = GetCollisionCheckRadius(item);
            var colliders = Physics.OverlapSphere(position, collisionRadius, configuration.CollisionCheckMask);
            
            LogDebug($"Проверка коллизий: радиус={collisionRadius}, найдено коллайдеров={colliders.Length}");
            
            foreach (var collider in colliders)
            {
                if (!ShouldIgnoreCollider(collider))
                {
                    LogDebug($"❌ Обнаружена коллизия с {collider.name}");
                    return false;
                }
                else
                {
                    LogDebug($"✅ Игнорируем коллайдер: {collider.name}");
                }
            }
            
            LogDebug("✅ Коллизий не обнаружено");
            return true;
        }
        
        private bool CheckSurfaceValidity(IItem item, Vector3 position, Quaternion rotation)
        {
            // Проверяем, есть ли подходящая поверхность под предметом
            Vector3 checkPosition = position + Vector3.down * 0.1f;
            Collider[] surfaceColliders = Physics.OverlapSphere(checkPosition, 0.1f, configuration.SurfaceCheckMask);
            
            LogDebug($"Найдено {surfaceColliders.Length} поверхностей для проверки");
            LogDebug($"Позиция проверки: {checkPosition}");
            LogDebug($"Surface Check Mask: {configuration.SurfaceCheckMask}");
            
            // Если система слоев включена, проверяем PlacementLayerComponent
            if (configuration.EnableLayerSystem)
            {
                LogDebug("Система слоев включена, проверяем PlacementLayerComponent");
                
                foreach (Collider col in surfaceColliders)
                {
                    LogDebug($"Проверяем коллайдер: {col.name} (слой: {col.gameObject.layer})");
                    
                    // Исключаем превью объект
                    if (previewInstance != null && (col.gameObject == previewInstance || 
                        col.transform.IsChildOf(previewInstance.transform)))
                    {
                        LogDebug($"Исключаем превью объект: {col.name}");
                        continue;
                    }
                    
                    // Проверяем, является ли это поверхностью
                    PlacementLayerComponent surfaceComponent = col.GetComponent<PlacementLayerComponent>();
                    if (surfaceComponent != null)
                    {
                        LogDebug($"Найден PlacementLayerComponent на {col.name}");
                        LogDebug($"  - IsSurface: {surfaceComponent.IsSurface}");
                        LogDebug($"  - PlacementLayer: {surfaceComponent.PlacementLayer}");
                        
                        if (surfaceComponent.IsSurface)
                        {
                            PlacementLayer surfaceLayer = surfaceComponent.PlacementLayer;
                            bool canPlaceOnSurface = configuration.DefaultPlacementLayer.CanPlaceOn(surfaceLayer);
                            bool onSurface = surfaceComponent.IsPositionOnSurface(position);
                            
                            LogDebug($"Поверхность валидна: {col.name}");
                            LogDebug($"  - Слой поверхности: {surfaceLayer}");
                            LogDebug($"  - CanPlaceOn: {canPlaceOnSurface}");
                            LogDebug($"  - OnSurface: {onSurface}");
                            
                            if (canPlaceOnSurface && onSurface)
                            {
                                LogDebug($"✅ Поверхность валидна: {col.name}");
                                return true;
                            }
                            else
                            {
                                LogDebug($"❌ Поверхность не валидна: {col.name}");
                                LogDebug($"  - canPlaceOnSurface: {canPlaceOnSurface}");
                                LogDebug($"  - onSurface: {onSurface}");
                            }
                        }
                        else
                        {
                            LogDebug($"Объект {col.name} имеет PlacementLayerComponent, но IsSurface=false");
                        }
                    }
                    else
                    {
                        LogDebug($"Объект {col.name} не имеет PlacementLayerComponent");
                    }
                }
                
                // Если не нашли PlacementLayerComponent, но есть коллайдеры - разрешаем размещение
                if (surfaceColliders.Length > 0)
                {
                    LogDebug("Не найдено PlacementLayerComponent, но есть коллайдеры - разрешаем размещение");
                    return true;
                }
                
                LogDebug("❌ Не найдено валидных поверхностей");
                return false;
            }
            
            // Если система слоев отключена, просто проверяем наличие коллайдеров
            bool hasSurface = surfaceColliders.Length > 0;
            LogDebug($"Система слоев отключена. Найдено поверхностей: {surfaceColliders.Length}");
            return hasSurface;
        }
        
        private bool CheckObjectOverlap(IItem item, Vector3 position, Quaternion rotation)
        {
            if (!configuration.PreventObjectOverlap) return true;
            
            var itemSize = GetItemSize(item, rotation);
            var bounds = new Bounds(position, itemSize);
            
            // Проверяем наложение с другими объектами
            tempColliders.Clear();
            var colliders = Physics.OverlapBox(position, itemSize * 0.5f, rotation, configuration.CollisionCheckMask);
            
            LogDebug($"Проверка наложения: размер={itemSize}, найдено коллайдеров={colliders.Length}");
            
            foreach (var collider in colliders)
            {
                if (!ShouldIgnoreCollider(collider))
                {
                    tempColliders.Add(collider);
                    LogDebug($"❌ Обнаружено наложение с {collider.name}");
                }
                else
                {
                    LogDebug($"✅ Игнорируем коллайдер при наложении: {collider.name}");
                }
            }
            
            bool isValid = tempColliders.Count == 0;
            LogDebug($"Результат проверки наложения: {(isValid ? "✅ Наложений нет" : $"❌ Найдено {tempColliders.Count} наложений")}");
            return isValid;
        }
        
        private bool ShouldIgnoreCollider(Collider col)
        {
            if (col == null) return true;
            
            // Проверяем кэш
            if (ignoredCollidersCache.TryGetValue(col, out var cachedResult))
            {
                return cachedResult;
            }
            
            // Исключаем превью объект
            if (previewInstance != null && (col.gameObject == previewInstance || 
                col.transform.IsChildOf(previewInstance.transform)))
            {
                LogDebug($"Исключаем превью объект: {col.name}");
                ignoredCollidersCache[col] = true;
                return true;
            }
            
            // Исключаем объекты с PlacementLayerComponent (поверхности)
            var placementComponent = col.GetComponent<PlacementLayerComponent>();
            if (placementComponent != null)
            {
                LogDebug($"Исключаем из коллизий объект с PlacementLayerComponent: {col.name} (слой: {placementComponent.PlacementLayer})");
                ignoredCollidersCache[col] = true;
                return true;
            }
            
            // Проверяем специальные слои
            string layerName = LayerMask.LayerToName(col.gameObject.layer);
            if (layerName == "IgnoreCollision" || layerName == "Ignore Raycast")
            {
                LogDebug($"Исключаем объект на слое {layerName}: {col.name}");
                ignoredCollidersCache[col] = true;
                return true;
            }
            
            // Проверяем теги
            foreach (var ignoredTag in configuration.IgnoredTags)
            {
                if (col.CompareTag(ignoredTag))
                {
                    LogDebug($"Исключаем объект с тегом {ignoredTag}: {col.name}");
                    ignoredCollidersCache[col] = true;
                    return true;
                }
            }
            
            // Проверяем слои из конфигурации
            if (((1 << col.gameObject.layer) & configuration.IgnoredLayers) != 0)
            {
                LogDebug($"Исключаем объект на игнорируемом слое {layerName}: {col.name}");
                ignoredCollidersCache[col] = true;
                return true;
            }
            
            // Если объект на слое Surface - исключаем из коллизий (но не из проверки поверхностей)
            if (layerName == "Surface")
            {
                LogDebug($"Исключаем поверхность из коллизий: {col.name}");
                ignoredCollidersCache[col] = true;
                return true;
            }
            
            LogDebug($"Коллизия с объектом: {col.name} (слой: {layerName})");
            ignoredCollidersCache[col] = false;
            return false;
        }
        
        private float GetCollisionCheckRadius(IItem item)
        {
            var itemSize = GetItemSize(item, Quaternion.identity);
            return Mathf.Max(itemSize.x, itemSize.z) * 0.5f + configuration.CollisionCheckRadius;
        }
        
        private Vector3 GetItemSize(IItem item, Quaternion rotation)
        {
            // Пытаемся получить размер из оригинального Item
            if (item is ItemWrapper wrapper && wrapper.GetOriginalItem()?.prefab != null)
            {
                var prefab = wrapper.GetOriginalItem().prefab;
                var renderer = prefab.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var size = renderer.bounds.size;
                    LogDebug($"Размер предмета {item.ItemName}: {size}");
                    return size;
                }
            }
            
            // Fallback: используем размер по умолчанию
            var defaultSize = Vector3.one * 0.5f;
            LogDebug($"Используем размер по умолчанию для {item.ItemName}: {defaultSize}");
            return defaultSize;
        }
        
        private Vector3[] GenerateTestPositions(Vector3 center, float radius, int count)
        {
            var positions = new Vector3[count];
            var angleStep = 360f / count;
            
            for (int i = 0; i < count; i++)
            {
                var angle = i * angleStep * Mathf.Deg2Rad;
                var offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                positions[i] = center + offset;
            }
            
            return positions;
        }
        
        public override void OnCleanup()
        {
            ignoredCollidersCache.Clear();
            tempColliders.Clear();
            base.OnCleanup();
        }
        
        [ContextMenu("Тест валидации")]
        public void TestValidation()
        {
            var testItem = InventorySystem.Factories.InventoryFactory.CreateTestItem();
            var testPosition = Vector3.zero;
            var testRotation = Quaternion.identity;
            
            var result = CanPlaceItem(testItem, testPosition, testRotation);
            LogDebug($"Тест валидации: {result}");
        }
    }
} 