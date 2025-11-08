using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentSpawner : MonoBehaviour
{
    public List<Monster> monsterPool; // Assign in Inspector
    public Transform spawnPoint;      // Where monsters appear
    public GameObject vent1;      // Where monsters appear
    public GameObject vent2;      // Where monsters appear

    private void Start()
    {
        if (vent2 != null) vent2.SetActive(false);
        if (vent1 != null) vent1.SetActive(true);

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            Monster monsterToSpawn = GetValidMonster();
            if (monsterToSpawn != null)
            {
                SpawnMonster(monsterToSpawn);
                GridManager.currentMonsterDifficulty += monsterToSpawn.difficulty;
            }

        }
    }

    private Monster GetValidMonster()
    {
        List<Monster> validMonsters = new();

        foreach (Monster m in monsterPool)
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
        if (vent1 != null) vent1.SetActive(false);
        if (vent2 != null) vent2.SetActive(true);
        Instantiate(monster.body, spawnPoint.position, Quaternion.identity);
    }
}