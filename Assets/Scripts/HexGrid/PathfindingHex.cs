using System;
using System.Collections.Generic;
using Dalton.Utils;
using Drawing;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HexGrid
{
    public class PathfindingHex
    {
        public const int MOVE_STRAIGHT_COST = 10;
        public const int MOVE_DIAGONAL_COST = 10;
        private const int DEBUG_VISUAL_SCALE_MULTIPLIER = 4;
        private const string SORTING_LAYER = "Player";

        public GridHex<PathNodeHex> grid { get; }

        private SpriteRenderer[,] _debugWalkableArray;
        private SpriteRenderer[,] _debugTerrainArray;
        private List<PathNodeHex> _openList;
        private List<PathNodeHex> _closedList;

        readonly Transform _debugRoot;
        
    
        public PathfindingHex(int width, int height, float cellSize, Transform pfHex)
        {
            grid = new GridHex<PathNodeHex>(width, height, cellSize, Vector3.zero, (g, x, y) => new PathNodeHex(g, x, y));
        
            
            // ------------------------------ DEBUG VISUALS ------------------------------
            _debugWalkableArray = new SpriteRenderer[width, height];
            _debugTerrainArray = new SpriteRenderer[width, height];
            
            _debugRoot = new GameObject("DebugRoot").transform;
            Transform debugWalkableSquares = new GameObject("DebugWalkableSquares").transform;
            debugWalkableSquares.SetParent(_debugRoot);
            Transform debugTerrainSquares = new GameObject("DebugTerrainSquares").transform;
            debugTerrainSquares.SetParent(_debugRoot);
            Transform hexVisuals = new GameObject("HexVisuals").transform;
            
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    SpriteRenderer spriteRenderer = CreateDebugSquare(cellSize, x, y, debugWalkableSquares, "W");
                    _debugWalkableArray[x, y] = spriteRenderer;
                    
                    spriteRenderer = CreateDebugSquare(cellSize, x, y, debugTerrainSquares,"T", 2);
                    _debugTerrainArray[x, y] = spriteRenderer;
                    
                    Transform visualTransform =  GameObject.Instantiate(pfHex, grid.GetWorldPosition(x, y), Quaternion.identity);
                    visualTransform.SetParent(hexVisuals);
                    visualTransform.localScale *= cellSize;
                    grid.GetGridObject(x, y).VisualTransform = visualTransform;
                    grid.GetGridObject(x, y).Hide();
                }
            }
        
            grid.OnGridObjectChanged += (sender, args) =>
            {
                _debugWalkableArray[args.x, args.y].color = args.gridObject.walkable ? Color.green : Color.red;

                Color GetColor()
                {
                    switch (args.gridObject.terrainType)
                    {
                        case TerrainType.Forest:
                            return Color.blue;
                        case TerrainType.Grass:
                            return Color.green;
                        default:
                            return Color.white;
                    }
                }

                _debugTerrainArray[args.x, args.y].color = GetColor();
            };
        }

        private SpriteRenderer CreateDebugSquare(float cellSize, int x, int y, Transform parentNode, string squareName, int slot = 1)
        {
            float squareSize = 4f;
            Sprite squareSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, squareSize, squareSize), new Vector2(0f, 0f), 100);
            Vector3 pos = GetRandomPointInHex(grid.GetWorldPosition(x, y), cellSize / 2.5f);
            
            using (Draw.WithDuration(4))
            {
                Draw.ingame.Circle(
                    pos,
                    Vector3.forward, 
                    cellSize / 20,
                    Color.black);
            }
            
            TextMeshPro text = Utils.CreateWorldText(
                $"{squareName}",
                null,
                pos,
                Mathf.RoundToInt(cellSize),
                Color.black,
                TextAlignmentOptions.Center);
            text.sortingLayerID = SortingLayer.NameToID(SORTING_LAYER);
            text.transform.SetParent(parentNode);
            
            SpriteRenderer spriteRenderer = new GameObject($"({x}, {y})").AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = squareSprite;
            spriteRenderer.transform.position = pos;
            spriteRenderer.transform.localScale *= cellSize * DEBUG_VISUAL_SCALE_MULTIPLIER;
            spriteRenderer.transform.SetParent(parentNode);
            spriteRenderer.sortingLayerName = SORTING_LAYER;
            
            return spriteRenderer;
        }

        private Vector3 GetRandomPointInHex(Vector3 center, float radius)
        {
            return center + radius * (Vector3)Random.insideUnitCircle;
        }

        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
        {
            grid.GetGridPosition(startWorldPosition, out int startX, out int startY);
            grid.GetGridPosition(endWorldPosition, out int endX, out int endY);
        
            return FindPath(startX, startY, endX, endY)?.ConvertAll(node => grid.GetWorldPosition(node.x, node.y));
        }

        public List<PathNodeHex> FindPath(int startX, int startY, int endX, int endY)
        {
            PathNodeHex startNode = grid.GetGridObject(startX, startY);
            PathNodeHex endNode = grid.GetGridObject(endX, endY);
        
            if (startNode == null || endNode == null) return null;
        
            _openList = new List<PathNodeHex> { startNode };
            _closedList = new List<PathNodeHex>();

            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    PathNodeHex pathNode = grid.GetGridObject(x, y);
                    pathNode.gCost = int.MaxValue;
                    pathNode.CalculateFCost();
                    pathNode.parent = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            while (_openList.Count > 0)
            {
                PathNodeHex currentNode = GetLowestFCostNode(_openList);
                if (currentNode == endNode)
                {
                    // Reached final node
                    return CalculatePath(endNode);
                }
            
                _openList.Remove(currentNode);
                _closedList.Add(currentNode);

                foreach (PathNodeHex neighbor in GetNeighborList(currentNode))
                {
                    if (_closedList.Contains(neighbor)) continue;

                    if (!neighbor.walkable)
                    {
                        _closedList.Add(neighbor);
                        continue;
                    }

                    int tentativeGCost = currentNode.gCost * (int)currentNode.terrainType +
                                         CalculateDistanceCost(currentNode, neighbor);
                    if (tentativeGCost < neighbor.gCost)
                    {
                        neighbor.gCost = tentativeGCost;
                        neighbor.hCost = CalculateDistanceCost(neighbor, endNode);
                        neighbor.parent = currentNode;
                        neighbor.CalculateFCost();

                        if (!_openList.Contains(neighbor))
                        {
                            _openList.Add(neighbor);
                        }
                    }
                }
            }
        
            // out of nodes on open list. no path found.
        
            return null;
        }

        public PathNodeHex GetNode(int x, int y)
        {
            return grid.GetGridObject(x, y);
        }
    
        private List<PathNodeHex> GetNeighborList(PathNodeHex node)
        {
            List<PathNodeHex> neighborList = new List<PathNodeHex>();
            bool nodeXPlusOneIsValid = node.x + 1 < grid.width;
            bool nodeYPlusOneIsValid = node.y + 1 < grid.height;
            bool nodeXMinusOneIsValid = node.x - 1 >= 0;
            bool nodeYMinusOneIsValid = node.y - 1 >= 0;
        
            // left
            if (nodeXMinusOneIsValid) neighborList.Add(grid.GetGridObject(node.x - 1, node.y));
            // right
            if (nodeXPlusOneIsValid) neighborList.Add(grid.GetGridObject(node.x + 1, node.y));
        
            if (node.y % 2 == 1) // odd
            {
                // upper left
                if (nodeYPlusOneIsValid) neighborList.Add(grid.GetGridObject(node.x, node.y + 1));
                // upper right
                if (nodeXPlusOneIsValid && nodeYPlusOneIsValid) neighborList.Add(grid.GetGridObject(node.x + 1, node.y + 1));
                // down left
                if (nodeYMinusOneIsValid) neighborList.Add(grid.GetGridObject(node.x, node.y - 1));
                // down right
                if (nodeXPlusOneIsValid && nodeYMinusOneIsValid) neighborList.Add(grid.GetGridObject(node.x + 1, node.y - 1));
            }
            else // even
            {
                // upper left
                if (nodeXMinusOneIsValid && nodeYPlusOneIsValid) neighborList.Add(grid.GetGridObject(node.x - 1, node.y + 1));
                // upper right
                if (nodeYPlusOneIsValid) neighborList.Add(grid.GetGridObject(node.x, node.y + 1));
                // down left
                if (nodeXMinusOneIsValid && nodeYMinusOneIsValid) neighborList.Add(grid.GetGridObject(node.x - 1, node.y - 1));
                // down right
                if (nodeYMinusOneIsValid) neighborList.Add(grid.GetGridObject(node.x, node.y - 1));
            }
        
            return neighborList;
        }

        private List<PathNodeHex> CalculatePath(PathNodeHex endNode)
        {
            List<PathNodeHex> path = new List<PathNodeHex>();
            path.Add(endNode);
            PathNodeHex currentNode = endNode;
        
            while (currentNode.parent != null)
            {
                path.Add(currentNode.parent);
                currentNode = currentNode.parent;
            }
        
            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(PathNodeHex a, PathNodeHex b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }
    
        private PathNodeHex GetLowestFCostNode(List<PathNodeHex> pathNodeList)
        {
            PathNodeHex lowestFCostNode = pathNodeList[0];
            foreach (PathNodeHex pathNode in pathNodeList)
            {
                if (pathNode.fCost < lowestFCostNode.fCost)
                {
                    lowestFCostNode = pathNode;
                }
            }
            return lowestFCostNode;
        }

        public void UpdateDebugVisuals(bool debug)
        {
            _debugRoot.gameObject.SetActive(debug);
        }
    }
}
