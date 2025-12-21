using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomGenerator : MonoBehaviour
{
    public List<RoomSpawnEntry> possibleRoomsToSpawn;
    public List<RoomSpawnEntry> specificRoomsToSpawn;
    public List<Room> spawnedRooms;
    [HideInInspector]
    public List<Vector3> roomsPositions = new List<Vector3>();
    [HideInInspector]
    public List<Vector3> bonusRoomsPositions = new List<Vector3>();
    [HideInInspector]
    public List<Doorway> allDoors;
    public GameObject NPC;

    public List<(Doorway, float)> unusedDoors = new List<(Doorway, float)>();
    [HideInInspector]
    public List<Transform> grid = new List<Transform>();
    public bool debug;
    public GridManager gridManager;
    public ItemManager itemManager;
    public List<NPCBase> list;
    public static RoomGenerator i;
    int bonusToCheck;
    private void Awake()
    {
        i = this;
    }
    private void Start()
    {
        if (debug)
        {
            bonusToCheck = 1;
        }
        else
        {
            bonusToCheck = SceneData.missionToTransfer.monsters.Count;
            foreach (NPCEntry roomToAdd in SceneData.missionToTransfer.monsters)
            {
                specificRoomsToSpawn.Add(new RoomSpawnEntry(roomToAdd.room, 1));
            }
        }

        allDoors.AddRange(spawnedRooms[0].doors);
        foreach (var door in spawnedRooms[0].doors)
        {
            unusedDoors.Add((door, door.hallChance));
        }
        BoxCollider[] colliders = spawnedRooms[0].GetComponents<BoxCollider>();

        foreach (BoxCollider col in colliders)
        {
            Vector3 localCenter = col.center;
            roomsPositions.Add(SetToResolution(spawnedRooms[0].transform.TransformPoint(localCenter)));
        
            Destroy(col);
        }
        SpawnRooms(possibleRoomsToSpawn, 0);
    }
    public void ClearNPC()
    {
        foreach (Transform child in NPC.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void ClearRooms()
    {
        foreach (Transform child in transform.Cast<Transform>().Skip(1))
        {
            Destroy(child.gameObject);
        }
    }
    public void SpawnRooms(List<RoomSpawnEntry> possibleRooms, int bonusRoomCount)
    {
        bool stopGeneration = true;
        if (SceneData.GetNumberOfRooms() + bonusRoomCount > spawnedRooms.Count)
        {
            Room newRoom = null;
            Room listRoom = null;
            Doorway newDoor = null;
            bool foundRoom = false;

            List<(Room, Doorway, int)> posibleRoomsToAdd = new List<(Room, Doorway, int)>();
            List<(Doorway, float)> shuffledDoors = unusedDoors.OrderBy(x => Guid.NewGuid()).ToList();
            List<RoomSpawnEntry> shuffledRooms = possibleRooms.OrderBy(x => Guid.NewGuid()).ToList();
            List<RoomSpawnEntry> roomsHall = possibleRooms.Where(entry => entry.room.hall).OrderBy(x => Guid.NewGuid()).ToList();
            List<RoomSpawnEntry> roomsNoHall = possibleRooms.Where(entry => !entry.room.hall).OrderBy(x => Guid.NewGuid()).ToList();

            float roll = UnityEngine.Random.Range(0f, 100f);
            float threshold = shuffledDoors[0].Item2; // value between 0 and 100

            if (bonusRoomCount == 0)
            {
                if ((roll <= threshold && roomsHall.Count > 0) || roomsNoHall.Count == 0 || shuffledDoors.Count == 1)
                {
                    shuffledRooms = roomsHall;
                }
                else
                {
                    shuffledRooms = roomsNoHall;
                }
            }

            while (shuffledDoors.Count > 0)
            {
                List<RoomSpawnEntry> copyOfShuffledRooms = new List<RoomSpawnEntry>(shuffledRooms);

                while (copyOfShuffledRooms.Count > 0 && !foundRoom)
                {
                    int numberOfDoors = 0;
                    bool overlaps = false;
                    List<Vector3> roomBorders = new List<Vector3>();

                    listRoom = copyOfShuffledRooms[0].room;
                    newRoom = Instantiate(listRoom);
                    newDoor = shuffledDoors[0].Item1;

                    newRoom.PrepareDoors();

                    if (copyOfShuffledRooms[0].amount != 0)
                    {
                        newRoom.transform.position = newDoor.transform.position;
                        newRoom.transform.rotation = newDoor.transform.rotation;
                        newRoom.transform.SetParent(bonusRoomCount == 0 ? transform : NPC.transform);
                        BoxCollider[] newColliders = newRoom.GetComponents<BoxCollider>();

                        foreach (BoxCollider col in newColliders)
                        {
                            Vector3 localCenter = col.center;
                            roomBorders.Add(SetToResolution(newRoom.transform.TransformPoint(localCenter)));
                        }
                        foreach (Vector3 col in roomBorders)
                        {
                            if (roomsPositions.Contains(col))
                            {
                                overlaps = true;
                                break;
                            }
                        }

                        foreach (var door in newRoom.doors)
                        {
                            bool isConnected = false;
                            foreach (var maybeDoorConnected in unusedDoors)
                            {
                                if (door.transform.position == maybeDoorConnected.Item1.transform.position)
                                {
                                    isConnected = true;
                                }
                            }
                            if (isConnected)
                                numberOfDoors++;
                        }
                    }
                    else
                    {
                        overlaps = true;
                    }

                    if (!overlaps)
                    {
                        posibleRoomsToAdd.Add((listRoom, newDoor, numberOfDoors));
                        foundRoom = true;
                    }
                    Destroy(newRoom.gameObject); // Optional: remove overlapping room
                    copyOfShuffledRooms.RemoveAt(0);
                }
                shuffledDoors.RemoveAt(0);
            }

            if (foundRoom)
            {
                int maxValue = posibleRoomsToAdd.Max(x => x.Item3);
                var highestRooms = posibleRoomsToAdd.Where(x => x.Item3 == maxValue).ToList();
                var chosen = highestRooms[UnityEngine.Random.Range(0, highestRooms.Count)];
                listRoom = chosen.Item1;
                newDoor = chosen.Item2;

                newRoom = Instantiate(listRoom);
                newRoom.CollectSpawnPoints();
                newRoom.PrepareLayout();
                newRoom.transform.position = newDoor.transform.position;
                newRoom.transform.rotation = newDoor.transform.rotation;
                newRoom.transform.SetParent(bonusRoomCount == 0 ? transform : NPC.transform);
                spawnedRooms.Add(newRoom); // Add to list if no overlap
                unusedDoors.RemoveAll(pair => pair.Item1 == newDoor);
                newDoor.ConnectTo(newRoom.startingDoor);

                foreach (var door in newRoom.doors)
                {
                    door.connectedTo = null;

                    bool isConnected = false;
                    (Doorway, float) element = new(null, 0);
                    foreach (var maybeDoorConnected in unusedDoors)
                    {
                        if (door.transform.position == maybeDoorConnected.Item1.transform.position)
                        {
                            element = maybeDoorConnected;
                            isConnected = true;
                            door.ConnectTo(maybeDoorConnected.Item1);
                        }
                    }
                    if (!isConnected)
                        unusedDoors.Add((door, door.hallChance));
                    else
                        unusedDoors.Remove(element);
                }

                allDoors.AddRange(newRoom.doors);

                int index = possibleRooms.FindIndex(entry => entry.room == listRoom);
                RoomSpawnEntry entry = possibleRooms[index];
                entry.amount -= 1;
                if (entry.amount == 0)
                    possibleRooms.RemoveAt(index);
                else
                    possibleRooms[index] = entry;

                BoxCollider[] newColliders = newRoom.GetComponents<BoxCollider>();
                foreach (BoxCollider col in newColliders)
                {
                    Vector3 localCenter = col.center;
                    roomsPositions.Add(SetToResolution(newRoom.transform.TransformPoint(localCenter)));
                }

                if (newRoom.GetComponent<ContainmentUnit>() != null)
                {
                    list.Add(newRoom.GetComponent<ContainmentUnit>().npc);
                }
                newRoom.surface.BuildNavMesh();

                if (SceneData.GetNumberOfRooms() + bonusRoomCount > spawnedRooms.Count)
                {
                    SpawnRooms(possibleRooms, bonusRoomCount);
                }
                else
                {
                    SpawnRooms(specificRoomsToSpawn, specificRoomsToSpawn.Count);
                    return;
                }
                stopGeneration = false;
            }
        }
        if (stopGeneration)
        {
            if (SceneData.GetNumberOfRooms() + bonusToCheck != spawnedRooms.Count)
            {

                print(SceneData.GetNumberOfRooms() + bonusToCheck);
                print(spawnedRooms.Count);
                SceneManager.LoadScene("Mission");
            }

            foreach (Doorway door in allDoors)
            {
                bool isHall1 = door.GetComponentInParent<Room>().hall;
                bool isHall2 = false;
                if (door.connectedTo)
                {
                    isHall2 = door.connectedTo.GetComponentInParent<Room>().hall;
                    door.connectedTo.Fill(isHall1 && isHall2);
                }
                door.Fill(isHall1 && isHall2);
            }

            foreach (Room room in spawnedRooms)
            {
                foreach (Transform node in room.nodes)
                {
                    grid.Add(node);
                }
            }
            if(!debug)
            
            SceneData.npcInScene = list;
            
            gridManager.GridReady(grid);
            itemManager.ItemReady(spawnedRooms);
        }
    }
    public Transform FindClosestRoom()
    {
        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (Room room in spawnedRooms)
        {
            if (room == null) continue;
            float dist = Vector3.Distance(transform.position, room.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = room.transform;
            }
        }

        return closest != null ? closest : transform; // Fallback to self
    }
    public Doorway GetClosestUnusedDoor(Transform target)
    {
        Doorway closestDoor = null;
        float closestDistance = float.MaxValue;

        foreach (var (door, _) in unusedDoors)
        {
            float distance = Vector3.Distance(target.position, door.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestDoor = door;
            }
        }

        return closestDoor;
    }
    public List<Vector3> AllOccupiedSpaces(List<Vector3> roomsToExclude = null)
    {
        List<Vector3> allRoomsPositions = new List<Vector3>(roomsPositions);
        allRoomsPositions.AddRange(bonusRoomsPositions);

        if (roomsToExclude != null)
        {
            allRoomsPositions.RemoveAll(pos => roomsToExclude.Contains(pos));
        }

        return allRoomsPositions;
    }
    public void RemoveRoomPosition(Vector3 positionToRemove)
    {
        roomsPositions.RemoveAll(pos => pos == positionToRemove);
        bonusRoomsPositions.RemoveAll(pos => pos == positionToRemove);
    }

    Vector3 SetToResolution(Vector3 worldCenter)
    {
        worldCenter.x = Mathf.RoundToInt(worldCenter.x);
        worldCenter.y = Mathf.RoundToInt(worldCenter.y);
        worldCenter.z = Mathf.RoundToInt(worldCenter.z);
        return worldCenter;
    }
    void OnDrawGizmos()
    {
        if (!debug) return;
        if (unusedDoors == null) return;

        for (int i = 0; i < unusedDoors.Count; i++)
        {
            Transform door = unusedDoors[i].Item1.transform;
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

[System.Serializable]
public struct RoomSpawnEntry
{
    public Room room;
    public int amount;

    public RoomSpawnEntry(Room placeStart, int v) : this()
    {
        this.room = placeStart;
        this.amount = v;
    }
}