using System.Collections.Generic;
using UnityEngine;

public enum ActionType { Rotate, Move, Resize, Spawn, Contain, PlaySound }
public enum Axis { X, Y, Z }

[System.Serializable]
public class ActionData
{
    public ActionSystem target;
    public ActionType type;
    public Axis axis = Axis.X;
    public float openValue = 1f;
    public float closedValue = 0f;
    public float duration = 1f;

    public GameObject objectToSpawn;

    public enum ExecuteMode { After, WithPrevious }
    public ExecuteMode mode = ExecuteMode.After;
    public ExecuteMode reverseMode = ExecuteMode.After;

    public Vector3 AxisToVector()
    {
        switch (axis)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            case Axis.Z: return Vector3.forward;
        }
        return Vector3.right;
    }
}


public class ActionList : MonoBehaviour
{
    [Tooltip("Ordered list of actions")]
    public List<ActionData> actions = new List<ActionData>();
}
