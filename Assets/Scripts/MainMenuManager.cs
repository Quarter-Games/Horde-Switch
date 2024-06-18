using Fusion;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] NetworkRunner networkRunner;
    [SerializeField] TMPro.TMP_InputField RoomNameInputField;
    [SerializeField] GameObject WaitingWindow;
    [SerializeField] GameSettings _gameSettings;
    [SerializeField] TurnManager _turnManagerPrefab;
    [SerializeField] NetworkEvents Callbacks;
    [SerializeField] Button PlayButton;
    public TurnManager TurnManagerReference;

    public PlayerController PlayerPrefab;
    private void Awake()
    {
        Callbacks.OnShutdown.AddListener(OnShutdown);
        Callbacks.OnSessionListUpdate.AddListener(SessionsUpdate);
        _gameSettings = Resources.LoadAll<GameSettings>("Game Settings")[0];
        networkRunner.JoinSessionLobby(SessionLobby.Shared).ContinueWith(AfterSessionJoining);
        PlayerController.PlayerCreated += PlayerCreated;

    }

    private void AfterSessionJoining(Task<StartGameResult> task)
    {
        if (task.Result.Ok)
        {
            Debug.Log("Joined session lobby");
            PlayButton.interactable = true;
        }
        else
        {
            Debug.LogError(task.Result.ShutdownReason);
        }
    }

    private void OnShutdown(NetworkRunner runner, ShutdownReason reason)
    {
        Debug.Log(reason.ToString());
    }

    private void PlayerCreated(PlayerController controller)
    {
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
        foreach (var session in sessionList)
        {
            Debug.Log(session.Name);
        }
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
            GameMode = GameMode.AutoHostOrClient,
            SessionName = generateName ? null : RoomNameInputField.text,
            IsVisible = generateName,
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
        if (networkRunner.LocalPlayer.PlayerId == 1)
        {

            TurnManagerReference = networkRunner.Spawn(_turnManagerPrefab, inputAuthority: PlayerRef.None);
            DontDestroyOnLoad(TurnManagerReference);
            Debug.Log(TurnManagerReference.name + " is Created");
            PlayerController.players[0].isThisTurn = true;
            PlayerController.players[0].MainRow = 0;
            PlayerController.players[1].MainRow = 2;
            PlayerController.players[0].HP = 2;
            PlayerController.players[1].HP = 2;
            
        }
    }
    public void PlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            var obj = networkRunner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity, inputAuthority: player);
            obj.PlayerID = player.PlayerId;
            obj.isLocalPlayer = player == networkRunner.LocalPlayer;
        }
    }
    private void OnDestroy()
    {
        Callbacks.OnShutdown.RemoveListener(OnShutdown);
        Callbacks.OnSessionListUpdate.RemoveListener(SessionsUpdate);
        PlayerController.PlayerCreated -= PlayerCreated;
    }
}
