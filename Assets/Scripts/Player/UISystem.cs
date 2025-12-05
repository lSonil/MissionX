using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UISystem : MonoBehaviour
{
    public static UISystem i;
    [Header("Other UI")]
    [SerializeField] private Image damageOverlay;
    [SerializeField] private Image stunOverlay;
    [SerializeField] private float fadeInStep = 10f;
    [SerializeField] private float maxAlpha = 0.5f;
    [SerializeField] private float fadeOutSpeed = 0.5f;
    private Coroutine fadeRoutine;
    public GameObject endOfMissionCamera;

    [Header("Interaction UI")]
    public GameObject interactButton;
    public GameObject usePrimaryButton;
    public GameObject useSecundaryButton;
    public GameObject dropButton;
    public PopUpHandle popUpHanle;

    [Header("Inventory UI")]
    public Image[] slotIcons;
    public Sprite fullIcon;
    public Sprite emptyIcon;
    public ResultsHandler results;
    public TextMeshProUGUI currentDay;

    private void Awake()
    {
        i = this;
    }
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        currentDay.text = SceneData.day.ToString();
        if (SceneData.showResults)
        {
            SceneData.PrepareResults(false);
            StartCoroutine(ShowResults());
        }
        else
            results.gameObject.SetActive(false);
        if (damageOverlay != null)
        {
            Color c = damageOverlay.color;
            c.a = 0f;
            damageOverlay.color = c;
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentDay.text = SceneData.day.ToString();

        if (SceneData.showResults)
        {
            SceneData.PrepareResults(false);
            StartCoroutine(ShowResults());
        }
        else
        {
            results.gameObject.SetActive(false);
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
        results.gameObject.SetActive(true); // Make visible
        results.DisplayResults(SceneData.containmentResults);
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