using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public List<Transform> grid;
    public static GridManager i;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        i = this;    
    }
    public void GridReady(List<Transform> fullGrid)
    {
        grid = fullGrid;
        if (GetComponent<NavMeshSurface>() != null)
        {
            GetComponent<NavMeshSurface>().BuildNavMesh();
        }
        else
        {
            Debug.LogWarning("NavMeshSurface not found on GridManager.");
        }
    }
    public Transform GetClosestPoint(Transform target)
    {
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform point in grid)
        {
            float dist = Vector3.Distance(target.position, point.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = point;
            }
        }

        return closest;
    }
    public Transform GetFurthestPoint(Transform target)
    {
        Transform furthest = null;
        float maxDistance = 0f;

        foreach (Transform point in grid)
        {
            float dist = Vector3.Distance(target.position, point.position);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                furthest = point;
            }
        }

        return furthest;
    }
    public Transform GetRandomPointInRange(Transform target, float range)
    {
        List<Transform> candidates = new();

        foreach (Transform point in grid)
        {
            if (Vector3.Distance(target.position, point.position) <= range)
            {
                candidates.Add(point);
            }
        }

        if (candidates.Count == 0) return null;

        return candidates[Random.Range(0, candidates.Count)];
    }
    public Transform GetPlayerTransform()
    {
        GameObject player = GameObject.Find("Player");
        return player != null ? player.transform : null;
    }

}
