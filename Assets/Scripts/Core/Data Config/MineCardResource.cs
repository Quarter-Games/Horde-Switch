﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Core.Data_Config
{
    internal class MineCardResource : CardResources
    {
        [SerializeField] new protected MineCardData data;
        public override CardData cardData { get => data; }
        public static MineCardResource Create(List<string> data)
        {
            MineCardResource cardResources = CreateInstance<MineCardResource>();
            cardResources.data = new MineCardData(data);
            return cardResources;
        }
    }
}

[Serializable]
public class MineCardData : CardData
{
    public MineCardData(List<string> data) : base(data)
    {
    }
    public override void ApplyEffect(TurnManager manager, Enemy enemy)
    {
        manager.RPC_SetIfCardWasPlayed(PlayerController.players.Find(x => x.isLocalPlayer).PlayerID);
        HandCardVisual.selectedCard.UseCards();
        enemy.PlaceMine();
    }
    public override bool IsMonoSelectedCard()
    {
        return true;
    }
    public override List<Enemy> GetPossibleEnemies(List<Enemy> enemies, int playerRow)
    {
        var list = enemies.FindAll(x => x.rowNumber == 1);

        return list;
    }
}