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
        
        public PathfindingHex Pathfinding { get; private set; }

        private PathNodeHex _lastGridObject;

        private void Awake()
        {
            Instance = this;
            Pathfinding = new PathfindingHex(width, height, cellSize, pfHex);
            Pathfinding.UpdateDebugVisuals(debug);
        }

        private void OnValidate()
        {
            if (Pathfinding != null)
                Pathfinding.UpdateDebugVisuals(debug);
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

            var newGridObject = Pathfinding.grid.GetGridObject(Utils.GetMouseWorldPosition());
            
            if (_lastGridObject != null && _lastGridObject != newGridObject)
            {
                _lastGridObject.Hide();
            }
        
            _lastGridObject = newGridObject;
            if (_lastGridObject != null && _lastGridObject.Selected.gameObject.activeSelf == false)
            {
                _lastGridObject.Show();
            }
        }
        
        public List<Vector3> FindPath(Vector3 start, Vector3 end)
        {
            return Pathfinding.FindPath(start, end);
        }
        
        public void FindPath(Vector3 start, Vector3 end, out List<Vector3> path)
        {
            path = Pathfinding.FindPath(start, end);
        }

        public void FindPath(Vector3 start, Vector3 end, out List<PathNodeHex> path)
        {
            Pathfinding.FindPath(start, end, out path);
            path.RemoveAt(0);
        }
        
        private void DrawPathToMouse()
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
        
        private void SetTerrainTypeAtMousePosition()
        {
            Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
            Pathfinding.grid.GetGridPosition(mouseWorldPosition, out int x, out int y);
            Pathfinding.GetNode(x, y).SetTerrainType(TerrainType.Forest);
        }
    }
}