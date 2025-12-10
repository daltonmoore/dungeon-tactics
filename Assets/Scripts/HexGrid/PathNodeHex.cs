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
    public TerrainType terrainType;
    public PathNodeHex parent;
    public Transform VisualTransform;
    
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
        VisualTransform.Find("Selected").gameObject.SetActive(true);
    }
        
    public void Hide()
    {
        VisualTransform.Find("Selected").gameObject.SetActive(false);
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