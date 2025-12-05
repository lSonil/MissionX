using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private GameObject ragdollPrefab;

    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Heal();
    }

    public int CurrentHealth() => currentHealth;
    public void Heal()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // trigger overlay effect
        UISystem.i.FadeDamageRoutine();
        GetComponent<AudioSystem>()?.PlayDamage();

        // call tilt on damage
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
            GameObject body = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            body.name = "Player";
        }


        if (SceneManager.GetActiveScene().name == "Mission")
        {
            MissionTerminal.i.AbortMission();
        }
        Destroy(gameObject);
    }
}
