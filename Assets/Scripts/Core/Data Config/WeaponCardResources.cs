using System.Collections.Generic;
using UnityEngine;

public class WeaponCardResources : CardResources
{
    [SerializeField] new protected CardData data;

    public override CardData cardData { get => data; }

    new public static WeaponCardResources Create(List<string> data)
    {
        WeaponCardResources cardResources = CreateInstance<WeaponCardResources>();
        cardResources.data = new CardData(data);
        return cardResources;
    }
}
