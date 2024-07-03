using System;
using System.Collections.Generic;
using UnityEngine;
using static CardData;

[CreateAssetMenu(fileName = "CardResources", menuName = "Config/CardResources")]
abstract public class CardResources : DataBaseSynchronizedScribtableObject
{
    abstract public CardData cardData { get; }
    [SerializeField] public Sprite cardSprite;


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
                default:
                    cardResources = PlayerCardResources.Create(data);
                    break;
            }
            UnityEditor.AssetDatabase.CreateAsset(cardResources, $"Assets/Resources/Cards/{cardResources.cardData.ID}.asset");
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
        /*//foreach (var card in a)
        //{
        //    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CardResources>($"Assets/Resources/Cards/{card.Key}.asset");
        //    if (asset == null)
        //    {

        //        CardResources cardResources = CreateInstance<CardResources>();
        //        cardResources.cardData = card.Value;
        //        UnityEditor.AssetDatabase.CreateAsset(cardResources, $"Assets/Resources/Cards/{card.Key}.asset");
        //    }
        //    else
        //    {
        //        asset.cardData = card.Value;
        //    }
        //    Debug.Log($"Key: {card.Key}, ID: {card.Value.ID}, Value: {card.Value.Value}");
        //}*/
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
        Debug.Log($"ID: {data[0]}; Type: {this.GetType()}");

    }
    public static CardData CardDataFactory(List<string> data)
    {
        if (data.Count <= 4) return new CardData(data);

        if (Enum.TryParse(typeof(CardType), data[4], out var cardType))
        {
            switch (cardType)
            {
                case CardType.Creature:
                    return new CreatureCardData(data);
                case CardType.Portal:
                    return new PortalCardData(data);
                default:
                    return new PlayerCardData(data);
            }
        }
        else
        {
            Debug.LogError("CardType not found");
            Debug.Log($"ID: {data[0]}; Data: {data[4]}");
        }
        return new CardData(data);
    }
    virtual public bool IsMonoSelectedCard()
    {
        return false;
    }
    virtual public List<Enemy> GetPossibleEnemies(List<Enemy> enemies, int playerRow)
    {
        var temp = enemies.FindAll(x => x.Card.cardValue.cardData.Value <= Value && x.rowNumber == playerRow);
        foreach (var item in temp)
        {
            Debug.Log($"Value: {item.Card.cardValue.cardData.Value}; Pos Y and X: {item.columnNumber}, {item.rowNumber}");
        }
        return temp;
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
[Serializable]
public class PlayerCardData : CardData
{
    public PlayerCardData(List<string> data) : base(data)
    {
    }
}
[Serializable]
public class CreatureCardData : CardData
{
    public CreatureCardData(List<string> data) : base(data)
    {
    }
}