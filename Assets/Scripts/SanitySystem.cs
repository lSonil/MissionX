using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SanitySystem : MonoBehaviour
{
    [Header("Sanity Settings")]
    [Range(0, 100)] public float maxSanity = 100f;
    public float currentSanity;
    public float regenRate = 2f;

    [Header("Thresholds")]
    public float mildThreshold = 80f;
    public float mediumThreshold = 60f;
    public float severeThreshold = 40f;
    public float criticalThreshold = 20f;

    [Header("Events")]
    public UnityEvent OnMildDistortion;
    public UnityEvent OnMediumDistortion;
    public UnityEvent OnSevereDistortion;
    public UnityEvent OnCriticalDistortion;
    public UnityEvent OnInsanity;
    public UnityEvent<float, float> OnSanityChanged;

    private bool inSafeZone = false;
    private Coroutine regenCoroutine;

    private void Start()
    {
        currentSanity = maxSanity;
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    public void DrainSanity(float amount)
    {
        Debug.Log("Drain sanity " + amount);
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

    private void CheckSanityLevels()
    {
        if (currentSanity <= 0)
            TriggerInsanity();
        else if (currentSanity <= criticalThreshold)
            OnCriticalDistortion?.Invoke();
        else if (currentSanity <= severeThreshold)
            OnSevereDistortion?.Invoke();
        else if (currentSanity <= mediumThreshold)
            OnMediumDistortion?.Invoke();
        else if (currentSanity <= mildThreshold)
            OnMildDistortion?.Invoke();
    }

    private void TriggerInsanity()
    {
        OnInsanity?.Invoke();
        Debug.Log("Player lost all sanity!");
    }
}
