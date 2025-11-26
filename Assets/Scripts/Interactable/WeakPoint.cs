using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;

    [SerializeField] private int currentHealth=10;

    private void Start()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
    }
    public bool HasHP()
    {
        return currentHealth > 0;
    }
    public void IsAlive(GameObject destrory)
    {
        if (currentHealth <= 0)
            Destroy(destrory);
    }
}
