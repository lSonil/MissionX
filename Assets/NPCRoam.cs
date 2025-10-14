using UnityEngine;
using UnityEngine.AI;

public class NPCRoam : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform currentTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (GridManager.i != null)
        {
            SetFurthestPoint();
        }
        else
        {
            Debug.LogError("GridManager not found in scene.");
        }
    }

    void Update()
    {
        if (GridManager.i == null || agent.pathPending) return;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            SetFurthestPoint();
        }
    }

    void SetFurthestPoint()
    {
        currentTarget = GridManager.i.GetFurthestPoint(transform);
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
    }
}
