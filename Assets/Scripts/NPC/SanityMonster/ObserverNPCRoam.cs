using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObserverNPCRoam : NPCBase
{
    public float sanityDrainRate = 5f;

    [Header("Ruby Patrol Settings")]
    public float stareTimeAtRuby = 3f;
    public float rubyProximityThreshold = 1.5f;

    [Header("Chase State")]
    public bool following;
    public Transform currentSeenPlayer; // Dedicated reference for chase behavior

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
        // Priority 1: Check if contained (final state)
        if(contained != ContainedState.Free && currentState is not SanityContainedState)
        {
            ChangeState(new SanityContainedState());
            return;
        }
        if (contained != ContainedState.Free) return;
        
        // Priority 2: Check if ritual is complete (override all other states)
        if (RitualManager.i != null && RitualManager.i.ritualComplete && currentState is not GoToContainmentState)
        {
            ChangeState(new GoToContainmentState());
            return;
        }
        
        if (GridManager.i == null || agent.pathPending) return;
        RaycastCone(); // Visualize cone and update following status
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
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        float halfH = horizontalAngle / 2f;
        float halfV = verticalAngle / 2f;

        int combinedMask = LayerMask.GetMask("Player", "Structure", "Interactable");
        bool playerSeen = false;
        
        for (int v = 0; v < verticalRays; v++)
        {
            float vAngle = Mathf.Lerp(-halfV, halfV, v / (float)(verticalRays - 1));

            for (int h = 0; h < horizontalRays; h++)
            {
                float hAngle = Mathf.Lerp(-halfH, halfH, h / (float)(horizontalRays - 1));

                Quaternion rotation = Quaternion.Euler(vAngle, hAngle, 0);
                Vector3 direction = rotation * forward;

                if (Physics.Raycast(origin, direction, out RaycastHit hit, rayLength, combinedMask))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.DrawLine(origin, hit.point, Color.blue);
                        
                        playerSeen = true;
                        following = true;
                        currentSeenPlayer = hit.collider.transform; // Update dedicated player reference
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
        {
            following = false;
            // Don't clear currentSeenPlayer here - keep it for LastFollowState
        }
    }
}
