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
    public static string playerName;
    public static int AvatarID;
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
    [SerializeField] List<Image> Avatars;
    [SerializeField] PlayerAvatars playerAvatars;
    [SerializeField] Image PlayerAvatar;
    [SerializeField] TMPro.TMP_Text PlayerName;

    public PlayerController PlayerPrefab;
    [SerializeField] private bool isRoomPrivate;
    private void Awake()
    {
        playerName = PlayerPrefs.GetString("PlayerName", "Player");
        AvatarID = PlayerPrefs.GetInt("AvatarID", 0);
        PlayerName.text = playerName;
        PlayerAvatar.sprite = playerAvatars[AvatarID];
        for (int i = 0; i < Avatars.Count; i++)
        {
            Image avatar = Avatars[i];
            if (i >= playerAvatars.Count) avatar.gameObject.SetActive(false);
            else
            {
                avatar.sprite = playerAvatars[i];
                avatar.gameObject.SetActive(true);
            }
        }
        PlayerNameInputField.placeholder.GetComponent<TMPro.TMP_Text>().text = playerName;
        Application.targetFrameRate = 60;
        Callbacks.OnShutdown.AddListener(OnShutdown);
        Callbacks.OnSessionListUpdate.AddListener(SessionsUpdate);
        networkRunner.JoinSessionLobby(SessionLobby.Shared).ContinueWith(AfterSessionJoining);
        PlayerController.PlayerCreated += PlayerCreated;

    }
    public void SelectAvatar(int id)
    {
        if (id >= playerAvatars.Count) return;
        AvatarID = id;
        PlayerAvatar.sprite = playerAvatars[id];
        PlayerPrefs.SetInt("AvatarID", id);
    }

    private void AfterSessionJoining(Task<StartGameResult> task)
    {
        Debug.Log(playerName);
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
    public void SetPlayerName(string Name)
    {
        playerName = Name;
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerName.text = playerName;
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
            IsVisible = !isRoomPrivate,
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
