using System.Collections;
using UnityEngine;

public class FollowState : IObserverState
{
    private Coroutine drainRoutine;
    private float initialStoppingDistance;

    public void Enter(ObserverNPCRoam npc)
    {
        Debug.Log("Enter FollowState");
        npc.agent.ResetPath();
        initialStoppingDistance = npc.agent.stoppingDistance;
        npc.agent.stoppingDistance = 0.5f;
        npc.agent.SetDestination(GridManager.i.GetPlayerTransform().position);
        drainRoutine = npc.StartCoroutine(DrainSanityWhileFollowing(npc));
    }

    public void Execute(ObserverNPCRoam npc)
    {
        Debug.Log("following = " + npc.following);
        if (!npc.following)
        {
            Debug.Log("change state: follow => patrol");
            npc.ChangeState(new LastFollowState());
        }
        else
        {
            Debug.Log("set destination");
            npc.agent.SetDestination(GridManager.i.GetPlayerTransform().position);
        }
    }

    public void Exit(ObserverNPCRoam npc)
    {
        Debug.Log("Exit FollowState");
        npc.agent.stoppingDistance = initialStoppingDistance;
        if (drainRoutine != null)
            npc.StopCoroutine(drainRoutine);
    }

    private IEnumerator DrainSanityWhileFollowing(ObserverNPCRoam npc)
    {
        SanitySystem sanity = GridManager.i.GetPlayerTransform().GetComponent<SanitySystem>();
        if (sanity == null)
            yield break;

        while (npc.following)
        {
            sanity.DrainSanity(npc.sanityDrainRate);
            yield return new WaitForSeconds(1f);
        }
    }
}
