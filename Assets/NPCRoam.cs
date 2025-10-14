using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NPCRoam : MonoBehaviour
{
    private NavMeshAgent agent;
    public List<Transform> usedGrid;
    public bool following;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (GridManager.i != null)
        {
            usedGrid = GridManager.i.grid;
        }
        else
        {
            Debug.LogError("GridManager not found in scene.");
        }
    }

    void Update()
    {
        if (GridManager.i == null || agent.pathPending) return;
        RaycastCone(); // Visualize cone
        if ((!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance) && !following)
        {
            NormalPatrol();
        }
    }

    public void NormalPatrol()
    {
        Transform closestTarget = GridManager.i.GetRandomPointInRange(usedGrid, transform, 10);
        Transform furtherTarget = GridManager.i.GetFurthestPoint(usedGrid, transform);
        if (closestTarget != null)
        {
            agent.SetDestination(closestTarget.position);
            usedGrid.Remove(closestTarget);
        }
        else if (furtherTarget != null)
        {
            agent.SetDestination(furtherTarget.position);
            usedGrid.Remove(furtherTarget);
        }
        else
        {
            usedGrid = GridManager.i.grid;
            furtherTarget = GridManager.i.GetFurthestPoint(usedGrid, transform);
            agent.SetDestination(furtherTarget.position);
            usedGrid.Remove(furtherTarget);
        }
    }
    public void RaycastCone(float horizontalAngle = 60f, float verticalAngle = 30f, int horizontalRays = 10, int verticalRays = 5, float rayLength = 20f)
    {
        Vector3 origin = transform.position; // Slightly above ground
        Vector3 forward = transform.forward;

        float halfH = horizontalAngle / 2f;
        float halfV = verticalAngle / 2f;

        for (int v = 0; v < verticalRays; v++)
        {
            float vAngle = Mathf.Lerp(-halfV, halfV, v / (float)(verticalRays - 1));

            for (int h = 0; h < horizontalRays; h++)
            {
                float hAngle = Mathf.Lerp(-halfH, halfH, h / (float)(horizontalRays - 1));

                Quaternion rotation = Quaternion.Euler(vAngle, hAngle, 0);
                Vector3 direction = rotation * forward;

                int obstacleMask = LayerMask.GetMask("Structure");
                int playerMask = LayerMask.GetMask("Player");

                if (Physics.Raycast(origin, direction, out RaycastHit hit, rayLength, obstacleMask))
                {
                    if (Physics.Raycast(origin, direction, out RaycastHit hunt, rayLength, playerMask) && !following)
                    {
                        Debug.DrawLine(origin, hit.point, Color.blue);
                        agent.SetDestination(GridManager.i.GetPlayerTransform().position);
                        StartCoroutine(WaitToLeave());
                        following = true;
                    }
                    else
                        Debug.DrawLine(origin, hit.point, Color.red);
                }
                else
                {
                    Debug.DrawRay(origin, direction * rayLength, Color.green);
                }

            }
        }

    }

    private IEnumerator WaitToLeave()
    {
        yield return new WaitForSeconds(10f);

        following = false;
    }

}