using System.Collections.Generic;
using UnityEngine;

public class RitualManager : MonoBehaviour
{
    public static RitualManager i;

    public int requiredRubies = 4;
    private HashSet<RubyPlacement> placedZones = new HashSet<RubyPlacement>();

    public Collider containmentTrigger;
    public bool ritualComplete { get; private set; } = false;

    private void Awake()
    {
        i = this;
        containmentTrigger.enabled = false;
    }

    public void NotifyRubyPlaced(RubyPlacement zone)
    {
        if (!placedZones.Contains(zone))
            placedZones.Add(zone);

        Debug.Log($"Ritual progress: {placedZones.Count}/{requiredRubies}");

        if (placedZones.Count >= requiredRubies)
        {
            ActivateContainmentZone();
        }
    }

    private void ActivateContainmentZone()
    {
        Debug.Log("All rubies placed! Containment zone ACTIVE.");
        ritualComplete = true;
        containmentTrigger.enabled = true;
    }
}
