using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;

    [Header("Color Preset")]
    [SerializeField] private Color redColor = new Color32(235, 70, 70, 255);
    [SerializeField] private Color blueColor = new Color32(55, 255, 217, 255);
    [SerializeField] private Color greenColor = new Color32(7, 255, 67, 255);

    public Card Current { get; private set; }
    public bool HasCard { get; private set; }

    public void SetCard(Card data)
    {
        if (data == null)
        {
            ClearCard();
            return;
        }
        gameObject.SetActive(true);
        Current = data;
        HasCard = true;

        numberText.text = data.number.ToString();
        numberText.color = GetTextColor(data.color);
    }

    public void ClearCard()
    {
        gameObject.SetActive(true);
        HasCard = false;
        Current = null;
        numberText.text = "0";
        numberText.color = Color.white;
    }

    public bool TryTakeCard(out Card takenCard)
    {
        if (!HasCard || Current == null)
        {
            takenCard = null;
            return false;
        }

        takenCard = Current;
        HasCard = false;
        Current = null;
        return true;
    }




    private Color GetTextColor(CardColor color)
    {
        switch (color)
        {
            case CardColor.Red: return redColor;
            case CardColor.Blue: return blueColor;
            default: return greenColor;
        }
    }

}
