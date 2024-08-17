using Assets.Scripts.SoundSystem;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TurnManager : NetworkBehaviour, IEffectPlayer
{
    public static Action<PlayerController> TurnChanged;
    public static event Action CardStateUpdate;
    public static event Action<PlayerController> PlayerDied;
    public static event Action<PlayerController> PlayerGotDamage;
    public List<FloorTile> GridTiles;
    public static TurnManager Instance { get; set; }
    public GameSettings gameSettings;
    [SerializeField] AnimationCurve MovementCurve;
    public PlayerController _localPlayer;
    [SerializeField] private PlayerController _opponentPlayer;
    [SerializeField] Grid grid;
    [SerializeField] Enemy enemyPrefab;
    [Networked, Capacity(12)] public NetworkLinkedList<Enemy> EnemyList => default;
    [SerializeField] Vector2 EnemyGridSize;

    [Networked] public Deck EnemyDeck { get => default; set { } }
    [Networked] public Deck PlayersDeck { get => default; set { } }
    [Networked] public Deck DiscardPile { get => default; set { } }
    [Networked] public Deck EnemyGraveyard { get => default; set { } }
    #region Unity Methods
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
        gameSettings = Resources.LoadAll<GameSettings>("Game Settings")[0];
        if (_localPlayer.IsThisTurn)
        {
            Debug.Log("This one is \"Host\"");
            SetUp();
        }

    }
    private void OnEnable()
    {
        Enemy.OnEnemyClick += EnemyClick;
        GameplayUIHandler.RequestTurnSwap += RPC_EndTurnRequest;
        HandCardVisual.selectedCards.Changed += CardClicked;
        HandCardVisual.CardDiscarded += RPC_CardDiscarded;
        HandCardVisual.OnCardPlaySound += RPC_PlayCardSound;
        CardData.CardIsPlayed += RPC_CardIsPlayedVFX;

    }


    private void OnDisable()
    {
        Enemy.OnEnemyClick -= EnemyClick;
        GameplayUIHandler.RequestTurnSwap -= RPC_EndTurnRequest;
        HandCardVisual.selectedCards.Changed -= CardClicked;
        HandCardVisual.CardDiscarded -= RPC_CardDiscarded;
        HandCardVisual.OnCardPlaySound -= RPC_PlayCardSound;
        CardData.CardIsPlayed -= RPC_CardIsPlayedVFX;

    }
    #endregion

    #region RPC Host Methods
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    private void RPC_CardDiscarded(Card card)
    {
        var discard = DiscardPile;
        discard.AddCard(card);
        DiscardPile = discard;
        if (_localPlayer.IsThisTurn)
        {
            _localPlayer.RPC_RemoveCard(card);
        }
        else
        {
            _opponentPlayer.RPC_RemoveCard(card);
        }
        RPC_UpdateCardState();


    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_KillEnemy(Enemy enemy)
    {
        //Remove enemy from list
        var enemies = EnemyList;
        EnemyList.Remove(enemy);
        enemies = EnemyList;

        //Add enemy to graveyard
        var discard = EnemyGraveyard;
        discard.AddCard(enemy.Card);
        EnemyGraveyard = discard;


        //Move enemie down
        int xPos = enemy.ColumnNumber;
        int yPos = enemy.RowNumber;
        var enemieToMove = EnemyList.First(x => x.ColumnNumber == xPos && x.RowNumber == 1);
        enemieToMove.RowNumber = yPos;
        var pos = grid.CellToWorld(new Vector3Int(xPos, 0, yPos)) - new Vector3Int(1, 0, 0);
        //Destroy enemy
        RPC_EnemyDiedInvokeSound();
        Runner.Despawn(enemy.Object);
        StartCoroutine(MoveWithDelay(enemieToMove, new(xPos, 0, yPos), xPos));
        Debug.Log(pos, enemieToMove);


    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_SwapEnemies(Enemy enemy, Enemy secondEnemy)
    {
        var columnNum1 = secondEnemy.ColumnNumber;
        var columnNum2 = enemy.ColumnNumber;
        var pos1 = secondEnemy.RowNumber;
        var pos2 = enemy.RowNumber;
        StartCoroutine(MoveWithDelay(enemy, new Vector3(columnNum1, 0, pos1), pos1, false));
        enemy.RowNumber = pos1;
        enemy.ColumnNumber = columnNum1;
        StartCoroutine(MoveWithDelay(secondEnemy, new Vector3(columnNum2, 0, pos2), pos2, false));
        secondEnemy.RowNumber = pos2;
        secondEnemy.ColumnNumber = columnNum2;
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_SetIfCardWasPlayed(int id)
    {
        if (_localPlayer.PlayerID == id)
        {
            _localPlayer.IsPlayedInThisTurn = true;
        }
        else
        {
            _opponentPlayer.IsPlayedInThisTurn = true;
        }
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    private void RPC_EndTurnRequest()
    {
        HandCardVisual.selectedCards.Clear();
        if (_localPlayer.IsThisTurn)
        {
            if (_localPlayer.IsPlayedInThisTurn) DrawCardForPlayer(_localPlayer);
            else
            {
                RemovePlayerHealth(_localPlayer);
                if (_localPlayer.HP <= 0) return;
            }
        }
        else
        {
            if (_opponentPlayer.IsPlayedInThisTurn) DrawCardForPlayer(_opponentPlayer);
            else
            {
                RemovePlayerHealth(_opponentPlayer);
                if (_opponentPlayer.HP <= 0) return;
            }
        }
        RPC_TurnSwap();
        RPC_ClearSelectedCards();
        RPC_UpdateCardState();
        _localPlayer.IsPlayedInThisTurn = false;
        _opponentPlayer.IsPlayedInThisTurn = false;

    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_TurnSwap()
    {
        _localPlayer.ChangeTurn();
        _opponentPlayer.ChangeTurn();
        if (_localPlayer.IsThisTurn)
        {
            RPC_CallTurnChangeEvent(_localPlayer);
        }
        else
        {
            RPC_CallTurnChangeEvent(_opponentPlayer);
        }
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_Disconnect(PlayerRef player)
    {
        Runner.Disconnect(player);
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_UseSniperCard(int cardIndex)
    {
        // Remove the card from the inactive player's hand
        var inactivePlayer = _localPlayer.IsThisTurn ? _opponentPlayer : _localPlayer;
        var cardToRemove = inactivePlayer.Hand[cardIndex];
        inactivePlayer.RPC_RemoveCard(cardToRemove);

        // Draw a new card for the inactive player
        DrawCardForPlayer(inactivePlayer);
    }
    #endregion

    #region RPC Clients Methods
    [Rpc]
    private void RPC_PlayCardSound(Card card)
    {
        IEffectPlayer.OnPlaySFX?.Invoke(card.CardValue.OnBeingPlayed);
    }
    [Rpc]
    public void RPC_SetEnemy(int tileIndex, Enemy enemy)
    {
        GridTiles[tileIndex].SetEnemy(enemy);
        CardClicked();
    }
    [Rpc]
    private void RPC_CallDamageEvent(PlayerController player)
    {
        IEffectPlayer.OnPlaySFX?.Invoke(gameSettings.HPLostSound);

        PlayerGotDamage?.Invoke(player);
    }
    [Rpc]
    private void RPC_CallTurnChangeEvent(PlayerController player)
    {
        TurnChanged?.Invoke(player);
        CardClicked();
    }
    [Rpc]
    public void RPC_UpdateCardState()
    {
        CardStateUpdate?.Invoke();
        CardClicked();
    }
    [Rpc]
    private void RPC_InvokePlayerDieEvent(PlayerController player)
    {
        PlayerDied?.Invoke(player);
        Runner.Shutdown();
    }
    [Rpc]
    private void RPC_EnemyDiedInvokeSound()
    {
        IEffectPlayer.OnPlaySFX?.Invoke(gameSettings.EnemyDiedSound);
    }
    [Rpc]
    private void RPC_CardIsPlayedVFX(Card card, Vector3 pos)
    {
        VFXManager.PlayVFX(card.CardValue.OnActivateEffect, pos, Quaternion.identity);
    }
    [Rpc]
    public void RPC_ClearSelectedCards()
    {
        HandCardVisual.selectedCards.Clear();
        CardClicked();
    }
    #endregion

    private void CardClicked()
    {
        var enemies = HandCardVisual.selectedCards.GetValidEnemies(EnemyList.ToList(), _localPlayer.MainRow);
        foreach (var enemy in EnemyList)
        {
            FloorTile tile = GridTiles[11 - ((enemy.RowNumber) * 4 + enemy.ColumnNumber + 1)];
            tile.SetEnemy(enemy);
            tile.UpdateHighlightStatus(enemies.Contains(enemy) ? HighlightStatus.Clickable : HighlightStatus.None);
        }

    }
    public void SetUp()
    {
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
        if (!_localPlayer.IsThisTurn)
        {
            Debug.Log("Not Your Turn");
            return;
        }
        if (HandCardVisual.selectedCards.GetValidEnemies(EnemyList.ToList(), _localPlayer.MainRow).Contains(enemy))
        {
            HandCardVisual.selectedCards[0].CardData.CardValue.DataOfCard.ApplyEffect(this, enemy);
        }
        RPC_UpdateCardState();
        CardClicked();
    }
    private IEnumerator MoveWithDelay(Enemy enemy, Vector3 position, int xPos, bool needToSpawn = true)
    {
        Vector3 WorldPos = (GridTiles[11 - ((int)(position.z) * 4 + (int)position.x + 1)].transform.position + GridTiles[11 - ((int)(position.z) * 4 + (int)position.x + 1)].transform.right);
        WorldPos -= new Vector3(1, 0, 0);
        var time = MovementCurve.keys[MovementCurve.length - 1].time;
        for (int i = 0; i <= time * 60; i++)
        {
            enemy.transform.position = Vector3.Lerp(enemy.transform.position, WorldPos, MovementCurve.Evaluate(i / 60.0f));
            yield return new WaitForFixedUpdate();
        }
        enemy.transform.position = WorldPos;
        if (needToSpawn) SpawnEnemy(new Vector3Int(xPos, 0, 1));

    }
    public void SetUpEnemies()
    {
        List<Card> cards = new();
        for (int i = 0; i < gameSettings.gameConfig.EnemyDeckSize; i++)
        {
            //Declare enemy Deck
            int k = gameSettings.gameConfig.EnemiesCardPull[i % gameSettings.gameConfig.EnemiesCardPull.Count];
            cards.Add(Card.Create(k));
        }
        EnemyDeck = new Deck(cards);
        grid.transform.position = new Vector3(0, 0.32f, -2);
        for (int i = 0; i < EnemyGridSize.x; i++)
        {
            for (int j = 0; j < EnemyGridSize.y; j++)
            {

                Vector3Int position = new(i - 1, 0, j);
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

        FloorTile tile = GridTiles[11 - ((position.z) * 4 + position.x + 1)];
        Vector3 WorldPos = tile.transform.position + tile.transform.right;

        if (EnemyDeck.Count == 0)
        {
            EnemyDeck = new(EnemyGraveyard);
            EnemyGraveyard = new Deck();
        }
        var _deck = EnemyDeck;
        var card = _deck.Draw();
        EnemyDeck = _deck;

        WorldPos -= new Vector3(1f, 0, 0);
        var enemy = Runner.Spawn((card.CardValue as CreatureCardResources).enemyPrefab, position: WorldPos, rotation: Quaternion.identity, PlayerRef.MasterClient);
        enemy.Card = card;
        enemy.transform.parent = transform;
        enemy.RowNumber = position.z;
        enemy.ColumnNumber = position.x;
        RPC_SetEnemy(11 - ((position.z) * 4 + position.x + 1), enemy);
        EnemyList.Add(enemy);
    }
    private void RemovePlayerHealth(PlayerController player)
    {
        player.HP--;
        if (player.HP <= 0)
        {
            RPC_InvokePlayerDieEvent(player);
            return;
        }

        for (int i = 0; i < player.Hand.Count; i++)
        {
            var card = player.Hand[i];
            RPC_CardDiscarded(card);
        }
        for (int i = 0; i < 4; i++)
        {
            DrawCardForPlayer(player);
        }
        RPC_CallDamageEvent(player);
    }
    public void DrawCardForPlayer(PlayerController player)
    {
        if (PlayersDeck.Count == 0)
        {
            PlayersDeck = new(DiscardPile);
            DiscardPile = new Deck();
        }
        player.RPC_DrawCard(PlayersDeck);
        var DeckCopy = PlayersDeck;
        DeckCopy.Draw();
        PlayersDeck = DeckCopy;
    }
    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            if (Runner.IsConnectedToServer) return;
            SceneManager.LoadScene(0);
            Destroy(Runner);
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        if (_localPlayer != null)
        {
            Destroy(_localPlayer.gameObject);
        }
        if (_opponentPlayer != null)
        {
            Destroy(_opponentPlayer.gameObject);
        }
        try
        {
            Runner.Shutdown();
        }
        catch (Exception)
        {
            Debug.Log("Runner is already destroyed");
        }

    }
}
