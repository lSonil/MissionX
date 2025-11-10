using UnityEngine;

public class Item : MonoBehaviour, IInteraction
{
    [SerializeField] private string promptUse = "Pick Up";
    [SerializeField] private string promptPrimary = "";
    [SerializeField] private string promptSecundary = "";
    public string PromptUse => promptUse;
    public string PromptPrimaryUse => promptPrimary;
    public string PromptSecundaryUse => promptSecundary;

    public string GetTextUse() => PromptUse;
    public string GetTextPrimary() => promptPrimary;
    public string GetTextSecundary() => promptSecundary;

    public void Action()
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
}
