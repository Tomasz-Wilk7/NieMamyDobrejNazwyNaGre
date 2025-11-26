using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ProjectileConfigSO projectileConfig;
    [SerializeField] private LayerMask hitLayer;

    private float timeElapsed;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnSpawned()
    {
        timeElapsed = 0f;
        rb.linearVelocity = transform.right * projectileConfig.speed;
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= projectileConfig.lifetime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & hitLayer) != 0)
        {
            if (collision.TryGetComponent<AttackScript>(out var targetCombat))
            {
                targetCombat.TakeDamage(projectileConfig.damage);
            }

            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(projectileConfig.poolTag, gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }
}
