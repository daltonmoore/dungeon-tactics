using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Drawing;
using Events;
using TacticsCore.Data;
using TacticsCore.EventBus;
using TacticsCore.Events;
using TacticsCore.HexGrid;
using TacticsCore.Interfaces;
using TacticsCore.Units;
using Units;
using UnityEngine;
using UnityEngine.AI;

namespace TacticsCore.Units
{
    public class LeaderUnit : AbstractUnit, ISelectable
    {
        private static readonly int AnimatorDirectionHash = Animator.StringToHash("Direction");
        private static readonly int AnimatorSpeedHash = Animator.StringToHash("Speed");
        private const float StoppingDistance = 0.1f;
        
        [field: SerializeField] public List<BattleUnitData> PartyList { get; set; } = new();
        [SerializeField] private Transform flagPrefab;
        [SerializeField] protected float moveSpeed = 10f;
        
        public PathNodeHex BattleNode { get; set; }
        public LeaderUnit EnemyUnit { get; set; }
        public List<PathNodeHex> Path { get; set; }private int _movePointsLeft;
        public bool IsSelected { private set; get; }
        
        private Transform _flagParent;
        private Rigidbody2D _rigidbody2D; 
        private Vector3 _targetPosition;
        private bool _moving;
        private Vector2 _previousPosition;

        protected override void Awake()
        {
            base.Awake();
            
            _movePointsLeft = UnitSO.MovePoints;
            _flagParent = new GameObject("MoveFlags").transform;
            _rigidbody2D = GetComponent<Rigidbody2D>();

            foreach (UnitSO unitData in PartyList)
            {
                unitData.owner = Owner;
            }
        }
        
        private void Start()
        {
            var gridObject = Pathfinder.Instance.Pathfinding.Grid.GetGridObject(transform.position);
            gridObject.IsOccupied = true;
            gridObject.Occupant = this;
        }
        
        private void FixedUpdate()
        {
            if (_moving)
            {
                // Vector3 direction =  new Vector2(_targetPosition.x, _targetPosition.y) - _rigidbody2D.position;
                // _rigidbody2D.AddForce(direction.normalized * moveSpeed, ForceMode2D.Force);
                _previousPosition = _rigidbody2D.position;
                _rigidbody2D.MovePosition(Vector2.MoveTowards(_rigidbody2D.position, _targetPosition, moveSpeed * Time.deltaTime)); 
            }
        }
        
        private void OnDestroy()
        {
            if (_flagParent != null)
                Destroy(_flagParent.gameObject);
        }

        private IEnumerator TravelPath(List<PathNodeHex> path, Action<bool> callback)
        {
            if (path.Count == 0) yield break;
            
            HidePath();


            foreach (PathNodeHex node in path)
            {
                _targetPosition = node.worldPosition;
                
                Animator.SetFloat(AnimatorSpeedHash, 1);
                while (Vector3.Distance(transform.position, _targetPosition) > StoppingDistance && _movePointsLeft - node.gCost >= 0)
                {
                    Vector2 currentVelocity = (_rigidbody2D.position - _previousPosition) / Time.fixedDeltaTime;
                    
                    Debug.Log($"Current Velocity is: {currentVelocity}");
                    switch (currentVelocity)
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
                bool isAtDestination = distance < StoppingDistance;
                _moving = false;
                callback(isAtDestination);
            }
        }


        public void Attack(LeaderUnit enemyLeader)
        {
            // we clicked directly on the hex where the occupant is, so we need to move to the hex just before it.
            var tempPath = new List<PathNodeHex>(Path);
            
            if (Path[^1].Occupant != null)
            {
                tempPath = Path.SkipLast(1).ToList();
            }

            if (BattleNode != null)
            {
                Pathfinder.Instance.Pathfinding.FindPath(transform.position, BattleNode.worldPosition, out tempPath);
            }
            
            MoveTo(tempPath, arrivedAtDestination =>
            {
                Debug.Log(arrivedAtDestination ? "Arrived at destination" : "Failed to reach destination");
                bool arrivedAtBattleNode = BattleNode != null &&
                                           Vector3.Distance(transform.position, BattleNode.worldPosition) <=
                                           PathfindingHex.CellSize + StoppingDistance;
                if (arrivedAtDestination || arrivedAtBattleNode)
                {
                    Bus<EngageInBattleEvent>.Raise(
                        Owner.Player1,
                        new EngageInBattleEvent(
                            this,
                            PartyList, 
                            enemyLeader.PartyList
                        )
                    );
                }
            });
        }

        public void ShowPath(List<PathNodeHex> path, out  PathNodeHex battleNode, out LeaderUnit enemyUnity)
        {
            HidePath();

            int movePointsLeft = _movePointsLeft;
            battleNode = null;
            enemyUnity = null;
            
            if (path.Count > 0)
            {
                Path = path;

                for (int index = 0; index < path.Count; index++)
                {
                    PathNodeHex node = path[index];
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

                        Debug.Log($"Looking at node ({node.x}, {node.y})'s neighbors");
                        foreach (var neighbor in Pathfinder.Instance.Pathfinding.GetNeighborList(node))
                        {
                            Debug.Log($"Looking at neighbor ({neighbor.x}, {neighbor.y})");
                            if (neighbor.IsOccupied && neighbor.Occupant != null && neighbor.Occupant.Owner != Owner)
                            {
                                Pathfinder.Instance.Pathfinding.Grid.GetGridPosition(neighbor.worldPosition, out int x,
                                    out int y);
                                Debug.Log($"Neighbor ({x}, {y}) is occupied by {neighbor.Occupant.name}");
                                Debug.Log($"Node ({node.x}, {node.y}) is a battle node due to this nearby enemy.");
                                enemyUnity = neighbor.Occupant as LeaderUnit;
                                battleNode = node; // this is the first node on our path that is in range of the enemy.
                                break;
                            }
                        }
                    }
                    if (battleNode != null) flagColor = Color.red;
                    flag.GetComponent<SpriteRenderer>().color = flagColor;
                }
            }
        }

        private void HidePath()
        {
            foreach (Transform flag in _flagParent)
            {
                Destroy(flag.gameObject);
            }
            
            Path = null;
        }

        public void MoveTo(List<PathNodeHex> path, Action<bool> callback = null)
        {
            _moving = true;
            StopAllCoroutines();
            StartCoroutine(TravelPath(path, callback));
        }
        
        public void Select()
        {
            if (decal != null)
            {
                decal.gameObject.SetActive(true);
            }
            
            IsSelected = true;
            Bus<UnitSelectedEvent>.Raise(Owner, new UnitSelectedEvent(this));
        }

        public void Deselect()
        {
            if (decal != null)
            {
                decal.gameObject.SetActive(false);
            }
            IsSelected = false;
        }
        
        
        private IEnumerator MoveToPoint(List<Vector3> path)
        {
            if (path.Count == 0) yield break;

            foreach (Vector3 targetPosition in path)
            {
                while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }
            }
        }

        private void OnDrawGizmos()
        {
            // DrawGizmos(agent, showPath, showAhead);
        }

        public static void DrawGizmos(NavMeshAgent agent, bool showPath, bool showAhead)
        {
            if (Application.isPlaying && agent != null)
            {
                if (showPath && agent.hasPath)
                {
                    var corners = agent.path.corners;
                    if (corners.Length < 2) { return; }
                    int i = 0;
                    for (; i < corners.Length - 1; i++)
                    {
                        Debug.DrawLine(corners[i], corners[i + 1], Color.blue);
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(agent.path.corners[i + 1], 0.03f);
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
                    }
                    Debug.DrawLine(corners[0], corners[1], Color.blue);
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(agent.path.corners[1], 0.03f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(agent.path.corners[0], agent.path.corners[1]);
                }

                if (showAhead)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(agent.transform.position, agent.transform.up * 0.5f);
                }
            }
        }
    }
}