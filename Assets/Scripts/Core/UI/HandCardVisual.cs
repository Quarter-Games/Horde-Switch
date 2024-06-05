using Fusion;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardVisual : NetworkBehaviour, IPointerClickHandler
{
    public static HandCardVisual selectedCard;
    public Image cardImage;
    public string Card_ID { get; private set; }
    private CardResources cardData;
    public static Action<HandCardVisual> OnCardClicked;

    public void SetUpVisual(string card_id)
    {

        gameObject.SetActive(card_id != "0");
        if (card_id == "0")
        {
            return;
        }
        Card_ID = card_id;
        cardData = Resources.Load<CardResources>("Cards/"+card_id);
        cardImage.sprite = cardData.cardSprite;

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked?.Invoke(this);
        Debug.Log("Card Selected");
    }
    public void SelectCard()
    {
        if (selectedCard != null) selectedCard.DeselectCard();
        cardImage.color = Color.green;
        selectedCard = this;
    }
    public void DeselectCard()
    {
        cardImage.color = Color.white;
    }

}
