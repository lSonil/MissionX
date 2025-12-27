using System.Collections.Generic;

public static class LobyData
{
    public static List<PlayerCore> players = new List<PlayerCore>();
    public static void Register(PlayerCore core) { if (!players.Contains(core)) players.Add(core); }
    public static void Unregister(PlayerCore core) { players.Remove(core); }
}
