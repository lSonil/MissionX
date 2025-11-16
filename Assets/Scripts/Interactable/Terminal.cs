using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Terminal : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI terminalText;
    public float scrollSpeed = 900f; // pixels per wheel tick
    public ScrollRect scrollRect;

    private RectTransform content;
    private RectTransform viewport;
    private string lastText = "";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnSubmit("esc");
        }

        HandleScrollWheel();

        // Poll for text changes if someone sets terminalText.text directly
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

    public virtual void Start()
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

        // Initial sizing
        if (terminalText != null)
        {
            lastText = terminalText.text;
            AdjustContentHeight();
        }
    }

    /// <summary>
    /// Safe way to set terminal text and auto-adjust content height.
    /// </summary>
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

        // Reset scroll position when text changes
        if (scrollRect != null)
        {
            // Snap to top
            scrollRect.verticalNormalizedPosition = 1f;

            // If you prefer snapping to bottom instead, use:
            // scrollRect.verticalNormalizedPosition = 0f;
        }

        // Also reset content anchoredPosition to ensure consistency
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
