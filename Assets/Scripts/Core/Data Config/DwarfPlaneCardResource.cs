using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DwarfPlaneCardResource : CardResources
{
    [SerializeField] protected DwarfPlaneCardData data;

    public override CardData DataOfCard { get => data; }

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
    public override bool IsMonoSelectedCard() => true;
    public override void ApplyEffect(TurnManager manager, Enemy enemy)
    {
        manager.RPC_SetIfCardWasPlayed(PlayerController.players.Find(x => x.isLocalPlayer).PlayerID);
        Card usedCard;
        usedCard = HandCardVisual.selectedCards.UseCards();
        var row = enemy.RowNumber;
        var column = enemy.ColumnNumber;
        if (column == -1) column = 0;
        else if (column == 0) column = 1;
        else if (column == 1) column = 0;
        else if (column == 2) column = 1;
        manager.RPC_KillEnemy(manager.EnemyList.First(x => x.RowNumber == row && x.ColumnNumber == column));
        manager.RPC_KillEnemy(enemy);
        CardIsPlayed?.Invoke(usedCard, enemy.GetEffectSpawnPosition());
    }
    public override List<Enemy> GetPossibleEnemies(List<Enemy> enemies, int playerRow)
    {
        return enemies.FindAll(x => x.RowNumber == playerRow);
    }
}