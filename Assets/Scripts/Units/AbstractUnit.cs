using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Battle;
using Drawing;
using HexGrid;
using UnityEditor;
using UnityEngine;

namespace Units
{
    [ExecuteInEditMode]
    public abstract class AbstractUnit : AbstractCommandable, IAttacker, IAttackable
    {
        [SerializeField] private Transform flagPrefab;
        [SerializeField] protected float moveSpeed = 10f;
        [field: SerializeField] public List<BattleUnitData> PartyList { get; set; } = new();

        private int _movePointsLeft;
        protected UnitSO unitSO;

        private Transform _flagParent;

        protected override void Awake()
        {
            base.Awake();
            unitSO = UnitSO as UnitSO;
            _movePointsLeft = unitSO.MovePoints;
            _flagParent = new GameObject("MoveFlags").transform;
        }

        private void Start()
        {
            Pathfinder.Instance.Pathfinding.grid.GetGridObject(transform.position).IsOccupied = true;
        }

        public void SwapUnits(BattleUnitPosition from, BattleUnitPosition to)
        {
            BattleUnitData fromUnit = PartyList.Find(u => u.battleUnitPosition == from);
            BattleUnitData toUnit = PartyList.Find(u => u.battleUnitPosition == to);

            if (fromUnit != null)
            {
                Debug.Log($"Swapping {fromUnit.name} to position {to}");
                fromUnit.battleUnitPosition = to;
            }
            
            if (toUnit != null)
            {
                Debug.Log($"Swapping {toUnit.name} to position {from}");
                toUnit.battleUnitPosition = from;
            }
        }

        public void MoveTo(List<PathNodeHex> path, Action<bool> callback = null)
        {
            StopAllCoroutines();
            StartCoroutine(TravelPath(path, callback));
        }
        
        private IEnumerator TravelPath(List<PathNodeHex> path, Action<bool> callback)
        {
            if (path.Count == 0) yield break;
            
            HidePath();
            
            
            foreach (PathNodeHex node in path)
            {
                Vector3 targetPosition = node.worldPosition;
                while (Vector3.Distance(transform.position, targetPosition) > 0.1f && _movePointsLeft - node.gCost >= 0)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }
                _movePointsLeft -= node.gCost;
            }

            if (callback != null)
            {
                float distance = Vector3.Distance(transform.position, path[^1].worldPosition);
                bool isAtDestination = distance < 0.1f;
                callback(isAtDestination);
            }
        }

        public List<PathNodeHex> Path { get; set; }

        public void Attack(IAttackable attackable)
        {
            MoveTo(Path, arrivedAtDestination =>
            {
                Debug.Log(arrivedAtDestination ? "Arrived at destination" : "Failed to reach destination");
                if (arrivedAtDestination)
                {
                    // BattleManager.Instance.StartBattle(new BattleStartArgs
                    //     { Party = COOLPARTY, EnemyParty = attackable.Party });
                }
            });
        }

        public void ShowPath(List<PathNodeHex> path)
        {
            HidePath();

            int movePointsLeft = _movePointsLeft;
            
            if (path.Count > 1)
            {
                path.RemoveAt(0);
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