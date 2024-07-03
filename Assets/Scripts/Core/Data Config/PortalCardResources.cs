using System.Collections.Generic;
using UnityEngine;

public class PortalCardResources : CardResources
{
    [SerializeField] protected PortalCardData data;

    public override CardData cardData { get => data; }

    public static PortalCardResources Create(List<string> data)
    {
        PortalCardResources cardResources = CreateInstance<PortalCardResources>();
        cardResources.data = new PortalCardData(data);
        return cardResources;
    }
}
