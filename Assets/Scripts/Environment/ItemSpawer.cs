using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Tooltip("List of item prefabs to choose from")]
    public List<Item> items = new List<Item>();

    [Tooltip("Chance (0–100) that an item will spawn")]
    [Range(0, 100)]
    public float spawnChancePercent = 50f;

    void Start()
    {
        TrySpawnItem();
        Destroy(gameObject);
    }

    void TrySpawnItem()
    {
        if (items == null || items.Count == 0)
        {
            return;
        }

        float roll = Random.Range(0f, 100f);
        if (roll > spawnChancePercent)
        {
            return;
        }

        int index = Random.Range(0, items.Count);
        Item chosenItem = items[index];

        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject spawned = Instantiate(chosenItem.gameObject, transform.position, randomRotation, transform.parent);
    }
}
