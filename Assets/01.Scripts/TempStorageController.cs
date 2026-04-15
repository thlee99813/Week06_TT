using System.Collections.Generic;
using UnityEngine;

public class TempStorageController : MonoBehaviour
{
    [Header("Temporary Storage Slots 2개")]
    [SerializeField] private CardView[] tempSlots = new CardView[2];

    public bool HasEmptySlot()
    {
        return FindFirstEmptySlot() != null;
    }

    public bool TryStoreCard(Card card)
    {
        if (card == null) return false;

        CardView targetSlot = FindFirstEmptySlot();
        if (targetSlot == null) return false;

        targetSlot.SetCard(card);
        return true;
    }

    public bool ContainsSlot(CardView slot)
    {
        if (slot == null || tempSlots == null) return false;

        for (int i = 0; i < tempSlots.Length; i++)
        {
            if (tempSlots[i] == slot) return true;
        }

        return false;
    }

    public void FlushAllToDiscard(List<Card> discardPile)
    {
        if (discardPile == null || tempSlots == null) return;

        for (int i = 0; i < tempSlots.Length; i++)
        {
            CardView slot = tempSlots[i];
            if (slot == null || !slot.HasCard) continue;

            if (slot.TryTakeCard(out Card card) && card != null)
                discardPile.Add(card);

            slot.ClearCard();
        }
    }

    private CardView FindFirstEmptySlot()
    {
        if (tempSlots == null) return null;

        for (int i = 0; i < tempSlots.Length; i++)
        {
            if (tempSlots[i] != null && !tempSlots[i].HasCard)
                return tempSlots[i];
        }

        return null;
    }
}
