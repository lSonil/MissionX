using UnityEngine;

public class RubyPlacement : MonoBehaviour
{
    public KeyCode placeKey = KeyCode.E;

    private bool playerInside = false;
    private InventorySystem playerInventory;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            playerInventory = other.GetComponent<InventorySystem>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            playerInventory = null;
        }
    }

    private void Update()
    {
        if (!playerInside) return;

        if (Input.GetKeyDown(placeKey))
        {
            TryPlaceRuby();
        }
    }

    void TryPlaceRuby()
    {
        if (playerInventory == null) return;

        // Find the first ruby in the inventory
        Item ruby = playerInventory.ConsumeFirstMatching(
            item => item.CompareTag("Ruby")
        );

        if (ruby != null)
        {
            // Place it visually in the placement zone
            ruby.transform.SetParent(transform);
            ruby.transform.position = transform.position;
            ruby.transform.rotation = transform.rotation;
            ruby.gameObject.SetActive(true);
            RitualManager.i.NotifyRubyPlaced(this);
            Debug.Log("Ruby placed!");
        }
        else
        {
            Debug.Log("No ruby in inventory.");
        }
    }
}
