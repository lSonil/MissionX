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

    public void Enter(ObserverNPCRoam npc)
    {
        Debug.Log("Enter LastFollowState");
        //npc.agent.speed *= 0.8f;
        if (GridManager.i.GetPlayerTransform() == null)
            return;
        initialStoppingDistance = npc.agent.stoppingDistance;
        npc.agent.stoppingDistance = 0.5f;
        npc.agent.SetDestination(GridManager.i.GetPlayerTransform().position);
        drainRoutine = npc.StartCoroutine(DrainSanityWhileLingering(npc));
    }

    public void Execute(ObserverNPCRoam npc)
    {
        timer += Time.deltaTime;
        if (npc.following)
        {
            npc.ChangeState(new FollowState());
            return;
        }
        if (timer >= lingerTime || GridManager.i.GetPlayerTransform() == null)
        {
            npc.ChangeState(new PatrolState());
            return;
        }
        npc.agent.SetDestination(GridManager.i.GetPlayerTransform().position);
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
        SanitySystem sanity = GridManager.i.GetPlayerTransform().GetComponent<SanitySystem>();
        if (sanity == null)
            yield break;

        while (true)
        {
            sanity.DrainSanity(npc.sanityDrainRate);
            yield return new WaitForSeconds(1f);
        }
    }

}
