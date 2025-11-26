using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackScript : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private CombatConfigSO combatConfig;
    [SerializeField] private CharacterStatsSO characterStats;
    [SerializeField] private Transform attackPoint;

    private float currentHealth;
    


    private bool canAttack = true;
    private int currentComboIndex = 0;
    private float lastAttackTime = 0.0f;
    private float lastComboTime = 0.0f;

    private bool canParry = true;
    private bool isParrying = false;
    private float parryStartTime;


    private PlayerInput playerInput;
    private InputAction attackAction;
    private InputAction parryAction;

    public System.Action<int> OnAttackPerformed;
    public System.Action<float> OnHealthChanged;
    public System.Action OnParryPerformed;
    public System.Action OnDeath;
    public System.Action OnParrySuccess;
    public System.Action OnParryFail;
    public System.Action<bool> OnParryStateChanged;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput != null)
        {
            attackAction = playerInput.actions["Attack"];
            parryAction = playerInput.actions["Parry"];
        }

        currentHealth = characterStats.MaxHealth;
    }

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.performed += OnAttack;
        }
        if (parryAction != null)
        {
            parryAction.performed += OnParryPressed;
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.performed -= OnAttack;
        }
        if (parryAction != null)
        {
            parryAction.performed -= OnParryPressed;
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!canAttack || isParrying) return;

        Debug.Log($"Attack Triggered");
        PerformAttack();
    }

    // Public method for Unity Events/Animation Events
    public void OnAttack()
    {
        if (!canAttack || isParrying) return;

        Debug.Log($"Attack Triggered (Public)");
        PerformAttack();
    }

    private void PerformAttack()
    {
        canAttack = false;
        lastAttackTime = Time.time;
        lastComboTime = Time.time;

        OnAttackPerformed?.Invoke(currentComboIndex);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            combatConfig.AttackRange,
            combatConfig.EnemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            // Try to damage entities with AttackScript (players or advanced enemies)
            if (enemy.TryGetComponent<AttackScript>(out var enemyCombat))
            {
                enemyCombat.TakeDamage(combatConfig.AttackDamage, gameObject);
            }
            // Try to damage basic enemies with Enemy component
            else if (enemy.TryGetComponent<Enemy>(out var basicEnemy))
            {
                basicEnemy.TakeDamage(combatConfig.AttackDamage, gameObject);
            }
        }

        currentComboIndex = (currentComboIndex + 1) % combatConfig.MaxComboCount;

        Invoke(nameof(ResetAttack), combatConfig.AttackCooldown);

        Debug.Log($"Attack: {currentComboIndex}! Hit: {hitEnemies.Length} enemies");
    }

    private void ResetAttack()
    {
        canAttack = true;
    }

    private void Update()
    {
        // Check parry window timeout
        if (isParrying && Time.time - parryStartTime > combatConfig.ParryWindow)
        {
            EndParry(false);
        }

        // Check combo reset timeout
        if (currentComboIndex > 0 && Time.time - lastComboTime > combatConfig.ComboResetTime)
        {
            currentComboIndex = 0;
            Debug.Log("Combo reset!");
        }
    }

    private void OnParryPressed(InputAction.CallbackContext context)
    {
        if (!canParry) return;
        StartParry();
    }

    // Public method for Unity Events/Animation Events
    public void OnParry()
    {
        if (!canParry) return;
        StartParry();
    }

    private void StartParry()
    {
        isParrying = true;
        canParry = false;
        parryStartTime = Time.time;

        OnParryStateChanged?.Invoke(true);
        Debug.Log("Parry active!");
    }

    private void EndParry(bool wasSuccessful)
    {
        isParrying = false;
        OnParryStateChanged?.Invoke(false);

        if (wasSuccessful)
        {
            OnParrySuccess?.Invoke();
            Debug.Log("Parry successful!");
            Invoke(nameof(ResetParry), combatConfig.ParryCooldown * 0.5f);
        }
        else
        {
            OnParryFail?.Invoke();
            Debug.Log("Parry failed!");
            Invoke(nameof(ResetParry), combatConfig.ParryCooldown);
        }
    }

    private void ResetParry()
    {
        canParry = true;
    }

    public void TakeDamage(float damage, GameObject attacker = null)
    {
        if (currentHealth <= 0) return;

        if (isParrying){

            EndParry(true);

            if (attacker != null && attacker.TryGetComponent<AttackScript>(out var attackerCombat))
            {
                attackerCombat.GetStunned(combatConfig.ParryStunDuration);
            }
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth);

        Debug.Log($"{gameObject.name} took {damage}. Health: {currentHealth}/{characterStats.MaxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void GetStunned(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        canAttack = false;
        canParry = false;

        Debug.Log($"{gameObject.name} is stunned!");

        yield return new WaitForSeconds(duration);

        canAttack = true;
        canParry = true;

        Debug.Log($"{gameObject.name} recovered!");
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    public float CurrentHealth => currentHealth;
    public float MaxHealth => characterStats.MaxHealth;
    public int ComboIndex => currentComboIndex;

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, combatConfig.AttackRange);
    }
}
