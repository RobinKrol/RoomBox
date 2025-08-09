using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Обрабатывает клики по сундуку, управляет анимацией открытия, тряской и блокировкой повторных нажатий.
/// </summary>
public class ChestClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ChestLidAnimator lidAnimator;
    [SerializeField] private ChestShaker chestShaker;
    [SerializeField] private LootboxSystem lootboxSystem;
    private bool canOpenChest = true;

    /// <summary>
    /// Обработка клика по сундуку.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canOpenChest) return;

        if (lootboxSystem != null && !lootboxSystem.CanOpenChest())
        {
            // Эффект тряски всего сундука при отсутствии попыток
            if (chestShaker != null)
            {
                chestShaker.ShakeChest();
                DisableChest(0.7f); // Заблокировать на время тряски
            }
            return;
        }

        if (lidAnimator != null)
        {
            StartCoroutine(OpenChestAndShowLootbox());
        }
        else if (lootboxSystem != null)
        {
            lootboxSystem.OpenLootboxAndShowUI();
        }
    }

    /// <summary>
    /// Временно блокирует возможность открытия сундука.
    /// </summary>
    public void DisableChest(float seconds)
    {
        StartCoroutine(DisableChestCoroutine(seconds));
    }

    private IEnumerator DisableChestCoroutine(float seconds)
    {
        canOpenChest = false;
        yield return new WaitForSeconds(seconds);
        canOpenChest = true;
    }

    /// <summary>
    /// Открывает крышку сундука, затем показывает лутбокс.
    /// </summary>
    private IEnumerator OpenChestAndShowLootbox()
    {
        canOpenChest = false;
        lidAnimator.OpenLid();
        yield return new WaitForSeconds(2f); // Подбери время под анимацию
        if (lootboxSystem != null)
            lootboxSystem.OpenLootboxAndShowUI();
    }

    /// <summary>
    /// Разрешает повторное открытие сундука (например, после закрытия окна).
    /// </summary>
    public void EnableChest()
    {
        canOpenChest = true;
    }
}
