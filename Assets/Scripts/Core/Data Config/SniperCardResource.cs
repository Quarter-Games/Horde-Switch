using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class SniperCardResource : CardResources
{
    [SerializeField] protected SniperCardData data;

    public override CardData DataOfCard { get => data; }

    public static SniperCardResource Create(List<string> data)
    {
        SniperCardResource cardResources = CreateInstance<SniperCardResource>();
        cardResources.data = new SniperCardData(data);
        return cardResources;
    }
}

[Serializable]
public class SniperCardData : CardData
{
    public SniperCardData(List<string> data) : base(data)
    {
    }
    public override bool IsMonoSelectedCard() => true;
    public void ApplyEffect(TurnManager manager, HandCardVisual cardVisual)
    {
        manager.RPC_SetIfCardWasPlayed(PlayerController.players.Find(x => x.isLocalPlayer).PlayerID);
        manager.RPC_UseSniperCard(cardVisual.IndexInHand);
        Card usedCard = HandCardVisual.selectedCards.UseCards();
        CardIsPlayed?.Invoke(usedCard, cardVisual.transform.position);
    }
}
