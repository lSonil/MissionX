using UnityEngine;

public class TheWhisper : MonoBehaviour
{
    public LayerMask blockingMask;     // e.g. Structure
    private enum VisibilityState
    {
        Unseen,
        Seen,
        LookedAway
    }

    private VisibilityState visibilityState = VisibilityState.Unseen;

    private float visibilityTimer = 0f;
    private float visibilityThreshold = 0.2f; // Seconds to confirm visibility change

    void Update()
    {
        bool currentlyVisible = IsVisibleToPlayer();

        switch (visibilityState)
        {
            case VisibilityState.Unseen:
                if (currentlyVisible)
                {
                    visibilityTimer += Time.deltaTime;
                    if (visibilityTimer >= visibilityThreshold)
                    {
                        visibilityState = VisibilityState.Seen;
                        visibilityTimer = 0f;
                    }
                }
                else
                {
                    visibilityTimer = 0f;
                }
                break;

            case VisibilityState.Seen:
                if (!currentlyVisible)
                {
                    visibilityTimer += Time.deltaTime;
                    if (visibilityTimer >= visibilityThreshold)
                    {
                        visibilityState = VisibilityState.LookedAway;
                        visibilityTimer = 0f;
                        TriggerLookAwayEvent();
                    }
                }
                else
                {
                    visibilityTimer = 0f;
                }
                break;

            case VisibilityState.LookedAway:
                if (currentlyVisible)
                {
                    visibilityTimer += Time.deltaTime;
                    if (visibilityTimer >= visibilityThreshold)
                    {
                        visibilityState = VisibilityState.Seen;
                        visibilityTimer = 0f;
                    }
                }
                else
                {
                    visibilityTimer = 0f;
                }
                break;
        }
    }
    public void TriggerLookAwayEvent()
    {
        Doorway door = GetComponent<Doorway>();
        bool randomHall = Random.value < 0.5f;
        bool randomFill = Random.value < 0.5f ? door.isFilled : !door.isFilled;
        if (Random.value < 0.5f)
        {
            if (door.connectedTo)
                GetComponent<Doorway>().ForceFillBoth(randomHall, randomFill);
            else
                GetComponent<Doorway>().ForceFill(randomHall, true);
        }

    }
    public bool IsVisibleToPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return false;

        Transform player = playerObj.transform;
        Vector3 from = transform.position + new Vector3(0, 0.45f, 0);
        Vector3 to = player.position;

        float distance = Vector3.Distance(from, to);
        if (distance > 10f) return false;

        bool hasLineOfSight = !Physics.Linecast(to, from, blockingMask);
        if (distance <= .5f) return true;
        if (!hasLineOfSight) return false;

        Vector3 directionToWhisper = (from - to).normalized;
        float angle = Vector3.Angle(player.forward, directionToWhisper);
        if (angle > 90f) return false;

        return true; // All conditions for green are met
    }

    private void OnDrawGizmos()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Transform player = playerObj.transform;
        Vector3 from = transform.position+new Vector3(0,.45f,0);
        Vector3 to = player.position;
        float distance = Vector3.Distance(from, to);

        Vector3 directionToWhisper = (from - to).normalized;
        float angle = Vector3.Angle(player.forward, directionToWhisper);

        bool hasLineOfSight = !Physics.Linecast(to, from, blockingMask);

        // Determine color
        if (distance > 10f)
        {
            Gizmos.color = Color.red; // Direct line of sight
        }
        else if (distance <=.5f)
        {
            Gizmos.color = Color.green; // Direct line of sight
        }
        else if (!hasLineOfSight)
        {
            Gizmos.color = Color.yellow; // Blocked line of sight
        }
        else if (angle > 90f)
        {
            Gizmos.color = Color.pink; // Fallback for distance > 0
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
