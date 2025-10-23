using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class Terminal : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI terminalText;
    private enum TerminalState { MainMenu, ErrorScreen }
    private TerminalState currentState = TerminalState.MainMenu;

    void OnEnable()
    {
        inputField.Select();
        inputField.ActivateInputField();
    }
    private void OnDisable()
    {
        inputField.DeactivateInputField(); // Prevent further typing until reactivated
    }
    void Start()
    {
        ShowMainMenu(); // ← show menu immediately
        StartCoroutine(RefocusInput());
        inputField.onSubmit.AddListener(OnSubmit);
    }

    IEnumerator RefocusInput()
    {
        yield return new WaitForEndOfFrame();
        inputField.Select();
        inputField.DeactivateInputField(); // Prevent further typing until reactivated
        this.enabled = false;
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
            LoadTargetScene(); // Call your scene-loading method
            return;
        }

        switch (input)
        {
            case "1":
                terminalText.text = "Traveling to: Titan Base\n\nPress Enter to return.";
                currentState = TerminalState.ErrorScreen;
                break;
            case "2":
                terminalText.text = "Traveling to: Forest Outpost\n\nPress Enter to return.";
                currentState = TerminalState.ErrorScreen;
                break;
            case "3":
                terminalText.text = "Traveling to: Dust Canyon\n\nPress Enter to return.";
                currentState = TerminalState.ErrorScreen;
                break;
            default:
                terminalText.text = "ERROR: Invalid input.\nPress Enter to return to main menu.";
                currentState = TerminalState.ErrorScreen;
                break;
        }

        inputField.text = "";
        inputField.ActivateInputField();
    }
    IEnumerator DelayedSceneLoad()
    {
        terminalText.text = "🧭 Initiating jump sequence...\nLoading new scene...";
        inputField.DeactivateInputField();

        yield return new WaitForSeconds(2f); // Delay for 2 seconds

        SceneManager.LoadScene("Mission"); // Replace with your actual scene name
    }

    void LoadTargetScene()
    {
        terminalText.text = "Initiating jump sequence...\nLoading new scene...";
        inputField.DeactivateInputField();

        // Replace "YourSceneName" with the actual scene name
        StartCoroutine(DelayedSceneLoad());
    }

    void ShowMainMenu()
    {
        terminalText.text =
            "Choose your mission:\n" +
            "1 - Mission 1\n" +
            "2 - Mission 2\n" +
            "3 - Mission 3\n\n" +
            "Enter a number:";
        currentState = TerminalState.MainMenu;
        inputField.text = "";
    }

    void HandleEscape()
    {
        ShowMainMenu(); // ← resets terminal to main menu
        inputField.DeactivateInputField(); // Prevent further typing until reactivated
        GetComponent<Panel>().Action(); // ← optional external logic
    }
}