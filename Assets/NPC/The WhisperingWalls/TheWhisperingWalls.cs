using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
public class TheWhisperingWalls : MonoBehaviour
{
    public List<RoomSpawnEntry> possibleRoomsToSpawn;
    public List<Room> spawnedRooms;
    public Room startRoom;
    public int maxNumberOfRooms;
    public List<Vector3> roomsPositions;
    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 startScale;
    //public List<Transform> grid = new List<Transform>();
    [HideInInspector]
    public List<(Doorway, float)> unusedDoors = new List<(Doorway, float)>();

    [HideInInspector]
    public List<Doorway> allDoors;

    public void Start()
    {
        startRoom = GetComponentInParent<Room>();
        transform.SetParent(RoomGenerator.i.NPC.transform);
        transform.position = Vector3.zero;
        startRoom.transform.SetParent(transform);
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;

        InitialStart();
    }
    public void InitialStart()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;
        StartGenerating();
    }
    public void StartGenerating()
    {
        roomsPositions.Clear();
        unusedDoors.Clear();
        allDoors.Clear();
        Room roomZero = spawnedRooms[0];
        spawnedRooms.Remove(spawnedRooms[0]);
        foreach (Room room in spawnedRooms)
        {
            Destroy(room.gameObject);
        }
        spawnedRooms.Clear();

        spawnedRooms.Add(roomZero);
        BoxCollider[] colliders = roomZero.GetComponents<BoxCollider>();

        Vector3 localCenter = colliders[0].center;
        roomsPositions.Add(SetToResolution(roomZero.transform.TransformPoint(localCenter)));

        allDoors.AddRange(startRoom.doors);
        unusedDoors.AddRange(GetDoors(startRoom));

        SpawnRooms();
    }

    public List<(Doorway, float)> GetDoors(Room room)
    {
        List<(Doorway, float)> unusedDoors = new List<(Doorway, float)>();

        foreach (var door in room.doors)
        {
            unusedDoors.Add((door, door.hallChance));
        }
        return unusedDoors;

    }
    public void SpawnRooms(bool alreadyReversed = false)
    {
        int frameCount = new StackTrace().FrameCount;
        bool stopGeneration = true;
        if (maxNumberOfRooms > spawnedRooms.Count)
        {
            Room listRoom = null;
            Room newRoom = null;
            Doorway newDoor = null;
            bool foundRoom = false;
            stopGeneration = false;

            List<(Doorway, float)> shuffledDoors = GetDoors(spawnedRooms[spawnedRooms.Count - 1]).OrderBy(x => Guid.NewGuid()).ToList();
            
            List<RoomSpawnEntry> roomsHall = possibleRoomsToSpawn.Where(entry => entry.room.hall).OrderBy(x => Guid.NewGuid()).ToList();
            List<RoomSpawnEntry> roomsNoHall = possibleRoomsToSpawn.Where(entry => !entry.room.hall).OrderBy(x => Guid.NewGuid()).ToList();
            List<RoomSpawnEntry> shuffledRooms = possibleRoomsToSpawn.OrderBy(x => Guid.NewGuid()).ToList();

            if (!alreadyReversed)
            {

                float threshold = shuffledDoors[0].Item2; // value between 0 and 100
                float roll = UnityEngine.Random.Range(0f, 100f);

                if ((roll <= threshold && roomsHall.Count > 0) || roomsNoHall.Count == 0)
                {
                    shuffledRooms = roomsHall;
                }
                else
                {
                    shuffledRooms = roomsNoHall;
                }
            }
            else
            {
                shuffledDoors.Clear();
                shuffledDoors.Add((spawnedRooms[spawnedRooms.Count - 1].startingDoor,100));
            }

            while (shuffledDoors.Count > 0 && !foundRoom)
            {
                List<RoomSpawnEntry> copyOfShuffledRooms = new List<RoomSpawnEntry>(shuffledRooms);
                bool overlaps = false;
                bool near = false;
                while (copyOfShuffledRooms.Count > 0 && !foundRoom)
                {
                    overlaps = false;
                    near = false;
                   
                    List<Vector3> roomBorders = new List<Vector3>();

                    listRoom = copyOfShuffledRooms[0].room;

                    newRoom = Instantiate(listRoom);
                    newDoor = shuffledDoors[0].Item1;

                    if (copyOfShuffledRooms[0].amount != 0)
                    {
                        newRoom.transform.position = newDoor.transform.position;
                        if (alreadyReversed)
                        {
                            newRoom.transform.rotation = newDoor.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
                            alreadyReversed=false;
                        }
                        else
                        {
                            newRoom.transform.rotation = newDoor.transform.rotation;
                        }
                        newRoom.transform.SetParent(transform);
                        BoxCollider[] newColliders = newRoom.GetComponents<BoxCollider>();

                        foreach (BoxCollider col in newColliders)
                        {
                            Vector3 localCenter = col.center;
                            roomBorders.Add(SetToResolution(newRoom.transform.TransformPoint(localCenter)));
                        }

                        List<Vector3> allRoomsPositions = new List<Vector3>(RoomGenerator.i.roomsPositions);
                        allRoomsPositions.AddRange(roomsPositions);
                        foreach (Vector3 col in roomBorders)
                        {

                            if (allRoomsPositions.Contains(col))
                            {
                                overlaps = true;
                                break;
                            }

                            near = near || RoomGenerator.i.roomsPositions.Any(roomPos =>
                                Mathf.Abs(col.x - roomPos.x) <= 2 &&
                                Mathf.Abs(col.y - roomPos.y) <= 2 &&
                                Mathf.Abs(col.z - roomPos.z) <= 2);
                        }
                    }
                    else
                    {
                        overlaps = true;
                    }

                    if (overlaps || !near)
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
                    if (shuffledDoors.Count == 0)
                    {
                        if (!alreadyReversed)
                        {
                            spawnedRooms.Reverse();
                            SpawnRooms(true);
                        }
                        return;
                    }
                }
            }

            if (foundRoom)
            {
                spawnedRooms.Add(newRoom); // Add to list if no overlap
                unusedDoors.RemoveAll(pair => pair.Item1 == newDoor);
                newDoor.ConnectTo(newRoom.startingDoor);

                List<(Doorway, float)> alUnusedDoors = new List<(Doorway, float)>(RoomGenerator.i.unusedDoors);
                alUnusedDoors.AddRange(unusedDoors);
                foreach (var door in newRoom.doors)
                {
                    bool isConnected = false;
                    foreach (var maybeDoorConnected in alUnusedDoors)
                    {
                        if (Vector3.Distance(door.transform.position, maybeDoorConnected.Item1.transform.position) < .1f && door != maybeDoorConnected.Item1)
                        {
                            isConnected = true;
                            door.ConnectTo(maybeDoorConnected.Item1);
                        }
                    }
                    if (!isConnected)
                        unusedDoors.Add((door, door.hallChance));
                }

                allDoors.AddRange(newRoom.doors);

                int index = possibleRoomsToSpawn.FindIndex(entry => entry.room == listRoom);
                RoomSpawnEntry entry = possibleRoomsToSpawn[index];
                entry.amount -= 1;
                possibleRoomsToSpawn[index] = entry;

                BoxCollider[] newColliders = newRoom.GetComponents<BoxCollider>();
                foreach (BoxCollider col in newColliders)
                {
                    Vector3 localCenter = col.center;
                    roomsPositions.Add(SetToResolution(newRoom.transform.TransformPoint(localCenter)));
                }
                SpawnRooms();
            }
        }

        if (stopGeneration)
        {

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

            //foreach (Room room in spawnedRooms)
            //{
            //    foreach (Transform node in room.nodes)
            //    {
            //        grid.Add(node);
            //    }
            //}

            StartCoroutine(Warping());
        }
    }


    Vector3 SetToResolution(Vector3 worldCenter)
    {
        worldCenter.x = Mathf.RoundToInt(worldCenter.x);
        worldCenter.y = Mathf.RoundToInt(worldCenter.y);
        worldCenter.z = Mathf.RoundToInt(worldCenter.z);
        return worldCenter;
    }

    private IEnumerator Warping()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            if (CanMove() && spawnedRooms.Count != 0)
            {
                Room roomToRemove = spawnedRooms[0];
                if (roomToRemove.startingDoor.connectedTo)
                    roomToRemove.startingDoor.Disconnect();

                foreach (Doorway door in roomToRemove.doors)
                {
                    if (door.connectedTo)
                        door.Disconnect();
                }
                spawnedRooms.RemoveAt(0);
                BoxCollider[] colliders = roomToRemove.GetComponents<BoxCollider>();
                List<Vector3> roomBorders = new List<Vector3>();

                foreach (var box in colliders)
                {
                    Vector3 localCenter = box.center;
                    roomBorders.Add(SetToResolution(roomToRemove.transform.TransformPoint(localCenter)));
                }

                foreach (Vector3 col in roomBorders)
                {
                    roomsPositions.Remove(col);
                }

                foreach (var door in roomToRemove.doors)
                {
                    unusedDoors.RemoveAll(pair => pair.Item1 == door);
                    allDoors.Remove(door);
                }

                Destroy(roomToRemove.gameObject);
                SpawnRooms();
                break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
    public bool CanMove()
    {
        if (spawnedRooms[spawnedRooms.Count - 1].GetComponent<IsInsideTheWalls>().isPlayerInside || spawnedRooms[0].GetComponent<IsInsideTheWalls>().isPlayerInside)
            return false;
        if (spawnedRooms[spawnedRooms.Count - 1].startingDoor.GetComponent<TheWhisper>().IsVisibleToPlayer()|| spawnedRooms[0].startingDoor.GetComponent<TheWhisper>().IsVisibleToPlayer())
            return false;

        foreach (Doorway door in spawnedRooms[0].doors)
        {
            if (door.GetComponent<TheWhisper>().IsVisibleToPlayer())
                return false;
        }
        foreach (Doorway door in spawnedRooms[spawnedRooms.Count - 1].doors)
        {
            if (door.GetComponent<TheWhisper>().IsVisibleToPlayer())
                return false;
        }
        return true;
    }
}