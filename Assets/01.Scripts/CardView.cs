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

    public bool TryConsumeForSquad(out Card consumedCard)
    {
        if (!HasCard || Current == null)
        {
            consumedCard = null;
            return false;
        }

        consumedCard = Current;
        HasCard = false;
        Current = null;
        gameObject.SetActive(false);
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
