using System;
using System.Collections.Generic;
using Dalton.Utils;
using TMPro;
using UnityEngine;
using Util;

/// <summary>
/// This hex grid class uses offset coordinates.
/// </summary>
/// <typeparam name="TGridObject"></typeparam>
public class GridHex<TGridObject>
{
    const float HEX_VERTICAL_OFFSET_MULTIPLIER = .75f;
    
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;

    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
        public TGridObject gridObject;
    }

    public int width { get; private set; }
    public int height { get; private set; }
    
    private float _cellSize;
    private Vector3 _originPosition;
    private TGridObject[,] _gridArray;
    private Transform _debugObjects;
    private TextMeshPro[,] _debugTextArray;

    public GridHex(int width, int height, float  cellSize, Vector3 originPosition, 
        Func<GridHex<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        _cellSize = cellSize;
        _originPosition = originPosition;
        
        _gridArray = new TGridObject[width, height];

        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                _gridArray[x, y] = createGridObject(this, x, y);
            }
        }
        
        _debugTextArray = new TextMeshPro[width, height];
        _debugObjects = new GameObject("DebugObjects").transform;
        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                _debugTextArray[x, y] = Utils.CreateWorldText(
                    _gridArray[x, y]?.ToString(),
                    null,
                    GetWorldPosition(x, y),
                    Mathf.RoundToInt(cellSize),
                    Color.white,
                    TextAlignmentOptions.Center,
                    0,
                    Constants.PlayerSortingLayer);
                
                _debugTextArray[x, y].transform.SetParent(_debugObjects);
                
                // Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                // Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
            }
        }
        
        // Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        // Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
        
        OnGridObjectChanged += (_, eventArgs) => {
            _debugTextArray[eventArgs.x, eventArgs.y].text = _gridArray[eventArgs.x, eventArgs.y].ToString();
        };
    }

    public void SetDebugVisible(bool visible)
    {
        _debugObjects.gameObject.SetActive(visible);
    }
    
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, 0) * _cellSize +
               new Vector3(0, y, 0) * (_cellSize * HEX_VERTICAL_OFFSET_MULTIPLIER) +
               ((y % 2) == 1 ? new Vector3(1, 0, 0) * (_cellSize * .5f) : Vector3.zero) +
               _originPosition;
    }

    public void GetGridPosition(Vector3 worldPosition, out int x, out int y)
    {
        int roughX = Mathf.RoundToInt((worldPosition - _originPosition).x / _cellSize);
        int roughY = Mathf.RoundToInt((worldPosition - _originPosition).y / _cellSize / HEX_VERTICAL_OFFSET_MULTIPLIER);
        
        Vector3Int roughXY = new (roughX, roughY, 0);

        List<Vector3Int> neighborXYList = new List<Vector3Int>
        {
            roughXY + new Vector3Int(-1, +0), // left
            roughXY + new Vector3Int(+1, +0), // right
        };
        
        if (roughY % 2 == 1) // odd
        {
            neighborXYList.AddRange(
                new[]
                {
                    
                    roughXY + new Vector3Int(+0, +1), // upper left
                    roughXY + new Vector3Int(+1, +1), // upper right
                    roughXY + new Vector3Int(+0, -1), // down left
                    roughXY + new Vector3Int(+1, -1), // down right
                }
            );
        }
        else // even
        {
            neighborXYList.AddRange(
                new[]
                {
                    roughXY + new Vector3Int(-1, +1), // upper left
                    roughXY + new Vector3Int(+0, +1), // upper right
                    roughXY + new Vector3Int(-1, -1), // down left
                    roughXY + new Vector3Int(+0, -1), // down right
                }
            );
        }

        Vector3Int closestHex = roughXY;

        foreach (Vector3Int neighbor in neighborXYList)
        {
            if (Vector3.Distance(worldPosition, GetWorldPosition(neighbor.x, neighbor.y)) 
                < Vector3.Distance(worldPosition, GetWorldPosition(closestHex.x, closestHex.y)))
            {
                closestHex = neighbor;
            }
        }

        x = closestHex.x;
        y = closestHex.y;
    }
    
    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            _gridArray[x, y] = value;
            OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
    }
    
    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        GetGridPosition(worldPosition, out int x, out int y);
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            SetGridObject(x, y, value);
        }
    }

    public void TriggerGridObjectChanged(int x, int y, TGridObject value)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y, gridObject = value});
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return _gridArray[x, y];
        }

        return default(TGridObject);
    }
    
    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        GetGridPosition(worldPosition, out int x, out int y);
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return _gridArray[x, y];
        }
        
        return default(TGridObject);
    }

    public float GetCellSize()
    {
        return _cellSize;
    }
}
