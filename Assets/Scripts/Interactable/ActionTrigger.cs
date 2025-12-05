using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerMode { NormalToggle, OneWay, AlwaysForward }

public class ActionTrigger : MonoBehaviour, IInteraction
{
    public string interactionPromptText;
    public ActionList actionList;
    public TriggerMode mode = TriggerMode.NormalToggle;
    public int itemToUseID = -1;

    [Tooltip("Only used for OneWay: trigger only if first action isOpen matches this")]
    public bool oneWayRequiredState = false;

    private bool running = false;

    public string GetTextUse() => interactionPromptText;
    public string GetTextPrimary() => "";
    public string GetTextSecundary() => "";

    public void Action(int i)
    {
        if (i == itemToUseID || itemToUseID == -1)
        {
            if (!running)
                StartCoroutine(RunActions());
        }
    }

    private IEnumerator RunActions()
    {
        running = true;

        List<ActionData> actions = new List<ActionData>(actionList.actions);
        if (actions.Count == 0)
        {
            running = false;
            yield break;
        }

        var mainTarget = actions[0].target;
        bool currentState = mainTarget.IsOpen();

        bool playForward = true;

        if (mode == TriggerMode.NormalToggle)
        {
            playForward = !currentState;
        }
        else if (mode == TriggerMode.OneWay)
        {
            if (oneWayRequiredState != currentState)
            {
                running = false;
                yield break;
            }

            playForward = !currentState;
        }
        else if (mode == TriggerMode.AlwaysForward)
        {
            playForward = true;
        }

        if (!playForward)
            actions.Reverse();

        List<Coroutine> activeRoutines = new List<Coroutine>();

        for (int i = 0; i < actions.Count; i++)
        {
            var a = actions[i];
            if (a.target == null)
                continue;

            ActionData.ExecuteMode actualMode = !playForward? a.reverseMode: a.mode;

            IEnumerator routine = a.target.DoAction(
                a.type,
                a.AxisToVector(),
                a.openValue,
                a.closedValue,
                a.duration,
                playForward
            );

            if (routine == null)
                continue;

            if (actualMode == ActionData.ExecuteMode.After)
            {
                foreach (var r in activeRoutines)
                    yield return r;

                activeRoutines.Clear();
            }

            activeRoutines.Add(StartCoroutine(routine));
        }

        foreach (var r in activeRoutines)
            yield return r;

        running = false;
    }
}
