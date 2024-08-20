using System;
using System.Collections.Generic;
using UnityEngine;

public class PortalCardResources : CardResources
{
    [SerializeField] protected PortalCardData data;

    public override CardData DataOfCard { get => data; }

    public static PortalCardResources Create(List<string> data)
    {
        PortalCardResources cardResources = CreateInstance<PortalCardResources>();
        cardResources.data = new PortalCardData(data);
        return cardResources;
    }
}
[Serializable]
public class PortalCardData : CardData
{
    static public Enemy ClickedFirst;
    static public ParticleSystem PortalEffect;
    public PortalCardData(List<string> data) : base(data)
    {
    }
    public override bool IsMonoSelectedCard()
    {
        return true;
    }
    public override List<Enemy> GetPossibleEnemies(List<Enemy> enemies, int playerRow)
    {
        var temp = enemies.FindAll(x => x.RowNumber == 0 || x.RowNumber == 2);
        if (ClickedFirst != null)
            temp = temp.FindAll(
                x => x.RowNumber != ClickedFirst.RowNumber || x.ColumnNumber == ClickedFirst.ColumnNumber);
        Debug.Log("Portal");
        foreach (var item in temp)
        {
            Debug.Log($"Value: {item.Card.CardValue.DataOfCard.Value}; Pos Y and X: {item.ColumnNumber}, {item.RowNumber}");
        }
        return temp;
    }
    public override void ApplyEffect(TurnManager manager, Enemy enemy)
    {
        if (ClickedFirst == null)
        {
            ClickedFirst = enemy;
            var card1 = HandCardVisual.selectedCards[0];
            HandCardVisual.selectedCards[0].Use(false);
            HandCardVisual.selectedCards.TriggerChanged();
            PortalEffect = GameObject.Instantiate(card1.CardData.CardValue.OnActivateEffect, enemy.transform.position + Vector3.up, Quaternion.Euler(-90, 0, 0));
            return;
        }
        PortalEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (ClickedFirst == enemy)
        {
            ClickedFirst = null;
            //HandCardVisual.selectedCards[0].StartCoroutine(HandCardVisual.selectedCards[0].CardResolving());
            HandCardVisual.selectedCards[0].DeselectCard();
            return;
        }
        manager.RPC_SwapEnemies(enemy, ClickedFirst);
        var card = HandCardVisual.selectedCards.UseCards();
        manager.RPC_SetIfCardWasPlayed(PlayerController.players.Find(x => x.isLocalPlayer).PlayerID);
        ClickedFirst = null;
    }

}
