using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckController : MonoBehaviour
{
    [Header("Hand Slots (9)")]
    [SerializeField] private CardView[] handSlots = new CardView[9];

    [Header("Deck Rule")]
    [SerializeField, Min(1)] private int setsPerColor = 3;
    [SerializeField, Min(1)] private int handSize = 9;

    [Header("Reroll Rule")]
    [SerializeField, Min(0)] private int rerollPerFire = 3;

    public int CurrentRerollCount { get; private set; }
    public int MaxRerollCount => rerollPerFire;

    public event Action<int, int> OnRerollCountChanged;

    [SerializeField] private List<Card> drawPile = new List<Card>();
    [SerializeField] private List<Card> discardPile = new List<Card>();

    [SerializeField] private SquadController squadController;


    private void Start()
    {
        RebuildDeck();
        DrawHand();
        ResetRerollCount();
    }

    public void RebuildAndDraw() 
    {
        ReturnHandToDiscard();   
        squadController.FlushAllToDiscard(discardPile);
        ReshuffleDiscardIntoDraw();
        DrawHand();
        ResetRerollCount();
        Debug.Log("발사 발동");
 
    }


    public void RerollHand()
    {
        if(CurrentRerollCount <= 0) return;
        ReturnHandToDiscard();
        DrawHand();
        CurrentRerollCount--;
        NotifyRerollCountChanged();
        Debug.Log("리롤 발동");

    }

    private void RebuildDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        AddColorCards(CardColor.Red);
        AddColorCards(CardColor.Blue);
        AddColorCards(CardColor.Green);

        Shuffle(drawPile);
    }

    private void AddColorCards(CardColor color)
    {
        for (int set = 0; set < setsPerColor; set++)
        {
            for (int n = 1; n <= 9; n++)
            {
                drawPile.Add(new Card(color, n));
            }
        }
    }

    private void DrawHand()
    {
        int count = Mathf.Min(handSize, handSlots.Length);

        for (int i = 0; i < count; i++)
        {
            if (TryDrawOne(out Card card))
            {
                handSlots[i].SetCard(card); 
            }
            else
            {
                handSlots[i].ClearCard();
            }
        }
    }

    private bool TryDrawOne(out Card card)
    {
        if (drawPile.Count == 0)
        {
            card = null;
            return false;
        }

        int last = drawPile.Count - 1;
        card = drawPile[last];
        drawPile.RemoveAt(last);
        return true;
    }


    private void ReturnHandToDiscard()
    {
        int count = Mathf.Min(handSize, handSlots.Length);

        for (int i = 0; i < count; i++)
        {
            CardView slot = handSlots[i];
            if (slot != null && slot.HasCard)
            {
                discardPile.Add(slot.Current);
                slot.ClearCard();
            }
        }
    }

    private void ReshuffleDiscardIntoDraw()
    {
        if (discardPile.Count == 0) return;

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
    }

    private void Shuffle(List<Card> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Card temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public void DiscardDeck()
    {
        Debug.LogWarning("Errror");
    }

    private void ResetRerollCount()
    {
        CurrentRerollCount = rerollPerFire;
        NotifyRerollCountChanged();
    }
    private void NotifyRerollCountChanged()
    {
        OnRerollCountChanged?.Invoke(CurrentRerollCount, MaxRerollCount);
    }
}
