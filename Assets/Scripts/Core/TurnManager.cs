using Fusion;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;
    public GameSettings gameSettings;
    [SerializeField] private PlayerController _localPlayer;
    [SerializeField] private PlayerController _opponentPlayer;
    public static Action<PlayerController> TurnChanged;
    [SerializeField] Grid grid;
    [SerializeField] Enemy enemyPrefab;
    [Networked, Capacity(12)] public NetworkLinkedList<Enemy> enemyList => default;
    [SerializeField] Vector2 EnemyGridSize;

    [Networked] public Deck enemyDeck { get => default; set { } }
    [Networked] public Deck PlayersDeck { get => default; set { } }
    [Networked] public Deck DiscardPile { get => default; set { } }
    [Networked] public Deck EnemyGraveyard { get => default; set { } }
    Camera _camera;
    public void Start()
    {
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
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (_localPlayer.isThisTurn)
        {
            Debug.Log("This one is \"Host\"");
            SetUp();
        }

    }

    private void OnEnable()
    {
        Enemy.OnEnemyClick += EnemyClick;
        GameplayUIHandler.RequestTurnSwap += EndTurnRequest;
        HandCardVisual.selectedCard.Changed += CardClicked;
        HandCardVisual.CardDiscarded += RPC_CardDiscarded;

    }
    [Rpc]
    private void RPC_CardDiscarded(Card card)
    {
        if (Runner.LocalPlayer.PlayerId != 1) return;
        var discard = DiscardPile;
        discard.AddCard(card);
        DiscardPile = discard;
    }

    private void CardClicked()
    {
        var enemies = HandCardVisual.selectedCard.GetValidEnemies(enemyList.ToList(), _localPlayer.MainRow);
        foreach (var enemy in enemyList)
        {
            enemy.HighLight(enemies.Contains(enemy));
        }

    }

    public override void Spawned()
    {
        base.Spawned();

    }
    public void SetUp()
    {
        gameSettings = Resources.LoadAll<GameSettings>("Game Settings")[0];
        SetUpPlayerCards();
        PlayersDeck = _localPlayer.SetUp(PlayersDeck, gameSettings.gameConfig.PlayerStartHandSize);
        PlayersDeck = _opponentPlayer.SetUp(PlayersDeck, gameSettings.gameConfig.PlayerStartHandSize);

        DiscardPile = new Deck();
        EnemyGraveyard = new Deck();
        StartCoroutine(LoadSceneWithDelay());
    }
    private IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(1f);
        SetUpEnemies();
        Runner.LoadScene("Gameplay Scene");
    }
    private void EnemyClick(Enemy enemy, PointerEventData data)
    {
        if (!_localPlayer.isThisTurn)
        {
            Debug.Log("Not Your Turn");
            return;
        }
        if (HandCardVisual.selectedCard.GetValidEnemies(enemyList.ToList(), _localPlayer.MainRow).Contains(enemy))
        {
            Debug.Log("Enemy Clicked");
            RPC_KillEnemy(enemy);
            HandCardVisual.selectedCard.UseCards();
        }
    }
    [Rpc]
    public void RPC_KillEnemy(Enemy enemy)
    {
        if (Runner.LocalPlayer.PlayerId != 1) return;
        //Remove enemy from list
        var enemies = enemyList;
        enemyList.Remove(enemy);
        enemies = enemyList;

        //Add enemy to graveyard
        var discard = EnemyGraveyard;
        discard.AddCard(enemy.Card);
        EnemyGraveyard = discard;

        //Move enemie down
        int xPos = enemy.columnNumber;
        int yPos = enemy.rowNumber;
        var enemieToMove = enemyList.First(x => x.columnNumber == xPos && x.rowNumber == 1);
        enemieToMove.rowNumber = yPos;
        var pos = grid.CellToWorld(new Vector3Int(xPos, 0, yPos)) - new Vector3Int(1, 0, 0);
        StartCoroutine(MoveWithDelay(enemieToMove, pos));
        Debug.Log(pos, enemieToMove);

        //Destroy enemy
        Runner.Despawn(enemy.Object);
        SpawnEnemy(new Vector3Int(xPos, 0, 1));

    }
    private IEnumerator MoveWithDelay(Enemy enemy, Vector3 position)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        enemy.transform.position = position;
        Debug.Log(enemy.transform.position);
    }
    public void SetUpEnemies()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < gameSettings.gameConfig.EnemyDeckSize; i++)
        {
            //Declare enemy Deck
            int k = (i % 13) + 13;
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
                SpawnEnemy(position);
            }
        }
    }
    public void SetUpPlayerCards()
    {
        var cards = new List<Card>();
        for (int i = 0; i < gameSettings.gameConfig.PlayerDeckSize; i++)
        {
            //Declare player Hand
            int k = (i % 13) + 1;
            cards.Add(Card.Create(k));
        }
        PlayersDeck = new Deck(cards);
    }
    public void SpawnEnemy(Vector3Int position)
    {
        var WorldPos = grid.CellToWorld(position);
        WorldPos -= new Vector3(1f, 0, 0);
        var enemy = Runner.Spawn(enemyPrefab, position: WorldPos, rotation: Quaternion.identity, PlayerRef.MasterClient);
        enemy.transform.parent = transform;
        enemy.rowNumber = position.z;
        enemy.columnNumber = position.x;
        var _deck = enemyDeck;
        var card = _deck.Draw();
        enemyDeck = _deck;
        enemy.Card = card;
        enemyList.Add(enemy);
    }
    private void EndTurnRequest()
    {
        if (_localPlayer.isThisTurn)
        {
            _localPlayer.DrawCard(PlayersDeck);
        }
        else
        {
            _opponentPlayer.DrawCard(PlayersDeck);
        }
        var DeckCopy = PlayersDeck;
        DeckCopy.Draw();
        PlayersDeck = DeckCopy;
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
        HandCardVisual.selectedCard.Changed -= CardClicked;
        HandCardVisual.CardDiscarded -= RPC_CardDiscarded;
    }
}
