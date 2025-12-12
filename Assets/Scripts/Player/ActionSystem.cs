using System.Collections;
using UnityEngine;

public class ActionSystem : MonoBehaviour
{
    private bool isBusy = false;
    private bool isOpen = false;

    public bool IsOpen() => isOpen;

    // If you ever need it:
    public void ForceOpen() => isOpen = true;

    public IEnumerator DoAction(
        ActionType type,
        Vector3 axis,
        float openValue,
        float closedValue,
        float duration,
        GameObject objToSpawn,
        bool playForward)
    {
        bool isTransformAction =
            type == ActionType.Move ||
            type == ActionType.Rotate ||
            type == ActionType.Resize;

        // Only block transform animations
        if (isTransformAction)
        {
            if (isBusy)
                yield break;

            isBusy = true;
        }

        // Non-transform actions run instantly and ignore all parameters
        switch (type)
        {
            case ActionType.PlaySound:
                GetComponent<AudioSource>()?.Play();
                yield break;

            case ActionType.Contain:
                GetComponent<NPCBase>()?.SetContained();
                yield break;

            case ActionType.Spawn:
                Instantiate(objToSpawn, transform.position, transform.rotation);
                yield break;
        }

        // TRANSFORM ACTIONS BELOW
        float startValue = playForward ? closedValue : openValue;
        float endValue = playForward ? openValue : closedValue;

        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;

        switch (type)
        {
            case ActionType.Move:
                start = transform.localPosition;
                end = start + axis.normalized * (endValue - startValue);
                break;

            case ActionType.Rotate:
                start = transform.localEulerAngles;
                end = start + axis.normalized * (endValue - startValue);
                break;

            case ActionType.Resize:
                start = transform.localScale;
                end = start + axis.normalized * (endValue - startValue);
                break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;

            switch (type)
            {
                case ActionType.Move:
                    transform.localPosition = Vector3.Lerp(start, end, t);
                    break;

                case ActionType.Rotate:
                    transform.localEulerAngles = Vector3.Lerp(start, end, t);
                    break;

                case ActionType.Resize:
                    transform.localScale = Vector3.Lerp(start, end, t);
                    break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to final
        switch (type)
        {
            case ActionType.Move:
                transform.localPosition = end;
                break;

            case ActionType.Rotate:
                transform.localEulerAngles = end;
                break;

            case ActionType.Resize:
                transform.localScale = end;
                break;
        }

        isOpen = playForward;
        isBusy = false;
    }
}
