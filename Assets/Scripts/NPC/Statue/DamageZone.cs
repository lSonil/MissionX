using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageZone : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;
    public bool destroyOnTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        bool collided = false;
        if (other.CompareTag("Player"))
        {
            collided = true;
            HealthSystem health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
                Debug.Log($"Player entered damage zone. Took {damageAmount} damage.");
            }
        }
        if (other.CompareTag("Fragile"))
        {
            collided = true;
            WeakPoint health = other.GetComponent<WeakPoint>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
                Debug.Log($"Entered damage zone. Took {damageAmount} damage.");
            }
        }
        if (other.CompareTag("Structure"))
        {
            collided = true;
        }
        if (collided && destroyOnTrigger ) {Destroy(gameObject);}
    }
}
