using System;
using System.Collections.Generic;
using UnityEngine;

public class PortalCardResources : CardResources
{
    [SerializeField] new protected PortalCardData data;

    public override CardData cardData { get => data; }

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
        var temp = enemies.FindAll(x => x.rowNumber == 0 || x.rowNumber == 2);
        if (ClickedFirst != null)
            temp = temp.FindAll(
                x => x.rowNumber != ClickedFirst.rowNumber || x.columnNumber == ClickedFirst.columnNumber);
        Debug.Log("Portal");
        foreach (var item in temp)
        {
            Debug.Log($"Value: {item.Card.cardValue.cardData.Value}; Pos Y and X: {item.columnNumber}, {item.rowNumber}");
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
        HandCardVisual.selectedCard.UseCards();
        manager.RPC_SwapEnemies(enemy, ClickedFirst);
        ClickedFirst = null;
    }

}
