using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Storage : MonoBehaviour
{
    public List<(Item, int, int)> items = new List<(Item, int, int)>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            if (!items.Contains((other.GetComponent<Item>(), other.GetComponent<Item>().dayCreated, other.GetComponent<Item>().itemWeight)))
            {
                other.transform.SetParent(transform);
                int weight = other.GetComponent<Item>().itemWeight;
                SceneData.currentAllSavedItemWeight += weight;
                if(other.GetComponent<Item>().dayCreated==SceneData.day)
                    SceneData.currentTodaySavedItemWeight += weight;
                items.Add((other.GetComponent<Item>(), other.GetComponent<Item>().dayCreated, other.GetComponent<Item>().itemWeight));
                foreach (PlayerCore p in LobyData.players)
                    p.uis.popUpHanle.ShowPopUp("+", $"+{other.GetComponent<Item>().itemWeight}");

            }
        }
    }

    public void Remove((Item,int, int) i)
    {
        int weight = i.Item3;
        SceneData.currentAllSavedItemWeight -= weight;
        if (i.Item2 == SceneData.day)
            SceneData.currentTodaySavedItemWeight += weight;
        items.Remove(i);
        foreach (PlayerCore p in LobyData.players)
            p.uis.popUpHanle.ShowPopUp("-", $"-{weight}");


    }
    private void LateUpdate()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].Item1 == null)
            {
                Remove(items[i]);
                continue;
            }
            else
            if (items[i].Item1.gameObject.layer != LayerMask.NameToLayer("Item"))
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
            Remove((other.GetComponent<Item>(), other.GetComponent<Item>().dayCreated, other.GetComponent<Item>().itemWeight));
        }
    }
}
