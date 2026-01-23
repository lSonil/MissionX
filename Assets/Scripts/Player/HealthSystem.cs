using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private GameObject ragdollPrefab;

    private int currentHealth;
    public bool isDead;
    public GameObject error;

    public bool IsDead() => isDead;
    private void Start()
    {
        currentHealth = maxHealth;
    }

    public int CurrentHealth() => currentHealth;
    public void Heal()
    {
        isDead = false;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        GetComponent<PlayerCore>().uis.FadeDamageRoutine();
        GetComponent<AudioSystem>()?.PlayDamage();

        MovementSystem movement = GetComponent<MovementSystem>();
        if (movement != null)
        {
            movement.TiltOnDamage();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (ragdollPrefab != null)
        {
            error.SetActive(true);

            GameObject body = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            isDead = true;
            body.name = "Player";
            GetComponentInParent<PlayerCore>().Stop(body.transform);
        }
    }
}