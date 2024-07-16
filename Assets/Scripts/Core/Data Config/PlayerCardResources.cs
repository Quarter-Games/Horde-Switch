﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardResources : CardResources
{
    protected PlayerCardData data;

    public override CardData cardData { get => data; }

    public static PlayerCardResources Create(List<string> data)
    {
        PlayerCardResources cardResources = CreateInstance<PlayerCardResources>();
        cardResources.data = new PlayerCardData(data);
        return cardResources;
    }
}

[Serializable]
public class PlayerCardData : CardData
{
    public PlayerCardData(List<string> data) : base(data)
    {
    }
}