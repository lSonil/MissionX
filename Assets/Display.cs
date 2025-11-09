using UnityEngine;

public class Display : MonoBehaviour, IInteraction
{
    public string interactionPromptText ="Use Terminal";

    public Transform targetPosition; // Assign in Inspector or dynamically
    public string GetTextUse() => interactionPromptText;
    public string GetTextPrimary() => "";
    public string GetTextSecundary() => "";
    public void Action()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        player.GetComponent<MovementSystem>().Block();
        GetComponent<Terminal>().enabled = !GetComponent<Terminal>().enabled;
        if (player.GetComponent<MovementSystem>().isBlocked)
        {
            // Force move player to target position
            player.transform.position = targetPosition.position;

            Camera mainCam = Camera.main;
            if (mainCam == null || targetPosition == null) return;

            // Move the camera to the target position
            mainCam.transform.position = targetPosition.position;

            // Rotate the camera to look at the target
            mainCam.transform.LookAt(transform);
        }
    }

}
