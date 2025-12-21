using System.Collections;
using UnityEngine;

public class FlashBang : MonoBehaviour
{
    public float maxIntensity=20;
    public float fadeSpeed=2;
    public LayerMask blockingMask;   // obstacles

    private void Start()
    {
        StartCoroutine(WhiteFade());
    }

    private IEnumerator WhiteFade()
    {
        Light light = GetComponent<Light>();
        light.intensity = 0; 
        yield return new WaitForSeconds(0.05f);
        Destroy(GetComponent<Collider>());

        while (light.intensity < maxIntensity)
        {
            light.intensity += fadeSpeed;
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(0.05f);
        while (light.intensity > 0)
        {
            light.intensity -= fadeSpeed;
            yield return new WaitForSeconds(0.05f);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Transform cam = Camera.main.transform;
        Vector3 playerPosition = cam.position;
        Vector3 targetPosition = transform.position;

        // Check distance

        // Check line of sight
        if (Physics2D.Linecast(playerPosition, targetPosition, blockingMask))
            return;

        // Direction from player to this object
        Vector3 directionToTarget = (targetPosition - playerPosition).normalized;
        Vector3 localDir = cam.InverseTransformDirection(directionToTarget);

        float horizAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        float vertAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

        bool withinHorizontal =
            (horizAngle >= 0 && horizAngle <= 60) ||
            (horizAngle < 0 && Mathf.Abs(horizAngle) <= 60);

        bool withinVertical =
            (vertAngle >= 0 && vertAngle <= 40) ||
            (vertAngle < 0 && Mathf.Abs(vertAngle) <= 40);

        if (withinHorizontal && withinVertical)
        {
            // Player is looking at this object
            other.GetComponent<PlayerCore>().uis.FadeFlashRoutine();
        }
    }
}
