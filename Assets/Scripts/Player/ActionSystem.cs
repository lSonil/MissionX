using System.Collections;
using UnityEngine;

public class ActionSystem : MonoBehaviour
{
    private bool isBusy = false;
    private bool isOpen = false;

    public bool IsOpen() => isOpen;
    public void ForceOpen() => isOpen = false;

    public IEnumerator DoAction(ActionType type, Vector3 axis, float openValue, float closedValue, float duration, bool playForward)
    {
        if (isBusy) yield break;
        isBusy = true;

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
            case ActionType.Spawn:
                print("to be implemented");
                break;
            case ActionType.Contain:
                GetComponent<NPCBase>().SetContained();
                break;
            case ActionType.PlaySound:
                GetComponent<AudioSource>().Play();
                break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            switch (type)
            {
                case ActionType.Move: transform.localPosition = Vector3.Lerp(start, end, t); break;
                case ActionType.Rotate: transform.localEulerAngles = Vector3.Lerp(start, end, t); break;
                case ActionType.Resize: transform.localScale = Vector3.Lerp(start, end, t); break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        switch (type)
        {
            case ActionType.Move: transform.localPosition = end; break;
            case ActionType.Rotate: transform.localEulerAngles = end; break;
            case ActionType.Resize: transform.localScale = end; break;
            default: break;
        }

        isOpen = playForward;
        isBusy = false;
    }
}
