using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TheWhisper : NPCBase
{
    public enum NPCState
    {
        Wait,
        Look
    }

    public GameObject eye;
    public GameObject eyebody;
    public GameObject mouth;
    private Animator animator;
    private Doorway doorway;
    public NPCState currentState = NPCState.Look;

    void Start()
    {
        eye.SetActive(true);
        mouth.SetActive(false);
        animator = GetComponent<Animator>();
        doorway = GetComponent<Doorway>();
        animator.SetBool("LookedAt", false);
        animator.SetTrigger("Look");

        StartCoroutine(RotateAndLook());
    }
    public void Move(Transform target)
    {
        if (RoomGenerator.i == null || target == null)
        {
            StartCoroutine(RotateAndLook());
            return;
        }

        if (doorway.connectedTo != null)
            doorway.connectedTo.Disconnect();

        BoxCollider[] colliders = GetComponents<BoxCollider>();
        List<Vector3> oldBorders = new List<Vector3>();
        foreach (BoxCollider col in colliders)
        {
            if (col.isTrigger) continue;
            Vector3 localCenter = col.center;
            Vector3 worldPos = transform.TransformPoint(localCenter);
            Vector3 resolved = SetToResolution(worldPos);
            oldBorders.Add(resolved);
        }
        foreach (Vector3 pos in oldBorders)
            RoomGenerator.i.bonusRoomsPositions.Remove(pos);

        Doorway closestDoor = null;
        float closestDist = float.MaxValue;

        foreach (var (candidateDoor, _) in RoomGenerator.i.unusedDoors)
        {
            float dist = Vector3.Distance(target.position, candidateDoor.transform.position);
            if (dist < closestDist)
            {
                transform.position = candidateDoor.transform.position + new Vector3(0, 1, 0);
                transform.rotation = candidateDoor.transform.rotation;

                List<Vector3> mimicBorders = new List<Vector3>();
                foreach (BoxCollider col in colliders)
                {
                    if (col.isTrigger) continue;
                    Vector3 localCenter = col.center;
                    Vector3 worldPos = transform.TransformPoint(localCenter);
                    Vector3 resolved = SetToResolution(worldPos);
                    mimicBorders.Add(resolved);
                }

                List<Vector3> allRoomPositions = new List<Vector3>(RoomGenerator.i.AllOccupiedSpaces());
                bool overlaps = mimicBorders.Any(pos => allRoomPositions.Contains(pos));

                if (!overlaps)
                {
                    closestDist = dist;
                    closestDoor = candidateDoor;
                }
            }
        }

        if (closestDoor != null)
        {
            transform.position = closestDoor.transform.position + new Vector3(0, 1, 0);
            transform.rotation = closestDoor.transform.rotation;

            closestDoor.ConnectTo(doorway);
            closestDoor.Fill(true);

            List<Vector3> newBorders = new List<Vector3>();
            foreach (BoxCollider col in colliders)
            {
                if (col.isTrigger) continue;
                Vector3 localCenter = col.center;
                Vector3 worldPos = transform.TransformPoint(localCenter);
                Vector3 resolved = SetToResolution(worldPos);
                newBorders.Add(resolved);
            }
            foreach (Vector3 pos in newBorders)
                RoomGenerator.i.bonusRoomsPositions.Add(pos);
        }
        StartCoroutine(RotateAndLook());
    }

    public void RoutineSelection()
    {
        currentState = currentState == NPCState.Wait? NPCState.Look : NPCState.Wait;
        switch (currentState)
        {
            case NPCState.Look:
                StartCoroutine(RotateAndLook());
                break;
            case NPCState.Wait:
                StartCoroutine(CloseAndWait());
                break;
        }
    }
    public void Initialize(float lineLength, LayerMask mask)
    {
        visionRange = lineLength;
        blockingMask = mask;
    }
    public IEnumerator WaitTheStun()
    {
        yield return new WaitForSeconds(5f);
        stunned = false;
        RoutineSelection();
    }
    public IEnumerator LookAtPlayer(float rotationSpeed = 5f, float waitTime = 3f)
    {
        bool stop = false;
        bool wasVisible = false;
        bool wasInMiddleRange = false;
        Transform lastTarget = visibleTargets[0];
        while (!stop)
        {
            if (visibleTargets.Count != 0)
            {
                Vector3 axisLocal = Vector3.forward;
                Vector3 currentLocal = eyebody.transform.InverseTransformDirection(viewPoint.forward);
                Vector3 desiredLocal = eyebody.transform.InverseTransformDirection(
                    (visibleTargets[0].position - viewPoint.position).normalized
                );

                Vector3 currentOnPlane = Vector3.ProjectOnPlane(currentLocal, axisLocal);
                Vector3 desiredOnPlane = Vector3.ProjectOnPlane(desiredLocal, axisLocal);

                float delta = Vector3.SignedAngle(currentOnPlane, desiredOnPlane, axisLocal);
                eyebody.transform.localRotation = Quaternion.AngleAxis(delta, axisLocal) * eyebody.transform.localRotation;

                if (IsInAttackRange(visibleTargets[0]))
                {
                    animator.SetTrigger("Bite");
                    yield return null;
                }
                else
                if (IsInMiddleRange(visibleTargets[0]))
                {
                    if (!wasInMiddleRange)
                    {
                        if(!wasVisible)
                        {
                            animator.SetBool("LookedAt", !wasVisible);
                            animator.SetTrigger("Look");
                        }
                        wasInMiddleRange = true;
                        wasVisible = true;
                    }
                }
                else
                if (wasVisible != isVisible || wasInMiddleRange)
                {
                    if(wasVisible != isVisible)
                    {
                        animator.SetBool("LookedAt", isVisible);
                        animator.SetTrigger("Look");
                    }
                    wasVisible = wasInMiddleRange ? true: isVisible;
                    wasInMiddleRange = false;
                }
                else
                if (stunned)
                {
                    stop = true;
                }
                if(visibleTargets.Count != 0)
                    lastTarget = visibleTargets[0];
                yield return null;
            }
            else
            {
                if(!wasVisible)
                {
                    wasVisible = true;
                    animator.SetBool("LookedAt", isVisible);
                    animator.SetTrigger("Look");
                }
                float timer = 0f;
                bool playerCameBack = false;

                while (timer < waitTime)
                {
                    if (visibleTargets.Count != 0)
                    {
                        playerCameBack = true;
                        break;
                    }

                    timer += Time.deltaTime;
                    yield return null;
                }

                if (!playerCameBack)
                {
                    Move(lastTarget);
                    yield break;
                }
            }
        }
    }
    private IEnumerator CloseAndWait()
    {
        animator.SetBool("LookedAt", true);
        animator.SetTrigger("Look");
        yield return new WaitForSeconds(1f);
        animator.SetBool("LookedAt", false);
        animator.SetTrigger("Look");
        RoutineSelection();
    }
    private IEnumerator RotateAndLook()
    {
        int repeatCount = Random.Range(2, 6);
        bool seen = false;
        for (int i = 0; i < repeatCount; i++)
        {
            if (visibleTargets.Count != 0)
            {
                StartCoroutine(LookAtPlayer());
                seen = true;
                break;
            }

            float randomZ = Random.Range(-40f, 40f);
            Vector3 currentEuler = eyebody.transform.localEulerAngles;
            Quaternion targetRot = Quaternion.Euler(currentEuler.x, currentEuler.y, randomZ);

            float elapsed = 0f;
            float duration = 1f;

            while (elapsed < duration)
            {
                if (visibleTargets.Count != 0)
                {
                    StartCoroutine(LookAtPlayer());
                    seen = true;
                    break;
                }

                eyebody.transform.localRotation = Quaternion.Slerp(
                    eyebody.transform.localRotation,
                    targetRot,
                    elapsed / duration
                );

                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(1f);
        }
        if (!seen)
            RoutineSelection();
    }
    public override void Stun()
    {
        eyebody.transform.localRotation = Quaternion.identity;
        stunned = true;
        StartCoroutine(WaitTheStun());
    }
    public override void Die()
    {
        doorway.connectedTo.Disconnect();
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        List<Vector3> mimicBorders = new List<Vector3>();

        foreach (BoxCollider col in colliders)
        {
            if (col.isTrigger)
                continue;
            Vector3 localCenter = col.center;
            Vector3 worldPos = transform.TransformPoint(localCenter);
            Vector3 resolved = SetToResolution(worldPos);
            mimicBorders.Add(resolved);
        }
        foreach (Vector3 pos in mimicBorders)
        {
            RoomGenerator.i.bonusRoomsPositions.Remove(pos);
        }

        base.Die();
    }
    public override void OnDrawGizmosSelected()
    {
        if (viewPoint == null || !debug) return;
        Vector3 origin = viewPoint.position;
        Vector3 forward = new Vector3(viewPoint.forward.x, 0f, viewPoint.forward.z).normalized;

        Handles.color = Color.yellow;

        // Draw central forward line
        Handles.DrawLine(origin, origin + forward * visionRange);

        Vector3 leftDir = Quaternion.AngleAxis(-maxAngle, Vector3.up) * forward;
        Handles.DrawLine(origin, origin + leftDir * visionRange);
        Vector3 rightDir = Quaternion.AngleAxis(maxAngle, Vector3.up) * forward;
        Handles.DrawLine(origin, origin + rightDir * visionRange);
        Handles.DrawWireArc(origin, Vector3.up, leftDir, maxAngle * 2, visionRange);

        Vector3 position = bodyReference.position;
        Vector3 a = new Vector3(bodyReference.right.x, 0f, bodyReference.right.z).normalized;
        Vector3 b = new Vector3(bodyReference.forward.x, 0f, bodyReference.forward.z).normalized;
        Vector3 axis = Vector3.Cross(a, b).normalized; // same plane as red arc

        Handles.color = Color.white;
        // Optional: draw left/right edges of vision cone
        Vector3 left = position + Quaternion.AngleAxis(-90f, axis) * b * visionRange;
        Vector3 right = position + Quaternion.AngleAxis(90f, axis) * b * visionRange;
        Handles.DrawLine(left, right);

        Handles.color = Color.red;
        Handles.DrawAAPolyLine(2f, Arch(a, b, (attckRange.y / 2f), attckRange.x));
        Handles.color = Color.white;
        Handles.DrawAAPolyLine(2f, Arch(a, b, visionRange, visionRange));
        Handles.color = Color.blue;
        Handles.DrawAAPolyLine(2f, Arch(a, b, middleRange, middleRange)); 
    }
}