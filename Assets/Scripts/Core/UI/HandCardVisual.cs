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

public class HandCardVisual : MonoBehaviour, IPointerClickHandler, IEffectPlayer, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static event Action<Card> CardDiscarded;
    public static SelectedCards selectedCards = new();
    public static Action<HandCardVisual> OnCardClicked;
    public static event Action<Card> OnCardPlaySound;
    [SerializeField] private bool isDragging;
    public string Card_ID { get; private set; }
    public Card CardData;
    public Image cardImage;
    public bool isComingToHand;
    [SerializeField] Material DisolvingMaterialStart;
    [SerializeField] Material DisolvingMaterialFinished;
    [SerializeField] AnimationCurve cardMovementCurve;
    public static FloorTile lastHoverTile;
    private bool isDisolving;
    private Coroutine currentMovementRoutine;
    [SerializeField] private bool isEnemyCard;
    [SerializeField] public int IndexInHand;

    public void SetUpVisual(Card card)
    {
        bool wasActive = cardImage.enabled;
        if (CardData.ID != card.ID) StartCoroutine(CardResolving());
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
        if (isEnemyCard)
        {
            CalculateClickOnEnemyCard();
            return;
        }
        if (isDragging) return;
        OnCardClicked?.Invoke(this);
    }
    private void CalculateClickOnEnemyCard()
    {
        if (selectedCards.Count != 1) return;
        if (selectedCards[0].CardData.CardValue is SniperCardResource sc)
        {
            (sc.DataOfCard as SniperCardData).ApplyEffect(TurnManager.Instance, this);
        }
    }
    public void SelectCard()
    {
        if (selectedCards.Contains(this))
        {
            DeselectCard();
            return;
        }
        selectedCards.SelectCard(this);
        cardImage.color = Color.green;
    }
    public void DeselectCard()
    {
        if (selectedCards.DeselectCard(this))
        {
            if (CardData.CardValue is PortalCardResources pC)
            {
                StartCoroutine(CardResolving());
                PortalCardData.ClickedFirst = null;
            }
        }
        cardImage.color = Color.white;
    }
    public void PlaySFX()
    {
        OnCardPlaySound.Invoke(CardData);
    }
    public void Use(bool isDiscarded = true)
    {
        isDragging = false;
        if (isDiscarded) DeselectCard();
        cardImage.color = Color.green;
        StartCoroutine(CardDisolving(isDiscarded));
    }
    private IEnumerator CardDisolving(bool isDiscarded = true)
    {
        if (isDisolving) yield break;
        isDisolving = true;
        for (int i = 0; i <= 59; i++)
        {
            if (!isDisolving) break;
            cardImage.material.Lerp(cardImage.material, DisolvingMaterialFinished, i / 59f);
            yield return new WaitForEndOfFrame();
        }
        isDisolving = false;
        if (isDiscarded) Discard();
    }
    public IEnumerator CardResolving()
    {
        isDisolving = false;
        cardImage.material = new Material(DisolvingMaterialFinished) { enableInstancing = true };
        for (int i = 0; i <= 59; i++)
        {
            cardImage.material.Lerp(cardImage.material, DisolvingMaterialStart, i / 59f);
            yield return new WaitForEndOfFrame();
        }
    }
    public void Discard()
    {
        CardDiscarded?.Invoke(CardData);

    }
    public IEnumerator LerpInDirection(Vector2 direction, float time = 15f)
    {
        var timer = cardMovementCurve.keys[1].time;
        Vector2 startPos = cardImage.rectTransform.anchoredPosition;
        for (int i = 0; i <= time; i++)
        {
            if (isDragging) break;
            cardImage.rectTransform.anchoredPosition = startPos + direction * cardMovementCurve.Evaluate(i / time);
            yield return new WaitForSeconds(timer / time);
        }
        isComingToHand = false;
    }
    public bool IsActive => cardImage.enabled;
    public void SetNewAnchoredPosition(Vector2 newPos)
    {
        if (currentMovementRoutine != null) StopCoroutine(currentMovementRoutine);
        currentMovementRoutine = StartCoroutine(LerpInDirection(newPos - cardImage.rectTransform.anchoredPosition));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isEnemyCard) return;
        if (isDragging) return;
        if (!selectedCards.Contains(this)) OnCardClicked?.Invoke(this);
        for (int i = 0; i < selectedCards.Count; i++)
        {
            HandCardVisual card = selectedCards[i];
            card.isDragging = true;
            card.cardImage.color = new Color(0, 1, 0, 0.5f);
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        var pos = eventData.position;
        float center = pos.x - 100;
        int step = 200;
        int activeCards = selectedCards.Count;
        int index = (activeCards - 1) * -100;
        for (int i = 0; i < selectedCards.Count; i++)
        {
            HandCardVisual cardVisual = selectedCards[i];
            if (cardVisual.IsActive)
            {
                var offset = center + index;
                cardVisual.transform.position = new Vector2(offset, pos.y - 175);
                index += step;
            }
        }
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100, LayerMask.GetMask("Default")))
        {
            Debug.Log("Dragging");
            if (hit.collider.TryGetComponent(out Enemy enemy))
            {
                ClearLastHoveredTile();
                if (!selectedCards.GetValidEnemies(TurnManager.Instance.EnemyList.ToList(), TurnManager.Instance._localPlayer.MainRow).Contains(enemy))
                    return;
                lastHoverTile = TurnManager.Instance.GridTiles.Find(x => x.Enemy == enemy);
                lastHoverTile.UpdateHighlightStatus(HighlightStatus.Selected);
                Debug.Log("Enemy");
            }
            else if (hit.collider.TryGetComponent(out FloorTile tile))
            {
                ClearLastHoveredTile();
                if (!selectedCards.GetValidEnemies(TurnManager.Instance.EnemyList.ToList(), TurnManager.Instance._localPlayer.MainRow).Contains(tile.Enemy))
                    return;
                lastHoverTile = tile;
                tile.UpdateHighlightStatus(HighlightStatus.Selected);
                Debug.Log("Tile");
            }
            else
            {
                ClearLastHoveredTile();
                Debug.Log("Nothing");
            }
        }
        else
        {
            ClearLastHoveredTile();
            Debug.Log("Nothing");
        }
    }

    private void ClearLastHoveredTile()
    {
        if (lastHoverTile == null) return;
        lastHoverTile.UpdateHighlightStatus(HighlightStatus.Clickable);
        lastHoverTile = null;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        if (lastHoverTile != null)
        {
            lastHoverTile.OnPointerClick(eventData);
        }
        else
        {
            for (int i = selectedCards.Count - 1; i >= 0; i--)
            {
                HandCardVisual card = selectedCards[i];
                card.isDragging = false;
                OnCardClicked?.Invoke(card);
            }
            TurnManager.Instance.RPC_UpdateCardState();
        }
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
        TriggerChanged();

    }
    public void TriggerChanged()
    {
        Changed?.Invoke();
    }
    public bool DeselectCard(HandCardVisual card)
    {
        var changed = Remove(card);
        TriggerChanged();
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
