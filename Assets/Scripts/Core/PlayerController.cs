using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static List<PlayerController> players = new();
    public static event System.Action<PlayerController> PlayerCreated;
    public int PlayerID;
    public bool isLocalPlayer;
    [Networked] public bool isThisTurn { get => default; set { } }
    [Networked] public Hand hand { get => default; set { } }
    [Networked] public Deck deck { get => default; set { } }
    [ContextMenu("Add Card")]

    [Rpc]
    public void RPC_ChangeTurn()
    {
        isThisTurn = !isThisTurn;
    }
    public void AddCard()
    {
        Debug.LogWarning("Delete This Method");
        //var copy = hand;
        //copy.AddCard(new Card { Value = 1 });
        //hand = copy;
    }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        players.Add(this);
        hand = new();
        deck = new();
        PlayerCreated?.Invoke(this);
    }
    public void SetUp(List<int> DeckCardIDs, int DeckSize, int HandSize)
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < DeckSize; i++)
        {
            //Declare player Deck
            int k = i % DeckCardIDs.Count;
            cards.Add(Card.Create(DeckCardIDs[k]));
        }
        deck = new Deck(cards);
        cards.Clear();
        for (int i = 0; i < HandSize; i++)
        {
            //Declare player Hand
            var _deck = this.deck;
            cards.Add(_deck.Draw());
            deck = _deck;
        }
        hand = new Hand(cards);

    }
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        hand = hand;
        deck = deck;
    }
    private void OnDestroy()
    {
        players.Remove(this);
    }
}
