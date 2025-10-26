using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterRoom", menuName = "Game/MonsterRoom")]
public class MonsterRoom : ScriptableObject
{
    public Room room;
    public Monster monster;
}
