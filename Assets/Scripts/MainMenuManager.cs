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
    [SerializeField] TurnManager _turnManagerPrefab;
    [SerializeField] NetworkEvents Callbacks;
    [SerializeField] Button PlayButton;
    [SerializeField] TMPro.TMP_Text RoomNameText; 
    public TurnManager TurnManagerReference;
    [Header("Customization Screen")]
    [SerializeField] private TMPro.TMP_InputField PlayerNameInputField;
    public static string playerName;

    public PlayerController PlayerPrefab;
    private bool isRoomPrivate;
    private void Awake()
    {
        playerName = PlayerPrefs.GetString("PlayerName", "Player");
        Application.targetFrameRate = 60;
        Callbacks.OnShutdown.AddListener(OnShutdown);
        Callbacks.OnSessionListUpdate.AddListener(SessionsUpdate);
        networkRunner.JoinSessionLobby(SessionLobby.Shared).ContinueWith(AfterSessionJoining);
        PlayerController.PlayerCreated += PlayerCreated;

    }

    private void AfterSessionJoining(Task<StartGameResult> task)
    {
        Debug.Log(playerName);
        if (task.Result.Ok)
        {
            Debug.Log("Joined session lobby");
            PlayButton.interactable = true;
            PlayerNameInputField.textComponent.text = playerName;
        }
        else
        {
            Debug.LogError(task.Result.ShutdownReason);
        }
    }
    public void SetPlayerName(string Name)
    {
        playerName = Name;
        PlayerPrefs.SetString("PlayerName", playerName);
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
    public void SessionsUpdate(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("SessionsUpdate");
        foreach (var session in sessionList)
        {
            Debug.Log(session.Name);
        }
    }
    public void ConnectToPrivateRoom()
    {
        isRoomPrivate = true;
        Connect();
    }
    public async void Connect()
    {
        var sessionName = isRoomPrivate ? null : RoomNameInputField.text;
        StartGameArgs args = new()
        {
            PlayerCount = 2,
            GameMode = GameMode.AutoHostOrClient,
            SessionName = sessionName,
            IsVisible = isRoomPrivate,
            CustomLobbyName = networkRunner.LobbyInfo.Name
        };
        RoomNameText.text = isRoomPrivate ? "Your room name is: " + sessionName : "";
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
            PlayerController.players[0].IsThisTurn = true;
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
