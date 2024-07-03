using System.Collections.Generic;
using UnityEngine;

public class WeaponCardResources : CardResources
{
    [SerializeField] protected PlayerCardData data;

    public override CardData cardData { get => data; }

    public static WeaponCardResources Create(List<string> data)
    {
        WeaponCardResources cardResources = CreateInstance<WeaponCardResources>();
        cardResources.data = new PlayerCardData(data);
        return cardResources;
    }
}
