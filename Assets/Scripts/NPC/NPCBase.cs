
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SearchService;
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
    public WeakPoint weakPoint;

    public ContainedState contained = ContainedState.Free;
    public LayerMask blockingMask;
    public LayerMask validMask;      // which objects count as NPCs/targets
    public float forwardLineLength = 10f;
    public float maxAngle = 80f;
    protected List<Transform> visibleTargets = new List<Transform>();

    public Transform viewPoint;

    //public bool useWideCone = false; // state flag

    public bool isVisible = false;
    public bool stunned = false;
    public bool debug = false;
    public virtual void Update()
    {
        ScanForVisibleTatgets();
        // to kill
        // if (weakPoint!=null) weakPoint.IsAlive(gameObject);
    }

    public void SetContained()
    {
        contained = contained == ContainedState.Free ? ContainedState.Contained : ContainedState.Free;
    }

    public void SetSuppresed()
    {
        contained = ContainedState.Suppressed;
    }

    void ScanForVisibleTatgets()
    {
        visibleTargets.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, forwardLineLength, validMask);

        foreach (Collider hit in hits)
        {
            Transform target = hit.transform;
            Vector3 playerPosition = viewPoint.position;
            Vector3 targetPosition = target.position;

            float distance = Vector3.Distance(playerPosition, targetPosition);
            if (distance > forwardLineLength) continue;
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

    public virtual void Stun() { print("stunned"); }
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

    private void OnDrawGizmosSelected()
    {
        if (viewPoint == null || !debug) return;

        Vector3 origin = viewPoint.position;
        Vector3 forward = viewPoint.forward;

        float length = forwardLineLength;
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
