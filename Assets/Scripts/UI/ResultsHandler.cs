using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject resultPrefab;
    public Transform container;

    public void DisplayResults(Dictionary<string, ContainedState> results, MissionData rewards)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        bool allFree = true;
        bool anyFree = false;

        foreach (var entry in results)
        {
            GameObject instance = Instantiate(resultPrefab, container);
            TextMeshProUGUI text = instance.GetComponentInChildren<TextMeshProUGUI>();

            string stateSymbol = entry.Value == ContainedState.Free ? "X" : "O";
            text.text = $"[{entry.Key}]: [{stateSymbol}]";

            if (entry.Value == ContainedState.Free)
                anyFree = true;
            else
                allFree = false;
        }


        Image containerImage = GetComponent<Image>();
        if (containerImage != null)
        {
            Color color;
            if (allFree)
            {
                foreach (var entry in rewards.debuffs)
                {
                    GameObject instance = Instantiate(resultPrefab, container);
                    TextMeshProUGUI text = instance.GetComponentInChildren<TextMeshProUGUI>();

                    text.text = entry.ToString();
                }
                color = Color.red;
            }
            else if (anyFree)
                color = Color.yellow;
            else
            {
                foreach (var entry in rewards.buffs)
                {
                    GameObject instance = Instantiate(resultPrefab, container);
                    TextMeshProUGUI text = instance.GetComponentInChildren<TextMeshProUGUI>();

                    text.text = entry.ToString();
                }
                color = Color.green;
            }
            color.a = 0.2f;
            containerImage.color = color;
        }

        StartCoroutine(HideAfterDelay(5f));
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
