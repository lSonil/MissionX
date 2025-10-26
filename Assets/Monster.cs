using UnityEngine;

[CreateAssetMenu(fileName = "NewMonster", menuName = "Game/Monster")]
public class Monster : ScriptableObject
{
    public string id = "000";
    public string monsterName = "no name"; // Renamed to avoid conflict with UnityEngine.Object.name
    public GameObject body;
    public float difficulty = 1;
}
