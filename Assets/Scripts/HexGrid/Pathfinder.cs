using System;
using System.Collections.Generic;
using Dalton.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HexGrid
{
    public class Pathfinder : MonoBehaviour
    {
        public static Pathfinder Instance { get; private set; }

        [SerializeField] private bool debug;
        [SerializeField] private bool showFogOfWar;
        [SerializeField] private Transform pfHex;
        [SerializeField] private Transform pfFogOfWarHex;
        [SerializeField] private float cellSize = 10f;
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 6;
        [SerializeField] private Tilemap hexTilemap;
        [SerializeField, Tooltip("Whether to use the width and height explicitly defined or use the hex tilemap")]
        private bool useStrictGridSize;
        
        public PathfindingHex Pathfinding { get; private set; }

        private PathNodeHex _lastGridObject;

        private void Awake()
        {
            Instance = this;
            if (useStrictGridSize)
            {
                Pathfinding = new PathfindingHex(width, height, cellSize, pfHex, pfFogOfWarHex, showFogOfWar);
            }
            else
            {
                
                // Get the bounds of the used area of the tilemap
                // It's often helpful to call CompressBounds() first to ensure the bounds are accurate
                hexTilemap.CompressBounds(); 
                BoundsInt bounds = hexTilemap.cellBounds; 

                // Get all the tiles within the bounds in a single array
                TileBase[] allTiles = hexTilemap.GetTilesBlock(bounds);
                List<Vector3> unwalkableTileWorldPositions = new List<Vector3>();
                // Iterate over the rectangular area covered by the bounds
                for (int x = 0; x < bounds.size.x; x++)
                {
                    for (int y = 0; y < bounds.size.y; y++)
                    {
                        // Calculate the index in the 1D array
                        int index = x + y * bounds.size.x;
                        TileBase tile = allTiles[index];

                        // Get the actual cell position in local tilemap coordinates
                        Vector3Int tilePosition = new Vector3Int(x + bounds.xMin, y + bounds.yMin, bounds.z);
                        
                        // Check if the tile is not null (i.e., a tile actually exists in that cell)
                        if (tile != null)
                        {
                            Debug.Log("Found tile: " + tile.name + " at position: " + tilePosition);
                    
                            // Optional: Get the world position of the tile center
                            // Vector3 worldPosition = tilemap.GetCellCenterWorld(tilePosition);
                        }
                        else
                        {
                            Debug.Log("No tile found at position: " + (x, y));
                            var pos = hexTilemap.CellToWorld(tilePosition);
                            Debug.Log($"World pos: {pos}");
                            unwalkableTileWorldPositions.Add(pos);
                        }
                    }
                }
                
                
                Pathfinding = new PathfindingHex(bounds.size.x, bounds.size.y, cellSize, pfHex, pfFogOfWarHex, showFogOfWar);

                foreach (Vector3 pos in unwalkableTileWorldPositions)
                {
                    Pathfinding.Grid.GetGridPosition(pos, out int x, out int y);
                    var tile = Pathfinding.Grid.GetGridObject(x, y);
                    if (tile == null)
                    {
                        Debug.LogWarning($"Tile at pos {pos} is null");
                        continue;
                    }
                    Debug.Log($"Unwalkable tile pos is {pos}");
                    Debug.Log($"Setting tile {tile.x}, {tile.y} to unwalkable");
                    tile.SetWalkable(false);
                }
            }

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

            var newGridObject = Pathfinding.Grid.GetGridObject(Utils.GetMouseWorldPosition());
            
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
            if (path == null || path.Count == 0)
            {
                Debug.Log("No path found");
            }
            else
            {
                path.RemoveAt(0);
            }
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
            Pathfinding.Grid.GetGridPosition(mouseWorldPosition, out int x, out int y);
            Pathfinding.GetNode(x, y).SetTerrainType(TerrainType.Forest);
        }
    }
}