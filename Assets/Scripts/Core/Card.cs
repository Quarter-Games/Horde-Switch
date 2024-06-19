using Fusion;
using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Card : INetworkStruct
{
    public static CardResources[] _CardResources;
    public int ID;
    public CardResources cardValue => GetCardResourcesByID(ID);

    /// <summary>
    /// Factory Method for creating different types of cards based on the data from configs
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Card Create(int id)
    {
        if (_CardResources == null)
        {
            _CardResources = Resources.LoadAll<CardResources>("Cards");
            _CardResources = _CardResources.ToList().OrderBy(x => x.cardData.ID).ToArray();
        }

        switch (_CardResources[id-1].cardData.cardType)
        {
            //case CardResources.CardData.CardType.Creature:
            //    return new CreatureCard { ID = id, Value = CardResources[id].cardData.Value };
            //case CardResources.CardData.CardType.Spell:
            //    return new SpellCard { ID = id, Value = CardResources[id].cardData.Value };
            default:
                return new Card(id-1);
        }
    }
    public static CardResources GetCardResourcesByID(int id) 
    {
        if (_CardResources == null)
        {
            _CardResources = Resources.LoadAll<CardResources>("Cards");
            _CardResources = _CardResources.ToList().OrderBy(x => x.cardData.ID).ToArray();
        }
        return _CardResources[id - 1];
    }
    private Card(int id)
    {
        ID = id;
    }
}
