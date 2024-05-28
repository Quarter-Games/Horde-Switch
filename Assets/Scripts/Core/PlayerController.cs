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
    [Networked] public Hand hand { get => default; set { } }
    [Networked] public Deck deck { get => default; set { } }
    [ContextMenu("Add Card")]
    public void AddCard()
    {
        var copy = hand;
        copy.AddCard(new Card { Value = 1 });
        hand = copy;
    }
    private void Start()
    {
        players.Add(this);
        hand = new();
        deck = new();
        PlayerCreated?.Invoke(this);
    }
    private void OnDestroy()
    {
        players.Remove(this);
    }
}
