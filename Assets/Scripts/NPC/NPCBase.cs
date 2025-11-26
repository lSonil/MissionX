
using UnityEngine;
using UnityEngine.AI;

public enum ContainedState
{
    Free,
    Contained,
    Suppressed
}
public abstract class NPCBase : MonoBehaviour
{
    public NavMeshAgent agent;

    public ContainedState contained= ContainedState.Free;
    public LayerMask blockingMask;
    public float forwardLineLength = 10f;
    public Transform viewPoint;

    public bool useWideCone = false; // state flag

    public bool isVisible = false;
    public bool debug=false;

    public bool PlayerInConeLineOfSight(float angleSize = 40f)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null || viewPoint == null) return false;

        Vector3 npcPosition = viewPoint.position;
        Vector3 playerPosition = playerObj.transform.position;
        Vector3 toPlayer = (playerPosition - npcPosition);
        float distToPlayer = toPlayer.magnitude;

        if (distToPlayer <= 2f)
            return true;

        Vector3 direction = toPlayer.normalized;
        float halfAngle = angleSize * 0.5f;

        // --- Narrow cone mode (single line) ---
        if (!useWideCone)
        {
            float angleToPlayer = Vector3.Angle(viewPoint.forward, direction);
            if (angleToPlayer <= halfAngle && distToPlayer <= forwardLineLength)
            {
                if (!Physics.Linecast(npcPosition, playerPosition, blockingMask))
                {
                    useWideCone = true; // switch to wide mode
                    return true;
                }
            }
            return false;
        }

        // --- Wide cone mode (7 parallel lines along one axis) ---
        float offsetStep = 0.1f; // tweak spacing
        bool anySuccess = false;

        for (int i = -3; i <= 3; i++)
        {
            Vector3 offset = viewPoint.right * (i * offsetStep); // horizontal offsets
            Vector3 castDir = (direction + offset).normalized;
            float castAngle = Vector3.Angle(viewPoint.forward, castDir);

            if (castAngle <= halfAngle && distToPlayer <= forwardLineLength)
            {
                if (!Physics.Linecast(npcPosition, npcPosition + castDir * distToPlayer, blockingMask))
                {
                    anySuccess = true;
                    break;
                }
            }
        }

        if (!anySuccess)
        {
            useWideCone = false; // revert back to narrow mode
            return false;
        }

        return true;
    }



    public virtual bool PlayerInView()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return false;
        if (playerObj.GetComponent<HealthSystem>().CurrentHealth() <= 0) return false;

        Transform player = playerObj.transform;
        Vector3 npcPosition = viewPoint.position;
        Vector3 playerPosition = player.position;

        // Direction from NPC to Player
        Vector3 directionToPlayer = (playerPosition - npcPosition).normalized;
        float angle = Vector3.Angle(viewPoint.forward, directionToPlayer);

        // Check if Player is in front of NPC

        if (angle < 80f && forwardLineLength >= Vector3.Distance(npcPosition, playerPosition))
        {
            //print(Vector3.Distance(npcPosition, playerPosition));
            // Check for wall obstruction
            bool blocked = Physics.Linecast(npcPosition, playerPosition, blockingMask);

            if (!blocked)
            {
                //Time.timeScale = 0f;
                return true;
            }
        }
        return false;
    }
    public virtual bool IsInRange(float range)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return false;
        Vector3 npcPosition = transform.position;
        Vector3 playerPosition = playerObj.transform.position;

        // Check if new X position is odd
        int xRounded = Mathf.RoundToInt(transform.position.x);
        bool isOdd = xRounded % 2 != 0;

        // Apply rotation based on X parity
        float yRotation = isOdd ? 90f : 0f;
        return range >= Vector3.Distance(npcPosition, playerPosition);
    }
}
