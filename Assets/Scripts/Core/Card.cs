using Fusion;
using System;
using UnityEngine;

[Serializable]
public struct Card : INetworkStruct
{
    public static CardResources[] _CardResources;
    public int ID;

    /// <summary>
    /// Factory Mthod for creating different types of cards based on the data from configs
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Card Create(int id)
    {
        if (_CardResources == null) _CardResources = Resources.LoadAll<CardResources>("Cards");
        switch (_CardResources[id].cardData.cardType)
        {
            //case CardResources.CardData.CardType.Creature:
            //    return new CreatureCard { ID = id, Value = CardResources[id].cardData.Value };
            //case CardResources.CardData.CardType.Spell:
            //    return new SpellCard { ID = id, Value = CardResources[id].cardData.Value };
            default:
                return new Card(id);
        }
    }
    private Card(int id)
    {
        ID = id;
    }
}
