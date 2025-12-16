using System.Collections;
using UnityEditor;
using UnityEngine;

public class TheBreeze : NPCBase
{
    public override void SetVisibility(bool state, Transform seenBy)
    {
        if (ItIsContained()) return;

        base.SetVisibility(state,seenBy);

        if (!state && !IsVisible())
        {
            if (!IsInMiddleRange(seenBy))
            {
                TriggerLookAwayEvent();
                if (IsInRange(seenBy))
                {
                    StartCoroutine(PreparePrepareTrigger());
                }
                else
                {
                    TriggerLookAwayEvent();
                }
            }
        }
    }
    IEnumerator PreparePrepareTrigger()
    {
        float timer = 0f;
        const float duration = 2f;

        while (timer < duration)
        {
            if (IsVisible())
            {
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
        TriggerLookAwayEvent();
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
}
