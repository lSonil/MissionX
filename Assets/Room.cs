using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Doorway> doors;
    public Doorway startingDoor;
    public List<Transform> nodes;
    public bool hall;
    private List<Bounds> placedColliderBounds = new();
    public void RegenerateColliders()
    {
        // Remove only BoxColliders on this object (not children)
        foreach (BoxCollider col in GetComponents<BoxCollider>())
        {
            DestroyImmediate(col);
        }

        // Settings
        Vector3 size = new Vector3(2f, 2f, 2f);
        Vector3 startLocal = new Vector3(0f, 0.8f, 1f);
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
        toCheck.Enqueue(startWorld);

        int placedCount = 0;
        debugOverlapPositions.Clear();
        placedColliderBounds.Clear();
        while (toCheck.Count > 0)
        {
            Vector3 currentWorld = toCheck.Dequeue();
            if (visited.Contains(currentWorld)) continue;
            visited.Add(currentWorld);
            debugOverlapPositions.Add(currentWorld);
            Bounds candidateBounds = new Bounds(currentWorld, size * 0.6f);

            // Skip if it overlaps with any already placed collider
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
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        UnityEditor.EditorUtility.SetDirty(gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        Debug.Log($"Collider regeneration complete for {gameObject.name}. Placed: {placedCount}");
    }
    private List<Vector3> debugOverlapPositions = new();

    bool OverlapsWithSelf(Vector3 worldCenter, Vector3 worldSize)
    {
        Bounds testBounds = new Bounds(worldCenter, worldSize);

        foreach (MeshCollider meshCol in GetComponentsInChildren<MeshCollider>())
        {
            if (!meshCol.enabled) continue;

            Bounds colBounds = meshCol.bounds;
            if (testBounds.Intersects(colBounds))
            {
                return true;
            }
        }

        return false;
    }
}