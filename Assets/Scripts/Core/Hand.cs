using Fusion;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Hand : INetworkStruct
{
    [Networked, Capacity(60)]
    [SerializeField]
    private NetworkLinkedList<Card> cards => default;
    public Hand(IEnumerable<Card> _cards)
    {
        foreach (var card in _cards)
        {
            cards.Add(card);
        }
    }
    public int Count => cards.Count;
    public void AddCard(Card card)
    {
        cards.Add(card);
    }
}
