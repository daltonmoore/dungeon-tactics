using System;
using EventBus;
using Events;
using Units;
using UnityEngine;

public class PathNodeHex
{
    private GridHex<PathNodeHex> _grid;

    
    
    public int x;
    public int y;
    public int gCost;
    public int hCost;
    public int fCost;
    public bool walkable;
    public Vector3 worldPosition => _grid.GetWorldPosition(x, y);
    public TerrainType terrainType;
    public PathNodeHex parent;
    public Transform VisualTransform;
    public Transform Selected;
    public bool IsOccupied;
    public AbstractUnit Occupant;
    
    public PathNodeHex(GridHex<PathNodeHex> grid, int x, int y)
    {
        _grid = grid;
        this.x = x;
        this.y = y;
        walkable = true;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
    
    public void SetWalkable(bool value)
    {
        walkable = value;
        _grid.TriggerGridObjectChanged(x, y, this);
    }
    
    public void SetTerrainType(TerrainType value)
    {
        terrainType = value;
        _grid.TriggerGridObjectChanged(x, y, this);
    }
    
    public void Show()
    {
        Selected.gameObject.SetActive(true);
        foreach (Transform child in Selected)
        {
            child.gameObject.SetActive(true);
        }
        Bus<HexHighlighted>.Raise(Owner.Player1, new HexHighlighted(this));
    }
        
    public void Hide()
    {
        Selected.gameObject.SetActive(false);
    }

    public override string ToString()
    {
        return x + ", " + y;
    }
}

public enum TerrainType
{
    Grass = 1,
    Forest = 2,
}