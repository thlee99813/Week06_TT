using System.Collections.Generic;
using UnityEngine;

public class SquadController : MonoBehaviour
{
    [System.Serializable]
    private class SquadGroup
    {
        public CardView[] slots = new CardView[3];
    }

    [SerializeField] private SquadGroup[] squads = new SquadGroup[3];

    public bool TryPlaceFromHand(CardView handCard, int squadIndex)
    {
        if (handCard == null) return false;
        if (squadIndex < 0 || squadIndex >= squads.Length) return false;

        CardView targetSlot = FindFirstEmptySlot(squads[squadIndex].slots);
        if (targetSlot == null) return false;

        if (!handCard.TryConsumeForSquad(out Card card)) return false;

        targetSlot.SetCard(card);
        return true;
    }

    public void FlushAllToDiscard(List<Card> discardPile)
    {
        if (discardPile == null) return;

        for (int i = 0; i < squads.Length; i++)
        {
            CardView[] slots = squads[i].slots;
            if (slots == null) continue;

            for (int j = 0; j < slots.Length; j++)
            {
                CardView slot = slots[j];
                if (slot == null || !slot.HasCard || slot.Current == null) continue;

                discardPile.Add(slot.Current);
                slot.ClearCard();
                slot.gameObject.SetActive(false);
            }
        }
    }

    private CardView FindFirstEmptySlot(CardView[] slots)
    {
        if (slots == null) return null;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && !slots[i].HasCard)
                return slots[i];
        }
        return null;
    }
}
