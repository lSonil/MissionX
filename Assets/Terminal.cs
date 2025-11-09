using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Terminal : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI terminalText;
    public virtual void Start()
    {
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnSubmit("esc");
        }
    }
    public void HandleEscape()
    {
        inputField.DeactivateInputField();
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
    public abstract void OnSubmit(string input);
    public abstract void ShowMainMenu();
    public abstract void ShowHelp();
}
