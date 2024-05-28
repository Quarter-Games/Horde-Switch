using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CardResources", menuName = "Config/CardResources")]
public class CardResources : DataBaseSynchronizedScribtableObject
{
    public CardData cardData;
    [Serializable]
    public class CardData
    {
        public int ID;
        public int Value;
    }

#if UNITY_EDITOR
    [ContextMenu("Pull")]
    public override async void Pull()
    {
        var settings = Resources.Load<SheetsSettings>("SheetsSettings");

        var a = await DataBaseLoader.ReadTableAsync(
            "Cards",
            (s) => s,
            (list) => new CardData { ID = int.Parse(list[0]), Value = int.Parse(list[1]) },
            settings
            );
        var assets = UnityEditor.AssetDatabase.FindAssets("t:CardResources", new string[] { "Cards" });
        foreach (var card in a)
        {
            if (assets == null || assets.Length == 0)
            {
                CardResources cardResources = CreateInstance<CardResources>();
                cardResources.cardData = card.Value;
                UnityEditor.AssetDatabase.CreateAsset(cardResources, $"Assets/Resources/Cards/{card.Key}.asset");
            }
            else
            {
                UnityEditor.AssetDatabase.LoadAssetAtPath<CardResources>($"Assets/Resources/Cards/{card.Key}.asset").cardData = card.Value;
            }
            Debug.Log($"Key: {card.Key}, ID: {card.Value.ID}, Value: {card.Value.Value}");
        }
    }
#endif
}
