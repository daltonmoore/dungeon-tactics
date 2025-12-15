using System;
using System.Collections;
using System.Collections.Generic;
using Drawing;
using HexGrid;
using UnityEngine;

namespace Units
{
    public abstract class AbstractUnit : AbstractCommandable
    {
        [SerializeField] private Transform flagPrefab;
        [SerializeField] protected float moveSpeed = 10f;
        
        private int _movePointsLeft;
        
        // stored path to compare when we click on a hex to show the path, then click again to move.
        public List<PathNodeHex> Path { get; private set; }
        
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

        public void MoveTo(List<PathNodeHex> path)
        {
            StopAllCoroutines();
            StartCoroutine(TravelPath(path));
        }
        
        private IEnumerator TravelPath(List<PathNodeHex> path)
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
}