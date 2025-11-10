using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterRoom", menuName = "Game/NPC")]
public class NPCEntry : ScriptableObject
{
    public string id = "000";
    public string monsterName = "no name"; // Renamed to avoid conflict with UnityEngine.Object.name
    public float difficulty = 1;

    public Room room;
    public NPCBase npc;
}
