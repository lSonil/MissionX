using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class NPCStatue : NPCBase
{
    private List<Transform> usedGrid;
    public Vector2 speed = new Vector3(5f, 20);
    public IsInsideNPC isInsideContainment;
    void Start()
    {
        agent.enabled = true;
        if (GridManager.i != null)
        {
            usedGrid = new List<Transform>(GridManager.i.grid);
        }
        else
        {
            Debug.LogError("GridManager not found in scene.");
        }

        StartCoroutine(Patrol());
    }
    public override void UpdateAnimatorStates()
    {
        //animator.SetBool("IsMoving", !(currentState == NPCState.Hide || currentState == NPCState.Lurk));
    }
    public Transform GetTargetDestination()
    {
        SetSpeed(speed[0]);

        Transform furtherTarget = GridManager.i.GetFurthestPoint(usedGrid, transform);
        Transform closestTarget = GridManager.i.GetRandomPointInRange(usedGrid, transform, 10);
        Transform target;
        if (closestTarget != null)
        {
            usedGrid.Remove(closestTarget);
            target = closestTarget;
        }
        else if (furtherTarget != null)
        {
            usedGrid.Remove(furtherTarget);
            target = furtherTarget;
        }
        else
        {
            usedGrid = new List<Transform>(GridManager.i.grid);
            Transform newTarget = GridManager.i.GetRandomPointInRange(usedGrid, transform, 10);
            usedGrid.Remove(newTarget);
            target = newTarget;
        }

        return target;
    }
    public IEnumerator Patrol()
    {
        while(contained == ContainedState.Contained)
        {
            yield return null;
        }
        bool targetFound = false;
        SetSpeed(speed.x);
        agent.SetDestination(GetTargetDestination().position);

        while (!HasReachedDestination(agent))
        {
            if (visibleTargets.Count != 0)
            {
                targetFound = true;
                agent.ResetPath();
                StartCoroutine(ChasePlayer());
                break;
            }
            if (contained == ContainedState.Contained)
            {
                agent.ResetPath();
                break;
            }
            foreach (Transform t in isVisibleTo)
            {
                if (IsInMiddleRange(t))
                {
                    agent.ResetPath();

                    Vector3 dir = (t.position - transform.position);
                    dir.y = 0f; // keep it flat

                    if (dir.sqrMagnitude > 0.0001f)
                        transform.rotation = Quaternion.LookRotation(dir);
                    agent.ResetPath();
                    StartCoroutine(ChasePlayer());
                    break;

                }
            }
            yield return null;
        }

        if (!targetFound)
            StartCoroutine(Patrol());
    }
    public IEnumerator ChasePlayer(float waitTime = 300f)
    {
        SetSpeed(speed.y);
        bool stop = false;
        Transform lastTarget = visibleTargets[0];
        float initialAngle = scanMaxAngle;
        scanMaxAngle = 180f;
        while (!stop)
        {
            if (visibleTargets.Count != 0)
            {
                if (!IsVisible())
                {
                    agent.SetDestination(visibleTargets[0].position);
                }
                else
                {
                    agent.ResetPath();
                }

                if (IsInAttackRange(visibleTargets[0]))
                {
                    visibleTargets[0].gameObject.GetComponent<HealthSystem>().Die();
                    stop = true;
                }

                if (visibleTargets.Count != 0)
                    lastTarget = visibleTargets[0];

                yield return null;
            }
            else
            {
                scanMaxAngle = initialAngle;
                float timer = 0f;
                bool playerCameBack = false;

                while (timer < waitTime)
                {
                    if (visibleTargets.Count != 0)
                    {
                        playerCameBack = true;
                        scanMaxAngle = 180f;
                        break;
                    }
                    agent.SetDestination(lastTarget.position);

                    timer += Time.deltaTime;
                    yield return null;
                }

                if (!playerCameBack)
                {
                    scanMaxAngle = initialAngle;
                    stop = true;
                }
            }
        }
        StartCoroutine(Patrol());
    }
    public override void SetContained()
    {
        if(isInsideContainment.isInside)
        {
            base.SetContained();
            if(contained == ContainedState.Free)
                StartCoroutine(Patrol());
        }
    }
    private void SetSpeed(float value)
    {
        agent.speed = value;
    }    
}