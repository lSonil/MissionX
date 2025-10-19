using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Doorway> doors;
    public Doorway startingDoor;
    public List<Transform> nodes;
    public bool hall;
}

[System.Serializable]
public struct RoomSpawnEntry
{
    public Room room;
    public int amount;
    private Room placeStart;
    private int v;

    public RoomSpawnEntry(Room placeStart, int v) : this()
    {
        this.placeStart = placeStart;
        this.v = v;
    }
}