using System.Collections.Generic;
using UnityEngine;

public static class NPCSelectionFunc
{
    public static List<Buffs> GetBuffPool()
    {
        return new List<Buffs>((Buffs[])System.Enum.GetValues(typeof(Buffs)));
    }
    public static List<Debuffs> GetDebuffPool()
    {
        return new List<Debuffs>((Debuffs[])System.Enum.GetValues(typeof(Debuffs)));
    }
    public static List<NPCEntry> TakeMonstersByDifficulty(List<NPCEntry> pool, int maxDifficulty)
    {
        List<NPCEntry> result = new List<NPCEntry>();
        float currentDifficulty = 0;

        while (pool.Count > 0)
        {
            NPCEntry entry = pool[0];
            if (currentDifficulty + entry.difficulty > maxDifficulty)
                break;

            result.Add(entry);
            currentDifficulty += entry.difficulty;
            pool.RemoveAt(0);
        }

        return result;
    }
    public static List<T> TakeByDifficultyEnum<T>(List<T> pool, int maxDifficulty) where T : System.Enum
    {
        MathFunc.Shuffle(pool);

        List<T> result = new List<T>();
        int current = 0;

        foreach (var e in pool)
        {
            int cost = (int)(object)e;
            if (current + cost <= maxDifficulty)
            {
                result.Add(e);
                current += cost;
            }
        }

        if (result.Count == 0 && pool.Count > 0)
        {
            T cheapest = pool[0];
            int cheapestCost = (int)(object)cheapest;

            for (int i = 1; i < pool.Count; i++)
            {
                int c = (int)(object)pool[i];
                if (c < cheapestCost)
                {
                    cheapest = pool[i];
                    cheapestCost = c;
                }
            }

            result.Add(cheapest);
        }

        return result;
    }


    public static string FormatBuffs(List<Buffs> buffs)
    {
        string result = "";
        foreach (var b in buffs)
            result += $"[{b}]";
        return result;
    }

    public static string FormatDebuffs(List<Debuffs> debuffs)
    {
        string result = "";
        foreach (var d in debuffs)
            result += $"[{d}]";
        return result;
    }
    public static string FormatMonsterIDs(List<NPCEntry> monsters)
    {
        string result = "";
        foreach (var e in monsters)
        {
            result += $"[{e.id}]";
        }
        return result;
    }
}
