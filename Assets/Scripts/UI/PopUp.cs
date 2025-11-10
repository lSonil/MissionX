using TMPro;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;

    public void PopUpFunction(string message)
    {
        if (textDisplay != null)
        {
            textDisplay.text = message;
        }
    }
}
