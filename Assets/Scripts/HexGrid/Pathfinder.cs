using System;
using System.Collections.Generic;
using Dalton.Utils;
using UnityEngine;

namespace HexGrid
{
    public class Pathfinder : MonoBehaviour
    {
        public static Pathfinder Instance { get; private set; }

        [SerializeField] private bool debug;
        [SerializeField] private Transform pfHex;
        [SerializeField] private float cellSize = 10f;
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 6;
        
        private PathfindingHex _pathfinding;
        private PathNodeHex _lastGridObject;

        private void Awake()
        {
            Instance = this;
            _pathfinding = new PathfindingHex(width, height, cellSize, pfHex);
        }

        private void OnValidate()
        {
            _pathfinding.UpdateDebugVisuals(debug);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetTerrainTypeAtMousePosition();
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                DrawPathToMouse();
            }
            
            if (_lastGridObject != null)
            {
                _lastGridObject.Hide();
            }
        
            _lastGridObject = _pathfinding.grid.GetGridObject(Utils.GetMouseWorldPosition());
            if (_lastGridObject != null)
            {
                _lastGridObject.Show();
            }
        }
        
        public List<Vector3> FindPath(Vector3 start, Vector3 end)
        {
            return _pathfinding.FindPath(start, end);
        }
        
        public void FindPath(Vector3 start, Vector3 end, out List<Vector3> path)
        {
            path = _pathfinding.FindPath(start, end);
        }
        
        private void DrawPathToMouse()
        {
            Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
            List<Vector3> path = _pathfinding.FindPath(Vector3.zero, mouseWorldPosition);

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
        
        private void SetTerrainTypeAtMousePosition()
        {
            Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
            _pathfinding.grid.GetGridPosition(mouseWorldPosition, out int x, out int y);
            _pathfinding.GetNode(x, y).SetTerrainType(TerrainType.Forest);
        }
    }
}