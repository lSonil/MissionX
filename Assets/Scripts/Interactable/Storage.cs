using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Storage : MonoBehaviour
{
    public static Storage i;
    public List<Item> items = new List<Item>();
    private void Start()
    {
        i = this;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            if (!items.Contains(other.GetComponent<Item>()))
            {
                other.transform.SetParent(transform);
                items.Add(other.GetComponent<Item>());
                foreach (PlayerCore p in LobyData.players)
                    p.uis.popUpHanle.ShowPopUp("+", $"+{other.GetComponent<Item>().itemWeight}");
            }
        }
    }

    public void Remove(Item i)
    {
        items.Remove(i);
        foreach (PlayerCore p in LobyData.players)
            p.uis.popUpHanle.ShowPopUp("-", $"-{i.itemWeight}");
    }

    private void LateUpdate()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i] == null)
            {
                items.Remove(items[i]);
                continue;
            }
            else
            if (items[i].gameObject.layer != LayerMask.NameToLayer("Item"))
            {
                Remove(items[i]);
                continue;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            Remove(other.GetComponent<Item>());
        }
    }
}
