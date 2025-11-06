using UnityEngine;

public interface IObserverState
{
    void Enter(ObserverNPCRoam npc);
    void Execute(ObserverNPCRoam npc);
    void Exit(ObserverNPCRoam npc);
}
