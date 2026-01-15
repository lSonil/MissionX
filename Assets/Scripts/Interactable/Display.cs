using UnityEngine;

public class Display : MonoBehaviour, IInteraction
{
    public string interactionPromptText ="Use Terminal";

    public Transform targetPosition;
    public Transform focusPosition;
    public string GetTextUse() => interactionPromptText;
    public string GetTextPrimary() => "";
    public string GetTextSecundary() => "";
    public void Action(Item i = null, PlayerCore p = null)
    {
        p = GetComponent<Terminal>().TurnOn(p);
        p.ms.Block();
        if (p.GetComponent<MovementSystem>().isBlocked)
        {
            p.its.TurnOffFlashLight();
            p.transform.position = targetPosition.position;

            Vector3 direction = focusPosition.position - p.transform.position;
            direction.y = 0f; // flatten to horizontal plane

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion faceForward = Quaternion.LookRotation(direction, Vector3.up);
                Quaternion faceLeft = faceForward * Quaternion.Euler(-80f, 0f, 0f);
                p.ms.body.transform.rotation = faceLeft;
            }
        }
    }

}
