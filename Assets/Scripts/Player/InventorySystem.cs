using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem i;
    //[SerializeField] private int maxSlots = 4;
    [SerializeField] private Transform handTransform;

    private Item[] items = new Item[4];
    private int currentIndex = 0;

    private void Start()
    {
        i = this;
        UISystem.i.UpdateInventoryUI(items, currentIndex);
        HoldItem(items[currentIndex]);
    }

    public Item GetHeldItem()
    {
        return items[currentIndex];
    }
    public int GetHeldItemId()
    {
        return items[currentIndex] == null ? -1 : items[currentIndex].itemId;
    }
    public int GetItemType()
    {
        return items[currentIndex] == null ? 0 : items[currentIndex].itemTypeId;
    }

    public bool HasItems()
    {
        return items.Length > 0;
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
    public void DropCurrentItem()
    {
        Item item = items[currentIndex];
        if (item == null) return;

        ReleaseItem(item);
        if(RoomGenerator.i!=null)
            item.transform.SetParent(RoomGenerator.i.FindClosestRoom().transform);
        else
            item.transform.SetParent(Object.FindObjectsByType<Room>(FindObjectsSortMode.None).FirstOrDefault().transform);
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
        item.gameObject.layer = 12;
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
            item.gameObject.layer = 11;

            item.transform.SetParent(null);
            item.gameObject.SetActive(true);

            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            Collider col = item.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }
    }
    public void Scroll(float scroll)
    {
        if (scroll == 0)return;

        int direction = scroll > 0 ? 1 : -1;
        ReleaseItem(items[currentIndex]);
        currentIndex = (currentIndex + direction + items.Length) % items.Length;

        HoldItem(items[currentIndex]);
        UISystem.i.UpdateInventoryUI(items, currentIndex);

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
