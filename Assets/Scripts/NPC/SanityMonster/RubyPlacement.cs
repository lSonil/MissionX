using UnityEngine;

public class RubyPlacement : MonoBehaviour
{
    public KeyCode placeKey = KeyCode.E;
    public GameObject rubyPrefab; // Add reference to ruby prefab
    public float heightOffset = 0.5f; // Height above placement zone

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
            // Destroy the inventory ruby
            Destroy(ruby.gameObject);
            
            // Instantiate a new ruby above the placement zone
            Vector3 spawnPosition = transform.position + Vector3.up * heightOffset;
            GameObject placedRuby = Instantiate(rubyPrefab, spawnPosition, transform.rotation);
            
            // Configure the placed ruby
            placedRuby.transform.SetParent(transform);
            placedRuby.gameObject.layer = LayerMask.NameToLayer("Default");
            
            Rigidbody rb = placedRuby.GetComponent<Rigidbody>();
            if (rb != null) 
            {
                rb.isKinematic = true; // Keep it static when placed
            }

            Collider col = placedRuby.GetComponent<Collider>();
            if (col != null) 
            {
                col.enabled = false; // Disable collision once placed
            }
            
            placedRuby.SetActive(true);
            
            RitualManager.i.NotifyRubyPlaced(this);
            Debug.Log("Ruby placed!");
        }
        else
        {
            Debug.Log("No ruby in inventory.");
        }
    }
}
