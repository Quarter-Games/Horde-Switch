using Fusion;
using System;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    private PlayerController _localPlayer;
    private PlayerController _opponentPlayer;
    public static Action<PlayerController> TurnChanged;
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
