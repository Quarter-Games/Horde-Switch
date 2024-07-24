using Assets.Scripts.Core.Data_Config;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CardData;

[CreateAssetMenu(fileName = "CardResources", menuName = "Config/CardResources")]
abstract public class CardResources : DataBaseSynchronizedScribtableObject
{

    public AudioClip OnBeingPlayed;
    public ParticleSystem OnActivateEffect;
    public ParticleSystem OnDeathEffect;
    public Sprite cardSprite;
    abstract public CardData DataOfCard { get; }


#if UNITY_EDITOR
    public static CardResources Factory(List<string> data)
    {
        if (Enum.TryParse(typeof(CardType), data[4], out var cardType))
        {
            CardResources cardResources = null;
            switch (cardType)
            {
                case CardType.Creature:
                    cardResources = CreatureCardResources.Create(data);
                    break;
                case CardType.Portal:
                    cardResources = PortalCardResources.Create(data);
                    break;
                case CardType.Weapon:
                    cardResources = WeaponCardResources.Create(data);
                    break;
                case CardType.Mine:
                    cardResources = MineCardResource.Create(data);
                    break;
                case CardType.DwarfishPlane:
                    cardResources = DwarfPlaneCardResource.Create(data);
                    break;
            }

            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CardResources>($"Assets/Resources/Cards/{cardResources.DataOfCard.ID}.asset");
            if (asset != null)
            {
                cardResources.cardSprite = asset.cardSprite;
                cardResources.OnBeingPlayed = asset.OnBeingPlayed;
                cardResources.OnActivateEffect = asset.OnActivateEffect;
                cardResources.OnDeathEffect = asset.OnDeathEffect;
            }
            UnityEditor.AssetDatabase.CreateAsset(cardResources, $"Assets/Resources/Cards/{cardResources.DataOfCard.ID}.asset");
            return cardResources;

        }
        else
        {
            throw new Exception("CardType not found");
        }
    }
    [ContextMenu("Pull")]
    public override async void Pull()
    {
        var settings = Resources.Load<SheetsSettings>("SheetsSettings");

        var a = await DataBaseLoader.ReadTableAsync(
            "Cards",
            (s) => s,
            (list) => Factory(list),
            settings
            );
    }
#endif
}
[Serializable]
public class CardData
{
    public int ID;
    public int Value;
    public string Name;
    public OwnerType ownerType;
    public CardType cardType;
    public static Action<Card,Vector3> CardIsPlayed;
    public CardData(List<string> data)
    {
        ID = int.Parse(data[0]);
        Value = int.Parse(data[1]);
        if (data.Count > 2) Name = data[2];
        if (data.Count > 3 && Enum.TryParse(typeof(OwnerType), data[3], out var owner)) ownerType = (OwnerType)owner;
        if (data.Count > 4 && Enum.TryParse(typeof(CardType), data[4], out var value)) cardType = (CardType)value;
        Debug.Log($"ID: {data[0]}; Type: {this.GetType()}");

    }
    virtual public bool IsMonoSelectedCard()
    {
        return false;
    }
    virtual public List<Enemy> GetPossibleEnemies(List<Enemy> enemies, int playerRow)
    {
        var temp = enemies.FindAll(x => x.Card.CardValue.DataOfCard.Value <= Value && x.RowNumber == playerRow);
        return temp;
    }
    virtual public void ApplyEffect(TurnManager manager, Enemy enemy)
    {
        manager.RPC_SetIfCardWasPlayed(PlayerController.players.Find(x => x.isLocalPlayer).PlayerID);
        Debug.Log($"Is enemy has mine on it: {enemy.HasMine}");
        Card usedCard;
        if (!enemy.HasMine)
        {
            usedCard = HandCardVisual.selectedCards.UseCards();
            manager.RPC_KillEnemy(enemy);
        }
        else
        {
            usedCard = HandCardVisual.selectedCards.UseRandomCard();
            enemy.RPC_RemoveMine();
        }
        CardIsPlayed?.Invoke(usedCard, enemy.GetEffectSpawnPosition());
    }
    public enum OwnerType
    {
        Player,
        Enemy
    }
    public enum CardType
    {
        Creature,
        Weapon,
        Portal,
        DwarfishPlane,
        Mine,
        Sniper
    }
}
