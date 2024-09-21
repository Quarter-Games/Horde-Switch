using UnityEngine;

public class HelpPopUp : MonoBehaviour
{
    [SerializeField] GameObject HelpObject;
    [SerializeField] TMPro.TMP_Text Desription;
    [SerializeField] HandCardVisual currentCard;
    [SerializeField] Sprite EnabledButton;
    [SerializeField] Sprite DisabledButton;

    private void OnEnable()
    {
        HandCardVisual.HelpButtonPressed += HandCardVisual_HelpButtonPressed;
        HandCardVisual.CardDiscarded += HandCardVisual_CardDiscarded;
    }

    private void HandCardVisual_CardDiscarded(HandCardVisual obj)
    {
        if (currentCard == obj)
        {
            obj.HelpButton.sprite = DisabledButton;
            HelpObject.SetActive(false);
            currentCard = null;
        }
    }

    private void OnDisable()
    {
        HandCardVisual.HelpButtonPressed -= HandCardVisual_HelpButtonPressed;
        HandCardVisual.CardDiscarded -= HandCardVisual_CardDiscarded;
    }
    private void HandCardVisual_HelpButtonPressed(HandCardVisual cardVisual)
    {
        if (cardVisual == currentCard)
        {
            cardVisual.HelpButton.sprite = DisabledButton;
            HelpObject.SetActive(false);
            currentCard = null;
            return;
        }
        if (currentCard != null)
        {
            currentCard.HelpButton.sprite = DisabledButton;
        }
        HelpObject.SetActive(true);
        Desription.text = cardVisual.CardData.CardValue.DataOfCard.Name;
        currentCard = cardVisual;
        cardVisual.HelpButton.sprite = EnabledButton;
        return;
    }
}
