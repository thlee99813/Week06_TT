using UnityEngine;

public enum CardColor
{
    Red,
    Blue,
    Green
}
[System.Serializable]

public class Card
{
    public CardColor color;
    public int number;

    public Card(CardColor color, int number)
    {
        this.color = color;
        this.number = number;
    }
}
