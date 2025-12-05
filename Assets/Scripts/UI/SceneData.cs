using System.Collections.Generic;
using static LobyTerminal;
public enum Debuffs
{
    endGame = 10,
    slowSpeed = 1,
    noFlashlight = 2,
    noJump = 2
}
public enum Buffs
{
    nothing = 10,
    fastSpeed = 2,
    lowGravity = 1
}
public static class SceneData
{
    public static int day;
    public static bool showResults=false;
    public static Dictionary<string, ContainedState> containmentResults = new Dictionary<string, ContainedState>();

    public static MissionData missionToTransfer;
    public static int maxMissions = 4;

    public static int GetAvailableMissionCount()
    {
        int cycle = day % maxMissions;
        int available = maxMissions - cycle;

        if (available < 1)
            available = 1;

        return available;
    }
    public static void SetMissionData(MissionData mission)
    {
        missionToTransfer = mission;
    }

    public static void PrepareResults(bool state = true)
    {
        showResults = state;
    }
    public static void IncrementDay()
    {
        day++;
    }
    public static void StoreContainmentResults(Dictionary<string, ContainedState> results)
    {
        containmentResults = new Dictionary<string, ContainedState>(results);
    }
}
