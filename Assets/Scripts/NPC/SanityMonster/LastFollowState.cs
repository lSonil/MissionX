using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class LastFollowState : IObserverState
{
    private readonly float lingerTime = 7f;
    private Coroutine drainRoutine;
    private float timer = 0f;
    private float initialStoppingDistance;
    private Vector3 lastKnownPosition;

    public void Enter(ObserverNPCRoam npc)
    {
        Debug.Log("Enter LastFollowState");
      
        lastKnownPosition = npc.agent.destination;
        initialStoppingDistance = npc.agent.stoppingDistance;
        npc.agent.stoppingDistance = 0.5f;
        npc.agent.SetDestination(npc.currentSeenPlayer.position != null ? npc.currentSeenPlayer.position : lastKnownPosition);
        
        if (npc.currentSeenPlayer != null)
        {
            drainRoutine = npc.StartCoroutine(DrainSanityWhileLingering(npc));
        }
    }

    public void Execute(ObserverNPCRoam npc)
    {
        timer += Time.deltaTime;
        
        if (npc.following)
        {
            npc.ChangeState(new FollowState());
            return;
        }
        
        if (timer >= lingerTime)
        {
            npc.ChangeState(new PatrolState());
            return;
        }

        npc.agent.SetDestination(npc.currentSeenPlayer.position != null ? npc.currentSeenPlayer.position : lastKnownPosition);
    }

    public void Exit(ObserverNPCRoam npc)
    {
        Debug.Log("Exit LastFollowState");
        npc.agent.stoppingDistance = initialStoppingDistance;
        if (drainRoutine != null)
            npc.StopCoroutine(drainRoutine);
    }

    private IEnumerator DrainSanityWhileLingering(ObserverNPCRoam npc)
    {
        if (npc.currentSeenPlayer == null)
            yield break;
            
        SanitySystem sanity = npc.currentSeenPlayer.GetComponent<SanitySystem>();
        if (sanity == null)
            yield break;

        while (timer < lingerTime && npc.currentSeenPlayer != null)
        {
            sanity.DrainSanity(npc.sanityDrainRate);
            yield return new WaitForSeconds(1f);
        }
    }
}
