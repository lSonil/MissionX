
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.HableCurve;

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
    public float maxAngle = 80f;
    public AudioSource asr;

    public Vector3 attckRange = Vector3.zero;
    public Transform viewPoint;
    protected List<Transform> visibleTargets = new List<Transform>();

    public bool isVisible = false;
    public bool stunned = false;
    public bool debug = false;
    private void Awake()
    {
        asr = GetComponent<AudioSource>();
    }
    public virtual void Update()
    {
        ScanForVisibleTatgets();
        // to kill
        // if (weakPoint!=null) weakPoint.IsAlive(gameObject);
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
                (horizAngle >= 0 && horizAngle <= maxAngle) ||
                (horizAngle < 0 && Mathf.Abs(horizAngle) <= maxAngle);
            
            bool withinVertical =
                (vertAngle >= 0 && vertAngle <= maxAngle) ||
                (vertAngle < 0 && Mathf.Abs(vertAngle) <= maxAngle);

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
    public bool IsInRange(Transform target)
    {
        if (bodyReference == null) return false;

        Vector3 toPlayer = target.position - bodyReference.position;

        Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float distance = flatDir.magnitude;

        Vector3 forward = new Vector3(bodyReference.forward.x, 0f, bodyReference.forward.z).normalized;
        bool isInFront = Vector3.Dot(forward, flatDir.normalized) >= 0f;
        return isInFront && distance <= visionRange;
    }
    public bool IsInMiddleRange(Transform target)
    {
        if (bodyReference == null) return false;

        Vector3 toPlayer = target.position - bodyReference.position;

        Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float distance = flatDir.magnitude;

        Vector3 forward = new Vector3(bodyReference.forward.x, 0f, bodyReference.forward.z).normalized;
        bool isInFront = Vector3.Dot(forward, flatDir.normalized) >= 0f;

        return isInFront && distance <= middleRange;
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
    public Vector3 SetToResolution(Vector3 worldCenter)
    {
        worldCenter.x = Mathf.RoundToInt(worldCenter.x);
        worldCenter.y = Mathf.RoundToInt(worldCenter.y);
        worldCenter.z = Mathf.RoundToInt(worldCenter.z);
        return worldCenter;
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
    public virtual void Die()
    {
        Destroy(gameObject);
    }
    public virtual void Stun()
    { print("stunned"); }
    public virtual void SetContained()
    {
        contained = contained == ContainedState.Free ? ContainedState.Contained : ContainedState.Free;
    }
    public virtual void SetSuppresed()
    {
        contained = ContainedState.Suppressed;
    }
    public virtual void SetVisibility(bool state)
    {
        isVisible = state;
    }
    public virtual void OnDrawGizmosSelected()
    {
        if (viewPoint == null || !debug) return;

        Vector3 origin = viewPoint.position;
        Vector3 forward = viewPoint.forward;

        float length = visionRange;
        float horizAngle = maxAngle;
        float vertAngle = maxAngle;

        // Compute cone radius at the far end
        float maxAng = Mathf.Max(horizAngle, vertAngle);
        float radius = Mathf.Tan(maxAngle * Mathf.Deg2Rad) * length;

        // Center of the base circle
        Vector3 baseCenter = origin + forward * length;

        // Draw the base circle
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(baseCenter, forward, radius);

        // Connect origin to points around the circle
        int segments = 24;
        for (int i = 0; i < segments; i++)
        {
            float angle = (360f / segments) * i;
            Vector3 dir = Quaternion.AngleAxis(angle, forward) * viewPoint.up;
            Vector3 pointOnCircle = baseCenter + dir * radius;
            Gizmos.DrawLine(origin, pointOnCircle);
        }
    }
}
