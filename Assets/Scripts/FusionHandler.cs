using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionHandler : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] Camera _camera;
    [SerializeField] GameObject _CameraPositionForSecondPLayer;
    [SerializeField] TMP_Text RoomName;
    public PlayerController PlayerPrefab;
    NetworkRunner runner;
    [SerializeField] NetworkEvents _networkRunnerCallbacks;

    private void Awake()
    {
        _networkRunnerCallbacks = FindAnyObjectByType<NetworkEvents>();
        _networkRunnerCallbacks.OnDisconnectedFromServer.AddListener(OnDisconnect);
        _networkRunnerCallbacks.OnShutdown.AddListener(OnShutDown);
        _networkRunnerCallbacks.PlayerLeft.AddListener(PlayerLeft);
        runner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
        PlayerJoined(runner.LocalPlayer);
        RoomName.text = runner.SessionInfo.Name;
    }

    private void PlayerLeft(NetworkRunner arg0, PlayerRef arg1)
    {
        Debug.Log("Player LEFT");
        Destroy(TurnManager.Instance.gameObject);
        SceneManager.LoadScene("Main Menu");
    }

    private void OnDisconnect(NetworkRunner arg0, NetDisconnectReason arg1)
    {

        Debug.Log("Player LEFT");
        Destroy(TurnManager.Instance.gameObject);
        SceneManager.LoadScene("Main Menu");
    }

    private void OnShutDown(NetworkRunner runner, ShutdownReason reason)
    {

        Debug.Log("Player LEFT");
        Destroy(TurnManager.Instance.gameObject);
        SceneManager.LoadScene("Main Menu");
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            if (player.PlayerId != 1)
            {
                _camera.transform.parent = _CameraPositionForSecondPLayer.transform;
                _camera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
    }
    public void LeaveGame()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RPC_Disconnect(runner.LocalPlayer);
        }
        SceneManager.LoadScene(0);
    }
}
