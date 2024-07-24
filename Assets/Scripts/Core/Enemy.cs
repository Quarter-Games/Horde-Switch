using Assets.Scripts.SoundSystem;
using Fusion;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Enemy : NetworkBehaviour, IPointerClickHandler,IEffectPlayer
{
    public static event Action MinePlaced;
    public static Action<Enemy, PointerEventData> OnEnemyClick;
    public NetworkTransform networkTransform;
    public bool HasVisiableMine;
    [Networked] public bool HasMine { get => default; set { } }
    [Networked] public int RowNumber { get => default; set { } }
    [Networked] public int ColumnNumber { get => default; set { } }
    [Networked] public Card Card { get => default; set { } }

    public GameObject enemyModel;
    public TMPro.TMP_Text enemyValue;
    public void Start()
    {
        IEffectPlayer.OnPlaySFX?.Invoke(Card.CardValue.OnBeingPlayed);
    }
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
        enemyValue.text = Card.CardValue.DataOfCard.Value.ToString();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnEnemyClick?.Invoke(this, eventData);
    }
    public void PlaceMine()
    {
        RPC_SetMine();
        HasVisiableMine = true;
        //TODO: Add visuals for the player, that put the mine
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    private void RPC_SetMine()
    {
        HasMine = true;
        RPC_RaiseMinePlaceEvent();
    }
    [Rpc]
    private void RPC_RaiseMinePlaceEvent()
    {
        MinePlaced?.Invoke();
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_RemoveMine()
    {
        HasMine = false;
        RPC_RemoveMineVisual();
    }
    [Rpc]
    private void RPC_RemoveMineVisual()
    {
        HasVisiableMine = false;
        //TODO: VFX of BOOM
    }
    public Vector3 GetEffectSpawnPosition()
    {

       return transform.position+transform.up*3;
    }
}
