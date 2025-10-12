using System.Collections.Generic;
using UnityEngine;
public enum GridCell
{
    Empty,
    Blocked,
    Hallway
}

public class AStarPathfinder
{
    private GridCell[,] grid;
    private int width;
    private int height;

    public AStarPathfinder(GridCell[,] grid)
    {
        this.grid = grid;
        width = grid.GetLength(0);
        height = grid.GetLength(1);
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        var open = new List<AStarNode>();
        var closed = new HashSet<Vector2Int>();
        var nodes = new Dictionary<Vector2Int, AStarNode>();

        AStarNode startNode = new AStarNode(start);
        startNode.gCost = 0;
        startNode.hCost = Vector2Int.Distance(start, end);
        open.Add(startNode);
        nodes[start] = startNode;

        while (open.Count > 0)
        {
            open.Sort((a, b) => a.FCost.CompareTo(b.FCost));
            AStarNode current = open[0];
            open.RemoveAt(0);
            closed.Add(current.position);

            if (current.position == end)
                return ReconstructPath(current);

            foreach (var neighborPos in GetNeighbors(current.position))
            {
                if (closed.Contains(neighborPos) || grid[neighborPos.x, neighborPos.y] == GridCell.Blocked)
                    continue;

                float tentativeG = current.gCost + 1;
                if (!nodes.ContainsKey(neighborPos))
                {
                    var neighbor = new AStarNode(neighborPos);
                    neighbor.gCost = tentativeG;
                    neighbor.hCost = Vector2Int.Distance(neighborPos, end);
                    neighbor.parent = current;
                    nodes[neighborPos] = neighbor;
                    open.Add(neighbor);
                }
                else if (tentativeG < nodes[neighborPos].gCost)
                {
                    nodes[neighborPos].gCost = tentativeG;
                    nodes[neighborPos].parent = current;
                }
            }
        }

        return new List<Vector2Int>();
    }

    List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var neighbors = new List<Vector2Int>();
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int next = pos + dir;
            if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height)
                neighbors.Add(next);
        }

        return neighbors;
    }

    List<Vector2Int> ReconstructPath(AStarNode node)
    {
        var path = new List<Vector2Int>();
        while (node != null)
        {
            path.Add(node.position);
            node = node.parent;
        }
        path.Reverse();
        return path;
    }
}
