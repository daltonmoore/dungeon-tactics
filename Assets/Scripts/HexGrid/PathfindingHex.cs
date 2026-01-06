using System;
using System.Collections.Generic;
using Dalton.Utils;
using Drawing;
using TMPro;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace HexGrid
{
    public class PathfindingHex
    {
        public const int MOVE_STRAIGHT_COST = 10;
        public const int MOVE_DIAGONAL_COST = 10;
        private const int DEBUG_VISUAL_SCALE_MULTIPLIER = 4;
        private const float INNER_HEX_CIRCLE_RADIUS_DIVISOR = 2.5f;

        public static float CellSize;
        
        public GridHex<PathNodeHex> Grid { get; }

        private SpriteRenderer[,] _debugWalkableArray;
        private SpriteRenderer[,] _debugTerrainArray;
        private DebugNodeCost[,] _debugNodeCostArray;
        private List<PathNodeHex> _openList;
        private List<PathNodeHex> _closedList;
        private Transform _debugRoot;
        
    
        public PathfindingHex(int width, int height, float cellSize, Transform pfHex, Transform pfFogOfWarHex, bool showFogOfWar)
        {
            Grid = new GridHex<PathNodeHex>(
                width,
                height,
                cellSize,
                Vector3.zero,
                (g, x, y) => new PathNodeHex(g, x, y)
            );
            CellSize = cellSize;
            
            InitializeFog(width, height, cellSize, pfFogOfWarHex, showFogOfWar);
            InitializeDebugVisuals(width, height, cellSize, pfHex);

            Grid.OnGridObjectChanged += (sender, args) =>
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
                
                // _debugNodeCostArray[args.x, args.y].gCost.text = $"g: {args.gridObject.gCost}";
                // _debugNodeCostArray[args.x, args.y].hCost.text = $"h: {args.gridObject.hCost}";
                // _debugNodeCostArray[args.x, args.y].fCost.text = $"f: {args.gridObject.fCost}";
            };
        }

        private void InitializeFog(int width, int height, float cellSize, Transform pfFogOfWarHex, bool showFogOfWar)
        {
            Transform fogOfWarRoot = new GameObject("FogOfWar").transform;

            for (int x = 0; x < Grid.width; x++)
            {
                for (int y = 0; y < Grid.height; y++)
                {
                    Transform visualTransform =  GameObject.Instantiate(pfFogOfWarHex, Grid.GetWorldPosition(x, y), Quaternion.identity);
                    visualTransform.SetParent(fogOfWarRoot);
                    // Grid.GetGridObject(x, y).VisualTransform = visualTransform;
                    // Grid.GetGridObject(x, y).Selected = visualTransform.Find("Selected");
                    // Grid.GetGridObject(x, y).Hide();
                }
            }
            
            fogOfWarRoot.gameObject.SetActive(showFogOfWar);
        }

        private void InitializeDebugVisuals(int width, int height, float cellSize, Transform pfHex)
        {
            _debugWalkableArray = new SpriteRenderer[width, height];
            _debugTerrainArray = new SpriteRenderer[width, height];
            _debugNodeCostArray = new DebugNodeCost[width, height];
            
            _debugRoot = new GameObject("DebugRoot").transform;
            
            Transform debugWalkableSquares = new GameObject("DebugWalkableSquares").transform;
            Transform debugTerrainSquares = new GameObject("DebugTerrainSquares").transform;
            Transform debugHexVisuals = new GameObject("HexVisuals").transform;
            
            debugWalkableSquares.SetParent(_debugRoot);
            debugTerrainSquares.SetParent(_debugRoot);
            debugHexVisuals.SetParent(_debugRoot);
            
            for (int x = 0; x < Grid.width; x++)
            {
                for (int y = 0; y < Grid.height; y++)
                {
                    SpriteRenderer spriteRenderer = CreateDebugSquare(cellSize, x, y, debugWalkableSquares, "W");
                    _debugWalkableArray[x, y] = spriteRenderer;
                    
                    spriteRenderer = CreateDebugSquare(cellSize, x, y, debugTerrainSquares,"T");
                    _debugTerrainArray[x, y] = spriteRenderer;

                    _debugNodeCostArray[x, y].gCost = 
                        Utils.CreateWorldText(
                            "g: 0",
                            debugHexVisuals, 
                            GetRandomPointInHex(
                                Grid.GetWorldPosition(x, y),
                                cellSize / INNER_HEX_CIRCLE_RADIUS_DIVISOR),
                            Mathf.RoundToInt(cellSize),
                            Color.black,
                            TextAlignmentOptions.Center,
                            0,
                            Constants.PlayerSortingLayer
                        );
                    _debugNodeCostArray[x, y].hCost = 
                        Utils.CreateWorldText(
                            "h: 0",
                            debugHexVisuals, 
                            GetRandomPointInHex(
                                Grid.GetWorldPosition(x, y),
                                cellSize / INNER_HEX_CIRCLE_RADIUS_DIVISOR),
                            Mathf.RoundToInt(cellSize),
                            Color.black,
                            TextAlignmentOptions.Center,
                            0,
                            Constants.PlayerSortingLayer
                        );
                    _debugNodeCostArray[x, y].fCost = 
                        Utils.CreateWorldText(
                            "f: 0", 
                            debugHexVisuals, 
                            GetRandomPointInHex(
                                Grid.GetWorldPosition(x, y),
                                cellSize / INNER_HEX_CIRCLE_RADIUS_DIVISOR),
                            Mathf.RoundToInt(cellSize),
                            Color.black,
                            TextAlignmentOptions.Center,
                            0,
                            Constants.PlayerSortingLayer
                        );
                    
                    Transform visualTransform =  GameObject.Instantiate(pfHex, Grid.GetWorldPosition(x, y), Quaternion.identity);
                    visualTransform.SetParent(debugHexVisuals);
                    visualTransform.localScale *= cellSize;
                    Grid.GetGridObject(x, y).VisualTransform = visualTransform;
                    Grid.GetGridObject(x, y).Selected = visualTransform.Find("Selected");
                    Grid.GetGridObject(x, y).Hide();
                }
            }
        }

        private SpriteRenderer CreateDebugSquare(float cellSize, int x, int y, Transform parentNode, string squareName)
        {
            float squareSize = 4f;
            Sprite squareSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, squareSize, squareSize), new Vector2(0f, 0f), 100);
            Vector3 pos = GetRandomPointInHex(Grid.GetWorldPosition(x, y), cellSize / INNER_HEX_CIRCLE_RADIUS_DIVISOR);
            
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
                TextAlignmentOptions.Center,
                0,
                Constants.PlayerSortingLayer);

            text.transform.SetParent(parentNode);

            SpriteRenderer spriteRenderer = new GameObject($"({x}, {y})").AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = squareSprite;
            spriteRenderer.transform.position = pos;
            spriteRenderer.transform.localScale *= cellSize * DEBUG_VISUAL_SCALE_MULTIPLIER;
            spriteRenderer.transform.SetParent(parentNode);
            spriteRenderer.sortingLayerName = Constants.PlayerSortingLayer;
            
            return spriteRenderer;
        }

        private Vector3 GetRandomPointInHex(Vector3 center, float radius)
        {
            return center + radius * (Vector3)Random.insideUnitCircle;
        }
        

        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
        {
            Grid.GetGridPosition(startWorldPosition, out int startX, out int startY);
            Grid.GetGridPosition(endWorldPosition, out int endX, out int endY);
        
            return FindPath(startX, startY, endX, endY)?.ConvertAll(node => Grid.GetWorldPosition(node.x, node.y));
        }

        public void FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition, out List<PathNodeHex> path)
        {
            Grid.GetGridPosition(startWorldPosition, out int startX, out int startY);
            Grid.GetGridPosition(endWorldPosition, out int endX, out int endY);
            
            path = FindPath(startX, startY, endX, endY);
        }

        public List<PathNodeHex> FindPath(int startX, int startY, int endX, int endY)
        {
            PathNodeHex startNode = Grid.GetGridObject(startX, startY);
            PathNodeHex endNode = Grid.GetGridObject(endX, endY);
        
            if (startNode == null || endNode == null) return null;
        
            _openList = new List<PathNodeHex> { startNode };
            _closedList = new List<PathNodeHex>();

            for (int x = 0; x < Grid.width; x++)
            {
                for (int y = 0; y < Grid.height; y++)
                {
                    PathNodeHex pathNode = Grid.GetGridObject(x, y);
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

                    int tentativeGCost = currentNode.gCost + (int)currentNode.terrainType +
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
            return Grid.GetGridObject(x, y);
        }
    
        public List<PathNodeHex> GetNeighborList(PathNodeHex node)
        {
            List<PathNodeHex> neighborList = new List<PathNodeHex>();
            bool nodeXPlusOneIsValid = node.x + 1 < Grid.width;
            bool nodeYPlusOneIsValid = node.y + 1 < Grid.height;
            bool nodeXMinusOneIsValid = node.x - 1 >= 0;
            bool nodeYMinusOneIsValid = node.y - 1 >= 0;
        
            // left
            if (nodeXMinusOneIsValid) neighborList.Add(Grid.GetGridObject(node.x - 1, node.y));
            // right
            if (nodeXPlusOneIsValid) neighborList.Add(Grid.GetGridObject(node.x + 1, node.y));
        
            if (node.y % 2 == 1) // odd
            {
                // upper left
                if (nodeYPlusOneIsValid) neighborList.Add(Grid.GetGridObject(node.x, node.y + 1));
                // upper right
                if (nodeXPlusOneIsValid && nodeYPlusOneIsValid) neighborList.Add(Grid.GetGridObject(node.x + 1, node.y + 1));
                // down left
                if (nodeYMinusOneIsValid) neighborList.Add(Grid.GetGridObject(node.x, node.y - 1));
                // down right
                if (nodeXPlusOneIsValid && nodeYMinusOneIsValid) neighborList.Add(Grid.GetGridObject(node.x + 1, node.y - 1));
            }
            else // even
            {
                // upper left
                if (nodeXMinusOneIsValid && nodeYPlusOneIsValid) neighborList.Add(Grid.GetGridObject(node.x - 1, node.y + 1));
                // upper right
                if (nodeYPlusOneIsValid) neighborList.Add(Grid.GetGridObject(node.x, node.y + 1));
                // down left
                if (nodeXMinusOneIsValid && nodeYMinusOneIsValid) neighborList.Add(Grid.GetGridObject(node.x - 1, node.y - 1));
                // down right
                if (nodeYMinusOneIsValid) neighborList.Add(Grid.GetGridObject(node.x, node.y - 1));
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
            Grid.SetDebugVisible(debug);
        }
    }

    struct DebugNodeCost
    {
        public TextMeshPro gCost;
        public TextMeshPro hCost;
        public TextMeshPro fCost;
    }
}
