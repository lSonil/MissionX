using System.Collections.Generic;
using UnityEngine;

public class RoomTemplate : MonoBehaviour
{
    public int width = 4;
    public int length = 4;
    public float height = 3f; // ✅ new height parameter
    public List<Transform> doors;
}
