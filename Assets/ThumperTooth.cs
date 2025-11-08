using UnityEngine;

public class ThumperTooth : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Body"))
        {
            print(1);
            Transform playerTransform = other.transform;
            playerTransform.SetParent(transform); // Reparent to ThumperTooth
            playerTransform.position = transform.position; // Move to same position
        }
    }
}
