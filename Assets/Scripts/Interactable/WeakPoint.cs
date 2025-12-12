using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;

    [SerializeField] private int currentHealth=10;

    [SerializeField] private NPCBase body;

    private void Start()
    {
        body=GetComponentInParent<NPCBase>();
        currentHealth = maxHealth;
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            body.Die();
    }
}
