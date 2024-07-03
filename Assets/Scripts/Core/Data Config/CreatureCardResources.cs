using System.Collections.Generic;
using UnityEngine;

public class CreatureCardResources : CardResources
{
    [SerializeField] protected CreatureCardData data;

    public override CardData cardData { get => data; }
    public static CreatureCardResources Create(List<string> data)
    {
        CreatureCardResources cardResources = CreateInstance<CreatureCardResources>();
        cardResources.data = new CreatureCardData(data);
        return cardResources;
    }
}
