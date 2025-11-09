using System.Collections;
using UnityEngine;

public class PopUpHandle : MonoBehaviour
{
    public GameObject prefabR;
    public GameObject prefabY;
    public GameObject prefabG;

    public void ShowPopUp(string prefabKey, string message)
    {
        GameObject selectedPrefab = prefabKey switch
        {
            "r" => prefabR,
            "y" => prefabY,
            "g" => prefabG,
            _ => null
        };

        if (selectedPrefab == null)
        {
            Debug.LogWarning($"Invalid prefab key: {prefabKey}");
            return;
        }
        string newMessage = prefabKey switch
        {
            "r" => $"Status- NPC [{message}] ESCAPED",
            "y" => $"Status- NPC [{message}] CONTAINED",
            "g" => $"Status- ALL NPC CONTAINED",
            _ => null

        };

        GameObject instance = Instantiate(selectedPrefab, transform);

        PopUp popup = instance.GetComponent<PopUp>();

        if (popup != null)
        {
            popup.PopUpFunction(newMessage);
        }
     
        StartCoroutine(DestroyAfterDelay(instance, 5f));
    }

    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }
}
