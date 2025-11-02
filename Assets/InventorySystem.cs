using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private int maxSlots = 4;
    [SerializeField] private Transform handTransform;

    private Item[] items = new Item[4];
    private int currentIndex = 0;

    private void Start()
    {
        UISystem.i.UpdateInventoryUI(items, currentIndex);
        HoldItem(items[currentIndex]);
    }

    void Update()
    {
        if (items.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                items[currentIndex]?.PrimaryUse();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                items[currentIndex]?.SecundaryUse();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                DropCurrentItem();
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {

                int direction = scroll > 0 ? 1 : -1;
                ReleaseItem(items[currentIndex]);
                currentIndex = (currentIndex + direction + items.Length) % items.Length;

                HoldItem(items[currentIndex]);
                UISystem.i.UpdateInventoryUI(items, currentIndex);
            }
        }
    }
    public Item GetHeldItem()
    {
        return items[currentIndex];
    }

    public bool TryAddItem(Item item)
    {
        // Try current slot first
        if (items[currentIndex] == null)
        {
            items[currentIndex] = item;
            HoldItem(item);
            UISystem.i.UpdateInventoryUI(items, currentIndex);
            return true;
        }

        // Try other slots
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item; // This was missing

                if (i == currentIndex)
                {
                    HoldItem(item);
                }
                else
                {
                    item.gameObject.SetActive(false);
                }

                UISystem.i.UpdateInventoryUI(items, currentIndex);
                return true;
            }
        }

        return false;
    }
    void DropCurrentItem()
    {
        Item item = items[currentIndex];
        if (item == null) return;

        ReleaseItem(item);
        item.transform.SetParent(RoomGenerator.i.FindClosestRoom().transform);
        item.gameObject.SetActive(true);
        item.transform.position = handTransform.position;

        items[currentIndex] = null;
        UISystem.i.UpdateInventoryUI(items, currentIndex);
    }

    void HoldItem(Item item)
    {
        if (item == null) return;

        item.transform.SetParent(handTransform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.gameObject.SetActive(true);
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider col = item.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void ReleaseItem(Item item)
    {
        if (item == null) return;

        if (item.transform.parent == handTransform)
        {
            item.transform.SetParent(null);
            item.gameObject.SetActive(false);

            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            Collider col = item.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }
    }

    public Item ConsumeFirstMatching(System.Predicate<Item> match)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && match(items[i]))
            {
                Item found = items[i];
                items[i] = null;
                if (i == currentIndex) ReleaseItem(found);
                UISystem.i.UpdateInventoryUI(items, currentIndex);
                return found;
            }
        }
        return null;
    }

}
