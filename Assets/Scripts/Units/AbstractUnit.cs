using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Battle;
using Drawing;
using Events;
using HexGrid;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Units
{
    public abstract class AbstractUnit : AbstractCommandable, IAttacker, IAttackable
    {
        private static readonly int AnimatorDirectionHash = Animator.StringToHash("Direction");
        private static readonly int AnimatorSpeedHash = Animator.StringToHash("Speed");
        private const float STOPPINGDISTANCE = 0.1f;
        
        [SerializeField] private Transform flagPrefab;
        [SerializeField] protected float moveSpeed = 10f;
        [SerializeField] private AnimationConfigSO animationConfig;
        [field: SerializeField] public List<BattleUnitData> PartyList { get; set; } = new();
        public PathNodeHex BattleNode { get; set; }
        public List<PathNodeHex> Path { get; set; }

        protected UnitSO unitSO;

        private int _movePointsLeft;
        private Transform _flagParent;
        private PositionDirectionTracker _directionTracker;
        private Rigidbody2D _rigidbody2D; 
        private Vector3 _targetPosition;
        private bool _moving;

        protected override void Awake()
        {
            base.Awake();
            unitSO = UnitSO as UnitSO;
            _movePointsLeft = unitSO.MovePoints;
            _flagParent = new GameObject("MoveFlags").transform;
            _directionTracker = GetComponent<PositionDirectionTracker>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void OnDestroy()
        {
            if (_flagParent != null)
                Destroy(_flagParent.gameObject);
        }

        private void Start()
        {
            var gridObject = Pathfinder.Instance.Pathfinding.grid.GetGridObject(transform.position);
            gridObject.IsOccupied = true;
            gridObject.Occupant = this;
        }

        private void FixedUpdate()
        {
            if (_moving)
            {
                _rigidbody2D.MovePosition(Vector2.MoveTowards(_rigidbody2D.position, _targetPosition, moveSpeed * Time.deltaTime));
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            Debug.Log($"{name} collided with {other.gameObject.name} which is on layer {other.gameObject.layer}");
            if (other.gameObject.layer == LayerMask.NameToLayer("FogOfWar"))
            {
                Tilemap tilemap = other.collider.GetComponent<Tilemap>();
                var cellPos = tilemap.WorldToCell(other.contacts[0].point);
                var tile = tilemap.GetTile(cellPos);
                Debug.Log($"Tile is {tile} at tile position {cellPos}");
                tilemap.SetTile(cellPos, null);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            
        }

        public void MoveTo(List<PathNodeHex> path, Action<bool> callback = null)
        {
            _moving = true;
            StopAllCoroutines();
            StartCoroutine(TravelPath(path, callback));
        }
        
        private IEnumerator TravelPath(List<PathNodeHex> path, Action<bool> callback)
        {
            if (path.Count == 0) yield break;
            
            HidePath();


            foreach (PathNodeHex node in path)
            {
                _targetPosition = node.worldPosition;
                
                Animator.SetFloat(AnimatorSpeedHash, 1);
                while (Vector3.Distance(transform.position, _targetPosition) > STOPPINGDISTANCE && _movePointsLeft - node.gCost >= 0)
                {
                    var direction = _directionTracker.currentMoveDirection;
                    switch (direction)
                    {
                        case { x: > 0.7f, y: < 0.7f and > -0.7f }:
                            Animator.SetInteger(AnimatorDirectionHash, (int)SpriteAnimation.Direction.Right);
                            break;
                        case { x: < -0.7f, y: < 0.7f and > -0.7f }:
                            Animator.SetInteger(AnimatorDirectionHash, (int)SpriteAnimation.Direction.Left);
                            break;
                        case {y: > 0.7f}:
                            Animator.SetInteger(AnimatorDirectionHash, (int)SpriteAnimation.Direction.Up);
                            break;
                        default:
                            Animator.SetInteger(AnimatorDirectionHash, (int)SpriteAnimation.Direction.Down);
                            break;
                    }
                    // transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }
                // TODO: decrease move points as they move.
                // _movePointsLeft -= node.gCost;
            }
            
            Animator.SetFloat(AnimatorSpeedHash, 0);
            Animator.SetInteger(AnimatorDirectionHash, 0);

            if (callback != null)
            {
                float distance = Vector3.Distance(transform.position, path[^1].worldPosition);
                bool isAtDestination = distance < STOPPINGDISTANCE;
                _moving = false;
                callback(isAtDestination);
            }
        }


        public void Attack(IAttackable attackable)
        {
            // we clicked directly on the hex where the occupant is, so we need to move to the hex just before it.
            var tempPath = new List<PathNodeHex>(Path);
            if (Path[^1].Occupant != null)
            {
                tempPath = Path.SkipLast(1).ToList();
            }
            MoveTo(tempPath, arrivedAtDestination =>
            {
                Debug.Log(arrivedAtDestination ? "Arrived at destination" : "Failed to reach destination");
                bool arrivedAtBattleNode = BattleNode != null &&
                                           Vector3.Distance(transform.position, BattleNode.worldPosition) <=
                                           PathfindingHex.CellSize + STOPPINGDISTANCE;
                if (arrivedAtDestination || arrivedAtBattleNode)
                {
                    EventBus.Bus<StartBattleEvent>.Raise(Owner.Player1,
                        new StartBattleEvent(
                            PartyList, 
                            attackable.PartyList
                            ));
                }
            });
        }

        public void ShowPath(List<PathNodeHex> path, IAttackable attackable, out  PathNodeHex battleNode)
        {
            HidePath();

            int movePointsLeft = _movePointsLeft;
            battleNode = null;
            
            if (path.Count > 0)
            {
                Path = path;

                foreach (PathNodeHex node in path)
                {
                    Vector3 position = node.worldPosition;

                    using (Draw.ingame.WithDuration(2))
                    {
                        Draw.ingame.WireBox(position, Vector3.one * 0.2f, Color.red);
                    }

                    Transform flag = Instantiate(flagPrefab, position, Quaternion.identity);
                    flag.SetParent(_flagParent);
                    Color flagColor = movePointsLeft - node.gCost >= 0 ? Color.blue : Color.white;
                    movePointsLeft -= node.gCost;
                    
                    // flag is red when attacking or moving within a hex of unit
                    if (battleNode == null)
                    {
                        if (node.IsOccupied && node.Occupant != null && node.Occupant.Owner != Owner)
                        {
                            battleNode = node;
                        }

                        foreach (var neighbor in Pathfinder.Instance.Pathfinding.GetNeighborList(node))
                        {
                            if (neighbor.IsOccupied && neighbor.Occupant != null && neighbor.Occupant.Owner != Owner)
                            {
                                Debug.Log($"Neighbor is occupied");
                                battleNode = neighbor;
                                break;
                            }
                        }
                    }
                    if (battleNode != null) flagColor = Color.red;
                    flag.GetComponent<SpriteRenderer>().color = flagColor;
                }
            }
        }
        
        public void HidePath()
        {
            foreach (Transform flag in _flagParent)
            {
                Destroy(flag.gameObject);
            }
            
            Path = null;
        }

    }
    
    public enum BattleUnitPosition
    {
        BackBottom = 0,     // mod 3 is back to 0
        BackCenter = 1,     // mod 3 is 1
        BackTop = 2,         // mod 3 is 2
        FrontBottom = 3,    // mod 3 is 0
        FrontCenter = 4,    // mod 3 is 1
        FrontTop = 5,       // mod 3 is 2
    }
}