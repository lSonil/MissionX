using System.Collections;
using UnityEngine;

public class FollowState : IObserverState
{
    private Coroutine drainRoutine;
    private float initialStoppingDistance;
    private float initialSpeed;

    public void Enter(ObserverNPCRoam npc)
    {
        Debug.Log("Enter FollowState");
        
        if (npc.currentSeenPlayer == null)
        {
            Debug.LogWarning("FollowState entered with no player target!");
            npc.ChangeState(new PatrolState());
            return;
        }
        
        npc.agent.ResetPath();
        
        initialSpeed = npc.agent.speed;
        npc.agent.speed *= 1.5f;
        
        initialStoppingDistance = npc.agent.stoppingDistance;
        npc.agent.stoppingDistance = 0.5f;
        npc.agent.SetDestination(npc.currentSeenPlayer.position);
        drainRoutine = npc.StartCoroutine(DrainSanityWhileFollowing(npc));
    }

    public void Execute(ObserverNPCRoam npc)
    {
        if (npc.currentSeenPlayer == null)
        {
            Debug.Log("Lost player reference - transitioning to PatrolState");
            npc.ChangeState(new PatrolState());
            return;
        }
        
        if (!npc.following)
        {
            Debug.Log("Player no longer visible - transitioning to LastFollowState");
            npc.ChangeState(new LastFollowState());
        }
        else
        {
            npc.agent.SetDestination(npc.currentSeenPlayer.position);
        }
    }

    public void Exit(ObserverNPCRoam npc)
    {
        Debug.Log("Exit FollowState");
        
        npc.agent.speed = initialSpeed;
        npc.agent.stoppingDistance = initialStoppingDistance;
        
        if (drainRoutine != null)
            npc.StopCoroutine(drainRoutine);
    }

    private IEnumerator DrainSanityWhileFollowing(ObserverNPCRoam npc)
    {
        if (npc.currentSeenPlayer == null)
            yield break;
            
        SanitySystem sanity = npc.currentSeenPlayer.GetComponent<SanitySystem>();
        if (sanity == null)
            yield break;

        while (npc.following && npc.currentSeenPlayer != null)
        {
            sanity.DrainSanity(npc.sanityDrainRate);
            yield return new WaitForSeconds(1f);
        }
    }
}
