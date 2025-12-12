using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
public class NPCTheBreathingHall : NPCBase
{
    public List<RoomSpawnEntry> possibleRoomsToSpawn;
    public List<Room> spawnedRooms;
    public List<Vector3> roomsPositions;
    public Room containmentUnit;
    public int maxNumberOfRooms;
    //public List<Transform> grid = new List<Transform>();

    [HideInInspector]
    public List<(Doorway, float)> unusedDoors = new List<(Doorway, float)>();
    [HideInInspector]
    public List<Doorway> allDoors;
    bool spawnedFirstRoom = false;
    bool willChange = false;
    Vector3 startingPos;
    Quaternion startingRot;
    Doorway startingDoor;
    public void Start()
    {
        containmentUnit = GetComponentInParent<Room>();
        transform.SetParent(RoomGenerator.i.NPC.transform);
        transform.position = Vector3.zero;
        containmentUnit.transform.SetParent(transform);
        allDoors.Add(containmentUnit.startingDoor.connectedTo);
        unusedDoors.Add((containmentUnit.startingDoor.connectedTo, 100));
        BoxCollider colliders = containmentUnit.GetComponent<BoxCollider>();
        Vector3 resolved = SetToResolution(containmentUnit.transform.TransformPoint(colliders.center));

        RoomGenerator.i.bonusRoomsPositions.Add(resolved);
        startingPos = containmentUnit.transform.position;
        startingRot = containmentUnit.transform.rotation;
        startingDoor = containmentUnit.startingDoor.connectedTo;
        SpawnRooms();
    }
    public override void Update()
    {
        if (containmentUnit.GetComponent<NPCBase>())
        {
            willChange = true;
        }
        if (willChange && !containmentUnit.GetComponent<NPCBase>())
        {
            willChange = false;
            PlaceContainment();
        }
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
    public List<Vector3> ContainmentBorders()
    {
        List<Vector3> containmentBorders = new List<Vector3>();
        BoxCollider[] containmentColliders = containmentUnit.GetComponents<BoxCollider>();

        foreach (BoxCollider col in containmentColliders)
        {
            Vector3 localCenter = col.center;
            containmentBorders.Add(SetToResolution(containmentUnit.transform.TransformPoint(localCenter)));
        }
        return containmentBorders;
    }
    public void SpawnRooms()
    {
        int frameCount = new StackTrace().FrameCount;
        Room listRoom = null;
        Room newRoom = null;
        Doorway newDoor = null;
        bool foundRoom = false;
        List<Room> roomsToUse = spawnedRooms.Count == 0 ? new List<Room> { containmentUnit } : new List<Room>(spawnedRooms);
        List<(Doorway, float)> shuffledDoors = GetDoors(roomsToUse[roomsToUse.Count - 1]).OrderBy(x => Guid.NewGuid()).ToList();

        List<RoomSpawnEntry> shuffledRooms = possibleRoomsToSpawn.OrderBy(x => Guid.NewGuid()).ToList();
        if (spawnedFirstRoom && roomsToUse.Count == 1)
        {
            shuffledDoors.Clear();
            shuffledDoors.Add((roomsToUse[0].startingDoor, 100));
            print(100);
        }
        else
        if (roomsToUse.Contains(containmentUnit))
        {

            shuffledDoors.Clear();
            shuffledDoors.Add((roomsToUse[0].startingDoor.connectedTo, 100));
            containmentUnit.startingDoor.Disconnect();
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

                    if (spawnedFirstRoom && roomsToUse.Count == 1)
                    {
                        newRoom.transform.rotation = newDoor.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
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

                    List<Vector3> allRoomsPositions = new List<Vector3>(RoomGenerator.i.AllOccupiedSpaces(ContainmentBorders()));
                    allRoomsPositions.AddRange(roomsPositions);

                    foreach (Vector3 col in roomBorders)
                    {

                        if (allRoomsPositions.Contains(col))
                        {
                            overlaps = true;
                            break;
                        }

                        near = near || RoomGenerator.i.AllOccupiedSpaces(roomsPositions).Any(roomPos =>
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
                    StartCoroutine(Warping(true));
                    return;
                }
            }
        }

        if (foundRoom)
        {
            if (containmentUnit.startingDoor.connectedTo)
                containmentUnit.startingDoor.Disconnect();

            if (spawnedRooms.Count == 1)
                spawnedFirstRoom = true;

            spawnedRooms.Add(newRoom); 
            unusedDoors.RemoveAll(pair => pair.Item1 == newDoor);
            newDoor.ConnectTo(newRoom.startingDoor);
            newDoor.ForceFillBoth(true, false);
            List<(Doorway, float)> allUnusedDoors = new List<(Doorway, float)>(RoomGenerator.i.unusedDoors);
            allUnusedDoors.AddRange(unusedDoors);
            foreach (var door in newRoom.doors)
            {
                bool isConnected = false;
                foreach (var maybeDoorConnected in allUnusedDoors)
                {
                    if ((Vector3.Distance(door.transform.position, maybeDoorConnected.Item1.transform.position) < .1f && door != maybeDoorConnected.Item1) && !containmentUnit.doors.Contains(maybeDoorConnected.Item1))
                    {
                        isConnected = true;
                        door.ConnectTo(maybeDoorConnected.Item1);
                        bool isHall1 = door.GetComponentInParent<Room>().hall;
                        bool isHall2 = door.connectedTo.GetComponentInParent<Room>().hall;
                        door.ForceFillBoth(isHall1 && isHall2,false);
                    }
                }
                if (!isConnected)
                { 
                    unusedDoors.Add((door, door.hallChance));
                    door.Fill(false);
                }

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
                Vector3 resolved = SetToResolution(newRoom.transform.TransformPoint(localCenter));

                roomsPositions.Add(resolved);
                RoomGenerator.i.bonusRoomsPositions.Add(resolved);
            }

            foreach (BoxCollider col in containmentUnit.GetComponents<BoxCollider>())
            {
                Vector3 localCenter = col.center;
                Vector3 resolved = SetToResolution(newRoom.transform.TransformPoint(localCenter));
                RoomGenerator.i.bonusRoomsPositions.Remove(resolved);
            }

            newRoom.surface.BuildNavMesh();

            StartCoroutine(Warping());
        }
    }
    public void PlaceContainment()
    {
        if (containmentUnit.startingDoor.GetComponentInChildren<TheBreeze>().isVisible)
            return;

        
        foreach (Doorway door in containmentUnit.doors)
        {
            if (door.GetComponentInChildren<TheBreeze>().isVisible)
                return;
        }

        if (containmentUnit.startingDoor.connectedTo)
            containmentUnit.startingDoor.Disconnect();

        foreach (Doorway door in containmentUnit.doors)
        {
            if (door.connectedTo)
                door.Disconnect();
        }

        Vector3 sparePos = containmentUnit.transform.position;
        Quaternion spareRot = containmentUnit.transform.rotation;

        List<(Doorway, float)> shuffledDoorsContainment = unusedDoors.OrderBy(_ => Guid.NewGuid()).ToList();

        foreach ((Doorway doorway, float weight) in shuffledDoorsContainment)
        {
            bool overlaps = false;
            containmentUnit.transform.position = doorway.transform.position;
            containmentUnit.transform.rotation = doorway.transform.rotation;

            BoxCollider newCollider = containmentUnit.GetComponent<BoxCollider>();

            Vector3 localCenter = newCollider.center;
            localCenter = SetToResolution(containmentUnit.transform.TransformPoint(localCenter));

            List<Vector3> allRoomsPositions = new List<Vector3>(RoomGenerator.i.AllOccupiedSpaces());
            allRoomsPositions.AddRange(roomsPositions);

            if (allRoomsPositions.Contains(localCenter))
            {
                overlaps = true;
            }

            if(doorway.GetComponentInChildren<TheBreeze>().isVisible)
            {
                overlaps = true;
            }
            if (!overlaps)
            {
                doorway.ConnectTo(containmentUnit.startingDoor);
                bool isHall2 = doorway.connectedTo.GetComponentInParent<Room>().hall;
                doorway.connectedTo.ForceFillBoth(doorway && isHall2, false);

                List<(Doorway, float)> contAllUnusedDoors = new List<(Doorway, float)>(RoomGenerator.i.unusedDoors);
                contAllUnusedDoors.AddRange(unusedDoors);
                foreach (var door in containmentUnit.doors)
                {
                    foreach (var maybeDoorConnected in unusedDoors)
                    {
                        if (Vector3.Distance(door.transform.position, maybeDoorConnected.Item1.transform.position) < .1f && door != maybeDoorConnected.Item1)
                        {
                            door.ConnectTo(maybeDoorConnected.Item1);
                            bool isHall1 = door.connectedTo.GetComponentInParent<Room>().hall;
                            door.connectedTo.ForceFillBoth(door && isHall1, false);

                            break;
                        }
                    }
                }
                break;
            }
        }
    }

    private IEnumerator Warping(bool stuck = false)
    {
        if (contained != ContainedState.Contained)
        {
            PlaceContainment();
        }
        yield return new WaitForSeconds(.1f);

        if (maxNumberOfRooms > spawnedRooms.Count)
            yield return new WaitForSeconds(1);
        else
            yield return new WaitForSeconds(1);
        while (true)
        {
            while (contained == ContainedState.Contained || containmentUnit.GetComponent<IsInsideTheRoom>().isPlayerInside)
            {
                yield return null;
            }

            if (CanMove())
            {
                if ((maxNumberOfRooms <= spawnedRooms.Count || stuck) && spawnedRooms.Count > 1)
                {
                    DestroyLastRoom();
                }

                SpawnRooms();
                break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
    public void DestroyLastRoom()
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
            Vector3 resolved = SetToResolution(roomToRemove.transform.TransformPoint(localCenter));
            roomsPositions.Remove(resolved);
            RoomGenerator.i.bonusRoomsPositions.Remove(resolved);
        }
        foreach (var door in roomToRemove.doors)
        {
            unusedDoors.RemoveAll(pair => pair.Item1 == door);
            allDoors.Remove(door);
        }

        Destroy(roomToRemove.gameObject);

    }
    public bool CanMove()
    {
        if(spawnedRooms.Count == 0)
        {
            if (containmentUnit.GetComponent<IsInsideTheRoom>().isPlayerInside || containmentUnit.startingDoor.GetComponentInChildren<TheBreeze>().isVisible)
                return false;
            else
                return true;
        }
        if (spawnedRooms[spawnedRooms.Count - 1].GetComponent<IsInsideTheRoom>().isPlayerInside || spawnedRooms[0].GetComponent<IsInsideTheRoom>().isPlayerInside || containmentUnit.GetComponent<IsInsideTheRoom>().isPlayerInside)
            return false;
        if (spawnedRooms[spawnedRooms.Count - 1].startingDoor.GetComponentInChildren<TheBreeze>().isVisible || spawnedRooms[0].startingDoor.GetComponentInChildren<TheBreeze>().isVisible)
            return false;
        foreach (Doorway door in spawnedRooms[0].doors)
        {
            if (door.GetComponentInChildren<TheBreeze>().isVisible)
                return false;
        }
        foreach (Doorway door in spawnedRooms[spawnedRooms.Count - 1].doors)
        {
            if (door.GetComponentInChildren<TheBreeze>().isVisible)
                return false;
        }
        return true;
    }
    public override void SetContained()
    {
        base.SetContained();
        if (contained == ContainedState.Contained)
        {
            spawnedFirstRoom = false;
            foreach (Room room in spawnedRooms)
            {
                if (room != null)
                {
                    foreach (Transform player in room.GetComponent<IsInsideTheRoom>().players)
                    {
                        player.GetComponent<MovementSystem>().Block();
                        player.transform.position = startingDoor.transform.position + new Vector3(0, 1, 0);
                        StartCoroutine(FreePlayer(player));

                    }

                    Destroy(room.gameObject);
                }
            }
            foreach (Room room in spawnedRooms)
            {
                foreach (Doorway door in room.doors)
                {
                    door.ForceFillBoth(true, true);
                }
                room.startingDoor.ForceFillBoth(true, true);
            }
            allDoors.Clear();
            unusedDoors.Clear();
            spawnedRooms.Clear();
            roomsPositions.Clear();

            Transform button = GetComponentInChildren<TheBreathingButton>().transform;

            Dictionary<Transform, Vector3> playerOffsets = new Dictionary<Transform, Vector3>();
            foreach (Transform player in containmentUnit.GetComponent<IsInsideTheRoom>().players)
            {
                Vector3 offset = player.position - button.position;
                playerOffsets[player] = offset;
            }

            containmentUnit.transform.position = startingPos;
            containmentUnit.transform.rotation = startingRot;

            foreach (Transform player in containmentUnit.GetComponent<IsInsideTheRoom>().players)
            {
                player.GetComponent<MovementSystem>().Block();

                Vector3 newButtonPos = button.position;
                player.position = newButtonPos + playerOffsets[player];

                StartCoroutine(FreePlayer(player));
            }


            containmentUnit.startingDoor.ConnectTo(startingDoor);
            containmentUnit.startingDoor.ForceFillBoth(false, false);
            List<(Doorway, float)> allUnusedDoors = new List<(Doorway, float)>(RoomGenerator.i.unusedDoors);

            foreach (var door in containmentUnit.doors)
            {
                bool isConnected = false;
                foreach (var maybeDoorConnected in allUnusedDoors)
                {
                    if ((Vector3.Distance(door.transform.position, maybeDoorConnected.Item1.transform.position) < .1f && door != maybeDoorConnected.Item1))
                    {
                        isConnected = true;
                        door.ConnectTo(maybeDoorConnected.Item1);
                    }
                }
                if (!isConnected)
                    unusedDoors.Add((door, door.hallChance));
            }

        }
    }
    IEnumerator FreePlayer(Transform player)
    {
        yield return new WaitForSeconds(.1f);
        player.GetComponent<MovementSystem>().Block();
    }
}