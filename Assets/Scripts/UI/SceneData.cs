using System;
using System.Collections.Generic;
using UnityEngine;
public enum Debuff
{
    DoubleQuota,
    SlowSpeed,
    NoFlashlight,
    NoJump
}

public static class EffectWeights
{
    private static readonly Dictionary<Enum, int> Weights = new()
    {
        // Debuffs
        { Debuff.DoubleQuota, 3 },
        { Debuff.SlowSpeed, 1 },
        { Debuff.NoFlashlight, 2 },
        { Debuff.NoJump, 2 },

        // Buffs
        { Buff.Halfquota, 3 },
        { Buff.DoubleJump, 2 },
        { Buff.Minimap, 2 },
        { Buff.Regen, 1 }
    };

    public static int GetWeight(Enum effect)
    {
        return Weights[effect];
    }
}

public enum Buff
{
    Halfquota,
    DoubleJump,
    Minimap,
    Regen
}
[System.Serializable]

public class BuffState
{
    public Buff buff;
    public int times;
}
[System.Serializable]
public class DebuffState
{
    public Debuff debuff;
    public int times;
}
[System.Serializable]
public class MissionData
{
    public List<NPCEntry> monsters = new List<NPCEntry>();
    public List<Buff> buffs = new List<Buff>();
    public List<Debuff> debuffs = new List<Debuff>();
}

public static class SceneData
{
    public static int day;

    public static int numberOfRooms = 50;
    public static int itemWeightCap = 200;
    public static int totalDataToErase = 500;
    public static int monsterDiffMap = 1;
    public static int buffCap = 1;
    public static int currentMissionItemWeight;
    public static int currentStoredItemWeight;
    public static int currentTodaySavedItemWeight;
    public static int currentAllSavedItemWeight;

    public static bool showResults = false;
    public static Dictionary<string, ContainedState> containmentResults = new Dictionary<string, ContainedState>();

    public static int maxMissions = 4;
    public static MissionData missionToTransfer;
    public static List<NPCBase> npcInScene;
    public static List<MissionData> lobbyMissions = new List<MissionData>();

    public static int GetItemsValue() => itemWeightCap + itemWeightCap * MathFunc.Triangular(day) / 100;
    public static int GetNumberOfRooms() => numberOfRooms + day * 5;
    public static int GetMonsterDifficulty() => monsterDiffMap + MathFunc.Fibonacci(day);
    public static int GetBuffDebuffMax() => buffCap + day;
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

        containmentResults = new Dictionary<string, ContainedState>(results);
        day++;
    }
    public static void AssignMonstersToMissions(List<NPCEntry> allMonsters)
    {
        npcInScene = null;

        missionToTransfer = new MissionData();

        lobbyMissions.Clear();

        int availableMissions = GetAvailableMissionCount();

        List<NPCEntry> monsterPool = new List<NPCEntry>(allMonsters);
        MathFunc.Shuffle(monsterPool);

        for (int m = 0; m < availableMissions; m++)
        {
            MissionData mission = new MissionData();

            mission.monsters = NPCSelectionFunc.TakeMonstersByDifficulty(monsterPool, GetMonsterDifficulty());

            var buffPool = NPCSelectionFunc.GetBuffPool();
            mission.buffs = NPCSelectionFunc.TakeByDifficultyEnum(buffPool, GetBuffDebuffMax());

            var debuffPool = NPCSelectionFunc.GetDebuffPool();
            mission.debuffs = NPCSelectionFunc.TakeByDifficultyEnum(debuffPool, GetBuffDebuffMax());

            lobbyMissions.Add(mission);
        }
    }
}