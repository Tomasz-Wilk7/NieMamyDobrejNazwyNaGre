using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;
    
    public float speed;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
 
    }

    public void TakeDamage(float damage, GameObject attacker = null)
    {
        currentHealth -= damage;
        Debug.Log($"Damage taken by {gameObject.name}: {damage}. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
}
