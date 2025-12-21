using System.Collections.Generic;
using UnityEngine;
public enum Debuffs
{
    doublequota = 3,
    slowSpeed = 1,
    noFlashlight = 2,
    noJump = 2
}
public enum Buffs
{
    halfquota = 3,
    fastSpeed = 1,
    minimap = 2,
    longrangeflashlight = 1
}

[System.Serializable]
public class BuffState
{
    public Buffs buff;
    public int times;
}
[System.Serializable]
public class DebuffState
{
    public Debuffs debuff;
    public int times;
}
[System.Serializable]
public class MissionData
{
    public List<NPCEntry> monsters = new List<NPCEntry>();
    public List<Buffs> buffs = new List<Buffs>();
    public List<Debuffs> debuffs = new List<Debuffs>();
}

public static class SceneData
{
    public static int day;

    public static int numberOfRooms = 50;
    public static int itemWeightCap = 200;
    public static int totalDataToErase = 500;
    public static int monsterDiffMap = 1;

    public static bool showResults = false;
    public static Dictionary<string, ContainedState> containmentResults = new Dictionary<string, ContainedState>();

    public static int maxMissions = 4;
    public static MissionData missionToTransfer;
    public static List<NPCBase> npcInScene;
    public static List<MissionData> lobbyMissions = new List<MissionData>();

    public static int GetNumberOfRooms() => numberOfRooms + day * 5;
    public static int GetItemsValue() => itemWeightCap + itemWeightCap * MathFunc.Triangular(day) / 100;
    public static int GetMonsterDifficulty() => monsterDiffMap + MathFunc.Fibonacci(day);

    public static int GetTotalDataValue()
    {
        float fastScale = itemWeightCap * 0.5f;

        int baseValue = totalDataToErase;
        float curve = fastScale * ((day - 1) / maxMissions) * Mathf.Log(((day - 1) / maxMissions) + 2);

        return Mathf.FloorToInt(baseValue + curve);
    }
    public static int GetAvailableMissionCount()
    {
        int cycle = day % maxMissions;
        int available = maxMissions - cycle;

        if (available < 1)
            available = 1;

        return available;
    }
    public static void DayEnd()
    {
        Dictionary<string, ContainedState> results = new Dictionary<string, ContainedState>();

        for (int i = 0; i < missionToTransfer.monsters.Count; i++)
        {
            string id = missionToTransfer.monsters[i].id;
            ContainedState state = npcInScene[i].contained;
            results[id] = state;
        }
        Debug.Log(missionToTransfer.debuffs.Count);
        Debug.Log(missionToTransfer.buffs.Count);
        containmentResults = new Dictionary<string, ContainedState>(results);
        day++;
    }
    public static void AssignMonstersToMissions(List<NPCEntry> allMonsters)
    {
        npcInScene = null;

        missionToTransfer = new MissionData();

        lobbyMissions.Clear();

        int availableMissions = SceneData.GetAvailableMissionCount();

        List<NPCEntry> monsterPool = new List<NPCEntry>(allMonsters);
        MathFunc.Shuffle(monsterPool);

        for (int m = 0; m < availableMissions; m++)
        {
            MissionData mission = new MissionData();

            mission.monsters = NPCSelectionFunc.TakeMonstersByDifficulty(monsterPool, SceneData.GetMonsterDifficulty());

            var buffPool = NPCSelectionFunc.GetBuffPool();
            mission.buffs = NPCSelectionFunc.TakeByDifficultyEnum(buffPool, SceneData.GetMonsterDifficulty());

            var debuffPool = NPCSelectionFunc.GetDebuffPool();
            mission.debuffs = NPCSelectionFunc.TakeByDifficultyEnum(debuffPool, SceneData.GetMonsterDifficulty());

            lobbyMissions.Add(mission);
        }
    }
}