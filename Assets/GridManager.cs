using System.Collections;
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
        StartCoroutine(DelayedBake());
    }

    private IEnumerator DelayedBake()
    {
        yield return new WaitForSeconds(.1f);

        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
        }
        else
        {
            Debug.LogWarning("NavMeshSurface not found on GridManager.");
        }
    }
    public Transform GetClosestPoint(List<Transform> usedGrid, Transform target)
    {
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform point in usedGrid)
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
    public Transform GetFurthestPoint(List<Transform> usedGrid, Transform target)
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
    public Transform GetRandomPointInRange(List<Transform> usedGrid, Transform target, float range)
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
