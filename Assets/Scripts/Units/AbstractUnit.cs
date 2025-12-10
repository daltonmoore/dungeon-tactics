using System;
using System.Collections;
using System.Collections.Generic;
using Drawing;
using UnityEngine;

namespace Units
{
    public abstract class AbstractUnit : AbstractCommandable
    {
        [SerializeField] private Transform flagPrefab;
        [SerializeField] protected float moveSpeed = 10f;
        
        protected UnitSO unitSO;
        
        private Transform _flagParent;

        protected override void Awake()
        {
            base.Awake();
            unitSO = UnitSO as UnitSO;
            _flagParent = new GameObject("MoveFlags").transform;
        }

        public void MoveTo(List<Vector3> path)
        {
            StopAllCoroutines();
            StartCoroutine(TravelPath(path));
        }
        
        private IEnumerator TravelPath(List<Vector3> path)
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

        public void ShowPath(List<Vector3> path)
        {
            foreach (Transform flag in _flagParent)
            {
                Destroy(flag.gameObject);
            }

            Color flagColor = Color.white;
            int movePointsLeft = unitSO.MovePoints;
            
            if (path.Count > 1)
            {
                path.RemoveAt(0);

                foreach (Vector3 position in path)
                {
                    using (Draw.ingame.WithDuration(2))
                    {
                        Draw.ingame.WireBox(position, Vector3.one * 0.2f, Color.red);
                    }
                    
                    Transform flag = Instantiate(flagPrefab, position, Quaternion.identity);
                    flag.SetParent(_flagParent);
                    flag.GetComponent<SpriteRenderer>().color = flagColor;
                }
            }
        }
    }
}