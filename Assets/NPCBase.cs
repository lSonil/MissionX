
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

    public virtual bool DirectLineToPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return false;
        if (playerObj.GetComponent<HealthSystem>().CurrentHealth() <= 0) return false;

        Transform player = playerObj.transform;
        Vector3 npcPosition = transform.position + new Vector3(0, 0.45f, 0);
        Vector3 playerPosition = player.position;

        // Direction from NPC to Player
        Vector3 directionToPlayer = (playerPosition - npcPosition).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

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
        if (playerObj == null || agent == null || !agent.isOnNavMesh) return false;
        if (playerObj.GetComponent<HealthSystem>().CurrentHealth() <= 0) return false;
        Transform player = playerObj.transform;
        Vector3 npcPosition = transform.position + new Vector3(0, 0.45f, 0);
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
