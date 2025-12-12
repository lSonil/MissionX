using UnityEngine;

public class Snatch : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = transform.position;
            other.transform.SetParent(transform, true);
        }
    }
}
