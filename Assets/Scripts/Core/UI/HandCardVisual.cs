using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardVisual : MonoBehaviour, IPointerClickHandler
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
        cardImage.enabled = card.ID != 0;
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
            Clear();
            Add(card);
        }
        else
        {
            if (card.CardData.cardValue.cardData.IsMonoSelectedCard())
            {
                Clear();
            }
            Add(card);
        }
        Changed?.Invoke();
    }
    public void DeselectCard(HandCardVisual card)
    {
        Remove(card);
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
        for (int i = Count - 1; i >= 0; i--)
        {
            this[i].DeselectCard();
        }
    }
    public void UseCards()
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            Debug.Log(Count);
            this[i].Use();
        }
    }
    public void UseRandomCard()
    {
        var list = this;
        list[UnityEngine.Random.Range(0, list.Count)].Use();
        Clear();
    }
}
