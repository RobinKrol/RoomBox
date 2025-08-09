using UnityEngine;
using InventorySystem.OptimizedComponents;

/// <summary>
/// Управляет логикой выпадения предметов из сундука, учётом попыток и взаимодействием с UI.
/// </summary>
public class LootboxSystem : MonoBehaviour
{
    [SerializeField] private OptimizedInventoryManager optimizedInventoryManager;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private LootboxUI lootboxUI;

    [SerializeField] private int maxAttempts = 3; // Максимальное количество попыток
    private int attemptsLeft;
    public int AttemptsLeft => attemptsLeft; // Только для чтения

    void Start()
    {
        attemptsLeft = maxAttempts;
        lootboxUI.UpdateAttempts(attemptsLeft); 
    }

    /// <summary>
    /// Проверяет, можно ли открыть сундук (есть ли попытки).
    /// </summary>
    public bool CanOpenChest()
    {
        return attemptsLeft > 0;
    }

    [Header("Настройки редкости")]
    [SerializeField] private float commonChance = 0.6f; // Шанс обычного предмета (60%)
    [SerializeField] private float rareChance = 0.3f; // Шанс редкого предмета (30%)
    [SerializeField] private float epicChance = 0.1f; // Шанс эпического предмета (10%)
    
    /// <summary>
    /// Открывает лутбокс, уменьшает количество попыток, возвращает выпавший предмет.
    /// </summary>
    public Item OpenLootbox()
    {
        if (attemptsLeft <= 0)
        {
            return null;
        }
        
        if (itemDatabase.items.Count == 0)
        {
            return null;
        }
        
        attemptsLeft--;
        lootboxUI.UpdateAttempts(attemptsLeft); // Обновляем UI

        // Определяем редкость предмета на основе шансов
        Item.ItemRarityType selectedRarity = DetermineRarity();
        
        // Фильтруем предметы по выбранной редкости
        var itemsOfRarity = itemDatabase.items.FindAll(item => item.rarity == selectedRarity);
        
        Item selectedItem;
        
        if (itemsOfRarity.Count == 0)
        {
            // Если предметов нужной редкости нет, берем случайный
            int index = Random.Range(0, itemDatabase.items.Count);
            selectedItem = itemDatabase.items[index];
        }
        else
        {
            // Выбираем случайный предмет нужной редкости
            int rarityIndex = Random.Range(0, itemsOfRarity.Count);
            selectedItem = itemsOfRarity[rarityIndex];
        }
        
        Debug.Log($"Выпал предмет: {selectedItem.itemName} ({selectedItem.rarity})");
        return selectedItem;
    }
    
    /// <summary>
    /// Определяет редкость предмета на основе настроенных шансов
    /// </summary>
    private Item.ItemRarityType DetermineRarity()
    {
        float randomValue = Random.value; // 0.0 - 1.0
        
        if (randomValue < epicChance)
        {
            return Item.ItemRarityType.Эпический;
        }
        else if (randomValue < epicChance + rareChance)
        {
            return Item.ItemRarityType.Редкий;
        }
        else if (randomValue < epicChance + rareChance + commonChance)
        {
            return Item.ItemRarityType.Обычный;
        }
        else
        {
            // Fallback - если сумма шансов меньше 1, возвращаем обычный
            return Item.ItemRarityType.Обычный;
        }
    }

    /// <summary>
    /// Открывает лутбокс и показывает UI с выпавшим предметом.
    /// </summary>
    public void OpenLootboxAndShowUI()
    {
        Item item = OpenLootbox();
        if (item != null && lootboxUI != null)
            lootboxUI.Show(item, 1); // Всегда показываем количество 1
        if (item != null && optimizedInventoryManager != null)
            optimizedInventoryManager.AddItem(item, 1); // Всегда добавляем 1 предмет
    }
}
