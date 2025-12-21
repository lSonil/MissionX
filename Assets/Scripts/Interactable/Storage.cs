using UnityEngine;
using System.Collections.Generic;

public class Storage : MonoBehaviour
{
    public List<Item> items;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            if (!items.Contains(other.GetComponent<Item>()))
            {
                other.transform.SetParent(transform);
                items.Add(other.GetComponent<Item>());
            }
        }
    }
    private void LateUpdate()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i] == null)
            {
                items.RemoveAt(i);
                continue;
            }
            if (items[i].gameObject.layer != LayerMask.NameToLayer("Item"))
            {
                items.RemoveAt(i);

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            items.Remove(other.GetComponent<Item>());
        }
    }
}
