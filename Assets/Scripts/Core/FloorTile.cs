using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloorTile : MonoBehaviour, IPointerClickHandler
{
    public static event Action<FloorTile> OnFloorTileClick;
    public int RowNumber;
    public int ColumnNumber;
    public HighlightStatus HighlightStatus;
    public Material BasicMaterial;
    public Material ClickableMaterial;
    public Material ChosenMaterial;
    public Material MinePlacedMaterial;
    [SerializeField] private MeshRenderer meshRenderer;
    private Enemy enemy;


    public void OnPointerClick(PointerEventData eventData)
    {
        enemy?.OnPointerClick(eventData);
    }
    public void UpdateHighlightStatus(HighlightStatus status)
    {
        switch (status)
        {
            case HighlightStatus.None:
                if (enemy.HasVisiableMine) meshRenderer.material = MinePlacedMaterial;
                else meshRenderer.material = BasicMaterial;
                break;
            case HighlightStatus.Clickable:
                meshRenderer.material = ClickableMaterial;
                break;
            case HighlightStatus.Selected:
                meshRenderer.material = ChosenMaterial;
                break;
            case HighlightStatus.MinePlaced:
                meshRenderer.material = MinePlacedMaterial;
                break;
        }
    }
    public void SetEnemy(Enemy enemy)
    {
        this.enemy = enemy;
        if (enemy.HasVisiableMine) UpdateHighlightStatus(HighlightStatus.MinePlaced);
        else UpdateHighlightStatus(HighlightStatus.None);
    }
}
public enum HighlightStatus
{
    None,
    Clickable,
    Selected,
    MinePlaced
}