using UnityEngine;

public class TheBreathingButton : NPCBase
{    public override void SetVisibility(bool state)
    {
        NPCTheBreathingHall brain = GetComponentInParent<NPCTheBreathingHall>();
        if (brain) if (GetComponentInParent<NPCTheBreathingHall>().contained == ContainedState.Contained) return;

        bool isInsideRoom = GetComponentInParent<IsInsideTheRoom>().isPlayerInside;
        if (!state && isVisible && !isInsideRoom)
        {
            GetComponentInParent<NPCTheBreathingHall>().PlaceContainment();
        }
        base.SetVisibility(state);
    }
}