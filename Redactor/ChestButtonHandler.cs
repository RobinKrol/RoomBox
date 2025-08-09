using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управляет кнопкой открытия сундука: скрывает RoomBox и показывает LootboxPanel.
/// </summary>
public class ChestButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject roomBox;         
    [SerializeField] private GameObject lootboxPanel;    
    [SerializeField] private Button chestButton;
    
    void Start()
    {
        chestButton.onClick.AddListener(OnChestButtonClick);
    }

    /// <summary>
    /// Скрывает RoomBox и показывает LootboxPanel.
    /// </summary>
    private void OnChestButtonClick()
    {
        if (roomBox != null && roomBox.activeSelf)
            roomBox.SetActive(false);

        if (lootboxPanel != null && !lootboxPanel.activeSelf)
            lootboxPanel.SetActive(true);
    }
}
