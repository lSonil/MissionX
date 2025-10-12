using BlueRaja;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class RoomConnector : MonoBehaviour
{
    public RoomSpawner roomSpawner;
    public Dictionary<int, List<Vector3>> debugWalkablePointsByFloor = new();
    public Dictionary<int, Dictionary<Vector3, Vector3>> doorMapByFloor = new();
    public Dictionary<int, Dictionary<Vector2Int, Vector2Int>> doorToHallConnection = new();

    public Dictionary<int, List<(PlacedRoom, Vector3, Vector3)>> doorConnectionsByFloor = new();
    public Dictionary<int, List<List<Vector2Int>>> allDoorPathsByFloor = new();

    public GameObject deadEndPrefab;
    public GameObject straightPrefab;
    public GameObject cornerPrefab;
    public GameObject tJunctionPrefab;
    public GameObject crossPrefab;
    public void ConnectRoomsOnFloor(int floor, List<PlacedRoom> rooms)
    {
        CollectWalkableGridPoints(floor, rooms);
        CollectClosestWalkablePointsToDoors(floor, rooms);
        CollectDoorToHallConnections(floor, rooms);
        BuildDoorConnections(floor, rooms);
        BuildAllDoorPaths(floor, rooms);
        SpawnSmartPathMarkers(floor);
    }

    public void CollectDoorToHallConnections(int floor, List<PlacedRoom> rooms)
    {
        Dictionary<Vector2Int, Vector2Int> connections = new();

        foreach (var room in rooms)
        {
            if (!room.activeDoorsByFloor.ContainsKey(floor)) continue;

            foreach (var door in room.activeDoorsByFloor[floor])
            {
                Vector2Int a = Vector2Int.zero;
                Vector2Int b = Vector2Int.zero;

                if (door.x==(int)door.x)
                {
                    b = WorldToGrid(door + new Vector3(-.5f, 0, 0));
                    a = WorldToGrid(door + new Vector3(.5f,0,0));
                }
                else
                {
                    b = WorldToGrid(door + new Vector3(0, 0, .5f));
                    a = WorldToGrid(door + new Vector3(0, 0, -.5f));
                }
                connections[a] = b;
            }
        }

        doorToHallConnection[floor] = connections;
    }


    public void SpawnSmartPathMarkers(int floor)
    {
        if (!allDoorPathsByFloor.ContainsKey(floor)) return;

        float floorY = floor;
        var connectionMap = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
        var spawnedMarkerPositions = new HashSet<Vector2Int>();

        // 🔗 Build connection map from path segments only
        foreach (var path in allDoorPathsByFloor[floor])
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2Int a = path[i];
                Vector2Int b = path[i + 1];

                if (!connectionMap.ContainsKey(a)) connectionMap[a] = new();
                if (!connectionMap.ContainsKey(b)) connectionMap[b] = new();

                connectionMap[a].Add(b);
                connectionMap[b].Add(a);
            }
        }

        // 🧠 Analyze each point
        foreach (var kvp in connectionMap)
        {
            Vector2Int point = kvp.Key;
            List<Vector2Int> convertedDoorPoints = new();

            var neighbors = kvp.Value;

            if (spawnedMarkerPositions.Contains(point)) continue;
            spawnedMarkerPositions.Add(point);

            var directions = neighbors.Select(n => n - point)
                                      .Select(NormalizeDirection)
                                      .Distinct()
                                      .ToList();
            if (doorToHallConnection[floor].ContainsKey(point) || doorToHallConnection[floor].ContainsValue(point))
            {
                Vector2Int pointA = new();
                if (doorToHallConnection[floor].ContainsKey(point))
                {
                    pointA = doorToHallConnection[floor][point];
                }
                else
                {
                    Vector2Int keyWithValue = Vector2Int.zero;
                    foreach (var entry in doorToHallConnection[floor])
                    {
                        if (entry.Value == point)
                        {
                            keyWithValue = entry.Key;
                            break;
                        }
                    }
                    pointA = keyWithValue;
                }

                if(pointA.x==point.x)
                {
                    if (pointA.y > point.y)
                        directions.Add(new Vector2Int(0, 1));
                    else
                        directions.Add(new Vector2Int(0, -1));
                }
                else
                {
                    if (pointA.x > point.x)
                        directions.Add(new Vector2Int(1, 0));
                    else
                        directions.Add(new Vector2Int(-1, 0));
                }
            }
            int count = directions.Count;
            GameObject prefabToSpawn = deadEndPrefab;
            Quaternion rotation = Quaternion.identity;

            if (count == 1)
            {
                prefabToSpawn = deadEndPrefab;
                rotation = DirectionToRotation(directions[0]);
            }
            else if (count == 2)
            {
                Vector2Int dirA = directions[0];
                Vector2Int dirB = directions[1];
                bool isStraight = (dirA.x == dirB.x || dirA.y == dirB.y);
                prefabToSpawn = isStraight ? straightPrefab : cornerPrefab;

                rotation = isStraight
                    ? (dirA.x != 0 ? Quaternion.Euler(0, 90, 0) : Quaternion.identity)
                    : CornerRotation(dirA, dirB);
            }
            else if (count == 3)
            {
                prefabToSpawn = tJunctionPrefab;
                var allDirs = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
                var missing = allDirs.FirstOrDefault(d => !directions.Contains(d));
                rotation = DirectionToRotation(missing);
            }
            else if (count >= 4)
            {
                prefabToSpawn = crossPrefab;
                rotation = Quaternion.identity;
            }

            Vector3 worldPos = GridToWorld(point, floorY);
            Instantiate(prefabToSpawn, worldPos, rotation, transform);
        }
    }

    private Vector2Int NormalizeDirection(Vector2Int dir)
    {
        if (dir.x > 0) return Vector2Int.right;
        if (dir.x < 0) return Vector2Int.left;
        if (dir.y > 0) return Vector2Int.up;
        if (dir.y < 0) return Vector2Int.down;
        return Vector2Int.zero;
    }

    private Quaternion DirectionToRotation(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return Quaternion.Euler(0, 0, 0);
        if (dir == Vector2Int.right) return Quaternion.Euler(0, 90, 0);
        if (dir == Vector2Int.down) return Quaternion.Euler(0, 180, 0);
        if (dir == Vector2Int.left) return Quaternion.Euler(0, 270, 0);
        return Quaternion.identity;
    }

    private Quaternion CornerRotation(Vector2Int a, Vector2Int b)
    {
        var dirs = new HashSet<Vector2Int> { a, b };

        if (dirs.Contains(Vector2Int.up) && dirs.Contains(Vector2Int.right)) return Quaternion.Euler(0, 0, 0);
        if (dirs.Contains(Vector2Int.right) && dirs.Contains(Vector2Int.down)) return Quaternion.Euler(0, 90, 0);
        if (dirs.Contains(Vector2Int.down) && dirs.Contains(Vector2Int.left)) return Quaternion.Euler(0, 180, 0);
        if (dirs.Contains(Vector2Int.left) && dirs.Contains(Vector2Int.up)) return Quaternion.Euler(0, 270, 0);

        return Quaternion.identity;
    }

    public class SimplePriorityQueue<T>
    {
        private List<(T item, float priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority)
        {
            elements.Add((item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;
            float bestPriority = elements[0].priority;

            for (int i = 1; i < elements.Count; i++)
            {
                if (elements[i].priority < bestPriority)
                {
                    bestPriority = elements[i].priority;
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].item;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }

        public bool Contains(T item)
        {
            return elements.Any(e => EqualityComparer<T>.Default.Equals(e.item, item));
        }
    }
    private Dictionary<int, HashSet<Vector2Int>> usedHallPointsByFloor = new();
    public List<Vector2Int> FindPathAStar(int floor, Vector2Int start, Vector2Int end, HashSet<Vector2Int> walkable)
    {
        var openSet = new SimplePriorityQueue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, float> { [start] = Vector2Int.Distance(start, end) };

        openSet.Enqueue(start, fScore[start]);

        Vector2Int[] directions = new[] {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.Dequeue();

            if (current == end)
            {
                var path = new List<Vector2Int> { current };
                while (cameFrom.ContainsKey(current))
                {
                    current = cameFrom[current];
                    path.Add(current);
                }
                path.Reverse();
                return path;
            }

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (!walkable.Contains(neighbor)) continue;

                float tentativeG = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;

                    bool isReused = usedHallPointsByFloor.ContainsKey(floor) && usedHallPointsByFloor[floor].Contains(neighbor);
                    float reuseWeight = isReused ? 0.5f : 50f; // Lower cost for reused, higher for new
                    fScore[neighbor] = tentativeG + Vector2Int.Distance(neighbor, end) + reuseWeight;

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        return null; // No path found
    }


    public void BuildAllDoorPaths(int floor, List<PlacedRoom> rooms)
    {
        var walkable = new HashSet<Vector2Int>(
            debugWalkablePointsByFloor[floor].Select(WorldToGrid)
        );

        var paths = new List<List<Vector2Int>>();
        if (!usedHallPointsByFloor.ContainsKey(floor))
            usedHallPointsByFloor[floor] = new HashSet<Vector2Int>();

        foreach (var (_, startWorld, endWorld) in doorConnectionsByFloor[floor])
        {
            Vector2Int start = WorldToGrid(doorMapByFloor[floor][startWorld]);
            Vector2Int end = WorldToGrid(doorMapByFloor[floor][endWorld]);

            var path = FindPathAStar(floor, start, end, walkable);
            if (path != null)
            {
                paths.Add(path);
                foreach (var point in path)
                    usedHallPointsByFloor[floor].Add(point);
            }
        }

        allDoorPathsByFloor[floor] = paths;
    }



    public Vector2Int WorldToGrid(Vector3 world)
    {
        Vector3 origin = new Vector3(-roomSpawner.gridWidth / 2f, 0f, -roomSpawner.gridHeight / 2f) * roomSpawner.tileSize;
        Vector3 local = (world - origin) / roomSpawner.tileSize;
        return new Vector2Int(Mathf.FloorToInt(local.x), Mathf.FloorToInt(local.z));
    }

    public void BuildDoorConnections(int floor, List<PlacedRoom> rooms)
    {
        List<Vector2> doorPoints = new();
        Dictionary<Vector2, PlacedRoom> doorToRoom = new();

        foreach (var room in rooms)
        {
            if (!room.activeDoorsByFloor.ContainsKey(floor)) continue;

            foreach (var door in room.activeDoorsByFloor[floor])
            {
                Vector2 pos = new Vector2(door.x, door.z);
                if (!doorToRoom.ContainsKey(pos))
                {
                    doorPoints.Add(pos);
                    doorToRoom[pos] = room;
                }
            }
        }

        List<(Vector2, Vector2, float)> edges = new();

        // Build all possible edges with distances
        for (int i = 0; i < doorPoints.Count; i++)
        {
            for (int j = i + 1; j < doorPoints.Count; j++)
            {
                Vector2 a = doorPoints[i];
                Vector2 b = doorPoints[j];

                if (doorToRoom[a] == doorToRoom[b]) continue; // 🚫 Skip intra-room edges

                float dist = Vector2.Distance(a, b);
                edges.Add((a, b, dist));
            }
        }


        // Sort edges by distance
        edges.Sort((e1, e2) => e1.Item3.CompareTo(e2.Item3));

        // Kruskal's MST
        Dictionary<Vector2, int> parent = new();
        for (int i = 0; i < doorPoints.Count; i++)
            parent[doorPoints[i]] = i;

        int Find(int i)
        {
            if (parent[doorPoints[i]] != i)
                parent[doorPoints[i]] = Find(parent[doorPoints[i]]);
            return parent[doorPoints[i]];
        }

        void Union(int i, int j)
        {
            int ri = Find(i);
            int rj = Find(j);
            if (ri != rj)
                parent[doorPoints[rj]] = ri;
        }

        List<(PlacedRoom, Vector3, Vector3)> connections = new();

        foreach (var (a, b, dist) in edges)
        {
            int ai = doorPoints.IndexOf(a);
            int bi = doorPoints.IndexOf(b);
            if (Find(ai) != Find(bi))
            {
                Union(ai, bi);

                PlacedRoom roomA = doorToRoom[a];
                Vector3 d1 = new Vector3(a.x, floor, a.y);
                Vector3 d2 = new Vector3(b.x, floor, b.y);

                connections.Add((roomA, d1, d2));
            }
        }
        List<(Vector2, Vector2, float)> leftoverEdges = new();

        foreach (var (a, b, dist) in edges)
        {
            int ai = doorPoints.IndexOf(a);
            int bi = doorPoints.IndexOf(b);
            if (Find(ai) != Find(bi))
            {
                Union(ai, bi);

                PlacedRoom roomA = doorToRoom[a];
                Vector3 d1 = new Vector3(a.x, floor, a.y);
                Vector3 d2 = new Vector3(b.x, floor, b.y);
                connections.Add((roomA, d1, d2));
            }
            else
            {
                leftoverEdges.Add((a, b, dist));
            }
        }
        leftoverEdges = leftoverEdges
        .Where(e => doorToRoom[e.Item1] != doorToRoom[e.Item2])
        .ToList();

        // 🔀 Add back some random edges
        int extrasToAdd = Mathf.FloorToInt(leftoverEdges.Count * 0.1f); // 30% of leftovers
        for (int i = 0; i < extrasToAdd && leftoverEdges.Count > 0; i++)
        {
            int index = Random.Range(0, leftoverEdges.Count);
            var (a, b, _) = leftoverEdges[index];
            leftoverEdges.RemoveAt(index);

            PlacedRoom roomA = doorToRoom[a];
            Vector3 d1 = new Vector3(a.x, floor, a.y);
            Vector3 d2 = new Vector3(b.x, floor, b.y);
            connections.Add((roomA, d1, d2));
        }

        doorConnectionsByFloor[floor] = connections;
    }


    public void CollectWalkableGridPoints(int floor, List<PlacedRoom> rooms)
    {
        List<Vector3> walkablePoints = new();

        int width = roomSpawner.gridWidth;
        int height = roomSpawner.gridHeight;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cell = new(x, y);
                bool isInsideRoom = false;

                foreach (var room in rooms)
                {
                    if (room.floor != floor) continue;
                    if (room.gridRect.Contains(cell))
                    {
                        isInsideRoom = true;
                        break;
                    }
                }

                if (!isInsideRoom)
                {
                    Vector3 worldPos = GridToWorld(cell, floor); // or use room.position.y
                    walkablePoints.Add(worldPos);
                }
            }
        }

        debugWalkablePointsByFloor[floor] = walkablePoints;
    }


    public Vector3 GridToWorld(Vector2Int gridPos, float floorY)
    {
        Vector3 origin = new Vector3(-roomSpawner.gridWidth / 2f, 0f, -roomSpawner.gridHeight / 2f) * roomSpawner.tileSize;
        return origin + new Vector3(gridPos.x + 0.5f, floorY, gridPos.y + 0.5f) * roomSpawner.tileSize;
    }

    public List<Vector2Int> GetWalkableGridPoints(int floor, List<PlacedRoom> rooms)
    {
        int width = roomSpawner.gridWidth;
        int height = roomSpawner.gridHeight;
        List<Vector2Int> walkablePoints = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cell = new(x, y);
                bool isInsideRoom = false;

                foreach (var room in rooms)
                {
                    if (room.floor != floor) continue;
                    if (room.gridRect.Contains(cell))
                    {
                        isInsideRoom = true;
                        break;
                    }
                }

                if (!isInsideRoom)
                    walkablePoints.Add(cell);
            }
        }

        return walkablePoints;
    }
    public void CollectClosestWalkablePointsToDoors(int floor, List<PlacedRoom> rooms)
    {
        if (!debugWalkablePointsByFloor.ContainsKey(floor)) return;

        List<Vector3> walkablePoints = debugWalkablePointsByFloor[floor];
        Dictionary<Vector3, Vector3> doorMap = new(); // door → closest walkable

        foreach (var room in rooms)
        {
            if (!room.activeDoorsByFloor.ContainsKey(floor)) continue;

            foreach (var door in room.activeDoorsByFloor[floor])
            {
                Vector3 bestWalkable = door;
                float bestWalkableDist = float.MaxValue;

                // 🔍 Find closest walkable point
                foreach (var walkable in walkablePoints)
                {
                    float dist = Vector3.Distance(new Vector3(door.x, 0f, door.z), new Vector3(walkable.x, 0f, walkable.z));
                    if (dist <= 1f && dist < bestWalkableDist)
                    {
                        bestWalkableDist = dist;
                        bestWalkable = walkable;
                    }
                }

                doorMap[door] = bestWalkable;
            }
        }

        doorMapByFloor[floor] = doorMap;
    }


    void OnDrawGizmos()
    {
        if (roomSpawner == null) return;

        int floorToShow = roomSpawner.gizmoFloor * 3;

        // 🟫 Draw door connections
        if (doorConnectionsByFloor.ContainsKey(floorToShow))
        {
            Gizmos.color = Color.black;
            foreach (var (room, start, end) in doorConnectionsByFloor[floorToShow])
            {
                Gizmos.DrawLine(start, end);
            }
        }


      // 🟦 Draw walkable grid points + elevated labels
      if (debugWalkablePointsByFloor.ContainsKey(floorToShow))
      {
          Gizmos.color = Color.gray;
          foreach (var point in debugWalkablePointsByFloor[floorToShow])
          {
              Gizmos.DrawSphere(point, 0.1f);

#if UNITY_EDITOR
                Vector2Int gridPos = WorldToGrid(point);
                Vector3 labelPos = point + Vector3.up * 0.3f; // Raised above sphere
                Handles.color = Color.white;
                Handles.Label(labelPos, $"{gridPos.x},{gridPos.y}");
#endif
          }
      }

        // 🟩 Draw door-to-walkable connections
        if (doorMapByFloor.ContainsKey(floorToShow))
        {
            Gizmos.color = Color.green;
            foreach (var pair in doorMapByFloor[floorToShow])
            {
                Gizmos.DrawLine(pair.Key, pair.Value);
                Gizmos.DrawSphere(pair.Value, 0.2f);
            }
        }

        Gizmos.color = Color.green;

        if (!allDoorPathsByFloor.ContainsKey(floorToShow)) return;

        foreach (var path in allDoorPathsByFloor[floorToShow])
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 a = GridToWorld(path[i], floorToShow);
                Vector3 b = GridToWorld(path[i + 1], floorToShow);
                Gizmos.DrawLine(a, b);
            }
        }
        if (!allDoorPathsByFloor.ContainsKey(floorToShow)) return;

        var connectionMap = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        // 🔗 Build connection map
        foreach (var path in allDoorPathsByFloor[floorToShow])
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2Int a = path[i];
                Vector2Int b = path[i + 1];

                if (!connectionMap.ContainsKey(a)) connectionMap[a] = new();
                if (!connectionMap.ContainsKey(b)) connectionMap[b] = new();

                connectionMap[a].Add(b);
                connectionMap[b].Add(a);
            }
        }

        // 🧠 Draw connection count labels
        foreach (var kvp in connectionMap)
        {
            Vector2Int point = kvp.Key;
            var neighbors = kvp.Value;

            var directions = neighbors.Select(n => n - point)
                                      .Select(NormalizeDirection)
                                      .Distinct()
                                      .ToList();

            int count = directions.Count;
            Vector3 worldPos = GridToWorld(point, floorToShow) + Vector3.up * 0.5f;

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(worldPos, count.ToString());
#endif
        }
        Gizmos.color = Color.yellow;

        foreach (var kvp in doorToHallConnection[floorToShow])
        {
            Vector3 doorWorld = GridToWorld(kvp.Key, floorToShow);
            Vector3 hallWorld = GridToWorld(kvp.Value, floorToShow);

            Gizmos.DrawLine(doorWorld, hallWorld);
        }
    }
}
