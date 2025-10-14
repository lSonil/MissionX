using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;
public class RoomGenerator : MonoBehaviour
{
    public List<RoomSpawnEntry> possibleRoomsToSpawn;
    public List<RoomSpawnEntry> specificRoomsToSpawn;
    public List<Room> spawnedRooms;
    public int maxNumberOfRooms;
    [HideInInspector]
    public List<Vector3> roomsPositions;
    [HideInInspector]
    public List<Transform> allDoors;


    public List<(Transform, float)> unusedDoors = new List<(Transform, float)>();
    public GameObject wall;
    public List<Transform> grid = new List<Transform>();
    public bool debug;
    public GridManager gridManager;
    private void Awake()
    {
        allDoors.AddRange(spawnedRooms[0].doors);
        foreach (var door in spawnedRooms[0].doors)
        {
            unusedDoors.Add((door, spawnedRooms[0].hallChance));
        }
        BoxCollider[] colliders = spawnedRooms[0].GetComponents<BoxCollider>();

        foreach (BoxCollider col in colliders)
        {
            Vector3 localCenter = col.center;
            roomsPositions.Add(SetToResolution(spawnedRooms[0].transform.TransformPoint(localCenter)));
        }
        SpawneRooms(possibleRoomsToSpawn, 0);
    }
    public void SpawneRooms(List<RoomSpawnEntry> possibleRooms, int bonusRoomCount)
    {
        bool stopGeneration = true;
        if (maxNumberOfRooms + bonusRoomCount > spawnedRooms.Count)
        {
            List<(Transform, float)> shuffledDoors = unusedDoors.OrderBy(x => Guid.NewGuid()).ToList();
            List<RoomSpawnEntry> shuffledRooms = possibleRooms.OrderBy(x => Guid.NewGuid()).ToList();

            float threshold = shuffledDoors[0].Item2; // value between 0 and 100
            float roll = UnityEngine.Random.Range(0f, 100f);
            List<RoomSpawnEntry> roomsHall = possibleRooms.Where(entry => entry.room.hall).OrderBy(x => Guid.NewGuid()).ToList();
            List<RoomSpawnEntry> roomsNoHall = possibleRooms.Where(entry => !entry.room.hall).OrderBy(x => Guid.NewGuid()).ToList();

            if (bonusRoomCount == 0)
            {
                if (roll <= threshold && roomsHall.Count > 0)
                {
                    shuffledRooms = roomsHall;
                }
                else
                {
                    shuffledRooms = roomsNoHall;
                }
            }
            
            Room listRoom = null;
            Room newRoom = null;
            Transform newDoor = null;
            bool foundRoom = false;

            while (shuffledDoors.Count > 0 && !foundRoom)
            {
                List<RoomSpawnEntry> copyOfShuffledRooms = new List<RoomSpawnEntry>(shuffledRooms);

                while (copyOfShuffledRooms.Count > 0 && !foundRoom)
                {
                    bool overlaps = false;
                    List<Vector3> roomPositions = new List<Vector3>();

                    listRoom = copyOfShuffledRooms[0].room;
                    newRoom = Instantiate(listRoom);
                    newDoor = shuffledDoors[0].Item1;

                    if (copyOfShuffledRooms[0].amount != 0)
                    {
                        newRoom.transform.position = newDoor.position;
                        newRoom.transform.rotation = newDoor.rotation;
                        newRoom.transform.SetParent(transform);
                        BoxCollider[] newColliders = newRoom.GetComponents<BoxCollider>();

                        foreach (BoxCollider col in newColliders)
                        {
                            Vector3 localCenter = col.center;
                            roomPositions.Add(SetToResolution(newRoom.transform.TransformPoint(localCenter)));
                        }
                        foreach (Vector3 col in roomPositions)
                        {
                            if (roomsPositions.Contains(col))
                            {
                                overlaps = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        overlaps = true;
                    }

                    if (overlaps)
                    {
                        Destroy(newRoom.gameObject); // Optional: remove overlapping room
                        copyOfShuffledRooms.RemoveAt(0);
                    }
                    else
                    {
                        foundRoom = true;
                        break;
                    }
                }

                if (!foundRoom)
                {
                    shuffledDoors.RemoveAt(0);
                }
            }

            if (foundRoom)
            {
                spawnedRooms.Add(newRoom); // Add to list if no overlap
                unusedDoors.RemoveAll(pair => pair.Item1 == newDoor);

                foreach (var door in newRoom.doors)
                {
                    unusedDoors.Add((door, newRoom.hallChance));
                }

                allDoors.AddRange(newRoom.doors);
                int index = possibleRooms.FindIndex(entry => entry.room == listRoom);
                RoomSpawnEntry entry = possibleRooms[index];
                entry.amount -= 1;
                possibleRooms[index] = entry;

                BoxCollider[] newColliders = newRoom.GetComponents<BoxCollider>();
                foreach (BoxCollider col in newColliders)
                {
                    Vector3 localCenter = col.center;
                    roomsPositions.Add(SetToResolution(newRoom.transform.TransformPoint(localCenter)));
                }

                if (maxNumberOfRooms > spawnedRooms.Count)
                {
                    SpawneRooms(possibleRooms, bonusRoomCount);
                }
                else
                {
                    SpawneRooms(specificRoomsToSpawn, specificRoomsToSpawn.Count);
                }
                stopGeneration = false;
            }
        }
        if(stopGeneration)
        {
            List<int> overlaps = new List<int>();
            for(int i = 0; i < unusedDoors.Count; i++)
            {
                for (int j = 0; j < unusedDoors.Count; j++)
                {
                    if(unusedDoors[i].Item1.position == unusedDoors[j].Item1.position && i!=j)
                    {
                        if(!overlaps.Contains(i))overlaps.Add(i);
                        if(!overlaps.Contains(j))overlaps.Add(j);
                        break;
                    }
                }
            }
            overlaps = overlaps.OrderByDescending(x => x).ToList();
            foreach (int i in overlaps)
            {
                unusedDoors.RemoveAt(i);
            }

            foreach (var door in unusedDoors)
            {
                GameObject cover = Instantiate(wall);
                cover.transform.position = door.Item1.position;
                cover.transform.rotation = door.Item1.rotation;
                cover.transform.SetParent(door.Item1.transform);
            }

            foreach (Room room in spawnedRooms)
            {
                foreach (Transform node in room.nodes)
                {
                    grid.Add(node);
                }
            }
            gridManager.GridReady(grid);
        }
    }

    Vector3 SetToResolution(Vector3 worldCenter)
    {
        float gridSize = 1f;
        float heightSize = 0.5f;
        worldCenter.x = Mathf.Round(worldCenter.x / gridSize) * gridSize;
        worldCenter.y = Mathf.Round(worldCenter.y / heightSize) * heightSize;
        worldCenter.z = Mathf.Round(worldCenter.z / gridSize) * gridSize;

        return worldCenter;

    }
    void OnDrawGizmos()
    {
        if (!debug) return;
        if (unusedDoors == null) return;

        for (int i = 0; i < unusedDoors.Count; i++)
        {
            Transform door = unusedDoors[i].Item1;
            if (door == null) continue;

            Vector3 pos = door.position;

            // Draw a small sphere at the door position
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(pos, 0.2f);

            // Draw the index label
#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(pos + Vector3.up * 0.3f, $"Door {i}\n{pos.ToString("F1")}");
#endif
        }
    }
}