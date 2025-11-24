using UnityEngine;

public class EnemyMeleeAI : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform playerTarget;
    [Tooltip("Auto find a player if none assigned (searches by tag 'Player' then AttackScript component).")]
    [SerializeField] private bool autoFindPlayer = true;

    [Tooltip("Distance at which the enemy will attempt a melee attack.")] 
    [SerializeField] private float attackRange = 1.2f;

    [Tooltip("Damage dealt to the player per hit.")] 
    [SerializeField] private float attackDamage = 10f;

    [Tooltip("Seconds between attacks.")] 
    [SerializeField] private float attackCooldown = 1.0f;
    [Tooltip("Optional separate attack point (child). If null, uses this transform's position.")] 
    [SerializeField] private Transform attackPoint;

    [Header("Movement")]
    [Tooltip("How close to stop moving toward the player (slightly larger than attack range).")]
    [SerializeField] private float stopDistanceBuffer = 0.2f;
    [Tooltip("If true, uses Rigidbody2D.MovePosition instead of directly changing transform.")] 
    [SerializeField] private bool useRigidbodyMovement = true;

    private float nextAttackTime;
    private Enemy enemyStats; // to reuse speed value
    private Rigidbody2D rb;
    private float targetSearchInterval = 1.0f; // seconds
    private float lastSearchTime;

    private void Awake()
    {
        enemyStats = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        if (attackPoint == null)
        {
            attackPoint = transform; // fallback
        }

        TryAcquirePlayerTarget();
    }

    private void TryAcquirePlayerTarget()
    {
        if (!autoFindPlayer || playerTarget != null) return;

        // First try tag
        GameObject tagged = GameObject.FindGameObjectWithTag("Player");
        if (tagged != null)
        {
            playerTarget = tagged.transform;
            return;
        }

        // Fallback: find any AttackScript (could be player combat component)
        AttackScript playerCombat = FindObjectOfType<AttackScript>();
        if (playerCombat != null)
        {
            playerTarget = playerCombat.transform;
        }
    }

    private void Update()
    {
        // Periodically re-acquire if lost
        if (autoFindPlayer && playerTarget == null && Time.time - lastSearchTime > targetSearchInterval)
        {
            lastSearchTime = Time.time;
            TryAcquirePlayerTarget();
        }

        if (playerTarget == null) return;

        if (enemyStats != null && enemyStats.speed <= 0f)
        {
            Debug.LogWarning($"{name}: Enemy speed is 0 or negative; cannot move. Set 'speed' on Enemy component.");
        }

        float distance = Vector2.Distance(transform.position, playerTarget.position);

        // Move toward player if outside stop distance
        float desiredStopDistance = attackRange + stopDistanceBuffer;
        if (distance > desiredStopDistance)
        {
            MoveTowardsPlayer();
        }
        else
        {
            // Face player when close
            FacePlayer();
        }

        // Attempt attack if in range
        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (enemyStats == null) return;
        if (enemyStats.speed <= 0f) return;
        Vector3 dir = (playerTarget.position - transform.position).normalized;

        if (useRigidbodyMovement && rb != null)
        {
            rb.MovePosition(rb.position + (Vector2)dir * enemyStats.speed * Time.deltaTime);
        }
        else
        {
            transform.position += dir * enemyStats.speed * Time.deltaTime;
        }
        FaceDirection(dir);
    }

    private void FacePlayer()
    {
        Vector3 dir = (playerTarget.position - transform.position).normalized;
        FaceDirection(dir);
    }

    private void FaceDirection(Vector3 dir)
    {
        if (dir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void PerformAttack()
    {
        Vector2 center = attackPoint.position;
        // Overlap circle to detect player AttackScript or Enemy component
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRange);
        int hitCount = 0;
        foreach (var col in hits)
        {
            if (col.transform == transform) continue; // skip self

            // Prefer player's AttackScript
            if (col.TryGetComponent<AttackScript>(out var playerCombat))
            {
                playerCombat.TakeDamage(attackDamage, gameObject);
                hitCount++;
            }
            else if (col.TryGetComponent<Enemy>(out var otherEnemy) && otherEnemy != enemyStats)
            {
                otherEnemy.TakeDamage(attackDamage, gameObject);
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            Debug.Log($"{gameObject.name} performed melee attack. Hits: {hitCount}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform point = attackPoint != null ? attackPoint : transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(point.position, attackRange);
    }
}
