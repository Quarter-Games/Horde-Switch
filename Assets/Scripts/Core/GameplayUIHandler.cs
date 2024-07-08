using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class GameplayUIHandler : MonoBehaviour
{
    public static Action RequestTurnSwap;
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
    [SerializeField] Button GameFinishButton;

    private void OnEnable()
    {
        HandCardVisual.OnCardClicked += CardClicked;
        HandCardVisual.CardDiscarded += CardDiscarded;
        TurnManager.TurnChanged += OnTurnSwap;
        TurnManager.CardStateUpdate += UpdateCardVisuals;
        TurnManager.PlayerGotDamage += UpdateHealth;
        TurnManager.PlayerDied += OnPlayerDied;
        Enemy.MinePlaced += Enemy_MinePlaced;
    }

    private void Enemy_MinePlaced()
    {
       StartCoroutine(ShowAndHideMessage("Mine placed"));
    }

    private void OnPlayerDied(PlayerController controller)
    {
        if (controller == _playerController)
        {
            Debug.Log("You Lost");
            TurnText.text = "You Lost";
            ShowMessageAndEndButton("You Lost");
            _localPlayerUIContainer.UpdateHealth(0);
        }
        else
        {
            Debug.Log("You Won");
            TurnText.text = "You Won";
            ShowMessageAndEndButton("You Won");
            _enemyPlayerUIContainer.UpdateHealth(0);
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
            StartCoroutine(ShowAndHideMessage("Your Turn"));
        }
        else
        {
            foreach (var card in PlayerCards)
            {
                card.DeselectCard();
            }
            Debug.Log("Opponent Turn");
            TurnText.text = "Opponent Turn";
            StartCoroutine(ShowAndHideMessage("Opponent Turn"));
        }
        UpdateCardVisuals();
    }

    public IEnumerator ShowAndHideMessage(string message)
    {
        PopUpWindowParent.gameObject.SetActive(true);
        PopUpText.text = message;
        yield return new WaitForSeconds(1);
        PopUpWindowParent.gameObject.SetActive(false);
    }
    public void ShowMessageAndEndButton(string message)
    {
        PopUpWindowParent.gameObject.SetActive(true);
        PopUpText.text = message;
        GameFinishButton.gameObject.SetActive(true);
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
        Enemy.MinePlaced -= Enemy_MinePlaced;
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