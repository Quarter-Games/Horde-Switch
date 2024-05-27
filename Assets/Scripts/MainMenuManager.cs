using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] NetworkRunner networkRunner;


    public async void Connect()
    {
        StartGameArgs args = new StartGameArgs() 
        { 
        PlayerCount =2,
        GameMode = GameMode.Shared
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
