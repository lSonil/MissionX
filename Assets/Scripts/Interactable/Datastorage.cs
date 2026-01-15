using UnityEngine;

public class Datastorage : MonoBehaviour, IInteraction
{
    public string interactionPromptText;

    public void Action(Item i = null, PlayerCore p = null)
    {
        if (i == null) return;

        SceneData.currentStoredItemWeight += i.itemWeight;
        p.ivs.DestroyCurrentItem();
    }

    public string GetTextUse() => interactionPromptText;
    public string GetTextPrimary() => "";
    public string GetTextSecundary() => "";
}
