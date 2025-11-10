using UnityEngine;

public class RegenSanitySpace : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SanitySystem>().OnPlayerEnterSafeZone();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SanitySystem>().OnPlayerLeaveSafeZone();
        }
    }
}
