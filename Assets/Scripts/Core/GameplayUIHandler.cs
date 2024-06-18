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
        TurnManager.TurnChanged += OnTurnSwap;
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
    }

    private void CardClicked(HandCardVisual visual)
    {
        if (_playerController.isThisTurn)
        {
            visual.SelectCard();
        }
    }

    public void AddCard()
    {
        _playerController.AddCard();
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
            _playerController.Object.RequestStateAuthority();
            for (int i = 0; i < PlayerCards.Count; i++)
            {
                HandCardVisual carVisual = PlayerCards[i];
                var card = controller.hand[i];
                carVisual.SetUpVisual(card);
            }
        }
        else
        {
            _opponentController = controller;
            int cards = _opponentController.hand.Count;
            for (int i = 0; i < EnemyCards.Count; i++)
            {
                GameObject card = EnemyCards[i];
                card.SetActive(i<=cards);
            }
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
    }

}
