using Fusion;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Collections.Unicode;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] NetworkRunner networkRunner;
    [SerializeField] TMPro.TMP_InputField RoomNameInputField;
    [SerializeField] GameObject WaitingWindow;
    [SerializeField] GameSettings _gameSettings;

    public PlayerController PlayerPrefab;
    private void Awake()
    {
        _gameSettings = Resources.LoadAll<GameSettings>("Game Settings")[0];
        networkRunner.JoinSessionLobby(SessionLobby.Shared);
        PlayerController.PlayerCreated += PlayerCreated;

    }

    private void PlayerCreated(PlayerController controller)
    {
        controller.SetUp(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, _gameSettings.gameConfig.PlayerDeckSize, _gameSettings.gameConfig.PlayerStartHandSize);
        if (!controller.isLocalPlayer)
        {
            LoadScene();
        }
    }

    private void Update()
    {
        //networkRunner.UpdateInternal(1);
    }
    public void SessionsUpdate(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("SessionsUpdate");
    }
    public async void Connect()
    {
        var generateName = false;
        if (string.IsNullOrEmpty(RoomNameInputField.text))
        {
            generateName = true;
        }
        StartGameArgs args = new StartGameArgs()
        {
            PlayerCount = 2,
            GameMode = GameMode.Shared,
            SessionName = generateName ? null : RoomNameInputField.text,
            IsVisible = false,
            CustomLobbyName = networkRunner.LobbyInfo.Name

        };
        var result = await networkRunner.StartGame(args);

        if (result.Ok)
        {
            WaitingWindow.SetActive(true);

            //LoadScene();
        }
        else
        {
            Debug.LogError(result.ShutdownReason);
        }

    }
    private void LoadScene()
    {
        SceneManager.LoadScene(1);
    }
    public void PlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            var obj = networkRunner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
            obj.PlayerID = player.PlayerId;
            obj.isLocalPlayer = player == networkRunner.LocalPlayer;
        }
    }
}
