using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] NetworkRunner networkRunner;
    [SerializeField] TMPro.TMP_InputField RoomNameInputField;

    private void Awake()
    {
        networkRunner.JoinSessionLobby(SessionLobby.Shared);
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
            LoadScene();
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
}
