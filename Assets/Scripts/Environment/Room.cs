using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Room : MonoBehaviour
{
    public Doorway startingDoor;
    public List<Transform> layouts;
    [HideInInspector]
    public List<Doorway> doors;
    public List<Transform> nodes;
    [HideInInspector]
    public List<ItemSpawner> spawnPoints;
    public bool hall;
    public List<Transform> Rubies;

    [HideInInspector]
    public NavMeshSurface surface;

    private List<Vector3> debugOverlapPositions = new();
    private List<Bounds> placedColliderBounds = new();

    public void CollectSpawnPoints()
    {
        spawnPoints.Clear();

        // true = include inactive, but we filter manually
        ItemSpawner[] all = GetComponentsInChildren<ItemSpawner>(true);

        foreach (var sp in all)
        {
            if (sp.gameObject.activeInHierarchy)
                spawnPoints.Add(sp);
        }
    }
    public void PrepareDoors()
    {
        doors.Clear();

        GameObject parent = GameObject.Find("Doors");
        if (parent != null)
        {
            foreach (Transform child in parent.transform)
            {
                Doorway d = child.GetComponent<Doorway>();
                if (d != null && d != startingDoor)
                    doors.Add(d);
            }
        }
    }
    public void PrepareLayout()
    {
        surface = GetComponent<NavMeshSurface>();

        layouts.Clear();

        Transform parent = transform.Find("Layouts");
        if (parent != null)
        {
            foreach (Transform child in parent)
            {
                layouts.Add(child);
            }

            int randomIndex = Random.Range(0, layouts.Count);

            for (int i = 0; i < layouts.Count; i++)
                layouts[i].gameObject.SetActive(false);

            layouts[randomIndex].gameObject.SetActive(true);
        }
        nodes.Clear();

        parent = transform.Find("Nodes");
        if (parent != null)
        {
            foreach (Transform child in parent.transform)
                nodes.Add(child);
        }
    }
    public void RegenerateColliders()
    {
        foreach (BoxCollider col in GetComponents<BoxCollider>())
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(col);
#else
            DestroyImmediate(col);
#endif
        }

        // Settings
        Vector3 size = new Vector3(2f, 2f, 2f);
        Vector3 startLocal = new Vector3(0f, 1f, 1f);
        Vector3 startWorld = transform.TransformPoint(startLocal);

        Vector3[] directions = new Vector3[]
        {
            new Vector3(2f, 0f, 0f),
            new Vector3(-2f, 0f, 0f),
            new Vector3(0f, 2f, 0f),
            new Vector3(0f, -2f, 0f),
            new Vector3(0f, 0f, 2f),
            new Vector3(0f, 0f, -2f)
        };

        HashSet<Vector3> visited = new();
        Queue<Vector3> toCheck = new();
        debugOverlapPositions.Clear();
        placedColliderBounds.Clear();

        // Force initial collider placement at starting position
        Bounds initialBounds = new Bounds(startWorld, size * 0.6f);
        BoxCollider initialCol = gameObject.AddComponent<BoxCollider>();
        initialCol.size = size;
        initialCol.isTrigger = true;
        initialCol.center = transform.InverseTransformPoint(startWorld);

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(initialCol, "Add Collider");
        PrefabUtility.RecordPrefabInstancePropertyModifications(initialCol);
        EditorUtility.SetDirty(initialCol);
#endif

        placedColliderBounds.Add(initialBounds);
        debugOverlapPositions.Add(startWorld);
        visited.Add(startWorld);

        foreach (Vector3 dir in directions)
        {
            Vector3 nextWorld = startWorld + dir;
            toCheck.Enqueue(nextWorld);
        }

        int placedCount = 1;

        while (toCheck.Count > 0)
        {
            Vector3 currentWorld = toCheck.Dequeue();
            if (visited.Contains(currentWorld)) continue;
            visited.Add(currentWorld);
            debugOverlapPositions.Add(currentWorld);

            Bounds candidateBounds = new Bounds(currentWorld, size * 0.6f);

            bool overlapsExisting = false;
            foreach (Bounds placed in placedColliderBounds)
            {
                if (candidateBounds.Intersects(placed))
                {
                    overlapsExisting = true;
                    break;
                }
            }

            if (OverlapsWithSelf(currentWorld, size * 0.6f) && !overlapsExisting)
            {
                BoxCollider newCol = gameObject.AddComponent<BoxCollider>();
                newCol.size = size;
                newCol.isTrigger = true;
                newCol.center = transform.InverseTransformPoint(currentWorld);
                placedCount++;

#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(newCol, "Add Collider");
                PrefabUtility.RecordPrefabInstancePropertyModifications(newCol);
                EditorUtility.SetDirty(newCol);
#endif

                placedColliderBounds.Add(candidateBounds);

                foreach (Vector3 dir in directions)
                {
                    Vector3 nextWorld = currentWorld + dir;
                    if (!visited.Contains(nextWorld))
                    {
                        toCheck.Enqueue(nextWorld);
                    }
                }
            }
        }

#if UNITY_EDITOR
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        EditorUtility.SetDirty(gameObject);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif

        Debug.Log($"Collider regeneration complete for {gameObject.name}. Placed: {placedCount}");
    }

    bool OverlapsWithSelf(Vector3 worldCenter, Vector3 worldSize)
    {
        Bounds testBounds = new Bounds(worldCenter, worldSize);

        foreach (MeshCollider meshCol in GetComponentsInChildren<MeshCollider>())
        {
            if (!meshCol.enabled || meshCol.sharedMesh == null) continue;

            Bounds colBounds = meshCol.bounds;
            if (testBounds.Intersects(colBounds))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 size = new Vector3(2f, 2f, 2f);

        foreach (Vector3 pos in debugOverlapPositions)
        {
            Gizmos.DrawWireCube(pos, size);
        }
    }
}
