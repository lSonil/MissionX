using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SanitySystem : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    [Range(0, 100)] public float currentSanity;
    public float regenRate = 2f;

    [Header("Thresholds")]
    public float mildThreshold = 80f;
    public float mediumThreshold = 60f;
    public float severeThreshold = 40f;
    public float criticalThreshold = 20f;

    [Header("Events")]
    public UnityEvent<float, float> OnSanityChanged;

    private bool inSafeZone = false;
    private Coroutine regenCoroutine;

    private HealthSystem playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<HealthSystem>();
    }
    private void Start()
    {
        currentSanity = maxSanity;
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    public void DrainSanity(float amount)
    {
        //Debug.Log("Drain sanity " + amount);
        currentSanity = Mathf.Clamp(currentSanity - amount, 0, maxSanity);
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
        CheckSanityLevels();
    }

    public void RestoreSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0, maxSanity);
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    public void OnPlayerEnterSafeZone()
    {
        inSafeZone = true;
        regenCoroutine = StartCoroutine(HandleRegeneration());
    }

    public void OnPlayerLeaveSafeZone()
    {
        inSafeZone = false;
        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);
    }

    private IEnumerator HandleRegeneration()
    {
        if (currentSanity >= maxSanity)
            yield break;

        while (inSafeZone)
        {
            RestoreSanity(regenRate);
            yield return new WaitForSeconds(1f);
        }
    }

    private Coroutine mildRoutine;
    private Coroutine mediumRoutine;
    private Coroutine severeRoutine;
    private Coroutine criticalRoutine;

    private void CheckSanityLevels()
    {
        if (currentSanity <= mildThreshold)
        {
            if (mildRoutine == null)
                mildRoutine = StartCoroutine(TriggerMildDistortion());
        }
        else if (mildRoutine != null)
        {
            StopCoroutine(mildRoutine);
            mildRoutine = null;
        }

        if (currentSanity <= mediumThreshold)
        {
            if (mediumRoutine == null)
                mediumRoutine = StartCoroutine(TriggerMediumDistortion());
        }
        else if (mediumRoutine != null)
        {
            StopCoroutine(mediumRoutine);
            mediumRoutine = null;
        }

        if (currentSanity <= severeThreshold)
        {
            if (severeRoutine == null)
                severeRoutine = StartCoroutine(TriggerSevereDistortion());
        }
        else if (severeRoutine != null)
        {
            StopCoroutine(severeRoutine);
            severeRoutine = null;
        }

        if (currentSanity <= criticalThreshold)
        {
            if (criticalRoutine == null)
                criticalRoutine = StartCoroutine(TriggerCriticalDistortion());
        }
        else if (criticalRoutine != null)
        {
            StopCoroutine(criticalRoutine);
            criticalRoutine = null;
        }

        if (currentSanity <= 0)
            TriggerInsanity();
    }
    private IEnumerator TriggerMildDistortion()
    {
        var filter = Camera.main.gameObject.GetComponent<AudioLowPassFilter>();
        if (filter == null) filter = Camera.main.gameObject.AddComponent<AudioLowPassFilter>();

        var echo = Camera.main.gameObject.GetComponent<AudioEchoFilter>();
        if (echo == null) echo = Camera.main.gameObject.AddComponent<AudioEchoFilter>();

        float targetCutoff = 500f;
        float speed = 100f;

        echo.delay = 200f;
        echo.decayRatio = 0.4f;
        echo.wetMix = 0.5f;
        echo.dryMix = 1f;

        while (currentSanity <= mildThreshold)
        {
            filter.cutoffFrequency = Mathf.MoveTowards(filter.cutoffFrequency, targetCutoff, speed * Time.deltaTime);
            echo.wetMix = Mathf.MoveTowards(echo.wetMix, 0.7f, 0.2f * Time.deltaTime);

            yield return null;
        }

        while (filter.cutoffFrequency < 22000f || echo.wetMix > 0f)
        {
            filter.cutoffFrequency = Mathf.MoveTowards(filter.cutoffFrequency, 22000f, speed * Time.deltaTime);
            echo.wetMix = Mathf.MoveTowards(echo.wetMix, 0f, 0.5f * Time.deltaTime); // fade echo out
            yield return null;
        }

        mildRoutine = null;
    }



    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private UnityEngine.Rendering.Volume postProcessVolume;

    private IEnumerator TriggerMediumDistortion()
    {
        if (!heartbeatSource.isPlaying)
            heartbeatSource.Play();

        heartbeatSource.volume = 0;

        float targetVolume = 0.7f;
        float fadeSpeed = 0.1f;

        while (currentSanity <= mediumThreshold)
        {
            heartbeatSource.volume = Mathf.MoveTowards(
                heartbeatSource.volume,
                targetVolume,
                fadeSpeed * Time.deltaTime
            );

            if (!heartbeatSource.isPlaying)
                heartbeatSource.Play();

            yield return null;
        }

        while (heartbeatSource.volume > 0f)
        {
            heartbeatSource.volume = Mathf.MoveTowards(
                heartbeatSource.volume,
                0f,
                fadeSpeed * Time.deltaTime
            );
            yield return null;
        }

        heartbeatSource.Stop();
        mediumRoutine = null;
    }


    private IEnumerator TriggerSevereDistortion()
    {
        float targetHeartbeat = 1f;
        float targetListenerVol = 0.5f;
        float targetSaturation = -100f;
        float targetVignette = 0.4f;
        float speed = 1f;

        if (postProcessVolume.profile.TryGet(out UnityEngine.Rendering.Universal.ColorAdjustments colorAdjust) &&
            postProcessVolume.profile.TryGet(out UnityEngine.Rendering.Universal.Vignette vignette))
        {
            float hueRange = 90f;
            float hueSpeed = 50f;

            while (currentSanity <= severeThreshold && colorAdjust.saturation.value > targetSaturation + 0.1f)
            {
                heartbeatSource.volume = Mathf.MoveTowards(heartbeatSource.volume, targetHeartbeat, speed * Time.deltaTime);
                AudioListener.volume = Mathf.MoveTowards(AudioListener.volume, targetListenerVol, speed * Time.deltaTime);

                colorAdjust.saturation.value = Mathf.MoveTowards(colorAdjust.saturation.value, targetSaturation, speed * 50f * Time.deltaTime);
                vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, targetVignette, speed * Time.deltaTime);

                yield return null;
            }

            while (currentSanity <= severeThreshold)
            {
                heartbeatSource.volume = Mathf.MoveTowards(heartbeatSource.volume, targetHeartbeat, speed * Time.deltaTime);
                AudioListener.volume = Mathf.MoveTowards(AudioListener.volume, targetListenerVol, speed * Time.deltaTime);

                colorAdjust.hueShift.value = Mathf.PingPong(Time.time * hueSpeed, hueRange * 2f) - hueRange;

                yield return null;
            }

            while (heartbeatSource.volume > 0.5f || AudioListener.volume < 1f ||
                   colorAdjust.saturation.value < 0f || vignette.intensity.value > 0f ||
                   Mathf.Abs(colorAdjust.hueShift.value) > 0.1f)
            {
                heartbeatSource.volume = Mathf.MoveTowards(heartbeatSource.volume, 0.5f, speed * Time.deltaTime);
                AudioListener.volume = Mathf.MoveTowards(AudioListener.volume, 1f, speed * Time.deltaTime);

                colorAdjust.saturation.value = Mathf.MoveTowards(colorAdjust.saturation.value, 0f, speed * 50f * Time.deltaTime);
                vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, 0f, speed * Time.deltaTime);
                colorAdjust.hueShift.value = Mathf.MoveTowards(colorAdjust.hueShift.value, 0f, hueSpeed * Time.deltaTime);

                yield return null;
            }
        }

        severeRoutine = null;
    }


    private IEnumerator TriggerCriticalDistortion()
    {
        yield return null;
    }
    private void TriggerInsanity()
    {
        playerHealth.Die();
    }
}