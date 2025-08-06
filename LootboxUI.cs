using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

/// <summary>
/// Управляет UI лутбокса, анимацией появления предмета, обновлением попыток и сбросом состояния.
/// </summary>
public class LootboxUI : MonoBehaviour
{
    [SerializeField] private GameObject itemCard;
    [SerializeField] private Image itemCardIcon;
    [SerializeField] private TextMeshProUGUI itemCardName;
    [SerializeField] private TextMeshProUGUI itemCardQuantity; // Текст для отображения количества
    [SerializeField] private CanvasGroup itemCardCanvasGroup;
    [SerializeField] private GameObject lootboxPanel;
    [SerializeField] private GameObject roomBox;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private GameObject chestWindow; 
    [SerializeField] private Button chestButton;
    [SerializeField] private ChestLidAnimator lidAnimator;
    [SerializeField] private ChestClickHandler chestClickHandler;
    [SerializeField] private TextMeshProUGUI attemptsText; // UI-текст для попыток

    void Start()
    {
        chestButton.onClick.AddListener(OpenChestWindow);
        chestWindow.SetActive(false);
    }

    /// <summary>
    /// Обновляет отображение количества попыток на UI.
    /// </summary>
    public void UpdateAttempts(int attemptsLeft)
    {
        if (attemptsText != null)
            attemptsText.text = attemptsLeft.ToString();
    }

    /// <summary>
    /// Открывает окно сундука и скрывает ItemCard.
    /// </summary>
    private void OpenChestWindow()
    {
        chestWindow.SetActive(true);
        itemCard.SetActive(false);
    }

    /// <summary>
    /// Закрывает окно сундука.
    /// </summary>
    public void CloseChestWindow()
    {
        chestWindow.SetActive(false);
    }

    /// <summary>
    /// Показывает ItemCard с анимацией и данными предмета.
    /// </summary>
    public void Show(Item item, int amount = 1)
    {
        itemCard.SetActive(true);
        itemCardIcon.sprite = item.icon;
        itemCardName.text = item.itemName;
        
        // Обновляем отображение количества
        if (itemCardQuantity != null)
        {
            if (amount > 1)
            {
                itemCardQuantity.text = $"x{amount}";
                itemCardQuantity.gameObject.SetActive(true);
            }
            else
            {
                itemCardQuantity.gameObject.SetActive(false);
            }
        }
        
        itemCardCanvasGroup.alpha = 0f;
        StartCoroutine(FadeInItemCard());
    }

    private IEnumerator FadeInItemCard()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            itemCardCanvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        itemCardCanvasGroup.alpha = 1f;

        // Ждём 2 секунды, затем сбрасываем окно
        yield return new WaitForSeconds(2f);
        ResetLootboxWindow();
    }

    /// <summary>
    /// Сбрасывает окно лутбокса: скрывает ItemCard, закрывает сундук, разрешает повторное открытие.
    /// </summary>
    public void ResetLootboxWindow()
    {
        itemCard.SetActive(false);
        if (lidAnimator != null)
            lidAnimator.CloseLid();
        if (chestClickHandler != null)
            chestClickHandler.EnableChest();
    }

    public void CloseLootboxPanel()
    {
        if (lootboxPanel != null)
            lootboxPanel.SetActive(false);
            if (roomBox != null)
            roomBox.SetActive(true);
    }

}
