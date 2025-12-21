using UnityEngine;

public class RubyZoneContainment : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "SanityMonster")
        {
            ObserverNPCRoam npc = other.GetComponent<ObserverNPCRoam>();
            if (npc != null)
            {
                Debug.Log("Monster has entered containment zone — captured!");
                npc.contained = ContainedState.Contained;
            }
        }
    }
}
