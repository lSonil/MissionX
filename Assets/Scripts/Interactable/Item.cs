using UnityEngine;

public class Item : MonoBehaviour, IInteraction
{
    public int itemId=0;
    public int itemTypeId=0;
    public int itemWeight =0;
    public int dayCreated =0;
    [SerializeField] private string promptUse = "Pick Up";
    [SerializeField] private string promptPrimary = "";
    [SerializeField] private string promptSecundary = "";
    public GameObject mapObject;

    public string PromptUse => promptUse;
    public string PromptPrimaryUse => promptPrimary;
    public string PromptSecundaryUse => promptSecundary;
    
    public string GetTextUse() => PromptUse;
    public string GetTextPrimary() => promptPrimary;
    public string GetTextSecundary() => promptSecundary;

    public void Action(Item i = null, PlayerCore p = null)
    {
        InventorySystem inv = FindFirstObjectByType<InventorySystem>();
        if (inv != null)
        {
            inv.TryAddItem(this);
        }
    }

    public virtual void PrimaryUse()
    {
        Debug.Log($"Using item: {promptPrimary}");
    }
    public virtual void SecundaryUse()
    {
        Debug.Log($"Using item: {promptSecundary}");
    }
    public virtual int GetWeight() => itemWeight;
    public virtual string GetName()
    {
        mapObject.SetActive(true);
        return gameObject.name;
    }
}
