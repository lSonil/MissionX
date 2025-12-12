using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public float range = 2;
    public ScanHandler scanVfxPrefab;
    public float lifetime;
    public float duration;
    public float sizeMin;
    public float sizeMax;
    public GameObject flashLight;

    [Header("Scan Settings")]
    public float maxDistance = 10f;
    public float maxAngleUp = 80f;
    public float maxAngleLeft = 80f;
    public LayerMask blockingMask;   // obstacles
    public LayerMask validMask;      // which objects count as NPCs/targets
    private List<Transform> visibleObjects = new List<Transform>();

    public float scanCooldown = 2f;   // cooldown duration
    private float lastScanTime = -999f; // track last scan time

    private InventorySystem inventory;
    private void Start()
    {
        inventory = GetComponent<InventorySystem>();
    }
    void Update()
    {
        ScanForVisibleObjects();

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        UISystem.i.EnableInteractButton(null);
        LayerMask interactionMask = LayerMask.GetMask("Interactable", "Item");
        if (Physics.Raycast(ray, out hit, range, interactionMask))
        {
            IInteraction interactable = hit.collider.GetComponent<IInteraction>();
            UISystem.i.EnableInteractButton(interactable);
            if (Input.GetKeyDown(KeyCode.E) && interactable != null && !GetComponent<MovementSystem>().isBlocked)
            {
                interactable.Action(inventory.GetHeldItemId());
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) && !GetComponent<MovementSystem>().isBlocked)
        {
            // Only allow scan if cooldown expired
            if (Time.time >= lastScanTime + scanCooldown)
            {
                SpawnScan();
                lastScanTime = Time.time;
            }
            else
            {
                Debug.Log("Scan on cooldown!");
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            SetFlashLight();
        }

        if (inventory.HasItems())
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                inventory.GetHeldItem()?.PrimaryUse();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                inventory.GetHeldItem()?.SecundaryUse();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                inventory.DropCurrentItem();
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            inventory.Scroll(scroll);
        }
    }
    public void TurnOffFlashLight()
    {
        GetComponent<AudioSystem>().PlayFlashlight();
        flashLight.SetActive(false);
    }
    public void SetFlashLight()
    {
        GetComponent<AudioSystem>().PlayFlashlight();
        flashLight.SetActive(!flashLight.activeInHierarchy);
    }
    public void SpawnScan()
    {
        GameObject scanVFX = Instantiate(scanVfxPrefab.gameObject, transform.position, Quaternion.identity);
        ScanHandler scanHVFX = scanVFX.GetComponent<ScanHandler>();

        scanHVFX.minSize = sizeMin;
        scanHVFX.maxSize = sizeMax;
        scanHVFX.growthSpeed = duration;
        scanHVFX.pingLifetime = lifetime;
    }
    void ScanForVisibleObjects()
    {
        List<Transform> wereVisibleObjects = new List<Transform>(visibleObjects);

        visibleObjects.Clear();

        Transform cam = Camera.main.transform;

        Collider[] hits = Physics.OverlapSphere(transform.position, maxDistance, validMask);

        foreach (Collider hit in hits)
        {
            Transform target = hit.transform;
            Vector3 playerPosition = cam.position;
            Vector3 targetPosition = target.position;

            float distance = Vector3.Distance(playerPosition, targetPosition);
            if (distance > maxDistance) continue;
            if (Physics.Linecast(playerPosition, targetPosition, blockingMask))
                continue;

            Vector3 directionToTarget = (targetPosition - playerPosition).normalized;
            Vector3 localDir = cam.InverseTransformDirection(directionToTarget);

            float horizAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

            float vertAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

            bool withinHorizontal =
                (horizAngle >= 0 && horizAngle <= maxAngleLeft) ||
                (horizAngle < 0 && Mathf.Abs(horizAngle) <= maxAngleLeft);

            bool withinVertical =
                (vertAngle >= 0 && vertAngle <= maxAngleUp) ||
                (vertAngle < 0 && Mathf.Abs(vertAngle) <= maxAngleUp);

            if (withinHorizontal && withinVertical)
            {
                NPCBase npc = target.GetComponent<NPCBase>();
                if (npc == null) continue;
                if (!npc.isVisible)
                target.GetComponent<NPCBase>().SetVisibility(true);
                visibleObjects.Add(target);
            }
        }

        // Mark NPCs that were visible but are no longer
        foreach (Transform npc in wereVisibleObjects)
        {
            if (npc != null)
            if (!visibleObjects.Contains(npc) && npc.GetComponent<NPCBase>() != null)
            {
                npc.GetComponent<NPCBase>().SetVisibility(false);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (visibleObjects == null) return;

        Gizmos.color = Color.green;
        foreach (Transform target in visibleObjects)
        {
            if (target != null)

            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
