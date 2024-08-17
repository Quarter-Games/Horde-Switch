using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Hand : INetworkStruct
{
    [Networked, Capacity(60)]
    [SerializeField]
    readonly private NetworkLinkedList<Card> ListOfCards => default;
    public Hand(IEnumerable<Card> _cards)
    {
        foreach (var card in _cards)
        {
            ListOfCards.Add(card);
        }
    }
    readonly public int Count => ListOfCards.Count;
    readonly public void AddCard(Card card)
    {
        var index = ListOfCards.IndexOf(default);
        ListOfCards.Set(index, card);
    } 
    readonly public void RemoveCard(Card card)
    {
        var index = ListOfCards.IndexOf(card);
        ListOfCards.Set(index, default);
    }
    readonly public void RemoveAt(int index)
    {
        ListOfCards.Set(index, default);
    }
    readonly public Card this[int index]
    {
        get { return ListOfCards[index]; }
    }
    readonly public void CopyTo(ref List<Card> _cards)
    {
        foreach (var card in _cards)
        {
            ListOfCards.Add(card);
        }
    }
}
