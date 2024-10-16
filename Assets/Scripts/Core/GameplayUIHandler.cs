using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using UnityEngine.InputSystem.XR;

public class GameplayUIHandler : MonoBehaviour
{
    public static event Action<int> DiscardCardMultiplayer;
    public static Action RequestTurnSwap;
    [SerializeField] PlayerAvatars PlayerAvatars;
    [SerializeField] PlayerController _playerController;
    [SerializeField] PlayerController _opponentController;
    [SerializeField] TMP_Text _localPlayerName;
    [SerializeField] TMP_Text _enemyPlayerName;
    [SerializeField] TMP_Text playerAmount;
    [SerializeField] TMP_Text opponentAmount;
    [SerializeField] List<HandCardVisual> PlayerCards;
    [SerializeField] List<HandCardVisual> EnemyCards;
    [SerializeField] PlayerUIContainer _localPlayerUIContainer;
    [SerializeField] PlayerUIContainer _enemyPlayerUIContainer;
    [SerializeField] TMP_Text TurnText;
    [SerializeField] Image PopUpWindowParent;
    [SerializeField] Image PopUpWindow;
    [SerializeField] TMP_Text PopUpText;
    [SerializeField] Button GameFinishButton;
    [SerializeField] TMP_Text CountDown;
    [SerializeField] Image TimerImage;
    [SerializeField] Button EndTurnButton;
    [SerializeField] TMP_Text EndTurnButtonText;
    [SerializeField] GameObject WinScreen;
    [SerializeField] GameObject LoseScreen;

    private void OnEnable()
    {
        HandCardVisual.OnCardClicked += CardClicked;
        TurnManager.TurnChanged += OnTurnSwap;
        TurnManager.CardStateUpdate += UpdateCardVisuals;
        TurnManager.PlayerGotDamage += UpdateHealth;
        TurnManager.PlayerDied += OnPlayerDied;
        Enemy.MinePlaced += Enemy_MinePlaced;
        HandCardVisual.selectedCards.Changed += UpdateCardVisuals;
        HandCardVisual.CardDiscarded += DiscardCard;
    }


    private void OnDisable()
    {
        HandCardVisual.OnCardClicked -= CardClicked;
        TurnManager.TurnChanged -= OnTurnSwap;
        TurnManager.CardStateUpdate -= UpdateCardVisuals;
        TurnManager.PlayerGotDamage -= UpdateHealth;
        TurnManager.PlayerDied -= OnPlayerDied;
        Enemy.MinePlaced -= Enemy_MinePlaced;
        HandCardVisual.selectedCards.Changed -= UpdateCardVisuals;
        HandCardVisual.CardDiscarded -= DiscardCard;
    }
    private void DiscardCard(HandCardVisual visual)
    {
        DiscardCardMultiplayer?.Invoke(PlayerCards.IndexOf(visual));
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
            StartCoroutine(ShowEndScreen(false));
            _localPlayerUIContainer.UpdateHealth(0);
        }
        else
        {
            Debug.Log("You Won");
            StartCoroutine(ShowEndScreen(true));
            _enemyPlayerUIContainer.UpdateHealth(0);
        }
    }

    public IEnumerator ShowEndScreen(bool isWin)
    {
        yield return new WaitForSeconds(1);
        if (isWin)
        {
            WinScreen.SetActive(true);
        }
        else
        {
            LoseScreen.SetActive(true);
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

    private void Start()
    {
        foreach (var player in PlayerController.players)
        {
            OnPlayerCreated(player);
        }
    }
    private void Update()
    {
        CountDown.text = ((int)TurnManager.Instance.Timer).ToString();
        TimerImage.fillAmount = TurnManager.Instance.Timer / TurnManager.MAX_TIME;
    }

    private void OnTurnSwap(PlayerController controller)
    {

        if (controller.isLocalPlayer && controller.IsThisTurn)
        {
            Debug.Log("Your Turn");
            TurnText.text = "Your Turn";
            EndTurnButton.interactable = true;
            StartCoroutine(ShowAndHideMessage("Your Turn"));
            UpdateCardVisuals();
            return;
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
    private void CardClicked(HandCardVisual visual)
    {
        if (_playerController.IsThisTurn)
        {
            visual.SelectCard();
            ChangeEnemyCardsVisual();
        }
    }

    private void ChangeEnemyCardsVisual()
    {
        EnemyCards.ForEach(x => x.cardImage.color = HandCardVisual.selectedCards.Any(x => x.CardData.CardValue.GetType() == typeof(SniperCardResource)) ? Color.green : Color.white);
    }

    private void OnPlayerCreated(PlayerController controller)
    {

        if (controller.isLocalPlayer)
        {
            _playerController = controller;
            _localPlayerName.text = controller.PlayerName;
            _localPlayerUIContainer.PlayerAvatar.sprite = PlayerAvatars[controller.ImageID];
            if (controller.IsThisTurn)
            {
                TurnText.text = "Your Turn";
                EndTurnButton.interactable = true;
            }
            else
            {
                TurnText.text = "Opponent Turn";
                EndTurnButton.interactable = false;
            }
        }
        else
        {
            _opponentController = controller;
            _enemyPlayerUIContainer.PlayerAvatar.sprite = PlayerAvatars[controller.ImageID];
            _enemyPlayerName.text = controller.PlayerName;
        }
        UpdateCardVisuals();
    }

    public void UpdateCardVisuals()
    {
        for (int i = 0; i < PlayerCards.Count; i++)
        {
            HandCardVisual cardVisual = PlayerCards[i];
            if (_playerController.Hand.Count <= i)
            {
                cardVisual.SetUpVisual(new Card());
                continue;
            }
            var card = _playerController.Hand[i];
            cardVisual.SetUpVisual(card);
        }
        var LeftX = -25;
        var RightX = 575;
        int center = (LeftX + RightX) / 2;
        int step = 200;
        int activeCards = PlayerCards.Count(x => x.IsActive);
        int index = (activeCards - 1) * -100;
        for (int i = 0; i < PlayerCards.Count; i++)
        {
            HandCardVisual cardVisual = PlayerCards[i];
            if (cardVisual.IsActive)
            {
                var offset = center + index;
                var cardPos = cardVisual.transform.position;
                cardVisual.isComingToHand = true;
                bool selected = HandCardVisual.selectedCards.Contains(cardVisual);
                cardVisual.SetNewAnchoredPosition(new Vector2(offset, -75 + (selected ? 100 : 0)));
                index += step;
            }
        }
        if (_opponentController == null) return;
        for (int i = 0; i < _opponentController.Hand.Count; i++)
        {
            GameObject cardVisual = EnemyCards[i].gameObject;
            if (_opponentController.Hand[i].ID <= 0)
            {
                cardVisual.SetActive(false);
                continue;
            }
            cardVisual.SetActive(true);
        }
        ChangeEnemyCardsVisual();
        bool canPlay = _playerController.CanPlayAnything();
        bool AlreadyPlayed = _playerController.IsPlayedInThisTurn;
        EndTurnButtonText.text = "End Turn";
        if (canPlay && !AlreadyPlayed)
        {
            EndTurnButton.interactable = false;
        }
        else if (canPlay && AlreadyPlayed)
        {
            EndTurnButton.interactable = true;
        }
        else if (!canPlay)
        {
            EndTurnButton.interactable = true;
            if (!AlreadyPlayed)
            {
                if (_playerController.HP == 2)
                {
                    EndTurnButtonText.text = "Lose Life";
                }
                else
                {
                    EndTurnButtonText.text = "The End is Here";
                }
            }
        }
    }
    public void EndTurn()
    {
        if (!_playerController.IsThisTurn) return;
        RequestTurnSwap?.Invoke();
        EndTurnButton.interactable = false;
    }
    public void LeaveGame()
    {
        Destroy(TurnManager.Instance.gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }

}
[Serializable]
public class PlayerUIContainer
{
    public TMPro.TMP_Text PlayerName;
    public Image PlayerAvatar;
    public Image FirstHealthIndicator;
    public Image SecondHealthIndicator;
    public Sprite FullHeart;
    public Sprite EmptyHeart;
    public void UpdateHealth(int health)
    {
        if (health == 2)
        {
            FirstHealthIndicator.sprite = FullHeart;
            SecondHealthIndicator.sprite = FullHeart;
        }
        else if (health == 1)
        {
            FirstHealthIndicator.sprite = FullHeart;
            SecondHealthIndicator.sprite = EmptyHeart;
        }
        else
        {
            FirstHealthIndicator.sprite = EmptyHeart;
            SecondHealthIndicator.sprite = EmptyHeart;
        }
    }
}