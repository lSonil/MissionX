using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class VentSpawner : MonoBehaviour
{
    public List<Monster> monsterPool; // Assign in Inspector
    public Transform spawnPoint;      // Where monsters appear
    public GameObject vent1;      // Where monsters appear
    public GameObject vent2;      // Where monsters appear

    private void Start()
    {
        vent2.SetActive(false);
        vent1.SetActive(true);

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            Monster monsterToSpawn = GetValidMonster();
            if (monsterToSpawn != null)
            {
                SpawnMonster(monsterToSpawn);
                GridManager.currentMonsterDifficulty += monsterToSpawn.difficulty;
            }

            yield return new WaitForSeconds(5f);
        }
    }

    private Monster GetValidMonster()
    {
        List<Monster> validMonsters = new();

        foreach(Monster m in monsterPool)
        {
            if (GridManager.currentMonsterDifficulty + m.difficulty <= GridManager.maxMonsterDifficulty)
            {
                validMonsters.Add(m);
            }
        }

        if (validMonsters.Count == 0) return null;

        return validMonsters[Random.Range(0, validMonsters.Count)];
    }

    private void SpawnMonster(Monster monster)
    {
        if (monster.body == null || spawnPoint == null) return;
        vent1.SetActive(false);
        vent2.SetActive(true);
        Instantiate(monster.body, spawnPoint.position, Quaternion.identity);
    }
}
