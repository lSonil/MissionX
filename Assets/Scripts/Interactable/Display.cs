using UnityEngine;

public class Display : MonoBehaviour, IInteraction
{
    public string interactionPromptText ="Use Terminal";

    public Transform targetPosition;
    public Transform focusPosition;
    public string GetTextUse() => interactionPromptText;
    public string GetTextPrimary() => "";
    public string GetTextSecundary() => "";
    public void Action(int i = -1)
    {
        GetComponent<Terminal>().TurnOn();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player.GetComponent<MovementSystem>() == null) return;
        player.GetComponent<MovementSystem>().Block();
        if (player.GetComponent<MovementSystem>().isBlocked)
        {
            player.GetComponent<InteractionSystem>().TurnOffFlashLight();
            player.transform.position = targetPosition.position;

            Vector3 direction = focusPosition.position - player.transform.position;
            direction.y = 0f; // flatten to horizontal plane

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion faceForward = Quaternion.LookRotation(direction, Vector3.up);
                Quaternion faceLeft = faceForward * Quaternion.Euler(-80f, 0f, 0f);
                player.GetComponent<MovementSystem>().body.transform.rotation = faceLeft;
            }
        }
    }

}
