using Fusion;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Enemy : NetworkBehaviour, IPointerClickHandler
{
    public static Action<Enemy, PointerEventData> OnEnemyClick;
    public NetworkTransform networkTransform;
    [Networked] public int rowNumber { get => default; set { } }
    [Networked] public int columnNumber { get => default; set { } }
    [Networked] public Card Card { get => default; set { } }

    public GameObject enemyModel;
    public GameObject enemyFloor;
    public TMPro.TMP_Text enemyValue;
    private void Update()
    {
        if (Card.ID == 0) return;
        if (enemyValue.text == "0")
        {
            SetValue();
        }
    }
    public void SetValue()
    {
        Debug.Log("SetUp");
        if (Camera.main.transform.position.z < 0)
        {
            enemyModel.transform.rotation = Quaternion.Euler(new(0, 180, 0));
        }
        enemyModel.transform.rotation *= Quaternion.Euler(new(-22.5f, 0, 0));
        enemyValue.text = Card.cardValue.cardData.Value.ToString();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnEnemyClick?.Invoke(this, eventData);
    }
    public void HighLight(bool value)
    {
        if (value)
        {
            enemyFloor.SetActive(true);
            //enemyModel.GetComponentInChildren<SkinnedMeshRenderer>().material.color = Color.green;
        }
        else
        {
            enemyFloor.SetActive(false);
            //enemyModel.GetComponentInChildren<SkinnedMeshRenderer>().material.color = Color.white;
        }
    }
}
