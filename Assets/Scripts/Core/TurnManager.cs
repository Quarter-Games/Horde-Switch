using Fusion;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurnManager : NetworkBehaviour
{
    public GameSettings gameSettings;
    private PlayerController _localPlayer;
    private PlayerController _opponentPlayer;
    public static Action<PlayerController> TurnChanged;
    [SerializeField] Grid grid;
    [SerializeField] Enemy enemyPrefab;
    [Networked,Capacity(12)] public NetworkLinkedList<Enemy> enemyList => default;
    [SerializeField] Vector2 EnemyGridSize;

    [Networked] public Deck enemyDeck { get => default; set { } }
    Camera _camera;
    private void OnEnable()
    {
        Enemy.OnEnemyClick += EnemyClick;
        GameplayUIHandler.RequestTurnSwap += EndTurnRequest;
        for (int i = 0; i < PlayerController.players.Count; i++)
        {
            PlayerController player = PlayerController.players[i];
            if (player.isLocalPlayer)
            {
                _localPlayer = player;
            }
            else
            {
                _opponentPlayer = player;
            }
        }

    }

    private void EnemyClick(Enemy enemy, PointerEventData data)
    {
        if (!_localPlayer.isThisTurn)
        {
            Debug.Log("Not Your Turn");
            return;
        }
        if (_localPlayer.PlayerID == 0 && enemy.rowNumber == 0)
        {
            Debug.Log("Attacked First Row");
        }
        else if (_localPlayer.PlayerID == 1 && enemy.rowNumber == 2)
        {
            Debug.Log("Attacked First Row");
        }
    }

    public void SetUpEnemies()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < gameSettings.gameConfig.EnemyDeckSize; i++)
        {
            //Declare enemy Deck
            int k = (i % 13)+1;
            cards.Add(Card.Create(k));
        }
        enemyDeck = new Deck(cards);
        _camera = Camera.main;
        grid.transform.position = new Vector3(0, 1.5f, -2);
        for (int i = 0; i < EnemyGridSize.x; i++)
        {
            for (int j = 0; j < EnemyGridSize.y; j++)
            {
                Vector3Int position = new Vector3Int(i - 1, 0, j);
                var WorldPos = grid.CellToWorld(position);
                WorldPos -= new Vector3(1f, 0, 0);
                var enemy = Runner.Spawn(enemyPrefab, position: WorldPos, rotation: Quaternion.identity, PlayerRef.None);
                enemy.transform.parent = transform;
                enemy.rowNumber = j;
                enemy.columnNumber = i;
                var _deck = enemyDeck;
                var card = _deck.Draw();
                enemyDeck = _deck;
                Debug.Log(card.ID);
                enemy.Card = card;
                enemyList.Add(enemy);
            }
        }
    }
    public void SpawnEnemy()
    {

    }
    private void EndTurnRequest()
    {
        RPC_TurnSwap();
    }

    public void RPC_TurnSwap()
    {
        if (PlayerController.players[0].isThisTurn)
        {
            PlayerController.players[0].RPC_ChangeTurn();
            PlayerController.players[1].RPC_ChangeTurn();
            TurnChanged?.Invoke(PlayerController.players[1]);
        }
        else
        {
            PlayerController.players[1].RPC_ChangeTurn();
            PlayerController.players[0].RPC_ChangeTurn();
            TurnChanged?.Invoke(PlayerController.players[0]);
        }
    }
    private void OnDisable()
    {
        Enemy.OnEnemyClick -= EnemyClick;
        GameplayUIHandler.RequestTurnSwap -= EndTurnRequest;
    }
}
