
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
    public bool PlayerInConeLineOfSight(float angleSize = 40f)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null || viewPoint == null) return false;

        Vector3 npcPosition = viewPoint.position;
        Vector3 playerPosition = playerObj.transform.position;

        // Direction to player
        Vector3 toPlayer = (playerPosition - npcPosition).normalized;
        float distToPlayer = Vector3.Distance(npcPosition, playerPosition);

        // Angle check
        float angleToPlayer = Vector3.Angle(viewPoint.forward, toPlayer);

        // Conditions: inside cone + within range
        if (angleToPlayer <= angleSize * 0.5f && distToPlayer <= forwardLineLength)
        {
            // Obstruction check
            if (!Physics.Linecast(npcPosition, playerPosition, blockingMask))
            {
                return true;
            }
        }
        if(distToPlayer <= 2)
                return true;


        return false;
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
    public virtual bool IsVisibleToPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj.GetComponent<HealthSystem>().CurrentHealth() <= 0) return false;
        Transform player = playerObj.transform;
        Vector3 npcPosition = viewPoint.position;
        Vector3 playerPosition = player.position;

        float distance = Vector3.Distance(npcPosition, playerPosition);
        if (distance > 10f) return false;

        bool hasLineOfSight = !Physics.Linecast(playerPosition, npcPosition, blockingMask);
        if (!hasLineOfSight) return false;

        Vector3 directionToNPC = (npcPosition - playerPosition).normalized;
        float angle = Vector3.Angle(player.forward, directionToNPC);

        // Return true if player is NOT looking at NPC
        return angle < 80f;
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
