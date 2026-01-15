using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Terminal : MonoBehaviour
{
    PlayerCore playerUsingTerminal;
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

    void Update()
    {
        if(!isTurnOn) return;
        if(changing) return;
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

        inputField.onSubmit.AddListener(OnSubmit);
        ShowMainMenu();
        if (terminalText != null)
        {
            lastText = terminalText.text;
            AdjustContentHeight();
        }
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

    bool isTurnOn;
    bool changing;
    public PlayerCore TurnOn(PlayerCore p)
    {
        PlayerCore uP;
        if (isTurnOn)
        {
            inputField.DeactivateInputField();
            uP = playerUsingTerminal;
            playerUsingTerminal =null;
        }
        else
        {
            inputField.Select();
            inputField.ActivateInputField();
            playerUsingTerminal = p;
            uP = p;
        }
        isTurnOn = !isTurnOn;
        return uP;
    }
    public void HandleEscape()
    {
        inputField.DeactivateInputField();
        EventSystem.current.SetSelectedGameObject(null);
        GetComponent<Display>().Action(null,playerUsingTerminal);
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
                HandleEscape();
                Go();
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
                        LobyManager.i.selectedMission = missionIndex;
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
    public void Go()
    {
        changing = true;
        terminalText.text = "Initiating jump sequence...\nLoading...";
        StartCoroutine(LobyManager.i.LoadMision());
        terminalType = TerminalType.LobyTerminal == terminalType ? TerminalType.GameTerminal : TerminalType.LobyTerminal;
    }
    public void ShowMainMenu()
    {
        changing = false;

        string menuText = "";
        switch (terminalType)
        {
            case TerminalType.LobyTerminal:

                int remainder = SceneData.day % 4;

                if (remainder == 0 && SceneData.day != 0)
                {
                    menuText += "Deadline today\n";
                }
                else
                {
                    int daysLeft = remainder == 0 ? 4 : 4 - remainder;
                    menuText += $"Days until deadline: {daysLeft}\n\n";
                }

                menuText += $"This week Quota: {SceneData.currentStoredItemWeight}/{SceneData.GetTotalDataValue()}\n";
                menuText += $"Colected data: {SceneData.GetTotalSavedDataValue()}\n";
                menuText += "Choose your mission:\n\nType help for details\n\n";

                for (int m = 0; m < SceneData.lobbyMissions.Count; m++)
                {
                    if (SceneData.lobbyMissions[m].monsters.Count > 0)
                    {
                        menuText += $"{m + 1} - Mission {m + 1} : {NPCSelectionFunc.FormatMonsterIDs(SceneData.lobbyMissions[m].monsters)}\n";
                        menuText += $"   Buffs: {NPCSelectionFunc.FormatBuffs(SceneData.lobbyMissions[m].buffs)}\n";
                        menuText += $"   Debuffs: {NPCSelectionFunc.FormatDebuffs(SceneData.lobbyMissions[m].debuffs)}\n";
                    }
                }

                string selectedText = LobyManager.i.selectedMission > 0 ? LobyManager.i.selectedMission.ToString() : "N/A";
                menuText += $"\nSelected mission: {selectedText}";
                menuText += "\nEnter a number:";

                break;
            case TerminalType.GameTerminal:
                menuText = "Mission Overview:\n";

                if (SceneData.missionToTransfer.monsters.Count == SceneData.npcInScene.Count)
                {
                    for (int i = 0; i < SceneData.missionToTransfer.monsters.Count; i++)
                    {
                        string state = SceneData.npcInScene[i].contained == ContainedState.Free ? " " : SceneData.npcInScene[i].contained == ContainedState.Contained ? "o" : "x";
                        menuText += $"- {SceneData.missionToTransfer.monsters[i].id} [{state}]\n";
                    }
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
}
