using Fusion;
using Fusion.Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionHandler : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] Camera _camera;
    [SerializeField] GameObject _CameraPositionForSecondPLayer;
    public PlayerMovement PlayerPrefab;
    NetworkRunner runner;
    private void Awake()
    {
        runner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
        PlayerJoined(runner.LocalPlayer);
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
        var obj = runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        obj.PlayerId = player.PlayerId;
    }

    public void LeaveGame()
    {
        runner.Shutdown();
        SceneManager.LoadScene(0);
    }


}
