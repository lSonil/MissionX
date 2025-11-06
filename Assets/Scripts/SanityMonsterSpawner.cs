using System.Collections;
using System.Linq;
using UnityEngine;

public class SanityMonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public Vector3 spawnOffset = new Vector3(0, 0.5f, 0);

    private RoomGenerator roomGenerator;

    private void Start()
    {
        roomGenerator = RoomGenerator.i;
        if (roomGenerator == null)
        {
            Debug.LogError("[MonsterSpawner] RoomGenerator not found in scene!");
            return;
        }

        StartCoroutine(WaitForRoomGeneration());
    }

    private IEnumerator WaitForRoomGeneration()
    {
        yield return new WaitUntil(() =>
            roomGenerator.spawnedRooms != null &&
            roomGenerator.spawnedRooms.Count > 0
        );

        yield return new WaitForSeconds(0.5f);
        SpawnMonster();
    }

    private void SpawnMonster()
    {
        Room monsterRoom = roomGenerator.spawnedRooms
            .FirstOrDefault(r => r.name.Contains("ObserverMonster"));

        if (monsterRoom == null)
        {
            Debug.LogWarning("[MonsterSpawner] No room named '...ObserverMonster' found!");
            return;
        }

        Vector3 spawnPosition = monsterRoom.transform.position + spawnOffset;

        GameObject monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
        monster.transform.SetParent(monsterRoom.transform);

        Debug.Log($"[MonsterSpawner] Spawned monster in room: {monsterRoom.name}");
    }
}
