using TMPro;
using UnityEngine;

public class PingInfo : MonoBehaviour
{
    public Transform trackedTarget;
    public float lifetime = 5f;
    public TextMeshProUGUI info;
    private void Start()
    {
        // Schedule destruction
        Invoke(nameof(SelfDestruct), lifetime);
    }
    private void Update()
    {
        // Find the player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {

            // Instantly face the player
            Vector3 direction = playerObj.transform.position - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
    void SelfDestruct()
    {
        ScanHandler.Untrack(trackedTarget);
        Destroy(gameObject);
    }
}
