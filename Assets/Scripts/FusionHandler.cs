using Fusion;
using Fusion.Photon.Realtime;
using System;
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

    private void Awake()
    {
        runner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
        PlayerJoined(runner.LocalPlayer);
        RoomName.text = runner.SessionInfo.Name;
    }
    private void OnEnable()
    {
        PlayerController.PlayerCreated += PlayerCreated;
    }

    private void PlayerCreated(PlayerController controller)
    {
        //Debug.LogWarning("Delete This Method");
        //controller.deck.AddCard(new Card { Value = 1 });
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            if (player.PlayerId != 1)
            {
                _camera.transform.parent = _CameraPositionForSecondPLayer.transform;
                _camera.transform.localPosition = Vector3.zero;
                _camera.transform.localRotation = Quaternion.identity;
            }
        }
    }
    private void OnDisable()
    {
        PlayerController.PlayerCreated -= PlayerCreated;
    }
    public void LeaveGame()
    {
        runner.Shutdown();
        SceneManager.LoadScene(0);
    }


}
