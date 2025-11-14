using System.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MimicSpawn : NPCBase
{
    public GameObject eye;
    public GameObject eyebody;
    public GameObject mouth;
    private Animator animator;

    public enum NPCState
    {
        Wait,
        Check
    }

    public NPCState currentState = NPCState.Check;
    public void Initialize(float lineLength, LayerMask mask)
    {
        forwardLineLength = lineLength;
        blockingMask = mask;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eye.SetActive(true);
        mouth.SetActive(false);
        animator = GetComponent<Animator>();
        animator.SetBool("LookedAt", false);
        animator.SetTrigger("Look");

        CheckOut();
    }
    public void RoutinSelection()
    {
        switch (currentState)
        {
            case NPCState.Check:
                CheckOut();
                break;
            case NPCState.Wait:
                StartCoroutine(WaitToLook());
                break;
        }
    }
    private IEnumerator WaitToLook()
    {
        animator.SetBool("LookedAt", true);
        animator.SetTrigger("Look");
        yield return new WaitForSeconds(1f);
        animator.SetBool("LookedAt", false);
        animator.SetTrigger("Look");
        currentState = NPCState.Check;
        RoutinSelection();

    }
    private IEnumerator WaitToNotBeSeen(Transform player)
    {
        animator.SetBool("LookedAt", IsVisibleToPlayer());
        animator.SetTrigger("Look");
        while (IsVisibleToPlayer())
        {
            Vector3 axisLocal = Vector3.forward;
            Vector3 currentLocal = eyebody.transform.InverseTransformDirection(viewPoint.forward);
            Vector3 desiredLocal = eyebody.transform.InverseTransformDirection(
                (player.position - viewPoint.position).normalized
            );

            Vector3 currentOnPlane = Vector3.ProjectOnPlane(currentLocal, axisLocal);
            Vector3 desiredOnPlane = Vector3.ProjectOnPlane(desiredLocal, axisLocal);

            float delta = Vector3.SignedAngle(currentOnPlane, desiredOnPlane, axisLocal);
            eyebody.transform.localRotation = Quaternion.AngleAxis(delta, axisLocal) * eyebody.transform.localRotation;

            yield return null;
        }
        animator.SetBool("LookedAt", IsVisibleToPlayer());
        animator.SetTrigger("Look");
        StartCoroutine(FocusOnPlayerCoroutine(player));
    }
    bool isWatching;
    bool isRotating;
    bool isFocused;
    bool isBiting;
    public void CheckOut()
    {
        isRotating = true;
        isWatching = true;

        // Start both coroutines
        StartCoroutine(CheckForPlayer());
        StartCoroutine(Rotate());
    }

    private IEnumerator CheckForPlayer()
    {
        while (isRotating)
        {
            if (PlayerInConeLineOfSight())
            {
                isFocused = true;
                isWatching = false;
                StopCoroutine(Rotate());

                Transform player = GameObject.FindGameObjectWithTag("Player").transform;
                StartCoroutine(FocusOnPlayerCoroutine(player));

                yield break;
            }

            yield return null;
        }
        isWatching = false;
    }

    private IEnumerator Rotate()
    {
        int repeatCount = Random.Range(2, 6);

        for (int i = 0; i < repeatCount; i++)
        {
            float randomZ = Random.Range(-40f, 40f);
            Vector3 currentEuler = eyebody.transform.localEulerAngles;
            Quaternion targetRot = Quaternion.Euler(currentEuler.x, currentEuler.y, randomZ);

            float elapsed = 0f;
            float duration = 1f;

            while (elapsed < duration && isWatching)
            {
                eyebody.transform.localRotation = Quaternion.Slerp(
                    eyebody.transform.localRotation,
                    targetRot,
                    elapsed / duration
                );

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!isWatching) { break; }
            yield return new WaitForSeconds(1f);
        }
        isRotating = false;
        if (!isFocused)
        {
            currentState = NPCState.Wait;
            RoutinSelection();
        }
    }


    public IEnumerator FocusOnPlayerCoroutine(Transform player, float rotationSpeed = 5f, float waitTime = 3f)
    {
        while (true)
        {
            if (PlayerInConeLineOfSight())
            {
                Vector3 axisLocal = Vector3.forward;
                Vector3 currentLocal = eyebody.transform.InverseTransformDirection(viewPoint.forward);
                Vector3 desiredLocal = eyebody.transform.InverseTransformDirection(
                    (player.position - viewPoint.position).normalized
                );

                Vector3 currentOnPlane = Vector3.ProjectOnPlane(currentLocal, axisLocal);
                Vector3 desiredOnPlane = Vector3.ProjectOnPlane(desiredLocal, axisLocal);

                float delta = Vector3.SignedAngle(currentOnPlane, desiredOnPlane, axisLocal);
                eyebody.transform.localRotation = Quaternion.AngleAxis(delta, axisLocal) * eyebody.transform.localRotation;

                if (isBiting)
                {
                    isBiting = false;

                    animator.SetTrigger("Bite");
                    currentState = NPCState.Check;

                    RoutinSelection();

                    yield break;
                }

                if (IsVisibleToPlayer())
                {
                    StartCoroutine(WaitToNotBeSeen(player));
                    yield break;
                }

                yield return null;
                continue;
            }

            float timer = 0f;
            bool playerCameBack = false;

            while (timer < waitTime)
            {
                if (PlayerInConeLineOfSight())
                {
                    playerCameBack = true;
                    break;

                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (!playerCameBack)
            {
                currentState = NPCState.Wait;
                RoutinSelection();

                yield break;
            }
        }
    }
    private void OnDrawGizmos()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null || viewPoint == null) return;

        Vector3 npcPosition = viewPoint.position;
        Vector3 playerPosition = playerObj.transform.position;

        // Direction to player
        Vector3 toPlayer = (playerPosition - npcPosition).normalized;
        float distToPlayer = Vector3.Distance(npcPosition, playerPosition);

        // Angle boundaries (cone)
        float angleSize = 40f;
        Vector3 leftBoundary = Quaternion.Euler(0, -angleSize * 0.5f, 0) * viewPoint.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, angleSize * 0.5f, 0) * viewPoint.forward;

        // --- Draw left boundary raycast ---
        if (Physics.Raycast(npcPosition, leftBoundary, out RaycastHit leftHit, forwardLineLength, blockingMask))
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(npcPosition, leftHit.point);
            Gizmos.DrawSphere(leftHit.point, 0.1f);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(npcPosition, npcPosition + leftBoundary * forwardLineLength);
        }

        // --- Draw right boundary raycast ---
        if (Physics.Raycast(npcPosition, rightBoundary, out RaycastHit rightHit, forwardLineLength, blockingMask))
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(npcPosition, rightHit.point);
            Gizmos.DrawSphere(rightHit.point, 0.1f);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(npcPosition, npcPosition + rightBoundary * forwardLineLength);
        }

        float angleToPlayer = Vector3.Angle(viewPoint.forward, toPlayer);

        if (angleToPlayer <= angleSize * 0.5f && distToPlayer <= forwardLineLength)
        {
            if (!Physics.Linecast(npcPosition, playerPosition, blockingMask))
            {
                Gizmos.color = Color.green; // clear line of sight
                Gizmos.DrawLine(npcPosition, playerPosition);
                Gizmos.DrawSphere(playerPosition, 0.2f);
            }
            else
            {
                Gizmos.color = Color.yellow; // blocked
                Gizmos.DrawLine(npcPosition, playerPosition);
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            isBiting = true;
        }
    }
}