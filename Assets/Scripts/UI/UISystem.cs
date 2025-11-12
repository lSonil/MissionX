using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UISystem : MonoBehaviour
{
    public static UISystem i;

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

    private void Start()
    {

        currentDay.text = SceneData.day.ToString();
        if (SceneData.showResults)
        {
            SceneData.PrepareResults(false);
            StartCoroutine(ShowResults());
        }
        else
            results.gameObject.SetActive(false);
    }

    public IEnumerator ShowResults()
    {
        results.DisplayResults(SceneData.containmentResults);
        results.gameObject.SetActive(true); // Make visible
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
    private void Awake()
    {
        i = this;
    }
}