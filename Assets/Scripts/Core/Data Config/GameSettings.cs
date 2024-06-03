using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : DataBaseSynchronizedScribtableObject
{
    public GameConfig gameConfig;
#if UNITY_EDITOR
    [ContextMenu("Pull")]
    public async override void Pull()
    {
        var settings = Resources.Load<SheetsSettings>("SheetsSettings");

        var a = await DataBaseLoader.ReadTableAsync(
            "GameSettings",
            (s) => s,
            (list) => new GameConfig(list),
            settings
            );
        //var assets = UnityEditor.AssetDatabase.FindAssets("t:GameSettings", new string[] { "Game Settings" });
        foreach (var config in a)
        {
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameSettings>($"Assets/Resources/Game Settings/{config.Key}.asset");
            if (asset==null)
            {
                GameSettings _settings = CreateInstance<GameSettings>();
                _settings.gameConfig = config.Value;
                UnityEditor.AssetDatabase.CreateAsset(_settings, $"Assets/Resources/Game Settings/{config.Key}.asset");
            }
            else
            {
                asset.gameConfig = config.Value;
            }
            Debug.Log($"Key: {config.Key}, Value: {config.Value}");
        }
    }
#endif
    [Serializable]
    public class GameConfig
    {
        public string name;
        public int PlayerDeckSize;
        public int EnemyDeckSize;
        public int PlayerStartHandSize;
        public GameConfig(List<string> data)
        {
            name = data[0];
            PlayerDeckSize = int.Parse(data[1]);
            EnemyDeckSize = int.Parse(data[2]);
            PlayerStartHandSize = int.Parse(data[3]);
        }
        public override string ToString()
        {
            return $"Name: {name}, Player Deck: {PlayerDeckSize}, Enemy Deck: {EnemyDeckSize}, Start Hand: {PlayerStartHandSize}";
        }
    }
}
