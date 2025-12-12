using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActionList))]
public class ActionListEditor : Editor
{
    SerializedProperty actionsProp;

    private void OnEnable()
    {
        actionsProp = serializedObject.FindProperty("actions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        for (int i = 0; i < actionsProp.arraySize; i++)
        {
            SerializedProperty element = actionsProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");

            DrawActionElement(element, i);

            if (GUILayout.Button("Remove Action"))
                actionsProp.DeleteArrayElementAtIndex(i);

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Action"))
            actionsProp.InsertArrayElementAtIndex(actionsProp.arraySize);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawActionElement(SerializedProperty element, int index)
    {
        SerializedProperty target = element.FindPropertyRelative("target");
        SerializedProperty type = element.FindPropertyRelative("type");
        SerializedProperty axis = element.FindPropertyRelative("axis");
        SerializedProperty openValue = element.FindPropertyRelative("openValue");
        SerializedProperty closedValue = element.FindPropertyRelative("closedValue");
        SerializedProperty duration = element.FindPropertyRelative("duration");
        SerializedProperty mode = element.FindPropertyRelative("mode");
        SerializedProperty reverseMode = element.FindPropertyRelative("reverseMode");
        SerializedProperty objectToSpawn = element.FindPropertyRelative("objectToSpawn");

        EditorGUILayout.LabelField($"Element {index}", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(target);
        EditorGUILayout.PropertyField(type);

        ActionType t = (ActionType)type.enumValueIndex;

        switch (t)
        {
            case ActionType.Move:
            case ActionType.Rotate:
            case ActionType.Resize:
                EditorGUILayout.PropertyField(axis);
                EditorGUILayout.PropertyField(openValue);
                EditorGUILayout.PropertyField(closedValue);
                EditorGUILayout.PropertyField(duration);
                break;

            case ActionType.Spawn:
                EditorGUILayout.PropertyField(objectToSpawn);
                break;

            case ActionType.PlaySound:
            case ActionType.Contain:
                break;
        }

        EditorGUILayout.PropertyField(mode);
        EditorGUILayout.PropertyField(reverseMode);
    }
}
