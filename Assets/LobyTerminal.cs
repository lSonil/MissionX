using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LobyTerminal : Terminal
{
    public List<NPCEntry> allMonsters = new List<NPCEntry>();

    private List<NPCEntry> mission1Monsters = new List<NPCEntry>();
    private List<NPCEntry> mission2Monsters = new List<NPCEntry>();
    private List<NPCEntry> mission3Monsters = new List<NPCEntry>();
    private List<NPCEntry> mainMonsters = new List<NPCEntry>();
    public int maxMissionDifficultyCap = 2;
    public int minMissionDifficultyCap = 2;

    private enum TerminalState { MainMenu, ErrorScreen }
    private TerminalState currentState = TerminalState.MainMenu;
    private int selectedMission = -1; // -1 means no mission selected

    public override void Start()
    {
        AssignMonstersToMissions();
        base.Start();
    }
    void AssignMonstersToMissions()
    {
        List<NPCEntry> pool = new List<NPCEntry>(allMonsters);
        Shuffle(pool);

        int cap1 = Random.Range(minMissionDifficultyCap, minMissionDifficultyCap + maxMissionDifficultyCap + 1);
        mission1Monsters = TakeMonstersByDifficulty(pool, cap1);

        int cap2 = Random.Range(minMissionDifficultyCap, minMissionDifficultyCap + maxMissionDifficultyCap + 1);
        mission2Monsters = TakeMonstersByDifficulty(pool, cap2);

        int cap3 = Random.Range(minMissionDifficultyCap, minMissionDifficultyCap + maxMissionDifficultyCap + 1);
        mission3Monsters = TakeMonstersByDifficulty(pool, cap3);
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
        SceneData.monstersToTransfer = new List<NPCEntry>(mainMonsters);

        //UISystem.i.mTerminal.PrepareInfo(mainMonsters);
        SceneManager.LoadScene("Mission"); // Replace with your actual scene name
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

        // 🔧 Global commands
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

        // 🔧 Mission selection — allowed from any state
        if (input == "1")
        {
            if (mission1Monsters.Count > 0)
            {
                mainMonsters = new List<NPCEntry>(mission1Monsters);
                selectedMission = 1;
                ShowMainMenu();
            }
            else
            {
                terminalText.text = "ERROR: Mission 1 is unavailable.\nPress Enter to return to main menu.";
                currentState = TerminalState.ErrorScreen;
            }
            return;
        }

        if (input == "2")
        {
            if (mission2Monsters.Count > 0)
            {
                mainMonsters = new List<NPCEntry>(mission2Monsters);
                selectedMission = 2;
                ShowMainMenu();
            }
            else
            {
                terminalText.text = "ERROR: Mission 2 is unavailable.\nPress Enter to return to main menu.";
                currentState = TerminalState.ErrorScreen;
            }
            return;
        }

        if (input == "3")
        {
            if (mission3Monsters.Count > 0)
            {
                mainMonsters = new List<NPCEntry>(mission3Monsters);
                selectedMission = 3;
                ShowMainMenu();
            }
            else
            {
                terminalText.text = "ERROR: Mission 3 is unavailable.\nPress Enter to return to main menu.";
                currentState = TerminalState.ErrorScreen;
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

        // 🔧 Fallback for invalid input
        terminalText.text = "ERROR: Invalid input.\nPress Enter to return to main menu.";
        currentState = TerminalState.ErrorScreen;
    }

    public override void ShowHelp()
    {
        string helpText = "Terminal Help Menu\n\n" +
                          "Available commands:\n" +
                          "- 1 : Start Mission 1 (if available)\n" +
                          "- 2 : Start Mission 2 (if available)\n" +
                          "- 3 : Start Mission 3 (if available)\n" +
                          "- go : Load the next scene\n" +
                          "- esc : Return to main menu or exit terminal\n" +
                          "- help : Show this help menu\n\n" +
                          "Press Enter to return to the main menu.";

        terminalText.text = helpText;
        currentState = TerminalState.ErrorScreen;
        inputField.text = "";
        inputField.ActivateInputField();
    }

    public override void ShowMainMenu()
    {
        string menuText = "Choose your mission:\nType help for details\n";

        if (mission1Monsters.Count > 0)
            menuText += $"1 - Mission 1 : {FormatMonsterIDs(mission1Monsters)}\n";

        if (mission2Monsters.Count > 0)
            menuText += $"2 - Mission 2 : {FormatMonsterIDs(mission2Monsters)}\n";

        if (mission3Monsters.Count > 0)
            menuText += $"3 - Mission 3 : {FormatMonsterIDs(mission3Monsters)}\n";

        string selectedText = selectedMission switch
        {
            1 => "1",
            2 => "2",
            3 => "3",
            _ => "N/A"
        };

        menuText += $"\nSelected mission: {selectedText}";
        menuText += "\nEnter a number:";

        terminalText.text = menuText;
        currentState = TerminalState.MainMenu;
        inputField.text = "";
    }
}
