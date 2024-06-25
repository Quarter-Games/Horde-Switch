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
    public static event Action CardStateUpdate;
    public static event Action<PlayerController> PlayerDied;
    public static event Action<PlayerController> PlayerGotDamage;
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
        GameplayUIHandler.RequestTurnSwap += RPC_EndTurnRequest;
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
            RPC_SetIfCardWasPlayed(_localPlayer.PlayerID);
            HandCardVisual.selectedCard.UseCards();
            RPC_KillEnemy(enemy);
        }
        RPC_UpdateCardState();
    }
    [Rpc]
    public void RPC_UpdateCardState()
    {
        CardStateUpdate?.Invoke();
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_KillEnemy(Enemy enemy)
    {
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
        //Destroy enemy
        Runner.Despawn(enemy.Object);

        StartCoroutine(MoveWithDelay(enemieToMove, pos,xPos));
        Debug.Log(pos, enemieToMove);


    }
    [Rpc]
    public void RPC_SetIfCardWasPlayed(int id)
    {
        if (Runner.LocalPlayer.PlayerId != 1) return;
        if (_localPlayer.PlayerID == id)
        {
            _localPlayer.isPlayedInThisTurn = true;
        }
        else
        {
            _opponentPlayer.isPlayedInThisTurn = true;
        }
    }
    private IEnumerator MoveWithDelay(Enemy enemy, Vector3 position,int xPos)
    {
        for (int i = 0; i <= 60; i++)
        {
            enemy.transform.position = Vector3.Lerp(enemy.transform.position, position, i/60);
            yield return new WaitForEndOfFrame();
        }
        enemy.transform.position = position;
        Debug.Log(enemy.transform.position);
        SpawnEnemy(new Vector3Int(xPos, 0, 1));
    }
    public void SetUpEnemies()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < gameSettings.gameConfig.EnemyDeckSize; i++)
        {
            //Declare enemy Deck
            int k = gameSettings.gameConfig.EnemiesCardPull[i % gameSettings.gameConfig.EnemiesCardPull.Count];
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
            int k = gameSettings.gameConfig.PlayerCardPull[i % gameSettings.gameConfig.PlayerCardPull.Count];
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
        if (enemyDeck.Count == 0)
        {
            //Reshuffling Enemy Deck
            enemyDeck = new(EnemyGraveyard);
            EnemyGraveyard = new Deck();
        }
        var _deck = enemyDeck;
        var card = _deck.Draw();
        enemyDeck = _deck;
        enemy.Card = card;
        enemyList.Add(enemy);
    }
    [Rpc]
    private void RPC_EndTurnRequest()
    {
        if (Runner.LocalPlayer.PlayerId != 1) return;
        if (_localPlayer.isThisTurn)
        {
            if (_localPlayer.isPlayedInThisTurn) DrawCardForPlayer(_localPlayer);
            else
            {
                RemovePlayerHealth(_localPlayer);
            }
        }
        else
        {
            if (_opponentPlayer.isPlayedInThisTurn) DrawCardForPlayer(_opponentPlayer);
            else
            {
                RemovePlayerHealth(_opponentPlayer);
            }
        }
        RPC_TurnSwap();
        _localPlayer.isPlayedInThisTurn = false;
        _opponentPlayer.isPlayedInThisTurn = false;

    }
    private void RemovePlayerHealth(PlayerController player)
    {
        player.HP--;
        if (player.HP <= 0)
        {
            PlayerDied?.Invoke(player);
            return;
        }

        while (player.hand.Count > 0)
        {
            var card = player.hand[0];
            player.RPC_RemoveCard(card);
            RPC_CardDiscarded(card);
        }
        for (int i = 0; i < 4; i++)
        {
            DrawCardForPlayer(player);
        }
        RPC_CallDamageEvent(player);
    }
    [Rpc]
    private void RPC_CallDamageEvent(PlayerController player)
    {
        PlayerGotDamage?.Invoke(player);
    }
    public void DrawCardForPlayer(PlayerController player)
    {
        if (PlayersDeck.Count == 0)
        {
            PlayersDeck = new(DiscardPile);
            DiscardPile = new Deck();
        }
        player.DrawCard(PlayersDeck);
        var DeckCopy = PlayersDeck;
        DeckCopy.Draw();
        PlayersDeck = DeckCopy;
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_TurnSwap()
    {
        _localPlayer.ChangeTurn();
        _opponentPlayer.ChangeTurn();
        if (_localPlayer.isThisTurn)
        {
            RPC_CallTurnChangeEvent(_localPlayer);
        }
        else
        {
            RPC_CallTurnChangeEvent(_opponentPlayer);
        }
    }
    [Rpc]
    private void RPC_CallTurnChangeEvent(PlayerController player)
    {
        TurnChanged?.Invoke(player);
    }
    private void OnDisable()
    {
        Enemy.OnEnemyClick -= EnemyClick;
        GameplayUIHandler.RequestTurnSwap -= RPC_EndTurnRequest;
        HandCardVisual.selectedCard.Changed -= CardClicked;
        HandCardVisual.CardDiscarded -= RPC_CardDiscarded;
    }

}
