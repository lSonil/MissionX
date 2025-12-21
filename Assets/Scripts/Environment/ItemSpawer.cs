using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    public List<Item> possibleItemsToSpawn = new List<Item>();

    public void Spawn(Item itemtoSpawn)
    {
        if (!possibleItemsToSpawn.Contains(itemtoSpawn)) return;

        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject spawned = Instantiate(itemtoSpawn.gameObject, transform.position, randomRotation, transform.parent);
    }
}
