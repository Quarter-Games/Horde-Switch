using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static List<PlayerController> players = new();
    public static event System.Action<PlayerController> PlayerCreated;
    [Networked] public int PlayerID { get => default; set { } }
    public bool isLocalPlayer;
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
        Debug.Log($"Deleted Card: {card.ID}");
        Debug.Log($"Hand Count: {Hand.Count}");

        var handCopy = Hand;
        handCopy.RemoveCard(card);
        Hand = handCopy;
        Debug.Log($"Hand Count: {Hand.Count}");
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_DrawCard(Deck cards)
    {
        var handCopy = Hand;
        handCopy.AddCard(cards.Draw());
        Hand = handCopy;

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
