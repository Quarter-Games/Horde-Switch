using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class GameplayUIHandler : MonoBehaviour
{
    [SerializeField] PlayerController _playerController;
    [SerializeField] PlayerController _opponentController;
    [SerializeField] TMP_Text playerAmount;
    [SerializeField] TMP_Text opponentAmount;
    [SerializeField] List<HandCardVisual> PlayerCards;
    [SerializeField] List<GameObject> EnemyCards;
    [SerializeField] PlayerUIContainer _localPlayerUIContainer;
    [SerializeField] PlayerUIContainer _enemyPlayerUIContainer;
    [SerializeField] TMP_Text TurnText;
    [SerializeField] Image PopUpWindowParent;
    [SerializeField] Image PopUpWindow;
    [SerializeField] TMP_Text PopUpText;
    public static Action RequestTurnSwap;

    private void OnEnable()
    {
        HandCardVisual.OnCardClicked += CardClicked;
        HandCardVisual.CardDiscarded += CardDiscarded;
        TurnManager.TurnChanged += OnTurnSwap;
        TurnManager.CardStateUpdate += UpdateCardVisuals;
        TurnManager.PlayerGotDamage += UpdateHealth;
        TurnManager.PlayerDied += OnPlayerDied;
    }

    private void OnPlayerDied(PlayerController controller)
    {
        if (controller == _playerController)
        {
            Debug.Log("You Lost");
            TurnText.text = "You Lost";
            StartCoroutine(ShowMessage("You Lost"));
        }
        else
        {
            Debug.Log("You Won");
            TurnText.text = "You Won";
            StartCoroutine(ShowMessage("You Won"));
        }
    }

    private void UpdateHealth(PlayerController controller)
    {
        if (controller == _playerController)
        {
            _localPlayerUIContainer.UpdateHealth(controller.HP);
        }
        else
        {
            _enemyPlayerUIContainer.UpdateHealth(controller.HP);
        }
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

        if (controller.isLocalPlayer && controller.isThisTurn)
        {
            Debug.Log("Your Turn");
            TurnText.text = "Your Turn";
            StartCoroutine(ShowMessage("Your Turn"));
        }
        else
        {
            foreach (var card in PlayerCards)
            {
                card.DeselectCard();
            }
            Debug.Log("Opponent Turn");
            TurnText.text = "Opponent Turn";
            StartCoroutine(ShowMessage("Opponent Turn"));
        }
        UpdateCardVisuals();
    }

    public IEnumerator ShowMessage(string message)
    {
        PopUpWindowParent.gameObject.SetActive(true);
        PopUpText.text = message;
        yield return new WaitForSeconds(1);
        PopUpWindowParent.gameObject.SetActive(false);
    }
    private void CardClicked(HandCardVisual visual)
    {
        if (_playerController.isThisTurn)
        {
            visual.SelectCard();
        }
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
        TurnManager.PlayerGotDamage -= UpdateHealth;
        TurnManager.PlayerDied -= OnPlayerDied;
    }

}
[Serializable]
public class PlayerUIContainer
{
    public TMPro.TMP_Text PlayerName;
    public Image PlayerAvatar;
    public Image FirstHealthIndicator;
    public Image SecondHealthIndicator;
    public void UpdateHealth(int health)
    {
        if (health == 2)
        {
            FirstHealthIndicator.color = Color.red;
            SecondHealthIndicator.color = Color.red;
        }
        else if (health == 1)
        {
            FirstHealthIndicator.color = Color.red;
            SecondHealthIndicator.color = Color.black;
        }
        else
        {
            FirstHealthIndicator.color = Color.black;
            SecondHealthIndicator.color = Color.black;
        }
    }
}
