using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISystem : MonoBehaviour
{
    public static UISystem i;

    [Header("Interaction UI")]
    public GameObject interactButton;
    public GameObject usePrimaryButton;
    public GameObject useSecundaryButton;
    public GameObject dropButton;

    [Header("Inventory UI")]
    public Image[] slotIcons;
    public Sprite fullIcon;
    public Sprite emptyIcon;
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