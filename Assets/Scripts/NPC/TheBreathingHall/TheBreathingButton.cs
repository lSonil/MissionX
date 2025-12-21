using UnityEngine;

public class TheBreathingButton : NPCBase
{    public override void SetVisibility(bool state, Transform t)
    {
        NPCTheBreathingHall brain = GetComponentInParent<NPCTheBreathingHall>();
        if (brain) if (GetComponentInParent<NPCTheBreathingHall>().contained == ContainedState.Contained) return;

        bool isInsideRoom = GetComponentInParent<IsInsideBreathingHall>().isPlayerInside;
        if (!state && IsVisible() && !isInsideRoom)
        {
            GetComponentInParent<NPCTheBreathingHall>().PlaceContainment();
        }
        base.SetVisibility(state, t);
    }
}