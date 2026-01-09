using UnityEngine;

public class RubyZoneContainment : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("SanityMonster"))
        {
            ObserverNPCRoam npc = other.GetComponent<ObserverNPCRoam>();
            if (npc == null)
            {
                // Try getting from parent
                npc = other.GetComponentInParent<ObserverNPCRoam>();
            }
            
            if (npc != null)
            {
                Debug.Log("Monster has entered containment zone — captured!");
                npc.contained = ContainedState.Contained;
            }
            else
            {
                Debug.LogWarning("Found monster object but no ObserverNPCRoam component!");
            }
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        // Continuously check in case OnTriggerEnter was missed
        if (other.name.Contains("SanityMonster"))
        {
            ObserverNPCRoam npc = other.GetComponent<ObserverNPCRoam>();
            if (npc == null) npc = other.GetComponentInParent<ObserverNPCRoam>();
            
            if (npc != null && npc.contained == ContainedState.Free)
            {
                Debug.Log("Monster captured via OnTriggerStay!");
                npc.contained = ContainedState.Contained;
            }
        }
    }
}
