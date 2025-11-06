using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RubiesGenerator : MonoBehaviour
{
    [Header("Crystal Settings")]
    public GameObject rubyPrefab;
    [Tooltip("Number of normal rooms per crystal room (e.g., 5 = 1 crystal room for 5 normal rooms).")]
    public int roomsPerRuby = 5;

    public static RubiesGenerator Instance;
    public List<GameObject> spawnedRubies = new List<GameObject>();

    private RoomGenerator roomGenerator;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        roomGenerator = RoomGenerator.i;
        if (roomGenerator == null)
        {
            Debug.LogError("[CrystalGenerator] RoomGenerator not found in scene!");
            return;
        }

        StartCoroutine(WaitForRoomGeneration());
    }

    private IEnumerator WaitForRoomGeneration()
    {
        yield return new WaitUntil(() => roomGenerator.spawnedRooms != null && roomGenerator.spawnedRooms.Count > 0);
        yield return new WaitForSeconds(1f);
        SpawnCrystals();
    }

    private void SpawnCrystals()
    {
        List<Room> allRooms = roomGenerator.spawnedRooms;

        // Exclude halls, SCP rooms, or other special rooms if needed
        List<Room> normalRooms = allRooms.Where(r => !r.CompareTag("SCP")).ToList();

        if (normalRooms.Count == 0)
        {
            Debug.LogWarning("[CrystalGenerator] No valid rooms found to spawn crystals.");
            return;
        }

        int numCrystalRooms = Mathf.Max(1, normalRooms.Count / roomsPerRuby);
        List<Room> crystalRooms = normalRooms.OrderBy(x => Random.value).Take(numCrystalRooms).ToList();

        foreach (Room room in crystalRooms)
        {
            SpawnCrystalsInRoom(room);
        }

        Debug.Log($"[CrystalGenerator] Spawned crystals in {crystalRooms.Count} rooms.");
    }

    private void SpawnCrystalsInRoom(Room room)
    {
        if (room.Rubies == null || room.Rubies.Count == 0) return;
        room.Rubies.ForEach(rubyTransform =>
        {
            var ruby = Instantiate(rubyPrefab, rubyTransform.position, Quaternion.identity, room.transform);
            spawnedRubies.Add(ruby);
        });
    }
}
