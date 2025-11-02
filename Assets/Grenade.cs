using UnityEngine;

public class Grenade : Item
{
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private GameObject explosionEffect;

    private bool pinPulled = false;
    private bool hasBeenThrown = false;
    private float pinPullTime;
    private bool countdownStarted = false;

    public override void PrimaryUse()
    {
        hasBeenThrown = true;
        transform.SetParent(null);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.VelocityChange);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        gameObject.SetActive(true);

        if (pinPulled)
        {
            StartCountdown(); // Start countdown only after throw
        }
    }
    public override void SecundaryUse()
    {
        if (pinPulled) return;

        pinPulled = true;
        Debug.Log("Pin pulled!");
    }

    private void Update()
    {
        if (countdownStarted && Time.time >= pinPullTime + fuseTime)
        {
            Explode();
        }
    }

    private void StartCountdown()
    {
        if (!countdownStarted)
        {
            pinPullTime = Time.time;
            countdownStarted = true;
        }
    }

    private void Explode()
    {
        Debug.Log("BOOM!");
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (pinPulled && !hasBeenThrown)
        {
            StartCountdown();
        }
    }
}
