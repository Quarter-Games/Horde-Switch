using Fusion;
using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Card : INetworkStruct
{
    public static CardResources[] _CardResources = null;
    public int ID;
    readonly public CardResources CardValue => GetCardResourcesByID(ID);
    
    public static Card Create(int id)
    {
        if (_CardResources == null)
        {
            _CardResources = Resources.LoadAll<CardResources>("Cards");
            _CardResources = _CardResources.OrderBy(x => x.DataOfCard.ID).ToArray();
        }
        var card = new Card(id);
        Debug.Log($"ID: {id}; Type: {card.CardValue.DataOfCard.GetType()}");
        return card;
    }
    public static CardResources GetCardResourcesByID(int id)
    {
        if (_CardResources == null)
        {
            _CardResources = Resources.LoadAll<CardResources>("Cards");
            _CardResources = _CardResources.OrderBy(x => x.DataOfCard.ID).ToArray();
        }
        return _CardResources[id - 1];
    }
    private Card(int id)
    {
        ID = id;
    }
}
