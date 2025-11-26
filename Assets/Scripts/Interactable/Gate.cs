using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Gate : MonoBehaviour, IInteraction
{
    public string interactionPromptText;
    public Gate connectedTo;
    public Transform target;
    public bool toggleResize = false;
    public float duration = 1.5f;

    private bool isBusy = false;
    public bool open = false;
    public UnityEvent onActionEvent;

    public string GetTextUse() => interactionPromptText;
    public string GetTextPrimary() => "";
    public string GetTextSecundary() => "";

    public void Action()
    {
        if (isBusy || target == null) return;

        onActionEvent?.Invoke();

        if (toggleResize)
            StartCoroutine(ResizeZCoroutine());
        else
            StartCoroutine(RotateXCoroutine());
    }

    private IEnumerator RotateXCoroutine()
    {
        isBusy = true;

        float startAngle = open ? -90f : 0;
        float endAngle = open ? 0 : -90f;
        open = !open;

        float elapsed = 0f;
        Vector3 startEuler = target.localEulerAngles;
        startEuler.z = startAngle;
        Vector3 endEuler = target.localEulerAngles;
        endEuler.z = endAngle;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            target.localEulerAngles = Vector3.Lerp(startEuler, endEuler, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localEulerAngles = endEuler;
        isBusy = false;
        if (connectedTo != null) connectedTo.open = open;
    }

    private IEnumerator ResizeZCoroutine()
    {
        isBusy = true;

        float startScale = open ? 0f : 1;
        float endScale = open ? 1 : 0f;
        open = !open;

        float elapsed = 0f;
        Vector3 startScaleVec = target.localScale;
        startScaleVec.y = startScale;
        Vector3 endScaleVec = target.localScale;
        endScaleVec.y = endScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            target.localScale = Vector3.Lerp(startScaleVec, endScaleVec, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localScale = endScaleVec;
        isBusy = false;
        if (connectedTo != null) connectedTo.open = open;
    }
}
