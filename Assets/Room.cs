using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Transform> doors;
    public List<Transform> nodes;
    public float hallChance;
    public bool hall;
}

[System.Serializable]
public struct RoomSpawnEntry
{
    public Room room;
    public int amount;
}

