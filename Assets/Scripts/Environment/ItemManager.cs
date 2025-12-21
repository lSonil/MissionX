using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public List<ItemSpawner> spawnPoints;
    public static ItemManager i;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        i = this;
    }
    public void ItemReady(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            room.CollectSpawnPoints();
            spawnPoints.AddRange(room.spawnPoints);
        }

        List<Item> possibleItems = GetAllPossibleItems();
        List<Item> itemPool;
        List<ItemSpawner> spawnPointsCopy = new List<ItemSpawner>(spawnPoints);

        itemPool = TakeItemsByWeight(possibleItems, SceneData.GetItemsValue());
        foreach (Item item in itemPool)
        {
            MathFunc.Shuffle(spawnPointsCopy);

            foreach (ItemSpawner spawnPoint in spawnPointsCopy)
            {
                if (spawnPoint.possibleItemsToSpawn.Contains(item))
                {
                    spawnPointsCopy.Remove(spawnPoint);
                    spawnPoint.Spawn(item);
                    break;
                }
            }
        }
    }

    public List<Item> GetAllPossibleItems()
    {
        HashSet<Item> unique = new HashSet<Item>();

        foreach (var sp in spawnPoints)
        {
            if (sp == null)
                continue;

            foreach (var item in sp.possibleItemsToSpawn)
            {
                if (item != null)
                    unique.Add(item);
            }
        }

        return unique.ToList();
    }
    List<Item> TakeItemsByWeight(List<Item> pool, float maxWeight)
    {
        int randomOffset = Random.Range(-30, 30 + 1);
        maxWeight = maxWeight + maxWeight * randomOffset / 100;

        List<Item> result = new List<Item>();
        float currentWeight = 0f;

        while (pool.Count > 0)
        {
            MathFunc.Shuffle(pool);

            Item item = pool[0];

            result.Add(item);
            currentWeight += item.GetWeight();
            
            if (currentWeight >= maxWeight)
                break;
        }

        return result;
    }
}
