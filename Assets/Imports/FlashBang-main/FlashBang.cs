using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashBang : MonoBehaviour
{
    public float maxIntensity=20;
    public float fadeSpeed=2;

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
}
