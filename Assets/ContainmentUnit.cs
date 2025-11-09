using System.Collections;
using UnityEngine;

public class ContainmentUnit : MonoBehaviour
{
    public NPCBase npc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(.1f);
        npc.gameObject.SetActive(true);
    }
}
