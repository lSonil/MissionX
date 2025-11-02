using System.Collections;
using UnityEngine;

public class IsInsideTheWalls : MonoBehaviour
{
    private int playerTriggerCount = 0;
    public bool isPlayerInside = false;

    [Header("Sanity Interaction")]
    public float sanityDrainRate = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTriggerCount++;
            isPlayerInside = true;
            StartCoroutine(Drain());
        }
    }

    IEnumerator Drain()
    {
        SanitySystem sanity = GridManager.i.GetPlayerTransform().gameObject.GetComponent<SanitySystem>();

        while (isPlayerInside)
        {
            sanity.DrainSanity(sanityDrainRate);
            yield return new WaitForSeconds(1f);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTriggerCount = Mathf.Max(0, playerTriggerCount - 1);
            if (playerTriggerCount == 0)
            {
                isPlayerInside = false;
            }
        }
    }
}
