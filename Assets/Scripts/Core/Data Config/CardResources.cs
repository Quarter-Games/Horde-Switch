using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardResources", menuName = "Config/CardResources")]
public class CardResources : DataBaseSynchronizedScribtableObject
{
    public CardData cardData;
    [SerializeField] public Sprite cardSprite;


#if UNITY_EDITOR
    [ContextMenu("Pull")]
    public override async void Pull()
    {
        var settings = Resources.Load<SheetsSettings>("SheetsSettings");

        var a = await DataBaseLoader.ReadTableAsync(
            "Cards",
            (s) => s,
            (list) => CardData.CardDataFactory(list),
            settings
            );
        //var assets = UnityEditor.AssetDatabase.FindAssets("t:CardResources");
        foreach (var card in a)
        {
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CardResources>($"Assets/Resources/Cards/{card.Key}.asset");
            if (asset == null)
            {
                CardResources cardResources = CreateInstance<CardResources>();
                cardResources.cardData = card.Value;
                UnityEditor.AssetDatabase.CreateAsset(cardResources, $"Assets/Resources/Cards/{card.Key}.asset");
            }
            else
            {
                asset.cardData = card.Value;
            }
            Debug.Log($"Key: {card.Key}, ID: {card.Value.ID}, Value: {card.Value.Value}");
        }
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

    public CardData(List<string> data)
    {
        ID = int.Parse(data[0]);
        Value = int.Parse(data[1]);
        if (data.Count > 2) Name = data[2];
        if (data.Count > 3 && Enum.TryParse(typeof(OwnerType), data[3], out var owner)) ownerType = (OwnerType)owner;
        if (data.Count > 4 && Enum.TryParse(typeof(CardType), data[4], out var value)) cardType = (CardType)value;
    }
    public static CardData CardDataFactory(List<string> data)
    {
        if (data.Count <= 3) return new CardData(data);

        if (Enum.TryParse(typeof(OwnerType), data[3], out var owner))
        {
            switch (owner)
            {
                case CardType.Creature:
                    return new CreatureCardData(data);
                case CardType.Portal:
                    return new PortalCardData(data);
                default:
                    return new PlayerCardData(data);
            }
        }
        return new CardData(data);
    }
    virtual public bool IsMonoSelectedCard()
    {
        return false;
    }
    virtual public List<Enemy> GetPossibleEnemies(List<Enemy> enemies, int playerRow)
    {
        return enemies.FindAll(x => x.Card.cardValue.cardData.Value <= Value && x.rowNumber == playerRow);
    }
    virtual public void ApplyEffect(TurnManager manager, Enemy enemy)
    {
        manager.RPC_SetIfCardWasPlayed(PlayerController.players.Find(x => x.isLocalPlayer).PlayerID);
        HandCardVisual.selectedCard.UseCards();
        manager.RPC_KillEnemy(enemy);
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

public class PortalCardData : CardData
{
    public PortalCardData(List<string> data) : base(data)
    {
    }
    public override bool IsMonoSelectedCard()
    {
        return true;
    }
    public override List<Enemy> GetPossibleEnemies(List<Enemy> enemies, int playerRow)
    {
        return enemies.FindAll(x => x.rowNumber == 0 || x.rowNumber == 2);
    }

}
public class PlayerCardData : CardData
{
    public PlayerCardData(List<string> data) : base(data)
    {
    }
}
public class CreatureCardData : CardData
{
    public CreatureCardData(List<string> data) : base(data)
    {
    }
}