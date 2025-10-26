using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Terminal : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI terminalText;
    public List<MonsterRoom> allMonsters = new List<MonsterRoom>();

    private List<MonsterRoom> mission1Monsters = new List<MonsterRoom>();
    private List<MonsterRoom> mission2Monsters = new List<MonsterRoom>();
    private List<MonsterRoom> mission3Monsters = new List<MonsterRoom>();
    private List<MonsterRoom> mainMonsters = new List<MonsterRoom>();
    public int maxMissionDifficultyCap = 2;
    public int minMissionDifficultyCap = 2;

    private enum TerminalState { MainMenu, ErrorScreen }
    private TerminalState currentState = TerminalState.MainMenu;

    void OnEnable()
    {
        inputField.Select();
        inputField.ActivateInputField();
    }

    private void OnDisable()
    {
        inputField.DeactivateInputField();
    }

    void Start()
    {
        AssignMonstersToMissions();
        ShowMainMenu();
        StartCoroutine(RefocusInput());
        inputField.onSubmit.AddListener(OnSubmit);
    }

    IEnumerator RefocusInput()
    {
        yield return new WaitForEndOfFrame();
        inputField.Select();
        inputField.DeactivateInputField();
        this.enabled = false;
    }

    void AssignMonstersToMissions()
    {
        List<MonsterRoom> pool = new List<MonsterRoom>(allMonsters);
        Shuffle(pool);

        int cap1 = Random.Range(minMissionDifficultyCap, minMissionDifficultyCap + maxMissionDifficultyCap + 1);
        mission1Monsters = TakeMonstersByDifficulty(pool, cap1);

        int cap2 = Random.Range(minMissionDifficultyCap, minMissionDifficultyCap + maxMissionDifficultyCap + 1);
        mission2Monsters = TakeMonstersByDifficulty(pool, cap2);

        int cap3 = Random.Range(minMissionDifficultyCap, minMissionDifficultyCap + maxMissionDifficultyCap + 1);
        mission3Monsters = TakeMonstersByDifficulty(pool, cap3);
    }

    List<MonsterRoom> TakeMonstersByDifficulty(List<MonsterRoom> pool, int maxDifficulty)
    {
        List<MonsterRoom> result = new List<MonsterRoom>();
        float currentDifficulty = 0;

        while (pool.Count > 0)
        {
            MonsterRoom candidate = pool[0];
            if (currentDifficulty + candidate.monster.difficulty > maxDifficulty)
                break;

            result.Add(candidate);
            currentDifficulty += candidate.monster.difficulty;
            pool.RemoveAt(0);
        }

        return result;
    }

    void Shuffle(List<MonsterRoom> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            MonsterRoom temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }


    void OnSubmit(string input)
    {
        input = input.Trim().ToLower();

        if (input == "esc")
        {
            HandleEscape();
            return;
        }

        if (currentState == TerminalState.ErrorScreen)
        {
            ShowMainMenu();
            inputField.ActivateInputField();
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

        switch (input)
        {
            case "1":
                if (mission1Monsters.Count > 0)
                {
                    terminalText.text = "Traveling to: Mission 1\n\nPress Enter to return.";
                    mainMonsters = new List<MonsterRoom>(mission1Monsters);
                    currentState = TerminalState.ErrorScreen;
                }
                else
                {
                    terminalText.text = "ERROR: Mission 1 is unavailable.\nPress Enter to return to main menu.";
                    currentState = TerminalState.ErrorScreen;
                }
                break;

            case "2":
                if (mission2Monsters.Count > 0)
                {
                    terminalText.text = "Traveling to: Mission 2\n\nPress Enter to return.";
                    mainMonsters = new List<MonsterRoom>(mission2Monsters);
                    currentState = TerminalState.ErrorScreen;
                }
                else
                {
                    terminalText.text = "ERROR: Mission 2 is unavailable.\nPress Enter to return to main menu.";
                    currentState = TerminalState.ErrorScreen;
                }
                break;

            case "3":
                if (mission3Monsters.Count > 0)
                {
                    mainMonsters = new List<MonsterRoom>(mission3Monsters);
                    terminalText.text = "Traveling to: Mission 3\n\nPress Enter to return.";
                    currentState = TerminalState.ErrorScreen;
                }
                else
                {
                    terminalText.text = "ERROR: Mission 3 is unavailable.\nPress Enter to return to main menu.";
                    currentState = TerminalState.ErrorScreen;
                }
                break;

            default:
                terminalText.text = "ERROR: Invalid input.\nPress Enter to return to main menu.";
                currentState = TerminalState.ErrorScreen;
                break;
        }


        inputField.text = "";
        inputField.ActivateInputField();
    }
    void ShowHelp()
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
    public RoomGenerator rooms;
    IEnumerator DelayedSceneLoad()
    {
        terminalText.text = "Initiating jump sequence...\nLoading new scene...";
        HandleEscape();
        yield return new WaitForSeconds(2f);
        foreach (MonsterRoom roomToAdd in mainMonsters)
        {
            rooms.specificRoomsToSpawn.Add(new RoomSpawnEntry(roomToAdd.room,1));
        }

        rooms.gameObject.SetActive(true);
    }

    void LoadTargetScene()
    {
        terminalText.text = "Initiating jump sequence...\nLoading new scene...";
        inputField.DeactivateInputField();
        StartCoroutine(DelayedSceneLoad());
    }

    void ShowMainMenu()
    {
        string menuText = "Choose your mission:\nType help for details\n";

        if (mission1Monsters.Count > 0)
            menuText += $"1 - Mission 1 : {FormatMonsterIDs(mission1Monsters)}\n";
        else
            menuText += "1 - Mission unavailable.\n";

        if (mission2Monsters.Count > 0)
            menuText += $"2 - Mission 2 : {FormatMonsterIDs(mission2Monsters)}\n";
        else
            menuText += "2 - Mission unavailable.\n";
        if (mission3Monsters.Count > 0)
            menuText += $"3 - Mission 3 : {FormatMonsterIDs(mission3Monsters)}\n";
        else
            menuText += "3 - Mission unavailable.\n";

        menuText += "\nEnter a number:";
        terminalText.text = menuText;

        currentState = TerminalState.MainMenu;
        inputField.text = "";
    }


    string FormatMonsterIDs(List<MonsterRoom> monsters)
    {
        string result = "";
        foreach (var m in monsters)
        {
            result += $"[{m.monster.id}]";
        }
        return result;
    }

    void HandleEscape()
    {
        ShowMainMenu();
        inputField.DeactivateInputField();
        GetComponent<Panel>().Action();
    }
}
