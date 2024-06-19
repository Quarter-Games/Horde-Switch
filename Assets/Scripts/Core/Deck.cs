using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Deck : INetworkStruct
{
    [Networked,Capacity(60)]
    [SerializeField]
    private NetworkLinkedList<Card> cards => default;
    public Deck(IEnumerable<Card> _cards)
    {
        foreach (var card in _cards)
        {
            cards.Add(card);
        }
        Shuffle();
    }
    public Deck(Deck deck)
    {
        foreach (var card in deck.cards)
        {
            cards.Add(card);
        }
        Shuffle();
    }
    public void AddCard(Card card)
    {
        cards.Add(card);
    }

    public Card Peek()
    {
        if (cards.Count == 0)
        {
            return default;
        }
        return cards[0];
    }
    public Card Draw()
    {
        if (cards.Count == 0)
        {
            return default;
        }
        var card = cards[0];
        cards.Remove(card);
        return card;
    }
    public void Shuffle()
    {
        List<Card> temp = new List<Card>(cards);
        cards.Clear();
        System.Random random = new System.Random();
        while (temp.Count > 0)
        {
            int index = random.Next(0, temp.Count);
            cards.Add(temp[index]);
            temp.RemoveAt(index);
        }
    }
    public int Count => cards.Count;
}
