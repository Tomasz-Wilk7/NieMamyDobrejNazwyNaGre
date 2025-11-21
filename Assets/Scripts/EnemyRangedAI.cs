using UnityEngine;

public class EnemyRangedAI : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Assign the Player's transform here.")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 2f;
    private float nextAttackTime;

    [Header("Projectile Setup")]
    [Tooltip("Tag for ObjectPooler")]
    [SerializeField] private string projectileTag = "EnemyBullet";
    [Tooltip("Point from which projectiles are fired.")]
    [SerializeField] private Transform muzzlePoint;

    void Update()
    {
        if (playerTarget == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            AimAndFire();
            nextAttackTime = Time.time + attackCooldown;
        }

        if (distanceToPlayer <= attackRange)
        {
            LookAtTarget();
        }
    }

    private void LookAtTarget()
    {
        Vector2 direction = (playerTarget.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void AimAndFire()
    {
        if (ObjectPooler.Instance != null)
        {
            GameObject projectile = ObjectPooler.Instance.SpawnFromPool(
                projectileTag,
                muzzlePoint.position,
                muzzlePoint.rotation
            );

            if (projectile != null)
            {
                Projectile projComponent = projectile.GetComponent<Projectile>();
                if (projComponent != null)
                {
                    projComponent.OnSpawned();
                }
            }
        }
        else
        {
            Debug.LogWarning("ObjectPooler instance not found. Cannot fire projectile.");
        }
    }
}
