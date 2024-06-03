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

    private void OnEnable()
    {
        foreach (var player in PlayerController.players)
        {
            OnPlayerCreated(player);
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
            for (int i = 0; i < PlayerCards.Count; i++)
            {
                HandCardVisual carVisual = PlayerCards[i];
                var card = controller.hand[i];
                carVisual.SetUpVisual(card.ID.ToString());
            }
        }
        else
        {
            _opponentController = controller;
        }
    }

}
