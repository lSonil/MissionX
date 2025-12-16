using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInsideNPC : MonoBehaviour
{
    public NPCBase npc;
    public bool isInside;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == npc.gameObject)
        {
            isInside = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == npc.gameObject)
        {
            isInside = false;
        }
    }
}
