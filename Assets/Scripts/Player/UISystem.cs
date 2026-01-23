using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISystem : MonoBehaviour
{
    [Header("Other UI")]
    [SerializeField] private Image damageOverlay;
    [SerializeField] private Image stunOverlay;
    [SerializeField] private float fadeInStep = 10f;
    [SerializeField] private float maxAlpha = 0.5f;
    [SerializeField] private float fadeOutSpeed = 0.5f;
    private Coroutine fadeRoutine;

    [Header("Interaction UI")]
    public GameObject interactButton;
    public GameObject usePrimaryButton;
    public GameObject useSecundaryButton;
    public GameObject dropButton;
    public GameObject loadingScreen;
    public PopUpHandle popUpHanle;

    [Header("Inventory UI")]
    public Image[] slotIcons;
    public Sprite fullIcon;
    public Sprite emptyIcon;
    public ResultsHandler results;
    public TextMeshProUGUI currentDay;
    private void Start()
    {
        currentDay.text = SceneData.day.ToString();
        results.gameObject.SetActive(false);
        loadingScreen.gameObject.SetActive(false);

        if (damageOverlay != null)
        {
            Color c = damageOverlay.color;
            c.a = 0f;
            damageOverlay.color = c;
        }
    }
    private IEnumerator DamageFlash()
    {
        Color c = damageOverlay.color;

        c.a = Mathf.Min(c.a + fadeInStep / 100f, maxAlpha);
        damageOverlay.color = c;

        while (c.a > 0f)
        {
            c.a -= fadeOutSpeed * Time.deltaTime;
            damageOverlay.color = c;
            yield return null;
        }

        fadeRoutine = null;
    }
    public IEnumerator LoadingRoutine()
    {
        loadingScreen.SetActive(true);

        yield return StartCoroutine(Fade(0, 1));

        while (!RoomGenerator.generated)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2.0f);

        yield return StartCoroutine(Fade(1, 0));

        loadingScreen.SetActive(false);
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0;
        Image fadeImage = loadingScreen.GetComponent<Image>();
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(start, end, elapsed / .2f);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, newAlpha);
            yield return null;
        }
    }
    private IEnumerator StunFlash()
    {
        AudioSource audio = stunOverlay.GetComponent<AudioSource>();
        audio.volume = 1f; 
        audio.Play();

        Color c = stunOverlay.color;
        c.a = 1;
        stunOverlay.color = c;
        yield return new WaitForSeconds(1);
        while (c.a > 0f)
        {
            c.a -= fadeOutSpeed * Time.deltaTime;
            stunOverlay.color = c;

            audio.volume -= fadeOutSpeed * Time.deltaTime;
            if (audio.volume < .2f) audio.volume = 2f;

            yield return new WaitForSeconds(0.02f);
        }
        while (audio.volume > 0f)
        {
            audio.volume -= fadeOutSpeed * Time.deltaTime;
            if (audio.volume < 0f) audio.volume = 0f;

            yield return new WaitForSeconds(0.02f);
        }

        fadeRoutine = null;
    }
    public void FadeDamageRoutine()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(DamageFlash());
    }
    public void FadeFlashRoutine()
    {
        StartCoroutine(StunFlash());
    }
    public IEnumerator ShowResults()
    {
        currentDay.text = SceneData.day.ToString();

        results.gameObject.SetActive(true); // Make visible
        StartCoroutine(results.DisplayResults(SceneData.containmentResults, SceneData.missionToTransfer));
        yield return new WaitForSeconds(5f);
        results.gameObject.SetActive(false); // Hide after 5 seconds
    }

    public void EnableInteractButton(IInteraction interaction)
    {
        interactButton.SetActive(interaction != null);
        if (interaction != null)
        {
            interactButton.GetComponent<TextMeshProUGUI>().text = interaction.GetTextUse() + " [E]";
        }
    }
    public void EnablePrimaryUseButton(IInteraction interaction)
    {
        usePrimaryButton.SetActive(interaction != null);
        if (interaction != null)
        {
            usePrimaryButton.SetActive(interaction.GetTextPrimary() != "");
            usePrimaryButton.GetComponent<TextMeshProUGUI>().text = interaction.GetTextPrimary() + " [F]";
        }
    }
    public void EnableSecundaryUseButton(IInteraction interaction)
    {
        useSecundaryButton.SetActive(interaction != null);
        if (interaction != null)
        {
            useSecundaryButton.SetActive(interaction.GetTextSecundary() != ""); 
            useSecundaryButton.GetComponent<TextMeshProUGUI>().text = interaction.GetTextSecundary() + " [R]";
        }
    }


    public void EnableDropButton(IInteraction interaction)
    {
        dropButton.SetActive(interaction != null);
    }
    public void UpdateInventoryUI(Item[] items, int currentIndex)
    {
        EnablePrimaryUseButton(items[currentIndex]);
        EnableSecundaryUseButton(items[currentIndex]);
        EnableDropButton(items[currentIndex]);
        for (int i = 0; i < slotIcons.Length; i++)
        {
            slotIcons[i].sprite = items[i] != null ? fullIcon : emptyIcon;
        }
    }
}