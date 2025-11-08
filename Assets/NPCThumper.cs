using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class NPCThumper : MonoBehaviour
{
    public enum NPCState
    {
        Patrol,
        Hide,

        Approach,
        Lurk,

        Chase
    }

    private NavMeshAgent agent;
    private List<Transform> usedGrid;
    public DamageZone damageZone;
    public float forwardLineLength = 10f;
    public float attackRange = 1f;
    public float chaseRange = 3f;
    public float lurkTime = 5;
    public float forgetTime = 10;

    public Vector3 speed = new Vector3(0.1f, 2, 4);
    public NPCState currentState = NPCState.Hide;
    public LayerMask blockingMask;
    private Animator animator;

    bool contained = false;

    bool forget = false;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;
        animator = GetComponent<Animator>();

        if (GridManager.i != null)
        {
            usedGrid = new List<Transform>(GridManager.i.grid);
        }
        else
        {
            Debug.LogError("GridManager not found in scene.");
        }

        StartCoroutine(WaitInPlace());
    }
    void Update()
    {
        UpdateAnimatorStates();
    }
    private void UpdateAnimatorStates()
    {
        animator.SetBool("IsMoving", !(currentState == NPCState.Hide || currentState == NPCState.Lurk));
    }
    public void RoutinSelection()
    {
        if (contained)
        {
            StartCoroutine(WaitInPlace());
            return;
        }
        switch (currentState)
        {
            case NPCState.Patrol:
                NormalPatrol();
                break;

            case NPCState.Hide:
                StartCoroutine(WaitInPlace());
                break;

            case NPCState.Approach:
                StartCoroutine(Approach());
                break;

            case NPCState.Lurk:
                StartCoroutine(WaitToMove());
                break;

            case NPCState.Chase:
                StartCoroutine(Chase());
                break;

        }
    }

    public IEnumerator WaitInPlace()
    {
        SetSpeed(speed[0]);

        float timer = 0f;
        while (timer < lurkTime)
        {
            if (DirectLineToPlayer())
            {
                break; // Exit early if condition is met
            }
            timer += Time.deltaTime;
            yield return null; // wait one frame
        }

        yield return new WaitForSeconds(.5f);

        if (DirectLineToPlayer())
            currentState = IsVisibleToPlayer() ? NPCState.Lurk : NPCState.Approach;
        else
            currentState = NPCState.Patrol;
        RoutinSelection();
    }
    public IEnumerator WaitToMove()
    {
        SetSpeed(speed[1]);

        agent.ResetPath();
        agent.velocity = Vector3.zero;
        while (IsVisibleToPlayer() && !IsInRange(attackRange))
        {
            yield return null; // wait one frame
        }
        if (!IsVisibleToPlayer())
        {
            yield return new WaitForSeconds(.1f);
            currentState = NPCState.Approach;
            RoutinSelection();
        }
        else
        {
            animator.SetTrigger("Bite");
            yield return new WaitForSeconds(.1f);
            currentState = NPCState.Chase;
            RoutinSelection();
        }
    }

    public void NormalPatrol()
    {
        SetSpeed(speed[0]);

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
        StartCoroutine(CheckDestination());
    }
    public IEnumerator Approach()
    {
        SetSpeed(speed[1]);

        forget = false;
        Coroutine forgetRoutine;
        forgetRoutine = StartCoroutine(ForgetPlayer());

        while (!IsVisibleToPlayer() && !IsInRange(chaseRange) && !forget)
        {
            Transform playerPos = GridManager.i.GetPlayerTransform();
            yield return null; // wait one frame
            agent.SetDestination(playerPos.position);
        }
        if (forgetRoutine != null)
        {
            StopCoroutine(forgetRoutine);
        }
        if (IsVisibleToPlayer())
        {
            yield return new WaitForSeconds(.1f);
            currentState = NPCState.Hide;
            RoutinSelection();
        }
        else if (IsInRange(chaseRange))
        {
            animator.SetTrigger("Morph");

            currentState = NPCState.Chase;
            RoutinSelection();
        }
        else
        {
            currentState = NPCState.Hide;
            RoutinSelection();
        }
    }

    public IEnumerator Chase()
    {
        SetSpeed(speed[2]);

        animator.SetBool("IsChasing", true);
        forget = false;
        Coroutine forgetRoutine;
        forgetRoutine = StartCoroutine(ForgetPlayer());

        while (!IsInRange(attackRange) && !forget)
        {
            Transform playerPos = GridManager.i.GetPlayerTransform();
            yield return null; // wait one frame
            if (playerPos == null)
            {
                forget = true;
            }
            else
            agent.SetDestination(playerPos.position);
        }
        if (forgetRoutine != null)
        {
            StopCoroutine(forgetRoutine);
        }
        if (forget)
        {
            currentState = NPCState.Hide;
            RoutinSelection();
        }
        else
        {
            animator.SetTrigger("Bite");
            yield return new WaitForSeconds(.1f);
            currentState = NPCState.Chase;
            RoutinSelection();
        }
    }
    
    public IEnumerator CheckDestination()
    {
        while (!HasReachedDestination(agent))
        {
            if (DirectLineToPlayer())
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                break;
            }
            yield return null;
        }
        currentState = DirectLineToPlayer() ? NPCState.Lurk : NPCState.Hide;
        RoutinSelection();
    }
    public IEnumerator ForgetPlayer()
    {
        float timer = 0f;
        while (!IsVisibleToPlayer() && timer < forgetTime)
        {
            if (DirectLineToPlayer())
            {
                break; // Exit early if condition is met
            }
            timer += Time.deltaTime;
            yield return null; // wait one frame
        }

        if (timer >= forgetTime)
        {

            forget = true;
        }
    }

    private bool HasReachedDestination(NavMeshAgent agent)
    {
        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }
    private bool DirectLineToPlayer()
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
    private bool IsVisibleToPlayer()
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
    private bool IsInRange(float range)
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

    private void SetSpeed(float value)
    {
        agent.speed = value;
    }    

    private void OnDrawGizmos()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Transform player = playerObj.transform;
        Vector3 npcPosition = transform.position + new Vector3(0, 0.45f, 0);
        Vector3 playerPosition = player.position;

        if (DirectLineToPlayer())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(npcPosition, playerPosition); // Optional: show blocked line in gray
        }
    }
}