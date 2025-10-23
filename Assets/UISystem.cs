using TMPro;
using UnityEngine;

public class UISystem : MonoBehaviour
{
    public static UISystem i;

    public GameObject interactButton;

    public void EnableInteractButton(IInteraction interaction)
    {
        interactButton.SetActive(interaction!=null);
        if (interaction != null)
        {
            interactButton.GetComponent<TextMeshProUGUI>().text = interaction.GetText() + " [E]";
        }
    }

    private void Awake()
    {
        i = this;
    }
}
