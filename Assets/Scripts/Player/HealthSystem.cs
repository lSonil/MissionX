using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private GameObject ragdollPrefab;

    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public int CurrentHealth() => currentHealth;

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

        gameObject.SetActive(false);
        if (SceneManager.GetActiveScene().name == "Mission")
        {
            MissionTerminal.i.AbortMission();
        }
    }
}
