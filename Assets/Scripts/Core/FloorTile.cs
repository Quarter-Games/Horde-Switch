using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloorTile : MonoBehaviour, IPointerClickHandler,IPointerEnterHandler, IPointerExitHandler
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

    public Enemy Enemy => enemy;
    public void OnPointerClick(PointerEventData eventData)
    {
        enemy.OnPointerClick(eventData);
    }
    public void UpdateHighlightStatus(HighlightStatus status)
    {
        switch (status)
        {
            case HighlightStatus.None:
                if (enemy.HasVisiableMine)
                {
                    meshRenderer.material = MinePlacedMaterial;
                    HighlightStatus = HighlightStatus.MinePlaced;
                }
                else
                {
                    meshRenderer.material = BasicMaterial;
                    HighlightStatus = HighlightStatus.None;
                }
                break;
            case HighlightStatus.Clickable:
                meshRenderer.material = ClickableMaterial;
                HighlightStatus = HighlightStatus.Clickable;
                break;
            case HighlightStatus.Selected:
                meshRenderer.material = ChosenMaterial;
                HighlightStatus = HighlightStatus.Selected;
                break;
            case HighlightStatus.MinePlaced:
                meshRenderer.material = MinePlacedMaterial;
                HighlightStatus = HighlightStatus.MinePlaced;
                break;
        }
    }
    public void SetEnemy(Enemy enemy)
    {
        this.enemy = enemy;
        if (enemy.HasVisiableMine) UpdateHighlightStatus(HighlightStatus.MinePlaced);
        else UpdateHighlightStatus(HighlightStatus.None);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HandCardVisual.selectedCards.Count == 0) return;
        if (HighlightStatus == HighlightStatus.Clickable)
        {
            UpdateHighlightStatus(HighlightStatus.Selected);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (HandCardVisual.selectedCards.Count == 0) return;
        if (HighlightStatus == HighlightStatus.Selected)
        {
            UpdateHighlightStatus(HighlightStatus.Clickable);
        }
    }
}
public enum HighlightStatus
{
    None,
    Clickable,
    Selected,
    MinePlaced
}