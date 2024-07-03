using System.Collections.Generic;
using UnityEngine;

public class PlayerCardResources : CardResources
{
    [SerializeField] protected PlayerCardData data;

    public override CardData cardData { get => data; }

    public static PlayerCardResources Create(List<string> data)
    {
        PlayerCardResources cardResources = CreateInstance<PlayerCardResources>();
        cardResources.data = new PlayerCardData(data);
        return cardResources;
    }
}
