using UnityEngine;

public class TheBreeze : NPCBase
{
    public bool isInside = false;
    public override void SetVisibility(bool state)
    {
        if (ItIsContained()) return;

        if (!state && isVisible && !isInside)
        {
            TriggerLookAwayEvent();
        }
        base.SetVisibility(state);
    }
    public void TriggerLookAwayEvent()
    {
        Doorway door = GetComponentInParent<Doorway>();
        bool randomHall = Random.value < 0.5f;
        bool randomFill = Random.value < 0.5f ? door.isFilled : !door.isFilled;
        if (Random.value < 0.25f)
        {
            if (door.connectedTo)
                GetComponentInParent<Doorway>().ForceFillBoth(randomHall, randomFill);
            else
                GetComponentInParent<Doorway>().ForceFill(randomHall, true);
        }
    }
    public bool ItIsContained()
    {
        NPCTheBreathingHall brain = GetComponentInParent<NPCTheBreathingHall>();

        return GetComponentInParent<NPCTheBreathingHall>().contained == ContainedState.Contained;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            isInside = true;
        }
    }
    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            isInside = false;
            if (!isVisible && !ItIsContained())
            {
                TriggerLookAwayEvent();
            }
        }
    }
}
