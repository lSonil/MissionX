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

    public IEnumerator DisplayResults(Dictionary<string, ContainedState> results, MissionData rewards)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        GameObject weekQuota = Instantiate(resultPrefab, container);
        TextMeshProUGUI weekQuotaText = weekQuota.GetComponentInChildren<TextMeshProUGUI>();
        weekQuotaText.text = $"This Week Quota: {SceneData.currentStoredItemWeight}/{SceneData.GetTotalDataValue()}";

        GameObject dayTotal = Instantiate(resultPrefab, container);
        TextMeshProUGUI dayTotalText = dayTotal.GetComponentInChildren<TextMeshProUGUI>();
        dayTotalText.text = $"This day results: {SceneData.currentDayStoredItemWeight}/{SceneData.currentMissionItemWeight}";

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
                GameObject type = Instantiate(resultPrefab, container);
                TextMeshProUGUI typeText = type.GetComponentInChildren<TextMeshProUGUI>();
                typeText.text = $"Debuffs:\n";
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
                GameObject type = Instantiate(resultPrefab, container);
                TextMeshProUGUI typeText = type.GetComponentInChildren<TextMeshProUGUI>();
                typeText.text = $"Buffs:\n";

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

        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
    }
}
