using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static List<PlayerController> players = new();
    public static event System.Action<PlayerController> PlayerCreated;
    [Networked] public string PlayerName { get => default; set { } }
    [Networked] public int ImageID { get => default; set { } }
    [Networked] public int PlayerID { get => default; set { } }
    public bool isLocalPlayer { get; set; }
    [Networked] public int MainRow { get => default; set { } }
    [Networked] public bool IsThisTurn { get => default; set { } }
    [Networked] public Hand Hand { get => default; set { } }
    [Networked] public int HP { get => default; set { } }
    [Networked] public bool IsPlayedInThisTurn { get => default; set { } }

    public void ChangeTurn()
    {
        IsThisTurn = !IsThisTurn;
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_RemoveCard(Card card)
    {
        var handCopy = Hand;
        handCopy.RemoveCard(card);
        Hand = handCopy;
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_RemoveCardAt(int index)
    {
        var handCopy = Hand;
        handCopy.RemoveAt(index);
        Hand = handCopy;
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_DrawCard(Deck cards)
    {
        var handCopy = Hand;
        handCopy.AddCard(cards.Draw());
        Hand = handCopy;

    }
    public bool CanPlayAnything()
    {
        int value = 0;
        for (int i = 0; i < Hand.Count; i++)
        {
            if (Hand[i].ID == 0) continue;
            if (Hand[i].CardValue.DataOfCard.IsMonoSelectedCard())
            {
                return true;
            }
            else
            {
                value += Hand[i].CardValue.DataOfCard.Value;
            }
            if (Hand[i].CardValue.DataOfCard.GetPossibleEnemies(TurnManager.Instance.EnemyList.ToList(), MainRow).Count != 0)
            {
                return true;
            }
        }
        if (TurnManager.Instance.EnemyList.ToList().Any(x => x.RowNumber == MainRow && x.Card.CardValue.DataOfCard.Value <= value)) return true;
        return false;
    }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public override void Spawned()
    {
        players.Add(this);
    }
    private void Start()
    {
        isLocalPlayer = Runner.LocalPlayer.PlayerId == PlayerID;
        PlayerCreated?.Invoke(this);
        if (isLocalPlayer)
        {
            RPC_SetNameAndImage(PlayerID, MainMenuManager.playerName, MainMenuManager.AvatarID);
        }
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetNameAndImage(int ID, string Name, int imageID)
    {
        if (ID == PlayerID)
        {
            PlayerName = Name;
            ImageID = imageID;
        }
    }
    public Deck SetUp(Deck _deck, int HandSize)
    {
        List<Card> cards = new();
        for (int i = 0; i < HandSize; i++)
        {
            //Declare player Hand
            var deckCopy = _deck;
            var card = deckCopy.Draw();
            _deck = deckCopy;
            cards.Add(card);
        }
        Hand = new Hand(cards);
        return _deck;

    }
    private void OnDestroy()
    {
        players.Remove(this);
    }
}
