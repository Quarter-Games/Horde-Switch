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
        }
        else
        {
            _opponentController = controller;
        }
    }

    private void OnDisable()
    {
    }
}
