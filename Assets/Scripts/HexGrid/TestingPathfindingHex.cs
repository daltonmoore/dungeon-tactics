using System;
using System.Collections.Generic;
using Dalton.Utils;
using UnityEngine;

public class TestingPathfindingHex : MonoBehaviour
{
    [SerializeField] private Transform pfHex;
    [SerializeField] private float cellSize = 10f;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 6;
    
    public PathfindingHex Pathfinding;
    private PathNodeHex _lastGridObject;
    
    private void Start()
    { 
        Pathfinding = new PathfindingHex(width, height, cellSize);

        Transform hexVisuals = new GameObject("HexVisuals").transform;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Transform visualTransform = Instantiate(pfHex, Pathfinding.grid.GetWorldPosition(x, y), Quaternion.identity);
                visualTransform.SetParent(hexVisuals);
                visualTransform.localScale *= cellSize;
                Pathfinding.grid.GetGridObject(x, y).VisualTransform = visualTransform;
                Pathfinding.grid.GetGridObject(x, y).Hide();
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
            List<Vector3> path = Pathfinding.FindPath(Vector3.zero, mouseWorldPosition);

            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(
                        path[i],
                        path[i + 1],
                        Color.yellow, 
                        3f);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
            Pathfinding.grid.GetGridPosition(mouseWorldPosition, out int x, out int y);
            Pathfinding.GetNode(x, y).SetWalkable(!Pathfinding.GetNode(x, y).walkable);
        }
        
        if (_lastGridObject != null)
        {
            _lastGridObject.Hide();
        }
        
        _lastGridObject = Pathfinding.grid.GetGridObject(Utils.GetMouseWorldPosition());
        if (_lastGridObject != null)
        {
            _lastGridObject.Show();
        }
    }
}