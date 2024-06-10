using Fusion;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    public int rowNumber;
    public string enemyID;
    public GameObject enemyModel;

    private void Start()
    {
        if (Camera.main.transform.position.z < 0)
        {
            enemyModel.transform.rotation=  Quaternion.Euler(new(0, 180, 0));
        }
        enemyModel.transform.rotation *= Quaternion.Euler(new(-22.5f, 0, 0));

    }

}
