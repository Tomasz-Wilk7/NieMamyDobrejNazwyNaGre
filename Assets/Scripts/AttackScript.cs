using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackScript : MonoBehaviour
{

    private float timeBtwAttack;
    public float startTimeBtwAttack;

    private InputAction attackAction;

    public Transform attackPos;
    public LayerMask whatIsEnemies;
    public float attackRange;

    public int damage;

    private void Awake()
    {
        var playerInput = GetComponent<PlayerInput>();
        attackAction = playerInput.actions.FindAction("Attack");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeBtwAttack -= Time.deltaTime;
    }

    private void OnEnable()
    {
        attackAction.performed += OnAttackPerformed;
        attackAction.Enable();
    }

    private void OnDisable()
    {
        attackAction.performed -= OnAttackPerformed;
        attackAction.Disable();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (timeBtwAttack <= 0)
        {
            timeBtwAttack = startTimeBtwAttack;
            Debug.Log("Attack performed");
            Attack();
        }
        else
        {
            Debug.Log("On cd");
        }
    }

    private void Attack()
    {
        Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);
        for (int i = 0; i < enemiesToDamage.Length; i++)
        {
            enemiesToDamage[i].GetComponent<Enemy>().TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }
}
