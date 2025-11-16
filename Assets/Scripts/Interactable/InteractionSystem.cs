using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public float maxDistance = 2;
    public ScanHandler scanVfxPrefab;
    public float lifetime;
    public float duration;
    public float sizeMin;
    public float sizeMax;


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
        if(Input.GetKeyDown(KeyCode.Mouse1) && !GetComponent<MovementSystem>().isBlocked)
        {
            SpawnScan();
        }
    }
    public void SpawnScan()
    {
        GameObject scanVFX = Instantiate(scanVfxPrefab.gameObject, transform.position,Quaternion.identity);
        ScanHandler scanHVFX = scanVFX.GetComponent<ScanHandler>();

        scanHVFX.minSize = sizeMin;
        scanHVFX.maxSize = sizeMax;
        scanHVFX.growthSpeed = duration;
        scanHVFX.pingLifetime = lifetime;
    }
}
