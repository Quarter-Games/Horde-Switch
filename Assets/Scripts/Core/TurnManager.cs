using Fusion;
using System;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    private PlayerController _localPlayer;
    private PlayerController _opponentPlayer;
    public static Action<PlayerController> TurnChanged;
    [SerializeField] Grid grid;
    [SerializeField] Enemy enemyPrefab;
    [SerializeField] Vector2 EnemyGridSize;
    Camera _camera;
    private void OnEnable()
    {

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
    public void SetUpEnemies()
    {
        _camera = Camera.main;
        for (int i = 0; i < EnemyGridSize.x; i++)
        {
            for (int j = 0; j < EnemyGridSize.y; j++)
            {
                Vector3Int position = new Vector3Int(i, 0, j);
                var WorldPos = grid.CellToWorld(position);
                Runner.Spawn(enemyPrefab, position: WorldPos, rotation: Quaternion.LookRotation((Vector3)WorldPos - _camera.transform.position));
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
        GameplayUIHandler.RequestTurnSwap -= EndTurnRequest;
    }
}
