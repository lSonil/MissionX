using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteraction
{
    public string interactionPromptText;

    private bool isOpen = false;
    private bool started = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.rotation;
    }
    public string GetTextUse() => interactionPromptText;
    public string GetTextPrimary() => "";
    public string GetTextSecundary() => "";

    public void Action(int i)
    {
        if (started) return;

        if (!isOpen)
        {
            // Determine open direction based on player position
            Vector3 toPlayer = GameObject.FindWithTag("Player").transform.position - transform.position;
            float dot = Vector3.Dot(transform.right, toPlayer); // right = hinge axis

            float deltaY = dot > 0 ? 90f : -90f;
            Vector3 openEuler = transform.eulerAngles;
            openEuler.y += deltaY;
            openRotation = Quaternion.Euler(openEuler);

            StartCoroutine(RotateTo(openRotation, 0.2f));
        }
        else
        {
            StartCoroutine(RotateTo(closedRotation, 0.2f));
        }

        isOpen = !isOpen;
    }

    IEnumerator RotateTo(Quaternion targetRotation, float time)
    {
        started = true;
        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < time)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        started = false;
    }
}