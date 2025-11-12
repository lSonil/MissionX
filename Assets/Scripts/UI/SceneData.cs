using System.Collections.Generic;

public static class SceneData
{
    public static List<NPCEntry> monstersToTransfer = new List<NPCEntry>();
    public static int day;
    public static bool showResults;
    public static Dictionary<string, ContainedState> containmentResults = new Dictionary<string, ContainedState>();

    public static void SetMonsters(List<NPCEntry> monstersOfTheDay)
    {
        monstersToTransfer = new List<NPCEntry>(monstersOfTheDay); ;
    }
    public static void PrepareResults(bool state=true)
    {
        showResults = state;
        day++;
    }
    public static void StoreContainmentResults(Dictionary<string, ContainedState> results)
    {
        containmentResults = new Dictionary<string, ContainedState>(results);
    }
}
