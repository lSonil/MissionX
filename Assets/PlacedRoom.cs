using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlacedRoom
{
    public RoomTemplate template;
    public Vector3 position;
    public Quaternion rotation;
    public Vector2Int size;
    public RectInt gridRect;
    public Dictionary<int, List<Vector3>> activeDoorsByFloor = new Dictionary<int, List<Vector3>>();
    public int floor;

    public PlacedRoom(RoomTemplate template, Vector3 position, Quaternion rotation, Vector2Int size, float tileSize, int gridWidth, int gridHeight, int floor = 0)
    {
        this.template = template;
        this.position = position;
        this.rotation = rotation;
        this.size = size;
        this.floor = floor;

        int x = Mathf.RoundToInt((position.x + gridWidth * tileSize * 0.5f) / tileSize - size.x / 2f);
        int y = Mathf.RoundToInt((position.z + gridHeight * tileSize * 0.5f) / tileSize - size.y / 2f);
        this.gridRect = new RectInt(x, y, size.x, size.y);

        GenerateActiveDoors();
    }

    void GenerateActiveDoors()
    {
        activeDoorsByFloor.Clear();
        if (template.doors == null || template.doors.Count == 0) return;

        Dictionary<int, List<Vector3>> temp = new();
        Dictionary<int, List<Vector3>> allDoorsByFloor = new();

        int maxY = int.MinValue;
        int minY = int.MaxValue;

        // Group all doors by floor
        foreach (var door in template.doors)
        {
            Vector3 worldPos = position + rotation * door.localPosition;
            int floorY = Mathf.RoundToInt(worldPos.y / 3f) * 3;

            maxY = Mathf.Max(maxY, floorY);
            minY = Mathf.Min(minY, floorY);

            if (!allDoorsByFloor.ContainsKey(floorY))
                allDoorsByFloor[floorY] = new List<Vector3>();

            allDoorsByFloor[floorY].Add(worldPos);
        }

        // Activate one random door per floor, then randomly activate others
        foreach (var kvp in allDoorsByFloor)
        {
            int floorY = kvp.Key;
            List<Vector3> allDoors = kvp.Value;
            List<Vector3> activeDoors = new();

            if (allDoors.Count > 0)
            {
                // Always activate one random door
                Vector3 guaranteed = allDoors[Random.Range(0, allDoors.Count)];
                activeDoors.Add(guaranteed);

                // Randomly activate others
                foreach (var door in allDoors)
                {
                    if (door == guaranteed) continue;
                    if (Random.value < 0.5f)
                        activeDoors.Add(door);
                }
            }

            temp[floorY] = activeDoors;
        }

        // Fill in empty floors with empty lists
        for (int y = minY; y <= maxY; y += 3)
        {
            if (!temp.ContainsKey(y))
                temp[y] = new List<Vector3>();
        }

        // Sort and assign
        foreach (var kvp in temp.OrderBy(k => k.Key))
        {
            activeDoorsByFloor[kvp.Key] = kvp.Value;
        }
    }
}
[System.Serializable]
public struct RoomEntry
{
    public RoomTemplate prefab;
    [HideInInspector]
    [Range(0f, 1f)] public float weight; // Sum of all weights must equal 1
}