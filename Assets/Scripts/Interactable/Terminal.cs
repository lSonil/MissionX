using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Terminal : MonoBehaviour
{

    public TMP_InputField inputField;
    public TextMeshProUGUI terminalText;
    public float scrollSpeed = 900f; // pixels per wheel tick
    public ScrollRect scrollRect;

    private RectTransform content;
    private RectTransform viewport;
    private string lastText = "";
    public enum TerminalState { MainMenu, ErrorScreen }
    private TerminalState currentState = TerminalState.MainMenu;
    public enum TerminalType { LobyTerminal, GameTerminal, Terminal }
    public TerminalType terminalType = TerminalType.Terminal;

    private int selectedMission = -1;

    void Update()
    {
        if (terminalType == TerminalType.GameTerminal && SceneManager.GetActiveScene().name == "Lobby")
        {
            terminalType = TerminalType.LobyTerminal;
        }
        if (terminalType == TerminalType.LobyTerminal && SceneManager.GetActiveScene().name != "Lobby")
        {
            terminalType = TerminalType.GameTerminal;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnSubmit("esc");
        }

        HandleScrollWheel();

        if (terminalText != null && terminalText.text != lastText)
        {
            lastText = terminalText.text;
            AdjustContentHeight();
        }
    }

    void HandleScrollWheel()
    {
        if (scrollRect == null || content == null || viewport == null) return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;
        float overflow = contentHeight - viewportHeight;

        if (overflow <= 0f) return;

        float delta = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(delta) < 0.001f) return;

        float pixelScroll = -delta * scrollSpeed;

        Vector2 pos = content.anchoredPosition;
        pos.y = Mathf.Clamp(pos.y + pixelScroll, 0f, overflow);
        content.anchoredPosition = pos;
    }

    public void Start()
    {
        if (scrollRect != null)
        {
            content = scrollRect.content;
            viewport = scrollRect.viewport != null
                ? scrollRect.viewport
                : scrollRect.GetComponent<RectTransform>();
        }

        ShowMainMenu();
        StartCoroutine(RefocusInput());
        inputField.onSubmit.AddListener(OnSubmit);

        if (terminalText != null)
        {
            lastText = terminalText.text;
            AdjustContentHeight();
        }
    }

    protected void SetTerminalText(string newText)
    {
        if (terminalText == null) return;
        terminalText.text = newText;
        lastText = newText;
        AdjustContentHeight();
    }

    private void AdjustContentHeight()
    {
        if (content == null || terminalText == null) return;

        terminalText.ForceMeshUpdate();
        float preferredHeight = terminalText.preferredHeight;

        Vector2 size = content.sizeDelta;
        size.y = preferredHeight;
        content.sizeDelta = size;

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        content.anchoredPosition = Vector2.zero;
    }

    IEnumerator RefocusInput()
    {
        yield return new WaitForEndOfFrame();
        inputField.Select();
        inputField.DeactivateInputField();
        this.enabled = false;
    }

    public void HandleEscape()
    {
        inputField.DeactivateInputField();
        print(EventSystem.current);
        EventSystem.current.SetSelectedGameObject(null);
        GetComponent<Display>().Action();
        ShowMainMenu();
    }

    void OnEnable()
    {
        inputField.Select();
        inputField.ActivateInputField();
    }

    void OnDisable()
    {
        inputField.DeactivateInputField();
    }
    IEnumerator LoadTargetScene()
    {
        terminalText.text = "Initiating jump sequence...\nLoading new scene...";
        HandleEscape();

        switch (terminalType)
        {
            case TerminalType.LobyTerminal:
                yield return new WaitForSeconds(2f);

                SceneManager.LoadSceneAsync("Mission");
                selectedMission = -1;
                terminalType = TerminalType.GameTerminal;
                while (SceneData.npcInScene == null)
                {
                    yield return null;
                }
                ShowMainMenu();
                StartCoroutine(MonitorContainmentStates());
                break;

            case TerminalType.GameTerminal:
                SceneData.DayEnd();
                yield return new WaitForSeconds(3f);

                terminalType = TerminalType.LobyTerminal;

                //foreach (PlayerCore p in Lobby.i.players)
                //    p.uis.endOfMissionCamera.SetActive(true);
                //
                //yield return new WaitForSeconds(3f);
                //
                //foreach (PlayerCore p in Lobby.i.players)
                //    p.uis.endOfMissionCamera.SetActive(false);
                ShowMainMenu();
                Lobby.i.SetBuffsAndDebuffs(SceneData.containmentResults, SceneData.missionToTransfer);

                foreach (PlayerCore p in Lobby.i.players)
                    StartCoroutine(p.uis.ShowResults());
                SceneManager.LoadScene("Lobby");
                SceneData.AssignMonstersToMissions(Lobby.i.possibleNPC);
                break;
        }
    }
    public void OnSubmit(string input)
    {
        input = input.Trim().ToLower();
        inputField.text = "";
        inputField.ActivateInputField();

        if (string.IsNullOrEmpty(input))
        {
            if (currentState == TerminalState.ErrorScreen)
            {
                ShowMainMenu();
            }
            return;
        }
        if (input == "esc")
        {
            HandleEscape();
            return;
        }
        if (input == "help")
        {
            ShowHelp();
            return;
        }
        if (input == "go")
        {
            if (SceneData.missionToTransfer.monsters.Count > 0)
            {
                StartCoroutine(LoadTargetScene());
            }
            else
            {
                terminalText.text = "NoMissionSelected\n\nPress Enter to return.";
                currentState = TerminalState.ErrorScreen;
            }
            return;
        }
        if (terminalType == TerminalType.LobyTerminal)
        {
            int missionIndex;
            if (int.TryParse(input, out missionIndex))
            {
                if (missionIndex >= 1 && missionIndex <= SceneData.lobbyMissions.Count)
                {
                    if (SceneData.lobbyMissions[missionIndex - 1].monsters.Count > 0)
                    {
                        SceneData.missionToTransfer = SceneData.lobbyMissions[missionIndex - 1];
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
        }
        terminalText.text = "ERROR: Invalid input.\nPress Enter to return to main menu.";
        currentState = TerminalState.ErrorScreen;
    }
    public void ShowMainMenu()
    {
        string menuText = "";
        switch (terminalType)
        {
            case TerminalType.LobyTerminal:
                menuText = "Choose your mission:\nType help for details\n";

                for (int m = 0; m < SceneData.lobbyMissions.Count; m++)
                {
                    if (SceneData.lobbyMissions[m].monsters.Count > 0)
                    {
                        menuText += $"{m + 1} - Mission {m + 1} : {NPCSelectionFunc.FormatMonsterIDs(SceneData.lobbyMissions[m].monsters)}\n";
                        menuText += $"   Buffs: {NPCSelectionFunc.FormatBuffs(SceneData.lobbyMissions[m].buffs)}\n";
                        menuText += $"   Debuffs: {NPCSelectionFunc.FormatDebuffs(SceneData.lobbyMissions[m].debuffs)}\n";
                    }
                }

                string selectedText = selectedMission > 0 ? selectedMission.ToString() : "N/A";
                menuText += $"\nSelected mission: {selectedText}";
                menuText += "\nEnter a number:";

                break;
            case TerminalType.GameTerminal:
                menuText = "Mission Overview:\n";

                if (SceneData.missionToTransfer.monsters.Count == SceneData.npcInScene.Count)
                    for (int i = 0; i < SceneData.missionToTransfer.monsters.Count; i++)
                    {
                        string state = SceneData.npcInScene[i].contained == ContainedState.Free ? " " : SceneData.npcInScene[i].contained == ContainedState.Contained ? "o" : "x";
                        menuText += $"- {SceneData.missionToTransfer.monsters[i].id} [{state}]\n";
                    }

                menuText += "\nType 'help' for assistance or 'esc' to exit.";

                break;
            default:
                break;
        }
        terminalText.text = menuText;
        currentState = TerminalState.MainMenu;
        inputField.text = "";
    }
    public void ShowHelp()
    {
        string helpText = "";
        switch (terminalType)
        {
            case TerminalType.LobyTerminal:
                helpText = "Terminal Help Menu\n\n" +
                           "Available commands:\n";

                for (int m = 0; m < SceneData.lobbyMissions.Count; m++)
                {
                    helpText += $"- {m + 1} : Start Mission {m + 1} (if available)\n";
                }

                helpText += "- go : Load the next scene\n" +
                            "- esc : Return to main menu or exit terminal\n" +
                            "- help : Show this help menu\n\n" +
                            "Press Enter to return to the main menu.";
                break;
            case TerminalType.GameTerminal:
                helpText = "Mission Terminal Help Menu\n\n" +
                  "Available commands:\n" +
                  "- go : Load the next scene\n" +
                  "- esc : Return to main menu or exit terminal\n" +
                  "- help : Show this help menu\n\n" +
                  "Press Enter to return to the main menu.";

                break;
            default:
                break;
        }
        terminalText.text = helpText;
        currentState = TerminalState.ErrorScreen;
        inputField.text = "";
        inputField.ActivateInputField();

    }

    private List<ContainedState> previousStates = new List<ContainedState>();

    IEnumerator MonitorContainmentStates()
    {
        bool print = true;
        previousStates = Enumerable.Repeat(ContainedState.Free, SceneData.npcInScene.Count).ToList();

        while (terminalType == TerminalType.GameTerminal)
        {
            for (int i = 0; i < SceneData.npcInScene.Count; i++)
            {
                var current = SceneData.npcInScene[i].contained;
                var previous = previousStates[i];

                if (current != previous)
                {
                    string name = SceneData.missionToTransfer.monsters[i].id;

                    if (previous == ContainedState.Contained && current == ContainedState.Free)
                    {
                        foreach (PlayerCore p in Lobby.i.players)
                            p.uis.popUpHanle.ShowPopUp("r", name);
                        print = true;
                    }
                    else if (previous == ContainedState.Free && (current == ContainedState.Contained || current == ContainedState.Suppressed))
                    {
                        foreach (PlayerCore p in Lobby.i.players)

                            p.uis.popUpHanle.ShowPopUp("y", name);
                        print = true;
                    }

                    previousStates[i] = current;
                }
            }

            if (SceneData.npcInScene.All(n => n.contained == ContainedState.Contained || n.contained == ContainedState.Suppressed) && print)
            {
                print = false;
                foreach (PlayerCore p in Lobby.i.players)
                    p.uis.popUpHanle.ShowPopUp("g", "All contained");
            }

            yield return new WaitForSeconds(0.5f); // Adjust polling rate as needed
        }
    }
}
