using System.Collections;
using System.Collections.Generic;
using Dalton.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Units
{
    public class BaseMilitaryUnit : AbstractUnit
    {
        [SerializeField] private TestingPathfindingHex testingPathfindingHex;
        [SerializeField] private float moveSpeed = 10f;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
                List<Vector3> path = testingPathfindingHex.Pathfinding.FindPath(transform.position, mouseWorldPosition);
                StopAllCoroutines();
                StartCoroutine(MoveToPoint(path));
                if (path != null)
                {
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        Debug.DrawLine(
                            path[i],
                            path[i + 1],
                            Color.green,
                            3f);
                    }
                }
            }
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