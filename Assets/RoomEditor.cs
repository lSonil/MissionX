using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Room room = (Room)target;
        if (GUILayout.Button("Regenerate Colliders"))
        {
            room.RegenerateColliders();
        }
    }
}
