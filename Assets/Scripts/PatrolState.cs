using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IObserverState
{
    private List<GameObject> rubyTargets;
    private int currentIndex = 0;
    private bool waiting;
    private Vector3 currentNavTarget;

    public void Enter(ObserverNPCRoam npc)
    {
        Debug.Log("Enter PatrolState");
        rubyTargets = RubiesGenerator.Instance != null
            ? new List<GameObject>(RubiesGenerator.Instance.spawnedRubies)
            : new List<GameObject>();

        if (currentIndex >= rubyTargets.Count)
            currentIndex = 0;
        waiting = false;

        if (rubyTargets.Count > 0)
            TrySetDestination(npc.agent, rubyTargets[currentIndex].transform.position);
    }

    public void Execute(ObserverNPCRoam npc)
    {
        if (npc.following)
        {
            npc.ChangeState(new FollowState());
            return;
        }

        if (rubyTargets.Count == 0) return;

        if (HasReachedDestination(npc.agent) && !waiting)
        {
            npc.StartCoroutine(WaitAtRuby(npc));
        }
    }

    public void Exit(ObserverNPCRoam npc)
    {
        Debug.Log("Exit PatrolState");
        npc.StopAllCoroutines();
    }

    private IEnumerator WaitAtRuby(ObserverNPCRoam npc)
    {
        waiting = true;
        //Debug.Log("at ruby");
        //Debug.Log(currentIndex);
        yield return new WaitForSeconds(npc.stareTimeAtRuby);
   
        currentIndex++;
        if (currentIndex >= rubyTargets.Count)
        {
            rubyTargets = rubyTargets.OrderBy(x => Random.value).ToList();
            currentIndex = 0;
        }

        if (rubyTargets[currentIndex] != null)
        {
            npc.agent.ResetPath();
            TrySetDestination(npc.agent, rubyTargets[currentIndex].transform.position);
            //Debug.Log($"Leaving ruby {currentIndex} at position {rubyTargets[currentIndex].transform.position}");
        }

        //Debug.Log("leave ruby");
        //Debug.Log(currentIndex);
        waiting = false;
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
            bool success = agent.SetDestination(hit.position);
            currentNavTarget = hit.position;
            //Debug.Log(success + " " + hit.position);
            if (!success)
                Debug.LogWarning($"SetDestination failed: {target}");
            return success;
        }
        else
        {
            Debug.LogWarning($"No NavMesh near target {target}");
            return false;
        }
    }

    private bool HasReachedDestination(NavMeshAgent agent)
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
