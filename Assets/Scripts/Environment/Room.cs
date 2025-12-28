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
    
    [Header("Collider Settings")]
    public Vector3 colliderSize = new Vector3(2f, 2f, 2f);
    public Vector3 startLocal = new Vector3(0f, 1f, 1f);
    public float overlapScale = 0.6f;

    public void RegenerateColliders()
    {
        // Remove old colliders
        foreach (BoxCollider col in GetComponents<BoxCollider>())
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(col);
#else
        DestroyImmediate(col);
#endif
        }

        debugOverlapPositions.Clear();
        placedColliderBounds.Clear();

        // Find the "Structure" object
        Transform structureRoot = transform.Find("Structure");
        if (structureRoot == null)
        {
            Debug.LogError("Room: No 'Structure' object found.");
            return;
        }

        // Track which snapped positions already have colliders
        HashSet<Vector3> usedPositions = new HashSet<Vector3>();

        // Your custom grid origin
        Vector3 gridOrigin = new Vector3(0f, 1f, 1f);

        foreach (Transform piece in structureRoot.GetComponentsInChildren<Transform>())
        {
            if (piece == structureRoot)
                continue;

            Vector3 worldPos = piece.position;

            // Apply offset BEFORE snapping
            Vector3 offset = -piece.right.normalized * 0.5f;
            Vector3 adjustedPos = worldPos + offset;

            // Snap adjusted position to your custom grid: origin (0,1,1), step 2
            Vector3 snapped = new Vector3(
                Mathf.Round((adjustedPos.x - gridOrigin.x) / 2f) * 2f + gridOrigin.x,
                Mathf.Round((adjustedPos.y - gridOrigin.y) / 2f) * 2f + gridOrigin.y,
                Mathf.Round((adjustedPos.z - gridOrigin.z) / 2f) * 2f + gridOrigin.z
            );

            // Skip duplicates
            if (usedPositions.Contains(snapped))
                continue;

            usedPositions.Add(snapped);

            // Create collider
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.size = colliderSize;
            col.isTrigger = true;
            col.center = transform.InverseTransformPoint(snapped);

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(col, "Add Collider");
            PrefabUtility.RecordPrefabInstancePropertyModifications(col);
            EditorUtility.SetDirty(col);
#endif

            placedColliderBounds.Add(new Bounds(snapped, colliderSize));
            debugOverlapPositions.Add(snapped);
        }


#if UNITY_EDITOR
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        EditorUtility.SetDirty(gameObject);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif

        Debug.Log($"Collider regeneration complete for {gameObject.name}. Placed: {usedPositions.Count}");
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        foreach (Vector3 pos in debugOverlapPositions)
            Gizmos.DrawWireCube(pos, colliderSize);
    }
}