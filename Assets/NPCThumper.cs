using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class NPCThumper : MonoBehaviour
{
    public enum NPCState
    {
        Wait,
        WaitToMove,
        Approach,
        Attack
    }
    private NavMeshAgent agent;
    public List<Transform> usedGrid;
    public bool following;
    private bool waitingForVisibilityDrop;
    public LayerMask blockingMask;
    public float forwardLineLength = 10f;
    public float attackRange = 2f;
    public NPCState currentState = NPCState.Wait;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (GridManager.i != null)
        {
            usedGrid = new List<Transform>(GridManager.i.grid);
        }
        else
        {
            Debug.LogError("GridManager not found in scene.");
        }
        Transform closestTarget = GridManager.i.GetClosestPoint(usedGrid, transform);
        transform.position = new Vector3(closestTarget.position.x, transform.position.y, closestTarget.position.z);

        // Check if new X position is odd
        int xRounded = Mathf.RoundToInt(transform.position.x);
        bool isOdd = xRounded % 2 != 0;

        // Apply rotation based on X parity
        float yRotation = isOdd ? 90f : 0f;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    void Update()
    {
        if (GridManager.i == null || !agent.isOnNavMesh || agent.pathPending) return;

        if ((!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance) && !following && !waitingForVisibilityDrop)
        {
            if (currentState == NPCState.Approach)
                PrepareAttack();
            if (currentState == NPCState.Attack)
                StartAttack();
        }


        if (DrawLineToPlayerIfVisible() && currentState == NPCState.Wait)
        {
            currentState = NPCState.WaitToMove;
            StartCoroutine(LostSight(2f));
            agent.ResetPath();
        }
        if (currentState == NPCState.WaitToMove || currentState == NPCState.Approach)
        {
            if (!IsVisibleToPlayer())
            {
                currentState = NPCState.WaitToMove;
                Transform closestTarget = GridManager.i.GetClosestPoint(usedGrid, transform);
                transform.position = new Vector3(closestTarget.position.x, transform.position.y, closestTarget.position.z);
                agent.ResetPath();
            }
            else
            {
                currentState = NPCState.Approach;
            }
            if (GetDistanceToPlayer())
            {
                currentState = NPCState.Attack;
            }
        }
    }
    public bool GetDistanceToPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector3 npcPosition = transform.position;
        Vector3 playerPosition = playerObj.transform.position;

        // Check if new X position is odd
        int xRounded = Mathf.RoundToInt(transform.position.x);
        bool isOdd = xRounded % 2 != 0;

        // Apply rotation based on X parity
        float yRotation = isOdd ? 90f : 0f;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        return attackRange >= Vector3.Distance(npcPosition, playerPosition);
    }

    public void PrepareAttack()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        agent.SetDestination(playerObj.transform.position);
    }
    public void StartAttack()
    {
        agent.speed = 4;
        StartCoroutine(Attack(2f));
    }
    private IEnumerator LostSight(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Waited " + delay + " seconds.");
    }
    private IEnumerator Attack(float delay)
    {
        while (true)
        {
            yield return null;
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            agent.SetDestination(playerObj.transform.position);
        }
    }
    public bool IsVisibleToPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null || agent == null || !agent.isOnNavMesh) return false;

        Transform player = playerObj.transform;
        Vector3 npcPosition = transform.position + new Vector3(0, 0.45f, 0);
        Vector3 playerPosition = player.position;

        float distance = Vector3.Distance(npcPosition, playerPosition);
        if (distance > 10f) return false;

        bool hasLineOfSight = !Physics.Linecast(playerPosition, npcPosition, blockingMask);
        if (!hasLineOfSight) return true;

        Vector3 directionToNPC = (npcPosition - playerPosition).normalized;
        float angle = Vector3.Angle(player.forward, directionToNPC);

        // Return true if player is NOT looking at NPC
        return angle > 90f;
    }


    public void NormalPatrol()
    {
        Transform closestTarget = GridManager.i.GetRandomPointInRange(usedGrid, transform, 10);
        Transform furtherTarget = GridManager.i.GetFurthestPoint(usedGrid, transform);

        if (closestTarget != null)
        {
            agent.SetDestination(closestTarget.position);
            usedGrid.Remove(closestTarget);
        }
        else if (furtherTarget != null)
        {
            agent.SetDestination(furtherTarget.position);
            usedGrid.Remove(furtherTarget);
        }
        else
        {
            usedGrid = new List<Transform>(GridManager.i.grid);
            furtherTarget = GridManager.i.GetFurthestPoint(usedGrid, transform);
            agent.SetDestination(furtherTarget.position);
            usedGrid.Remove(furtherTarget);
        }
    }
    private bool DrawLineToPlayerIfVisible()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return false;

        Transform player = playerObj.transform;
        Vector3 npcPosition = transform.position + new Vector3(0, 0.45f, 0);
        Vector3 playerPosition = player.position;

        // Direction from NPC to Player
        Vector3 directionToPlayer = (playerPosition - npcPosition).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        // Check if Player is in front of NPC
        if (angle < 90f)
        {
            // Check for wall obstruction
            bool blocked = Physics.Linecast(npcPosition, playerPosition, blockingMask);

            if (!blocked)
            {
                return true;
            }
        }
        return false;
    }


    private void OnDrawGizmos()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Transform player = playerObj.transform;
        Vector3 npcPosition = transform.position + new Vector3(0, 0.45f, 0);
        Vector3 playerPosition = player.position;

        // Direction from NPC to Player
        Vector3 directionToPlayer = (playerPosition - npcPosition).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        // Check if Player is in front of NPC
        if (angle < 90f)
        {
            // Check for wall obstruction
            bool blocked = Physics.Linecast(npcPosition, playerPosition, blockingMask);

            if (!blocked)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(npcPosition, playerPosition);
                Gizmos.DrawSphere(npcPosition, 0.2f);
                Gizmos.DrawSphere(playerPosition, 0.2f);
            }
            else
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(npcPosition, playerPosition); // Optional: show blocked line in gray
            }
        }
    }
}
