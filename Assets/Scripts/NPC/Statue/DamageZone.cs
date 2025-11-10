using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageZone : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HealthSystem health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
                Debug.Log($"Player entered damage zone. Took {damageAmount} damage.");
            }
        }
    }
}
