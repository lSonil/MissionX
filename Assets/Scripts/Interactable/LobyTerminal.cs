using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobyTerminal : Terminal
{
    public static LobyTerminal i;
    public List<NPCEntry> allMonsters = new List<NPCEntry>();

    [System.Serializable]
    public class MissionData
    {
        public List<NPCEntry> monsters = new List<NPCEntry>();
        public List<Buffs> buffs = new List<Buffs>();
        public List<Debuffs> debuffs = new List<Debuffs>();
    }

    public List<MissionData> missions = new List<MissionData>();

    private List<NPCEntry> mainMonsters = new List<NPCEntry>();

    public int maxMissionDifficultyCap = 2;
    public int minMissionDifficultyCap = 2;

    private enum TerminalState { MainMenu, ErrorScreen }
    private TerminalState currentState = TerminalState.MainMenu;
    private int selectedMission = -1;
    List<Buffs> GetBuffPool()
    {
        return new List<Buffs>((Buffs[])System.Enum.GetValues(typeof(Buffs)));
    }

    List<Debuffs> GetDebuffPool()
    {
        return new List<Debuffs>((Debuffs[])System.Enum.GetValues(typeof(Debuffs)));
    }

    // Generic enum selection using cap, with guaranteed fallback
    List<T> TakeByDifficultyEnum<T>(List<T> pool, int maxDifficulty) where T : System.Enum
    {
        // Shuffle for randomness
        for (int i = 0; i < pool.Count; i++)
        {
            int rand = Random.Range(i, pool.Count);
            T tmp = pool[i];
            pool[i] = pool[rand];
            pool[rand] = tmp;
        }

        List<T> result = new List<T>();
        int current = 0;

        // Try to add as many as fit within the cap
        foreach (var e in pool)
        {
            int cost = (int)(object)e;
            if (current + cost <= maxDifficulty)
            {
                result.Add(e);
                current += cost;
            }
        }

        // If none fit, pick the cheapest one to ensure at least one
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


    public override void Start()
    {
        i = this;
        AssignMonstersToMissions();
        base.Start();
    }

    void AssignMonstersToMissions()
    {
        missions.Clear();

        int availableMissions = SceneData.GetAvailableMissionCount();

        List<NPCEntry> monsterPool = new List<NPCEntry>(allMonsters);
        Shuffle(monsterPool);

        for (int m = 0; m < availableMissions; m++)
        {
            MissionData mission = new MissionData();

            int cap = Random.Range(minMissionDifficultyCap, minMissionDifficultyCap + maxMissionDifficultyCap + 1);

            mission.monsters = TakeMonstersByDifficulty(monsterPool, cap);

            if (availableMissions == 1)
            {
                // Special case: only one mission
                mission.buffs = new List<Buffs> { Buffs.nothing };
                mission.debuffs = new List<Debuffs> { Debuffs.endGame };
            }
            else
            {
                var buffPool = GetBuffPool();
                mission.buffs = TakeByDifficultyEnum(buffPool, cap);

                var debuffPool = GetDebuffPool();
                mission.debuffs = TakeByDifficultyEnum(debuffPool, cap);
            }

            missions.Add(mission);
        }
    }


    List<NPCEntry> TakeMonstersByDifficulty(List<NPCEntry> pool, int maxDifficulty)
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

    void Shuffle(List<NPCEntry> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            NPCEntry temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    IEnumerator DelayedSceneLoad()
    {
        terminalText.text = "Initiating jump sequence...\nLoading new scene...";
        HandleEscape();
        yield return new WaitForSeconds(2f);
        MissionData selected = missions[selectedMission - 1];
        SceneData.SetMissionData(selected);
        SceneManager.LoadScene("Mission");
    }

    void LoadTargetScene()
    {
        terminalText.text = "Initiating jump sequence...\nLoading new scene...";
        inputField.DeactivateInputField();
        StartCoroutine(DelayedSceneLoad());
    }

    string FormatMonsterIDs(List<NPCEntry> monsters)
    {
        string result = "";
        foreach (var e in monsters)
        {
            result += $"[{e.id}]";
        }
        return result;
    }

    public override void OnSubmit(string input)
    {
        input = input.Trim().ToLower();
        inputField.text = "";
        inputField.ActivateInputField();

        if (input == "esc")
        {
            HandleEscape();
            return;
        }

        if (input == "go")
        {
            if (mainMonsters.Count > 0)
            {
                LoadTargetScene();
            }
            else
            {
                terminalText.text = "NoMissionSelected\n\nPress Enter to return.";
                currentState = TerminalState.ErrorScreen;
            }
            return;
        }

        if (input == "help")
        {
            ShowHelp();
            return;
        }

        int missionIndex;
        if (int.TryParse(input, out missionIndex))
        {
            if (missionIndex >= 1 && missionIndex <= missions.Count)
            {
                MissionData chosen = missions[missionIndex - 1];
                if (chosen.monsters.Count > 0)
                {
                    mainMonsters = new List<NPCEntry>(chosen.monsters);
                    selectedMission = missionIndex;
                    ShowMainMenu();
                }
                else
                {
                    terminalText.text = $"ERROR: Mission {missionIndex} is unavailable.\nPress Enter to return to main menu.";
                    currentState = TerminalState.ErrorScreen;
                }
            }
            return;
        }

        if (string.IsNullOrEmpty(input))
        {
            if (currentState == TerminalState.ErrorScreen)
            {
                ShowMainMenu();
            }
            return;
        }

        terminalText.text = "ERROR: Invalid input.\nPress Enter to return to main menu.";
        currentState = TerminalState.ErrorScreen;
    }

    public override void ShowHelp()
    {
        string helpText = "Terminal Help Menu\n\n" +
                          "Available commands:\n";

        for (int m = 0; m < missions.Count; m++)
        {
            helpText += $"- {m + 1} : Start Mission {m + 1} (if available)\n";
        }

        helpText += "- go : Load the next scene\n" +
                    "- esc : Return to main menu or exit terminal\n" +
                    "- help : Show this help menu\n\n" +
                    "Press Enter to return to the main menu.";

        terminalText.text = helpText;
        currentState = TerminalState.ErrorScreen;
        inputField.text = "";
        inputField.ActivateInputField();
    }

    string FormatBuffs(List<Buffs> buffs)
    {
        string result = "";
        foreach (var b in buffs)
            result += $"[{b}]";
        return result;
    }

    string FormatDebuffs(List<Debuffs> debuffs)
    {
        string result = "";
        foreach (var d in debuffs)
            result += $"[{d}]";
        return result;
    }

    public override void ShowMainMenu()
    {
        string menuText = "Choose your mission:\nType help for details\n";

        for (int m = 0; m < missions.Count; m++)
        {
            if (missions[m].monsters.Count > 0)
            {
                menuText += $"{m + 1} - Mission {m + 1} : {FormatMonsterIDs(missions[m].monsters)}\n";
                menuText += $"   Buffs: {FormatBuffs(missions[m].buffs)}\n";
                menuText += $"   Debuffs: {FormatDebuffs(missions[m].debuffs)}\n";
            }
        }

        string selectedText = selectedMission > 0 ? selectedMission.ToString() : "N/A";
        menuText += $"\nSelected mission: {selectedText}";
        menuText += "\nEnter a number:";

        terminalText.text = menuText;
        currentState = TerminalState.MainMenu;
        inputField.text = "";
    }

}
