using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameplayUIHandler : MonoBehaviour
{
    [SerializeField] PlayerController _playerController;
    [SerializeField] PlayerController _opponentController;
    [SerializeField] TMP_Text playerAmount;
    [SerializeField] TMP_Text opponentAmount;
    [SerializeField] List<HandCardVisual> PlayerCards;
    [SerializeField] List<GameObject> EnemyCards;
    public static Action RequestTurnSwap;

    private void OnEnable()
    {
        HandCardVisual.OnCardClicked += CardClicked;
        HandCardVisual.CardDiscarded += CardDiscarded;
        TurnManager.TurnChanged += OnTurnSwap;
        TurnManager.CardStateUpdate += UpdateCardVisuals;
    }

    private void CardDiscarded(Card card)
    {
        _playerController.RPC_RemoveCard(card);
        UpdateCardVisuals();
    }

    private void Start()
    {
        foreach (var player in PlayerController.players)
        {
            OnPlayerCreated(player);
        }
    }

    private void OnTurnSwap(PlayerController controller)
    {

        if (controller.isLocalPlayer)
        {
            Debug.Log("Your Turn");
        }
        else
        {
            foreach (var card in PlayerCards)
            {
                card.DeselectCard();
            }
            Debug.Log("Opponent Turn");
        }
        UpdateCardVisuals();
    }

    private void CardClicked(HandCardVisual visual)
    {
        if (_playerController.isThisTurn)
        {
            visual.SelectCard();
        }
    }

    private void Update()
    {
        if (playerAmount != null && _playerController != null) playerAmount.text = $"You have {_playerController.hand.Count} cards";
        if (opponentAmount != null && _opponentController != null) opponentAmount.text = $"Opponent has {_opponentController.hand.Count} cards";
    }
    private void OnPlayerCreated(PlayerController controller)
    {
        if (controller.isLocalPlayer)
        {
            _playerController = controller;
        }
        else
        {
            _opponentController = controller;
        }
        UpdateCardVisuals();
    }

    public void UpdateCardVisuals()
    {
        for (int i = 0; i < PlayerCards.Count; i++)
        {
            HandCardVisual cardVisual = PlayerCards[i];
            if (_playerController.hand.Count <= i)
            {
                cardVisual.SetUpVisual(new Card());
                continue;
            }
            var card = _playerController.hand[i];
            cardVisual.SetUpVisual(card);
        }
        if (_opponentController == null) return;
        for (int i = 0; i < EnemyCards.Count; i++)
        {
            GameObject cardVisual = EnemyCards[i];
            Debug.Log("Enemies Card View Changes");
            if (_opponentController.hand.Count <= i)
            {
                cardVisual.SetActive(false);
                continue;
            }
            cardVisual.SetActive(true);
        }

    }

    public void EndTurn()
    {
        if (!_playerController.isThisTurn) return;
        RequestTurnSwap?.Invoke();
    }
    private void OnDisable()
    {
        HandCardVisual.OnCardClicked -= CardClicked;
        HandCardVisual.CardDiscarded -= CardDiscarded;
        TurnManager.TurnChanged -= OnTurnSwap;
        TurnManager.CardStateUpdate -= UpdateCardVisuals;
    }

}
