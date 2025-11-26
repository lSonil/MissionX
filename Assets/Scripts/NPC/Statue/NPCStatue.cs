using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCStatue : NPCBase
{
    public enum NPCState
    {
        Patrol,
        Hide,

        Approach,
        Lurk,

        Chase
    }

    private List<Transform> usedGrid;

    public float attackRange = 1f;
    public float chaseRange = 3f;
    public float lurkTime = 5;
    public float forgetTime = 10;

    public Vector2 speed = new Vector3(5f, 20);
    public NPCState currentState = NPCState.Hide;

    private Animator animator;

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
            if (visibleTargets.Count != 0)
            {
                break; // Exit early if condition is met
            }
            timer += Time.deltaTime;
            yield return null; // wait one frame
        }

        yield return new WaitForSeconds(.5f);

        if (visibleTargets.Count != 0)
            currentState = isVisible ? NPCState.Lurk : NPCState.Approach;
        else
            currentState = NPCState.Patrol;
        RoutinSelection();
    }
    public IEnumerator WaitToMove()
    {
        SetSpeed(speed[1]);

        agent.ResetPath();
        agent.velocity = Vector3.zero;
        while (isVisible)
        {
            yield return null; // wait one frame
        }
        if (!isVisible)
        {
            yield return new WaitForSeconds(.1f);
            currentState = NPCState.Approach;
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

        while (!isVisible && !IsInRange(chaseRange) && !forget)
        {
            Transform playerPos = GridManager.i.GetPlayerTransform();
            yield return null; // wait one frame
            agent.SetDestination(playerPos.position);
        }
        if (forgetRoutine != null)
        {
            StopCoroutine(forgetRoutine);
        }
        if (isVisible)
        {
            yield return new WaitForSeconds(.1f);
            currentState = NPCState.Hide;
            RoutinSelection();
        }
        else if (IsInRange(chaseRange))
        {
            GridManager.i.GetPlayerTransform().gameObject.GetComponent<HealthSystem>().Die();
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
        SetSpeed(speed[1]);

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
            yield return new WaitForSeconds(.1f);
            currentState = NPCState.Chase;
            RoutinSelection();
        }
    }

    public IEnumerator CheckDestination()
    {
        StartCoroutine(WaitInPlace());

        while (!HasReachedDestination(agent))
        {
            if (visibleTargets.Count != 0)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                break;
            }
            if (contained != ContainedState.Free)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
            yield return null;
        }

        currentState = visibleTargets.Count != 0 ? NPCState.Lurk : NPCState.Hide;
        RoutinSelection();
    }
    public IEnumerator ForgetPlayer()
    {
        float timer = 0f;
        while (!isVisible && timer < forgetTime)
        {
            if (visibleTargets.Count != 0)
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

    private void SetSpeed(float value)
    {
        agent.speed = value;
    }    
}