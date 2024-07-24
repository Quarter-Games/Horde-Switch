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
    [SerializeField] private Enemy ClickedFirst;
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
            return;
        }
        if (ClickedFirst == enemy)
        {
            ClickedFirst = null;
            return;
        }
        manager.RPC_SetIfCardWasPlayed(PlayerController.players.Find(x => x.isLocalPlayer).PlayerID);
        var card = HandCardVisual.selectedCards.UseCards();
        manager.RPC_SwapEnemies(enemy, ClickedFirst);
        ClickedFirst = null;
        CardIsPlayed?.Invoke(card, enemy.GetEffectSpawnPosition());
    }

}
