using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    protected override void Init()
    {
        gameOverImage.SetActive(false);
    }
    [SerializeField] private DeckController deckController;
    [SerializeField] private TMP_Text rerollCountText;
    [SerializeField] private Button rerollButton;

    [SerializeField] private GameObject gameOverImage;

    private void OnEnable()
    {
        if (deckController == null) return;

        deckController.OnRerollCountChanged += HandleRerollCountChanged;
        HandleRerollCountChanged(deckController.CurrentRerollCount, deckController.MaxRerollCount);
    }

    private void OnDisable()
    {
        if (deckController == null) return;
        deckController.OnRerollCountChanged -= HandleRerollCountChanged;
    }

    private void HandleRerollCountChanged(int current, int max)
    {
        if (rerollCountText != null)
            rerollCountText.text = $"{current} / {max}";

        if (rerollButton != null)
            rerollButton.interactable = current > 0;
    }
    public void GameOverImagePopup()
    {
        gameOverImage.SetActive(true);
    }
}
