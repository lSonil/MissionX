using UnityEngine;
using UnityEngine.Events;

public class SanitySystem : MonoBehaviour
{
    [Header("Sanity Settings")]
    [Range(0, 100)] public float maxSanity = 100f;
    public float currentSanity;
    public float regenRate = 2f;
    public float drainCooldown = 0.1f;

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
    private float lastDrainTime;

    private void Start()
    {
        currentSanity = maxSanity;
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    void Update()
    {
        HandleRegeneration();
        CheckSanityLevels();
        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);
    }

    public void DrainSanity(float amount)
    {
        if (Time.time - lastDrainTime < drainCooldown) return;
        Debug.Log("Drain sanity " + amount);

        currentSanity = Mathf.Clamp(currentSanity - amount, 0, maxSanity);
        lastDrainTime = Time.time;

        OnSanityChanged?.Invoke(currentSanity, maxSanity);

        if (currentSanity <= 0)
            TriggerInsanity();
    }

    public void RestoreSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0, maxSanity);
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    public void SetSafeZone(bool value)
    {
        inSafeZone = value;
    }

    private void HandleRegeneration()
    {
        if (inSafeZone && currentSanity < maxSanity)
        {
            currentSanity += regenRate;
            OnSanityChanged?.Invoke(currentSanity, maxSanity);
        }
    }

    private void CheckSanityLevels()
    {
        if (currentSanity <= criticalThreshold)
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
