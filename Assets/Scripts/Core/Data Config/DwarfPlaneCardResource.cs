using System;
using System.Collections.Generic;
using UnityEngine;

public class DwarfPlaneCardResource : CardResources
{
    [SerializeField] new protected DwarfPlaneCardData data;

    public override CardData cardData { get => data; }

    public static DwarfPlaneCardResource Create(List<string> data)
    {
        DwarfPlaneCardResource cardResources = CreateInstance<DwarfPlaneCardResource>();
        cardResources.data = new DwarfPlaneCardData(data);
        return cardResources;
    }
}

[Serializable]
public class DwarfPlaneCardData : CardData
{
    public DwarfPlaneCardData(List<string> data) : base(data)
    {
    }
}