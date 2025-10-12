using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomSpawner))]
public class RoomSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RoomSpawner spawner = (RoomSpawner)target;

        if (spawner.roomPool == null || spawner.roomPool.Count == 0)
        {
            base.OnInspectorGUI();
            return;
        }

        EditorGUILayout.LabelField("Room Pool (Weights must sum to 1.00)", EditorStyles.boldLabel);

        float totalWeight = 0f;

        for (int i = 0; i < spawner.roomPool.Count; i++)
        {
            RoomEntry entry = spawner.roomPool[i];

            EditorGUI.BeginChangeCheck();
            float newWeight = EditorGUILayout.Slider(entry.prefab.name, entry.weight, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                newWeight = Mathf.Round(newWeight * 100f) / 100f;

                float delta = newWeight - entry.weight;
                float remaining = 1f - newWeight;

                // Update selected entry
                RoomEntry updated = entry;
                updated.weight = newWeight;
                spawner.roomPool[i] = updated;

                // Adjust others proportionally
                float totalOthers = 0f;
                for (int j = 0; j < spawner.roomPool.Count; j++)
                {
                    if (j != i) totalOthers += spawner.roomPool[j].weight;
                }

                for (int j = 0; j < spawner.roomPool.Count; j++)
                {
                    if (j == i) continue;

                    RoomEntry other = spawner.roomPool[j];
                    float proportion = totalOthers > 0f ? other.weight / totalOthers : 1f / (spawner.roomPool.Count - 1);
                    other.weight = Mathf.Clamp01(Mathf.Round((proportion * remaining) * 100f) / 100f);
                    spawner.roomPool[j] = other;
                }

                EditorUtility.SetDirty(spawner);
            }

            totalWeight += spawner.roomPool[i].weight;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Total Weight: {totalWeight:F2}", EditorStyles.helpBox);

        DrawDefaultInspector();
    }
}
