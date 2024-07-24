using Assets.Scripts.SoundSystem;
using Fusion;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardVisual : MonoBehaviour, IPointerClickHandler, IEffectPlayer
{
    public static event Action<Card> CardDiscarded;
    public static SelectedCards selectedCard = new();
    public static Action<HandCardVisual> OnCardClicked;
    public static event Action<Card> OnCardPlaySound;
    public string Card_ID { get; private set; }
    public Card CardData;
    public Image cardImage;
    [SerializeField] Material DisolvingMaterialStart;
    [SerializeField] Material DisolvingMaterialFinished;

    public void SetUpVisual(Card card)
    {
        Material material = new(DisolvingMaterialStart)
        {
            enableInstancing = true
        };
        cardImage.material = material;
        CardData = card;
        cardImage.enabled = card.ID != 0;
        if (card.ID == 0)
        {
            return;
        }
        Card_ID = card.ID.ToString();
        cardImage.sprite = CardData.CardValue.cardSprite;

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
        StartCoroutine(LerpInDirection(Vector2.up * 100));
        cardImage.color = Color.green;
    }
    public void DeselectCard()
    {
        if (selectedCard.DeselectCard(this))
        {
            StartCoroutine(LerpInDirection(Vector2.down * 100));
        }
        cardImage.color = Color.white;
    }
    public void PlaySFX()
    {
        OnCardPlaySound.Invoke(CardData);
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
    public IEnumerator LerpInDirection(Vector2 direction)
    {
        for (int i = 0; i < 60; i++)
        {
            cardImage.rectTransform.anchoredPosition += (direction / 60f);
            yield return new WaitForSeconds(1 / 60f);
        }
    }
    public bool IsActive => cardImage.enabled;
    public void SetNewAnchoredPosition(Vector2 newPos)
    {
        StartCoroutine(LerpInDirection(newPos - cardImage.rectTransform.anchoredPosition));
    }

}
public class SelectedCards : List<HandCardVisual>
{
    public event Action Changed;
    public void SelectCard(HandCardVisual card)
    {
        if (this.Any(x => x.CardData.CardValue.DataOfCard.IsMonoSelectedCard()))
        {
            Clear();
            Add(card);
        }
        else
        {
            if (card.CardData.CardValue.DataOfCard.IsMonoSelectedCard())
            {
                Clear();
            }
            Add(card);
        }
        Changed?.Invoke();
    }
    public bool DeselectCard(HandCardVisual card)
    {
        var changed = Remove(card);
        Changed?.Invoke();
        return changed;
    }
    public int CardValues()
    {
        return this.Sum(x => x.CardData.CardValue.DataOfCard.Value);
    }
    public List<Enemy> GetValidEnemies(List<Enemy> enemies, int playerRow)
    {
        if (Count == 0) return new();
        if (Count == 1) return this[0].CardData.CardValue.DataOfCard.GetPossibleEnemies(enemies, playerRow);
        return enemies.FindAll(x => x.Card.CardValue.DataOfCard.Value <= CardValues() && x.RowNumber == playerRow);
    }
    new public void Clear()
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            this[i].DeselectCard();
        }
    }
    public Card UseCards()
    {
        HandCardVisual highestValueCard = this.OrderByDescending(card => card.CardData.CardValue.DataOfCard.Value).FirstOrDefault();
        highestValueCard.PlaySFX();
        for (int i = Count - 1; i >= 0; i--)
        {
            Debug.Log(Count);
            this[i].Use();
        }
        return highestValueCard.CardData;
    }
    public Card UseRandomCard()
    {
        var list = this;
        var card = list[UnityEngine.Random.Range(0, list.Count)];
        card.PlaySFX();
        card.Use();
        Clear();
        return card.CardData;
    }
}
