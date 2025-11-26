using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class NPCTheMimic : NPCBase
{
    public GameObject mimicInstance;
    public List<Doorway> spawnedRooms;
    public int maxNumberOfInstances;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    public override void Update()
    {
        if (contained == ContainedState.Contained) return;
        bool foundNull = false;

        for (int i = spawnedRooms.Count - 1; i >= 0; i--)
        {
            if (spawnedRooms[i] == null)
            {
                spawnedRooms.RemoveAt(i);
                foundNull = true;
            }
        }

        if (foundNull)
        {
            StartCoroutine(SpawnLoop());
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (spawnedRooms.Count < maxNumberOfInstances && RoomGenerator.i.unusedDoors.Count > 0)
            {
                SpawnRoom();
            }

            yield return new WaitForSeconds(2f); // Always wait before next attempt
        }
    }

    void SpawnRoom()
    {
        List<(Doorway, float)> shuffledDoors = new List<(Doorway, float)>(RoomGenerator.i.unusedDoors);
        shuffledDoors = shuffledDoors.OrderBy(d => Random.value).ToList();

        bool foundValid = false;

        while (shuffledDoors.Count > 0 && !foundValid)
        {
            var doorEntry = shuffledDoors[0];
            Doorway selectedDoor = doorEntry.Item1;
            shuffledDoors.RemoveAt(0);

            GameObject doorwayInstance = Instantiate(mimicInstance);
            doorwayInstance.GetComponent<MimicSpawn>().Initialize(forwardLineLength,blockingMask);
            doorwayInstance.transform.position = selectedDoor.transform.position+new Vector3(0,1,0);
            doorwayInstance.transform.rotation = selectedDoor.transform.rotation;
            doorwayInstance.transform.SetParent(transform);
            List<Vector3> mimicBorders = new List<Vector3>();
            BoxCollider[] colliders = doorwayInstance.GetComponents<BoxCollider>();

            foreach (BoxCollider col in colliders)
            {
                if (col.isTrigger)
                    continue;
                Vector3 localCenter = col.center;
                Vector3 worldPos = doorwayInstance.transform.TransformPoint(localCenter);
                Vector3 resolved = SetToResolution(worldPos);
                mimicBorders.Add(resolved);
            }

            List<Vector3> allRoomPositions = new List<Vector3>(RoomGenerator.i.AllOccupiedSpaces());
            bool overlaps = mimicBorders.Any(pos => allRoomPositions.Contains(pos));

            if (overlaps)
            {
                Destroy(doorwayInstance);
                continue;
            }
            print(doorwayInstance);
            selectedDoor.ConnectTo(doorwayInstance.GetComponent<Doorway>());
            selectedDoor.Fill(true);

            Doorway doorwayComponent = doorwayInstance.GetComponent<Doorway>();
            spawnedRooms.Add(doorwayComponent);

            foreach (Vector3 pos in mimicBorders)
            {
                RoomGenerator.i.bonusRoomsPositions.Add(pos);
            }

            foundValid = true;
        }
    }
    Vector3 SetToResolution(Vector3 worldCenter)
    {
        worldCenter.x = Mathf.RoundToInt(worldCenter.x);
        worldCenter.y = Mathf.RoundToInt(worldCenter.y);
        worldCenter.z = Mathf.RoundToInt(worldCenter.z);
        return worldCenter;
    }

}
