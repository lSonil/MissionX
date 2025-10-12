using UnityEngine;

public class AStarNode
{
    public Vector2Int position;
    public AStarNode parent;
    public float gCost;
    public float hCost;
    public float FCost => gCost + hCost;

    public AStarNode(Vector2Int pos)
    {
        position = pos;
    }
}
