using UnityEngine;

public class LightEmiter : MonoBehaviour
{
    public LayerMask blockingMask;     // e.g. Structure
    public bool isLightOn=true;
    public bool debug;
    void Update()
    {
        bool currentlyVisible = IsVisibleToPlayer();

        if (IsVisibleToPlayer())
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            playerObj.GetComponent<SanitySystem>().OnPlayerEnterSafeZone();
        }
    }

    public bool IsVisibleToPlayer()
    {
        if (!isLightOn) return false;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return false;
        Transform player = playerObj.transform;
        Vector3 from = transform.position + new Vector3(0, -0.1f, 0);
        Vector3 to = player.position;

        float distance = Vector3.Distance(from, to);
        if (distance > 10f) return false;

        bool hasLineOfSight = !Physics.Linecast(to, from, blockingMask);
        if (!hasLineOfSight) return false;

        return true; // ✅ All conditions for green are met
    }
    private void OnDrawGizmos()
    {
        if (!debug) return;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Transform player = playerObj.transform;
        Vector3 from = transform.position + new Vector3(0, -.1f, 0);
        Vector3 to = player.position;
        float distance = Vector3.Distance(from, to);


        bool hasLineOfSight = !Physics.Linecast(to, from, blockingMask);

        // Determine color
        if (distance > 10f)
        {
            Gizmos.color = Color.red; // Direct line of sight
        }
        else if (!hasLineOfSight)
        {
            Gizmos.color = Color.yellow; // Blocked line of sight
        }
        else
        {
            Gizmos.color = Color.green;
        }

        Gizmos.DrawLine(from, to);
        Gizmos.DrawSphere(from, 0.2f);
        Gizmos.DrawSphere(to, 0.2f);
    }
}
