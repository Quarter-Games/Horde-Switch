using Fusion;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardVisual : NetworkBehaviour, IPointerClickHandler
{
    public static SelectedCards selectedCard = new();
    public Image cardImage;
    public string Card_ID { get; private set; }
    public Card CardData;
    public static Action<HandCardVisual> OnCardClicked;

    public void SetUpVisual(Card card)
    {
        CardData = card;
        gameObject.SetActive(card.ID != 0);
        if (card.ID == 0)
        {
            return;
        }
        Card_ID = card.ID.ToString();
        cardImage.sprite = CardData.cardValue.cardSprite;

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked?.Invoke(this);
        Debug.Log("Card Selected");
    }
    public void SelectCard()
    {
        if(selectedCard.Contains(this))
        {
            DeselectCard();
            return;
        }
        selectedCard.SelectCard(this);
        cardImage.color = Color.green;
    }
    public void DeselectCard()
    {
        selectedCard.DeselectCard(this);
        cardImage.color = Color.white;
    }

}
public class SelectedCards : List<HandCardVisual>
{
    public event Action Changed;
    public void SelectCard(HandCardVisual card)
    {
        if (this.Any(x => x.CardData.cardValue.cardData.IsMonoSelectedCard()))
        {
            this.ForEach(x => x.DeselectCard());
            this.Add(card);
        }
        else
        {
            if (card.CardData.cardValue.cardData.IsMonoSelectedCard())
            {
                this.ForEach(x => x.DeselectCard());
            }
            this.Add(card);
        }
        Changed?.Invoke();
        Debug.Log(CardValues());
    }
    public void DeselectCard(HandCardVisual card)
    {
        this.Remove(card);
        Changed?.Invoke();
    }
    public int CardValues()
    {
        return this.Sum(x => x.CardData.cardValue.cardData.Value);
    }
    public List<Enemy> GetValidEnemies(List<Enemy> enemies,int playerRow)
    {
        if (Count == 0) return new();
        if (Count == 1) return this[0].CardData.cardValue.cardData.GetPossibleEnemies(enemies, playerRow);
        return enemies.FindAll(x => x.Card.cardValue.cardData.Value <= CardValues() && x.rowNumber == playerRow);
    }
}
