
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
    public Transform bodyReference;
    public WeakPoint weakPoint;

    public ContainedState contained = ContainedState.Free;
    public LayerMask blockingMask;
    public LayerMask validMask;      // which objects count as NPCs/targets
    public float visionRange = 10f;
    public float middleRange = 10f;
    [Range(0, 180)] public float scanMaxAngle = 80f;
    public AudioSource asr;

    public Vector3 attckRange = Vector3.zero;
    public Transform viewPoint;
    protected List<Transform> visibleTargets = new List<Transform>();
    protected List<Transform> isVisibleTo = new List<Transform>();
    private Animator animator;

    public bool stunned = false;

    public bool debug = false;
    private void Awake()
    {
        asr = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

    }
    public virtual void Update()
    {
        ScanForVisibleTatgets();
        UpdateAnimatorStates();
        // to kill
        // if (weakPoint!=null) weakPoint.IsAlive(gameObject);
    }
    public bool IsVisible()
    {
        return isVisibleTo.Count>0;
    }
    public bool IsInRange(Transform target)
    {
        if (bodyReference == null) return false;

        Vector3 toPlayer = target.position - bodyReference.position;

        Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float distance = flatDir.magnitude;

        //Vector3 forward = new Vector3(bodyReference.forward.x, 0f, bodyReference.forward.z).normalized;
        //bool isInFront = Vector3.Dot(forward, flatDir.normalized) >= 0f;
        return distance <= visionRange;
    }
    public bool IsInMiddleRange(Transform target)
    {
        if (bodyReference == null) return false;

        Vector3 toPlayer = target.position - bodyReference.position;

        Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float distance = flatDir.magnitude;

        //Vector3 forward = new Vector3(bodyReference.forward.x, 0f, bodyReference.forward.z).normalized;
        //bool isInFront = Vector3.Dot(forward, flatDir.normalized) >= 0f;

        return distance <= middleRange;
    }
    public bool IsInAttackRange(Transform target)
    {
        Vector3 toPlayer = target.position - bodyReference.position;

        Vector3 right = new Vector3(bodyReference.right.x, 0f, bodyReference.right.z).normalized;
        Vector3 forward = new Vector3(bodyReference.forward.x, 0f, bodyReference.forward.z).normalized;

        float x = Vector3.Dot(toPlayer, right);
        float z = Vector3.Dot(toPlayer, forward);

        float b = attckRange.y / 2f; // width
        float a = attckRange.x;      // height

        return z >= 0f && ((x * x) / (a * a) + (z * z) / (b * b)) <= 1f;
    }
    public bool HasReachedDestination(NavMeshAgent agent)
    {
        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }
    public void ScanForVisibleTatgets()
    {
        visibleTargets.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, validMask);

        foreach (Collider hit in hits)
        {
            Transform target = hit.transform;
            Vector3 playerPosition = viewPoint.position;
            Vector3 targetPosition = target.position;

            if (!IsInRange(target)) continue;
            if (Physics.Linecast(playerPosition, targetPosition, blockingMask))
                continue;

            Vector3 directionToTarget = (targetPosition - playerPosition).normalized;
            Vector3 localDir = viewPoint.InverseTransformDirection(directionToTarget);
            float horizAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

            float vertAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

            bool withinHorizontal =
                (horizAngle >= 0 && horizAngle <= scanMaxAngle) ||
                (horizAngle < 0 && Mathf.Abs(horizAngle) <= scanMaxAngle);
            
            bool withinVertical =
                (vertAngle >= 0 && vertAngle <= scanMaxAngle) ||
                (vertAngle < 0 && Mathf.Abs(vertAngle) <= scanMaxAngle);

            if (withinHorizontal && withinVertical)
            {
                if (target.CompareTag("Player"))
                {
                    visibleTargets.Add(target);
                }
                else
                {
                    Stun();
                }
            }
        }

        visibleTargets = visibleTargets
            .OrderBy(t => Vector3.Distance(transform.position, t.position))
            .ToList();
    }
    public virtual void Die()
    {
        Destroy(gameObject);
    }
    public virtual void SetContained()
    {
        contained = contained == ContainedState.Free ? ContainedState.Contained : ContainedState.Free;
    }
    public virtual void SetSuppresed()
    {
        contained = ContainedState.Suppressed;
    }
    public virtual void SetVisibility(bool state, Transform seenBy)
    {
        if (state)
            isVisibleTo.Add(seenBy);
        else
            isVisibleTo.Remove(seenBy);
    }
    public virtual void Stun(){}

    public virtual void UpdateAnimatorStates(){}
    public Vector3 SetToResolution(Vector3 worldCenter)
    {
        worldCenter.x = Mathf.RoundToInt(worldCenter.x);
        worldCenter.y = Mathf.RoundToInt(worldCenter.y);
        worldCenter.z = Mathf.RoundToInt(worldCenter.z);
        return worldCenter;
    }
    
    public void OnDrawGizmosSelected()
    {
        if (viewPoint == null || !debug) return;
        Vector3 origin = viewPoint.position;
        Vector3 forward = new Vector3(viewPoint.forward.x, 0f, viewPoint.forward.z).normalized;

        Handles.color = Color.yellow;

        // Draw central forward line
        Handles.DrawLine(origin, origin + forward * visionRange);

        Vector3 leftDir = Quaternion.AngleAxis(-scanMaxAngle, Vector3.up) * forward;
        Handles.DrawLine(origin, origin + leftDir * visionRange);
        Vector3 rightDir = Quaternion.AngleAxis(scanMaxAngle, Vector3.up) * forward;
        Handles.DrawLine(origin, origin + rightDir * visionRange);
        Handles.DrawWireArc(origin, Vector3.up, leftDir, scanMaxAngle * 2, visionRange);

        Vector3 position = bodyReference.position;
        Vector3 a = new Vector3(bodyReference.right.x, 0f, bodyReference.right.z).normalized;
        Vector3 b = new Vector3(bodyReference.forward.x, 0f, bodyReference.forward.z).normalized;
        Vector3 axis = Vector3.Cross(a, b).normalized; // same plane as red arc


        Handles.color = Color.red;

        Vector3[] arcPoints = Arch(a, b, (attckRange.y / 2f), attckRange.x);
        Handles.DrawAAPolyLine(2f, arcPoints);
        Handles.DrawLine(position, arcPoints[0]);
        Handles.DrawLine(position, arcPoints[arcPoints.Length - 1]);

        Handles.color = Color.white;
        Handles.DrawWireArc(bodyReference.position, Vector3.up, Vector3.forward, 360, visionRange);

        Handles.color = Color.blue;
        Handles.DrawWireArc(bodyReference.position, Vector3.up, Vector3.forward, 360, middleRange);
    }
    public Vector3[] Arch(Vector3 directionA, Vector3 DirectionB, float width, float lentgh)
    {
        int segments = Mathf.Max(8, (int)attckRange.z); // ensure at least 8 segments

        Vector3[] points = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(-90f, 90f, t) * Mathf.Deg2Rad;

            // ellipse parametric equation
            float x = Mathf.Cos(angle) * width; // width
            float z = Mathf.Sin(angle) * lentgh;        // height

            Vector3 local = DirectionB * x + directionA * z;
            points[i] = bodyReference.position + local;
        }
        return points;
    }
}
