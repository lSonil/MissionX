using UnityEngine;

public class SanityContainedState : IObserverState
{
    public void Enter(ObserverNPCRoam npc)
    {
        Debug.Log("Enter ContainedState");

        npc.following = false;
        npc.agent.ResetPath();
        npc.agent.isStopped = true;
        //npc.StopAllCoroutines();
    }

    public void Execute(ObserverNPCRoam npc)
    {
        
    }

    public void Exit(ObserverNPCRoam npc)
    {
        Debug.Log("Exit ContainedState (should never happen during runtime)");
    }
}
