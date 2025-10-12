using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    [Header("Room Pool")]
    public List<RoomEntry> roomPool;
    public RoomEntry startRoom;
    [Header("Grid Settings")]
    public int gridWidth = 60;
    public int gridHeight = 60;
    public float tileSize = 2f;
    public int roomCount = 10;
    public int maxHeight = 3;

    public Dictionary<int, List<PlacedRoom>> placedRooms= new();
    private Dictionary<int, List<RectInt>> placedRects = new();

    private void Start()
    {
        if (!placedRooms.ContainsKey(0))
            placedRooms[0] = new List<PlacedRoom>();

        if (!placedRects.ContainsKey(0))
            placedRects[0] = new List<RectInt>();
        // Ensure the key exists before using it
        SpawnRooms(0,0);
    }

    void SpawnRooms(int floor, int numberOfFloors)
    {
        // ✅ Ensure startRoom is placed first
        if (floor == 0 && placedRooms[floor].Count == 0)
        {
            RoomTemplate template = startRoom.prefab;
            int angle = 0;
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            Vector2Int size = GetRotatedSize(template, angle);

            int x = gridWidth / 2 - size.x / 2;
            int y = gridHeight / 2 - size.y / 2;
            RectInt candidate = new RectInt(x, y, size.x, size.y);

            Vector3 worldPos = GetRoomWorldPosition(x, floor, y, size);
            GameObject instance = Instantiate(template.gameObject, worldPos, rotation, transform);
            PlacedRoom placed = new PlacedRoom(template, worldPos, rotation, size, tileSize, gridWidth, gridHeight, floor);
            placedRooms[floor].Add(placed);
            placedRects[floor].Add(candidate);
        }
        int attempts = 0;
        int maxAttempts = roomCount * 10;
        while (placedRooms[floor].Count < roomCount && attempts < maxAttempts)
        {
            RoomTemplate template = GetWeightedRandomRoom();
            int angle = Random.Range(0, 4) * 90;
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            Vector2Int size = GetRotatedSize(template, angle);

            int x = Random.Range(1, gridWidth - size.x - 1);
            int y = Random.Range(1, gridHeight - size.y - 1);
            RectInt candidate = new RectInt(x, y, size.x, size.y);

            if (IsOverlapping(candidate, floor)) { attempts++; continue; }

            Vector3 worldPos = GetRoomWorldPosition(x, floor, y, size);
            GameObject instance = Instantiate(template.gameObject, worldPos, rotation, transform);
            PlacedRoom placed = new PlacedRoom(template, worldPos, rotation, size, tileSize, gridWidth, gridHeight, floor);
            placedRooms[floor].Add(placed);
            placedRects[floor].Add(candidate);
            attempts++;
        }

        List<int> floorsToAdd = new List<int>();
        Dictionary<int, List<PlacedRoom>> roomsToAdd = new();
        Dictionary<int, List<RectInt>> rectsToAdd = new();

        foreach (var room in placedRooms[floor])
        {
            int roomY = Mathf.RoundToInt(room.position.y);

            foreach (var kvp in room.activeDoorsByFloor)
            {
                int localDoorOffset = kvp.Key;
                int doorWorldY = roomY + localDoorOffset;
                // Skip if it's the same floor or below
                if (doorWorldY <= roomY)
                    continue;

                // Debug check

                if (!roomsToAdd.ContainsKey(localDoorOffset))
                {
                    if (!placedRooms.ContainsKey(localDoorOffset) && numberOfFloors + roomsToAdd.Count <= maxHeight)
                        floorsToAdd.Add(localDoorOffset);

                    roomsToAdd[localDoorOffset] = new List<PlacedRoom>();
                    rectsToAdd[localDoorOffset] = new List<RectInt>();
                }

                PlacedRoom logicalClone = new PlacedRoom(room.template, room.position, room.rotation, room.size, tileSize, gridWidth, gridHeight, localDoorOffset);

                roomsToAdd[localDoorOffset].Add(logicalClone);
                rectsToAdd[localDoorOffset].Add(room.gridRect);
            }
        }

        foreach (var kvp in roomsToAdd)
        {
            int floorKey = kvp.Key;
            if (!placedRooms.ContainsKey(floorKey))
                placedRooms[floorKey] = new List<PlacedRoom>();

            placedRooms[floorKey].AddRange(kvp.Value);
        }

        foreach (var kvp in rectsToAdd)
        {
            int floorKey = kvp.Key;
            if (!placedRects.ContainsKey(floorKey))
                placedRects[floorKey] = new List<RectInt>();

            placedRects[floorKey].AddRange(kvp.Value);
        }

        floorsToAdd.Sort();
        foreach (int flr in floorsToAdd)
        {
            if (!placedRooms.ContainsKey(flr))
            {
                continue;
            }

            int roomCount = placedRooms[flr].Count;
            int totalDoors = 0;

            foreach (var room in placedRooms[flr])
            {
                if (room.activeDoorsByFloor.ContainsKey(flr))
                {
                    int doorCount = room.activeDoorsByFloor[flr].Count;
                    totalDoors += doorCount;
                }
            }

            if (totalDoors > 0)
            {
                SpawnRooms(flr, numberOfFloors + placedRooms.Count);
            }
        }



        RoomConnector connector = GetComponent<RoomConnector>();
        connector.ConnectRoomsOnFloor(floor, placedRooms[floor]);
    }
    bool IsOverlapping(RectInt candidate, int floor)
    {
        RectInt padded = new RectInt(candidate.x - 1, candidate.y - 1, candidate.width + 2, candidate.height + 2);

        if (!placedRects.ContainsKey(floor))
            return false;

        foreach (var room in placedRects[floor])
        {
            RectInt existing = new RectInt(room.x - 1, room.y - 1, room.width + 2, room.height + 2);
            if (existing.Overlaps(padded))
                return true;
        }

        return false;
    }

    RoomTemplate GetWeightedRandomRoom()
    {
        float roll = Random.value;
        float cumulative = 0f;

        foreach (var entry in roomPool)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
                return entry.prefab;
        }

        return roomPool[roomPool.Count - 1].prefab;
    }

    Vector2Int GetRotatedSize(RoomTemplate template, int angle)
    {
        angle = angle % 360;
        if (angle == 90 || angle == 270)
            return new Vector2Int(template.length, template.width);
        return new Vector2Int(template.width, template.length);
    }

    Vector3 GetRoomWorldPosition(int x, int y, int z, Vector2Int size)
    {
        float ox = -gridWidth * tileSize * 0.5f;
        float oz = -gridHeight * tileSize * 0.5f;

        float px = (x + size.x / 2f) * tileSize + ox;
        float pz = (z + size.y / 2f) * tileSize + oz;

        return transform.position + new Vector3(px, y, pz);
    }

    public int gizmoFloor = 0; // Set this in the inspector to choose which floor to display

    private void OnDrawGizmos()
    {
        if (placedRooms == null || placedRooms.Count == 0) return;
        if (!placedRooms.ContainsKey(gizmoFloor * 3)) return;

        float ox = -gridWidth * tileSize * 0.5f;
        float oz = -gridHeight * tileSize * 0.5f;

        List<PlacedRoom> rooms = placedRooms[gizmoFloor * 3];

        // 🟫 Draw grid for this floor
        Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 from = transform.position + new Vector3(x * tileSize + ox, gizmoFloor * 3, oz);
            Vector3 to = transform.position + new Vector3(x * tileSize + ox, gizmoFloor * 3, gridHeight * tileSize + oz);
            Gizmos.DrawLine(from, to);
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 from = transform.position + new Vector3(ox, gizmoFloor * 3, y * tileSize + oz);
            Vector3 to = transform.position + new Vector3(gridWidth * tileSize + ox, gizmoFloor * 3, y * tileSize + oz);
            Gizmos.DrawLine(from, to);
        }

        // 🟦 Fill grid cells that overlap rooms
        Gizmos.color = new Color(1f, 1f, 1f, 1f);
        foreach (var room in rooms)
        {
            RectInt rect = room.gridRect;
            for (int x = rect.xMin; x < rect.xMax; x++)
            {
                for (int y = rect.yMin; y < rect.yMax; y++)
                {
                    float px = x * tileSize + ox;
                    float pz = y * tileSize + oz;
                    Vector3 center = transform.position + new Vector3(px + tileSize / 2f, gizmoFloor * 3 + 0.01f, pz + tileSize / 2f);
                    Vector3 size = new Vector3(tileSize, 0.01f, tileSize);
                    Gizmos.DrawCube(center, size);
                }
            }
        }

        // 🧱 Draw rooms
        foreach (var room in rooms)
        {
            Gizmos.color = Color.cyan;
            float w = room.size.x * tileSize;
            float h = room.size.y * tileSize;

            Vector3[] baseCorners = new Vector3[]
            {
            new Vector3(0, 0, 0),
            new Vector3(w, 0, 0),
            new Vector3(w, 0, h),
            new Vector3(0, 0, h)
            };

            for (int i = 0; i < baseCorners.Length; i++)
            {
                baseCorners[i] = room.position + room.rotation * (baseCorners[i] - new Vector3(w / 2, 0, h / 2));
            }

            for (int i = 0; i < 4; i++)
                Gizmos.DrawLine(baseCorners[i], baseCorners[(i + 1) % 4]);

            Gizmos.color = Color.magenta;
            foreach (var corner in baseCorners)
            {
                Vector3 top = corner + (Vector3.up * room.template.height * 3)-new Vector3(0, 1, 0);
                Gizmos.DrawLine(corner, top);
            }

            Gizmos.color = Color.blue;
            Vector3[] topCorners = baseCorners.Select(c => c + (Vector3.up * room.template.height * 3)-new Vector3(0,1,0)).ToArray();
            for (int i = 0; i < 4; i++)
                Gizmos.DrawLine(topCorners[i], topCorners[(i + 1) % 4]);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(room.position, 0.3f);

            if (room.activeDoorsByFloor.TryGetValue(gizmoFloor*3, out var doors))
            {
                foreach (var door in doors)

                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(door, 0.2f);

                    Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
                    Gizmos.DrawLine(room.position, door);
                }
            }

#if UNITY_EDITOR
            Gizmos.color = Color.yellow;
            Vector3 labelPos = room.position + Vector3.up * 0.5f;
            UnityEditor.Handles.Label(labelPos, $"Floor {gizmoFloor * 3}");
#endif
        }
    }

}
