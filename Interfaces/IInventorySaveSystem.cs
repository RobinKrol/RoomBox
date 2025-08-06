using System.Collections.Generic;

/// <summary>
/// Интерфейс для системы сохранения инвентаря
/// </summary>
public interface IInventorySaveSystem
{
    /// <summary>
    /// Сохранить состояние инвентаря
    /// </summary>
    /// <param name="inventoryManager">Менеджер инвентаря для сохранения</param>
    /// <param name="saveSlot">Слот сохранения</param>
    /// <returns>true если сохранение прошло успешно</returns>
    bool SaveInventory(IInventoryManager inventoryManager, int saveSlot = 0);
    
    /// <summary>
    /// Загрузить состояние инвентаря
    /// </summary>
    /// <param name="inventoryManager">Менеджер инвентаря для загрузки</param>
    /// <param name="saveSlot">Слот сохранения</param>
    /// <returns>true если загрузка прошла успешно</returns>
    bool LoadInventory(IInventoryManager inventoryManager, int saveSlot = 0);
    
    /// <summary>
    /// Сохранить размещенные предметы
    /// </summary>
    /// <param name="placedItems">Список размещенных предметов</param>
    /// <param name="saveSlot">Слот сохранения</param>
    /// <returns>true если сохранение прошло успешно</returns>
    bool SavePlacedItems(List<PlacedItemData> placedItems, int saveSlot = 0);
    
    /// <summary>
    /// Загрузить размещенные предметы
    /// </summary>
    /// <param name="saveSlot">Слот сохранения</param>
    /// <returns>Список размещенных предметов</returns>
    List<PlacedItemData> LoadPlacedItems(int saveSlot = 0);
    
    /// <summary>
    /// Проверить, существует ли сохранение
    /// </summary>
    /// <param name="saveSlot">Слот сохранения</param>
    /// <returns>true если сохранение существует</returns>
    bool HasSave(int saveSlot = 0);
    
    /// <summary>
    /// Удалить сохранение
    /// </summary>
    /// <param name="saveSlot">Слот сохранения</param>
    /// <returns>true если удаление прошло успешно</returns>
    bool DeleteSave(int saveSlot = 0);
    
    /// <summary>
    /// Получить информацию о сохранении
    /// </summary>
    /// <param name="saveSlot">Слот сохранения</param>
    /// <returns>Информация о сохранении</returns>
    SaveInfo GetSaveInfo(int saveSlot = 0);
    
    /// <summary>
    /// Получить список всех сохранений
    /// </summary>
    /// <returns>Список информации о сохранениях</returns>
    List<SaveInfo> GetAllSaves();
}

/// <summary>
/// Данные размещенного предмета
/// </summary>
[System.Serializable]
public struct PlacedItemData
{
    public string ItemId;
    public UnityEngine.Vector3 Position;
    public UnityEngine.Quaternion Rotation;
    public string CustomData; // Дополнительные данные в формате JSON
}

/// <summary>
/// Информация о сохранении
/// </summary>
[System.Serializable]
public struct SaveInfo
{
    public int SaveSlot;
    public System.DateTime SaveTime;
    public string SaveName;
    public int ItemCount;
    public int PlacedItemCount;
} 