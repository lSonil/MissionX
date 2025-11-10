using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private GameObject ragdollPrefab;

    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
       //if (currentHealth > 0)
       //{
       //    TakeDamage(1);
       //}
       //else
       //{
       //    Die();
       //}
    }

    public int CurrentHealth() => currentHealth;
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {

        if (ragdollPrefab != null)
        {
            Instantiate(ragdollPrefab, transform.position, transform.rotation);
        }

        gameObject.SetActive(false); // Disable player
    }

}
