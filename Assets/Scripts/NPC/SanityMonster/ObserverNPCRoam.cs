using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObserverNPCRoam : NPCBase
{
    public float sanityDrainRate = 0.01f;

    [Header("Ruby Patrol Settings")]
    public float stareTimeAtRuby = 3f;
    public float rubyProximityThreshold = 1.5f;

    public bool following;

    private IObserverState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        StartCoroutine(WaitForRubiesAndStart());
    }

    private IEnumerator WaitForRubiesAndStart()
    {
        yield return new WaitUntil(() => RubiesGenerator.Instance != null && RubiesGenerator.Instance.spawnedRubies.Count > 0);
        ChangeState(new PatrolState());
    }

    void Update()
    {
        if (GridManager.i == null || agent.pathPending) return;
        RaycastCone(); // Visualize cone
        currentState?.Execute(this);
    }

    public void ChangeState(IObserverState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    public void RaycastCone(float horizontalAngle = 60f, float verticalAngle = 30f, int horizontalRays = 10, int verticalRays = 5, float rayLength = 20f)
    {
        Vector3 origin = transform.position; // Slightly above ground
        Vector3 forward = transform.forward;

        float halfH = horizontalAngle / 2f;
        float halfV = verticalAngle / 2f;

        int combinedMask = LayerMask.GetMask("Player", "Structure", "Interactable");
        var playerSeen = false;

        for (int v = 0; v < verticalRays; v++)
        {
            float vAngle = Mathf.Lerp(-halfV, halfV, v / (float)(verticalRays - 1));

            for (int h = 0; h < horizontalRays; h++)
            {
                float hAngle = Mathf.Lerp(-halfH, halfH, h / (float)(horizontalRays - 1));

                Quaternion rotation = Quaternion.Euler(vAngle, hAngle, 0);
                Vector3 direction = rotation * forward;

                //int obstacleMask = LayerMask.GetMask("Structure");
                //int playerMask = LayerMask.GetMask("Player");

                if (Physics.Raycast(origin, direction, out RaycastHit hit, rayLength, combinedMask))
                {
                    //if (Physics.Raycast(origin, direction, out RaycastHit hunt, rayLength, playerMask))
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.DrawLine(origin, hit.point, Color.blue);
                        playerSeen = true;
                        following = true;
                    }
                    else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Structure") ||
                            hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
                    {
                        Debug.DrawLine(origin, hit.point, Color.red);
                    }
                }
                else
                {
                    Debug.DrawRay(origin, direction * rayLength, Color.green);
                }

            }
        }

        if (!playerSeen)
            following = false;
    }
}
