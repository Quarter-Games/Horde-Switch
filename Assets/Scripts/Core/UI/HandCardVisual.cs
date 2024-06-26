using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardVisual : NetworkBehaviour, IPointerClickHandler
{
    public static event Action<Card> CardDiscarded;
    public static SelectedCards selectedCard = new();
    public static Action<HandCardVisual> OnCardClicked;
    public string Card_ID { get; private set; }
    public Card CardData;
    [SerializeField] Image cardImage;
    [SerializeField] Material DisolvingMaterialStart;
    [SerializeField] Material DisolvingMaterialFinished;

    public void SetUpVisual(Card card)
    {
        Material material = new Material(DisolvingMaterialStart);
        material.enableInstancing = true;
        cardImage.material = material;
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
        if (selectedCard.Contains(this))
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
    public void Use()
    {
        DeselectCard();
        StartCoroutine(CardDisolving());
    }
    private IEnumerator CardDisolving()
    {
        for (int i = 0; i <= 59; i++)
        {            
            cardImage.material.Lerp(cardImage.material, DisolvingMaterialFinished, i / 59f);
            yield return new WaitForEndOfFrame();
        }
        CardDiscarded?.Invoke(CardData);
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
    public List<Enemy> GetValidEnemies(List<Enemy> enemies, int playerRow)
    {
        if (Count == 0) return new();
        if (Count == 1) return this[0].CardData.cardValue.cardData.GetPossibleEnemies(enemies, playerRow);
        return enemies.FindAll(x => x.Card.cardValue.cardData.Value <= CardValues() && x.rowNumber == playerRow);
    }
    new public void Clear()
    {
        this.ForEach(x => x.DeselectCard());
    }
    public void UseCards()
    {
        SelectedCards list = this;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            HandCardVisual card = list[i];
            card.Use();
        }
        base.Clear();
    }
}
