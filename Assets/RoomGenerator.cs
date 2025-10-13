using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;
public class RoomGenerator : MonoBehaviour
{
    public List<Room> possibleRoomsToSpawn;
    public List<Room> spawnedRooms;
    public int maxNumberOfRooms;
    [HideInInspector]
    public List<Vector3> roomsPositions;
    [HideInInspector]
    public List<Transform> allDoors;
    [HideInInspector]
    public List<Transform> unusedDoors;
    public GameObject wall;
    public List<Transform> grid;
    public bool debug;
    private void Awake()
    {
        allDoors.AddRange(spawnedRooms[0].doors);
        unusedDoors.AddRange(spawnedRooms[0].doors);
        BoxCollider[] colliders = spawnedRooms[0].GetComponents<BoxCollider>();

        foreach (BoxCollider col in colliders)
        {
            Vector3 localCenter = col.center;
            roomsPositions.Add(SetToResolution(spawnedRooms[0].transform.TransformPoint(localCenter)));
        }
        SpawneRooms();
    }
    public void SpawneRooms()
    {
        bool stopGeneration = true;
        if (maxNumberOfRooms > spawnedRooms.Count)
        {
            List<Transform> shuffledDoors = unusedDoors.OrderBy(x => Guid.NewGuid()).ToList();
            List<Room> shuffledRooms = possibleRoomsToSpawn.OrderBy(x => Guid.NewGuid()).ToList();
            Room newRoom = null;
            Transform newDoor = null;
            bool foundRoom = false;

            while (shuffledDoors.Count > 0 && !foundRoom)
            {
                List<Room> copyOfShuffledRooms = new List<Room>(shuffledRooms);

                while (copyOfShuffledRooms.Count > 0 && !foundRoom)
                {
                    List<Vector3> roomPositions = new List<Vector3>();
                    bool overlaps = false;

                    newRoom = Instantiate(copyOfShuffledRooms[0]);
                    newDoor = shuffledDoors[0];
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
                unusedDoors.Remove(newDoor);
                unusedDoors.AddRange(newRoom.doors);
                allDoors.AddRange(newRoom.doors);

                BoxCollider[] newColliders = newRoom.GetComponents<BoxCollider>();
                foreach (BoxCollider col in newColliders)
                {
                    Vector3 localCenter = col.center;
                    roomsPositions.Add(SetToResolution(newRoom.transform.TransformPoint(localCenter)));
                }

                SpawneRooms();
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
                    if(unusedDoors[i].position == unusedDoors[j].position && i!=j)
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

            foreach (Transform door in unusedDoors)
            {
                GameObject cover = Instantiate(wall);
                cover.transform.position = door.position;
                cover.transform.rotation = door.rotation;
                cover.transform.SetParent(door.transform);
            }

            foreach (Room room in spawnedRooms)
            {
                foreach (Transform node in room.nodes)
                {
                    grid.Add(node);
                }
            }
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
            Transform door = unusedDoors[i];
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