using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class MissionTerminal : Terminal
{
    public static MissionTerminal i;
    private enum TerminalState { MainMenu, ErrorScreen }
    private TerminalState currentState = TerminalState.MainMenu;
    public List<NPCEntry> activNPCInfo = new List<NPCEntry>();
    public List<NPCBase> activNPC = new List<NPCBase>();
    List<NPCEntry> entryList;
    private List<ContainedState> previousStates = new List<ContainedState>();
    public GameObject endOfMissionCamera;
    private void Awake()
    {
        i = this;
        endOfMissionCamera.SetActive(false);

        entryList = SceneData.monstersToTransfer;
        StartCoroutine(DelayedInfo());
    }

    IEnumerator DelayedInfo()
    {
        foreach (NPCEntry entry in entryList)
        {
            activNPCInfo.Add(entry);
            bool found = false;
            do
            {
                if (GameObject.Find(entry.name + "Containment(Clone)") != null)
                {
                    found = true;
                    activNPC.Add(GameObject.Find(entry.name + "Containment(Clone)").GetComponent<ContainmentUnit>().npc);
                    previousStates.Add(ContainedState.Free);
                }
                yield return null;
            }
            while (!found);
        }
        StartCoroutine(MonitorContainmentStates());
    }

    public void AbortMission()
    {
        StartCoroutine(DelayedSceneLoad());
    }

    IEnumerator MonitorContainmentStates()
    {
        bool print = true;
        while (true)
        {
            for (int i = 0; i < activNPC.Count; i++)
            {
                var current = activNPC[i].contained;
                var previous = previousStates[i];

                if (current != previous)
                {
                    string name = activNPCInfo[i].id;

                    if (previous == ContainedState.Contained && current == ContainedState.Free)
                    {
                        UISystem.i.popUpHanle.ShowPopUp("r", name);
                        print = true;
                    }
                    else if (previous == ContainedState.Free && (current == ContainedState.Contained || current == ContainedState.Suppressed))
                    {
                        UISystem.i.popUpHanle.ShowPopUp("y", name);
                        print = true;
                    }

                    previousStates[i] = current;
                }
            }

            // Check if all are contained or suppressed
            if (activNPC.All(n => n.contained == ContainedState.Contained || n.contained == ContainedState.Suppressed) && print)
            {
                print = false;
                UISystem.i.popUpHanle.ShowPopUp("g", "All contained");
            }

            yield return new WaitForSeconds(0.5f); // Adjust polling rate as needed
        }
    }
    private void StoreContainmentStatesToSceneData()
    {
        Dictionary<string, ContainedState> results = new Dictionary<string, ContainedState>();

        for (int i = 0; i < activNPCInfo.Count; i++)
        {
            string id = activNPCInfo[i].id;
            ContainedState state = activNPC[i].contained;
            results[id] = state;
        }

        SceneData.StoreContainmentResults(results);
    }
    IEnumerator DelayedSceneLoad()
    {
        StoreContainmentStatesToSceneData();
        SceneData.IncrementDay();
        SceneData.PrepareResults();
        yield return new WaitForSeconds(5f);

        HandleEscape();
        endOfMissionCamera.SetActive(true);
        terminalText.text = "Initiating jump sequence...\nLoading new scene...";

        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Lobby"); // Replace with your actual scene name
    }

    void LoadTargetScene()
    {
        terminalText.text = "Initiating jump sequence...\nLoading new scene...";
        inputField.DeactivateInputField();
        StartCoroutine(DelayedSceneLoad());
    }

    public override void OnSubmit(string input)
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

        if (input == "go")
        {
            LoadTargetScene();
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

        terminalText.text = "ERROR: Invalid input.\nPress Enter to return to main menu.";
        currentState = TerminalState.ErrorScreen;
    }

    public override void ShowMainMenu()
    {
        string menuText = "Mission Overview:\n";

        if (activNPCInfo.Count == activNPC.Count)
            for (int i = 0; i < activNPCInfo.Count; i++)
            {
                string state = activNPC[i].contained == ContainedState.Free ? " " : activNPC[i].contained == ContainedState.Contained ? "o" : "x";
                menuText += $"- {activNPCInfo[i].id} [{state}]\n";
            }

        menuText += "\nType 'help' for assistance or 'esc' to exit.";
        terminalText.text = menuText;
        currentState = TerminalState.MainMenu;
        inputField.text = "";
    }

    public override void ShowHelp()
    {
        string helpText = "Mission Terminal Help Menu\n\n" +
                          "Available commands:\n" +
                          "- go : Load the next scene\n" +
                          "- esc : Return to main menu or exit terminal\n" +
                          "- help : Show this help menu\n\n" +
                          "Press Enter to return to the main menu.";

        terminalText.text = helpText;
        currentState = TerminalState.ErrorScreen;
        inputField.text = "";
        inputField.ActivateInputField();
    }
}
