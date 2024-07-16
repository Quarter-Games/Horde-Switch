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
    [Networked] public bool isThisTurn { get => default; set { } }
    [Networked] public Hand hand { get => default; set { } }
    [Networked] public int HP { get => default; set { } }
    [Networked] public bool isPlayedInThisTurn { get => default; set { } }
    [ContextMenu("Add Card")]

    public void ChangeTurn()
    {
        isThisTurn = !isThisTurn;
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_RemoveCard(Card card)
    {
        Debug.Log($"Deleted Card: {card.ID}");
        Debug.Log($"Hand Count: {hand.Count}");

        var handCopy = hand;
        handCopy.RemoveCard(card);
        hand = handCopy;
        Debug.Log($"Hand Count: {hand.Count}");
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_DrawCard(Deck cards)
    {
        var handCopy = hand;
        handCopy.AddCard(cards.Draw());
        hand = handCopy;

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
        List<Card> cards = new List<Card>();
        for (int i = 0; i < HandSize; i++)
        {
            //Declare player Hand
            var deckCopy = _deck;
            var card = deckCopy.Draw();
            _deck = deckCopy;
            cards.Add(card);
        }
        hand = new Hand(cards);
        return _deck;

    }
    private void OnDestroy()
    {
        players.Remove(this);
    }
}
