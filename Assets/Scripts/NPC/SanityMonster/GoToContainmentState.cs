using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GoToContainmentState : IObserverState
{
    private float initialStoppingDistance;
    private Vector3 containmentCenter;

    public void Enter(ObserverNPCRoam npc)
    {
        Debug.Log("Enter GoToContainmentState - Monster is drawn to the ritual!");
        
        if (RitualManager.i != null && RitualManager.i.containmentTrigger != null)
        {
            containmentCenter = RitualManager.i.containmentTrigger.bounds.center;
        }
        else
        {
            Debug.LogError("No containment trigger found!");
            return;
        }
        
        npc.agent.ResetPath();
        initialStoppingDistance = npc.agent.stoppingDistance;
        npc.agent.stoppingDistance = 0.1f;
        npc.agent.speed *= 1.5f;

        TrySetDestination(npc.agent, containmentCenter);
    }

    public void Execute(ObserverNPCRoam npc)
    {
        if (npc.agent.remainingDistance > npc.agent.stoppingDistance)
        {
            TrySetDestination(npc.agent, containmentCenter);
        }
    }

    public void Exit(ObserverNPCRoam npc)
    {
        Debug.Log("Exit GoToContainmentState");
        npc.agent.stoppingDistance = initialStoppingDistance;
    }

    private bool TrySetDestination(NavMeshAgent agent, Vector3 target)
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning("Agent not on NavMesh!");
            return false;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 2.5f, NavMesh.AllAreas))
        {
            return agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning($"No NavMesh near target {target}");
            return false;
        }
    }
}
