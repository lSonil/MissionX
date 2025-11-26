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
    private Doorway doorway;

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
        doorway = GetComponent<Doorway>();
        animator.SetBool("LookedAt", false);
        animator.SetTrigger("Look");

        CheckOut();
    }
    public override void Update()
    {
        base.Update();
        if (weakPoint != null && doorway!=null)
        {
            if(!weakPoint.HasHP())
            {
                doorway.Disconnect();
                weakPoint.IsAlive(gameObject);
            }
        }
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
    private IEnumerator WaitToNotBeSeen()
    {
        animator.SetBool("LookedAt", isVisible);
        animator.SetTrigger("Look");
        while (isVisible & visibleTargets.Count!=0)
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
            yield return null;
        }
        animator.SetBool("LookedAt", isVisible);
        animator.SetTrigger("Look");
        StartCoroutine(FocusOnPlayerCoroutine());
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
            if (visibleTargets.Count != 0)
            {
                isFocused = true;
                isWatching = false;
                StopCoroutine(Rotate());

                if (isBiting)
                {
                    isBiting = false;
                    animator.SetTrigger("Bite");
                    currentState = NPCState.Check;

                    RoutinSelection();
                }
                else
                    StartCoroutine(FocusOnPlayerCoroutine());

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

    public IEnumerator FocusOnPlayerCoroutine(float rotationSpeed = 5f, float waitTime = 3f)
    {
        while (true)
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

                if (isBiting)
                {
                    isBiting = false;

                    animator.SetTrigger("Bite");
                    currentState = NPCState.Check;

                    RoutinSelection();

                    yield break;
                }

                if (isVisible)
                {
                    StartCoroutine(WaitToNotBeSeen());
                    yield break;
                }

                if (stunned)
                {
                    break;
                }

                yield return null;
                continue;
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
                currentState = NPCState.Wait;
                RoutinSelection();

                yield break;
            }
        }
    }
    public override void Stun() 
    {
        eyebody.transform.localRotation = Quaternion.identity;
        stunned = true;
        StartCoroutine(WaitTheStun());
    }

    public IEnumerator WaitTheStun()
    {
        yield return new WaitForSeconds(5f);
        currentState = NPCState.Check;
        stunned = false;
        RoutinSelection();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            isBiting = true;
        }
    }

}