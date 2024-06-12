using Fusion;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Enemy : NetworkBehaviour, IPointerClickHandler
{
    public static Action<Enemy, PointerEventData> OnEnemyClick;
    public int rowNumber;
    public int columnNumber;
    [Networked] public Card Card { get => default; set { } }
    public GameObject enemyModel;
    [SerializeField] TMPro.TMP_Text enemyValue;

    private void Start()
    {
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
}
