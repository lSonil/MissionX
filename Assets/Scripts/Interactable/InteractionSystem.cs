using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public float maxDistance = 2;

    void Update()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        UISystem.i.EnableInteractButton(null);
        LayerMask interactionMask = LayerMask.GetMask("Interactable", "Item");
        if (Physics.Raycast(ray, out hit, maxDistance, interactionMask))
        {
            IInteraction interactable = hit.collider.GetComponent<IInteraction>();
            UISystem.i.EnableInteractButton(interactable);
            if (Input.GetKeyDown(KeyCode.E) && interactable != null && !GetComponent<MovementSystem>().isBlocked)
            {
                interactable.Action();
            }
        }
    }
}
