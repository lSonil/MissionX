using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;




public class Generator3D : MonoBehaviour {
    enum Dir
    {
        North, // forward (Z+)
        East,  // right (X+)
        South, // back (Z-)
        West   // left (X-)
    }
    enum CellType
    {
        None,
        Room,
        Hallway,
        Stairs,
        Blocked // new: used to prevent hallways under stairs
    }
    bool IsBlocked(Vector3Int pos)
    {
        if (!grid.InBounds(pos)) return true;
        var type = grid[pos];
        return type == CellType.Room || type == CellType.Stairs || type == CellType.Blocked;
    }

    class Room {
        public BoundsInt bounds;

        public Room(Vector3Int location, Vector3Int size) {
            bounds = new BoundsInt(location, size);
        }

        public static bool Intersect(Room a, Room b) {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y)
                || (a.bounds.position.z >= (b.bounds.position.z + b.bounds.size.z)) || ((a.bounds.position.z + a.bounds.size.z) <= b.bounds.position.z));
        }
    }

    static readonly Dictionary<Dir, Vector3Int> dirToVector = new Dictionary<Dir, Vector3Int> {
        { Dir.North, Vector3Int.forward },
        { Dir.East, Vector3Int.right },
        { Dir.South, Vector3Int.back },
        { Dir.West, Vector3Int.left }
    };
    List<Dir> GetConnectedDirections(Vector3Int pos)
    {
        List<Dir> connected = new List<Dir>();

        foreach (var kvp in dirToVector)
        {
            Vector3Int neighbor = pos + kvp.Value;
            if (!grid.InBounds(neighbor)) continue;

            var cellType = grid[neighbor];
            if (cellType == CellType.Hallway || cellType == CellType.Room || cellType == CellType.Stairs)
            {
                connected.Add(kvp.Key);
            }
        }

        return connected;
    }


    [SerializeField]
    Vector3Int size;
    [SerializeField]
    int roomCount;
    [SerializeField]
    Vector3Int roomMaxSize;

    [SerializeField]
    GameObject[] roomPrefabs;
    [SerializeField] GameObject straightHallwayPrefab;
    [SerializeField] GameObject cornerHallwayPrefab;
    [SerializeField] GameObject tJunctionHallwayPrefab;
    [SerializeField] GameObject crossHallwayPrefab;
    [SerializeField] GameObject stairsUpPrefab;
    [SerializeField] GameObject stairsDownPrefab;

    [SerializeField]
    Material redMaterial;
    [SerializeField]
    Material blueMaterial;
    [SerializeField]
    Material greenMaterial;

    Random random;
    Grid3D<CellType> grid;
    List<Room> rooms;
    Delaunay3D delaunay;
    HashSet<Prim.Edge> selectedEdges;

    void Start() {
        random = new Random();
        grid = new Grid3D<CellType>(size, Vector3Int.zero);
        rooms = new List<Room>();

        PlaceRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();
    }

    void PlaceRooms()
    {
        int maxAttemptsPerRoom = 20;

        for (int i = 0; i < roomCount; i++)
        {
            Vector3Int roomSize = new Vector3Int(
                random.Next(1, roomMaxSize.x + 1),
                random.Next(1, roomMaxSize.y + 1),
                random.Next(1, roomMaxSize.z + 1)
            );

            bool placed = false;

            for (int attempt = 0; attempt < maxAttemptsPerRoom; attempt++)
            {
                Vector3Int location = new Vector3Int(
                    random.Next(0, size.x),
                    random.Next(0, size.y),
                    random.Next(0, size.z)
                );

                Room newRoom = new Room(location, roomSize);
                Room buffer = new Room(location + new Vector3Int(-1, 0, -1), roomSize + new Vector3Int(2, 0, 2));

                // Check bounds
                if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x ||
                    newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y ||
                    newRoom.bounds.zMin < 0 || newRoom.bounds.zMax >= size.z)
                {
                    continue; // out of bounds, try again
                }

                // Check for overlap
                bool intersects = false;
                foreach (var room in rooms)
                {
                    if (Room.Intersect(room, buffer))
                    {
                        intersects = true;
                        break;
                    }
                }

                if (!intersects)
                {
                    rooms.Add(newRoom);
                    PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                    foreach (var pos in newRoom.bounds.allPositionsWithin)
                    {
                        grid[pos] = CellType.Room;
                    }

                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                print($"Room {i} could not be placed after multiple attempts.");
            }
        }
    }


    void Triangulate() {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms) {
            vertices.Add(new Vertex<Room>((Vector3)room.bounds.position + ((Vector3)room.bounds.size) / 2, room));
        }

        delaunay = Delaunay3D.Triangulate(vertices);
    }

    void CreateHallways() {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges)
        {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        if (edges.Count == 0)
        {
            Debug.LogWarning("No edges found: possibly due to small height size. Skipping hallway creation.");
            return;
        }

        List<Prim.Edge> minimumSpanningTree = Prim.MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Prim.Edge>(minimumSpanningTree);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges) {
            if (random.NextDouble() < 0.125) {
                selectedEdges.Add(edge);
            }
        }
    }
    void PathfindHallways()
    {
        HashSet<Vector3Int> placedHallways = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, HashSet<Vector3Int>> hallwayConnections = new();

        DungeonPathfinder3D aStar = new DungeonPathfinder3D(size);

        foreach (var edge in selectedEdges)
        {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
            var endPos = new Vector3Int((int)endPosf.x, (int)endPosf.y, (int)endPosf.z);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b) => {
                var pathCost = new DungeonPathfinder3D.PathCost();
                var delta = b.Position - a.Position;

                // Prevent traversing blocked spaces
                if (grid[b.Position] == CellType.Blocked)
                    return pathCost; // non-traversable (traversable=false by default)

                if (delta.y == 0)
                {
                    // flat hallway
                    pathCost.cost = Vector3Int.Distance(b.Position, endPos);

                    if (grid[b.Position] == CellType.Stairs)
                        return pathCost; // don't overwrite or connect into stairs

                    else if (grid[b.Position] == CellType.Room)
                        pathCost.cost += 5; // small penalty to avoid cutting through rooms
                    else if (grid[b.Position] == CellType.None)
                        pathCost.cost += 1; // open space = cheap movement

                    pathCost.traversable = true;
                }
                else
                {
                    // stairs
                    if ((grid[a.Position] != CellType.None && grid[a.Position] != CellType.Hallway)
                        || (grid[b.Position] != CellType.None && grid[b.Position] != CellType.Hallway))
                        return pathCost; // can't go through solid/invalid space

                    pathCost.cost = 100 + Vector3Int.Distance(b.Position, endPos);
                    pathCost.traversable = true;
                    pathCost.isStairs = true;
                }

                return pathCost;
            });

            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    var current = path[i];

                    if (i > 0)
                    {
                        var prev = path[i - 1];
                        var delta = current - prev;

                        // Draw line centered between prev and current
                        Debug.DrawLine(
                            prev + new Vector3(0.5f, 0.5f, 0.5f),
                            current + new Vector3(0.5f, 0.5f, 0.5f),
                            Color.blue,
                            100,
                            false
                        );

                        if (delta.y != 0)  // Stair step
                        {
                            if (grid[current] == CellType.None)
                            {
                                grid[current] = CellType.Stairs;
                                bool goingUp = delta.y > 0;

                                PlaceStairs(current, goingUp);
                            }
                        }
                        else  // Flat hallway step
                        {
                            if (grid[current] == CellType.None)
                            {
                                grid[current] = CellType.Hallway;
                            }
                        }
                    }
                    else
                    {
                        // First position in path, mark as hallway if empty
                        if (grid[current] == CellType.None)
                        {
                            grid[current] = CellType.Hallway;
                        }
                    }
                }


                foreach (var pos in path)
                {
                    if (grid[pos] == CellType.Hallway && !placedHallways.Contains(pos))
                    {
                        PlaceHallway(pos);
                        placedHallways.Add(pos);
                    }
                }

            }
        }
    }

    void PlaceCube(Vector3Int location, Vector3Int size, GameObject prefab, Material material = null)
    {
        GameObject go = Instantiate(prefab, location, Quaternion.identity);
        go.transform.localScale = size;

        if (material != null)
        {
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
    }


    void PlaceRoom(Vector3Int location, Vector3Int size)
    {
        var prefab = roomPrefabs[random.Next(roomPrefabs.Length)];
        PlaceCube(location, size, prefab);
    }
    static readonly Vector3Int[] directions = new Vector3Int[] {
    Vector3Int.forward,   // Z+
    Vector3Int.back,      // Z-
    Vector3Int.right,     // X+
    Vector3Int.left,      // X-
    Vector3Int.up,        // Y+
    Vector3Int.down       // Y-
};

    void PlaceHallway(Vector3Int location)
    {
        var (prefab, rotation) = GetHallwayPrefabAndRotation(location);
        GameObject go = Instantiate(prefab, location, rotation);
        go.transform.localScale = Vector3.one;
    }
    (GameObject prefab, Quaternion rotation) GetHallwayPrefabAndRotation(Vector3Int pos)
    {
        var dirs = GetConnectedDirections(pos);
        dirs.Sort(); // So order doesn't affect comparison

        if (dirs.Count == 4)
        {
            return (crossHallwayPrefab, Quaternion.identity);
        }

        if (dirs.Count == 3)
        {
            // T-Junction
            if (!dirs.Contains(Dir.North)) return (tJunctionHallwayPrefab, Quaternion.Euler(0, 180, 0)); // missing N
            if (!dirs.Contains(Dir.South)) return (tJunctionHallwayPrefab, Quaternion.identity);         // missing S
            if (!dirs.Contains(Dir.West)) return (tJunctionHallwayPrefab, Quaternion.Euler(0, 90, 0));  // missing W
            if (!dirs.Contains(Dir.East)) return (tJunctionHallwayPrefab, Quaternion.Euler(0, -90, 0)); // missing E
        }

        if (dirs.Count == 2)
        {
            // Could be straight or corner
            bool hasNS = dirs.Contains(Dir.North) && dirs.Contains(Dir.South);
            bool hasEW = dirs.Contains(Dir.East) && dirs.Contains(Dir.West);

            if (hasNS) return (straightHallwayPrefab, Quaternion.identity);
            if (hasEW) return (straightHallwayPrefab, Quaternion.Euler(0, 90, 0));

            // L-shape corner
            if (dirs.Contains(Dir.North) && dirs.Contains(Dir.East)) return (cornerHallwayPrefab, Quaternion.identity);
            if (dirs.Contains(Dir.East) && dirs.Contains(Dir.South)) return (cornerHallwayPrefab, Quaternion.Euler(0, 90, 0));
            if (dirs.Contains(Dir.South) && dirs.Contains(Dir.West)) return (cornerHallwayPrefab, Quaternion.Euler(0, 180, 0));
            if (dirs.Contains(Dir.West) && dirs.Contains(Dir.North)) return (cornerHallwayPrefab, Quaternion.Euler(0, 270, 0));
        }
        // Fallback (shouldn't happen)
        return (straightHallwayPrefab, Quaternion.identity);
    }

    void PlaceStairs(Vector3Int location, bool goingUp)
    {
        GameObject prefab = goingUp ? stairsUpPrefab : stairsDownPrefab;
        PlaceCube(location, Vector3Int.one, prefab);

        // Reserve space above and below (to prevent hallway overlap)
        Vector3Int above = location + Vector3Int.up;
        Vector3Int below = location + Vector3Int.down;

        if (grid.InBounds(above) && grid[above] == CellType.None)
            grid[above] = CellType.Blocked;

        if (grid.InBounds(below) && grid[below] == CellType.None)
            grid[below] = CellType.Blocked;
    }

}
