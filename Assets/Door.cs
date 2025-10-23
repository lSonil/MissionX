using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Door : MonoBehaviour, IInteraction
{
    public string interactionPromptText;
    bool rotated = false;
    bool started = false;
    public void Action()
    {
        if(started) return;
        float rotationAmount = rotated ? 90f : -90f;
        StartCoroutine(RotateXByAmount(rotationAmount, 0.2f));
        rotated = !rotated;
    }

    IEnumerator RotateXByAmount(float deltaX, float time)
    {
        started = true;
        Quaternion startRotation = transform.rotation;
        Vector3 startEuler = startRotation.eulerAngles;

        // Calculate new X rotation by adding delta
        float targetX = startEuler.z + deltaX;
        Vector3 endEuler = new Vector3(startEuler.x, startEuler.y, targetX);
        Quaternion endRotation = Quaternion.Euler(endEuler);

        float elapsed = 0f;
        while (elapsed < time)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }
        started = false;
        transform.rotation = endRotation;
    }


    public string GetText()
    {
        return interactionPromptText;
    }
}
