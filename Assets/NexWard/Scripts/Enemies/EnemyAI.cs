
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1f;

    [Header("Tower Targeting")]
    [SerializeField] private float towerAggroRange = 2.5f;

    [Header("Attack")]
    [SerializeField] private float damage = 5f;
    [SerializeField] private float attackInterval = 1f;

    [Header("Player Targeting")]
    [SerializeField] private float playerAggroRange = 3f;

    private PlayerHealth player;

    [Header("Target")]
    [SerializeField] private CoreHealth core;

    private Vector2 knockbackDirection;
    private float knockbackSpeed;
    private float knockbackTimer;

    private Rigidbody2D rb;
    private float nextAttackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        player = FindAnyObjectByType<PlayerHealth>();
    }

    public void SetTarget(CoreHealth target)
    {
        core = target;
    }

    private void FixedUpdate()
    {
        if (core == null || core.CurrentHealth <= 0)
            return;

        if (knockbackTimer > 0f)
        {
            rb.MovePosition(
                rb.position +
                knockbackDirection * knockbackSpeed *
                Time.fixedDeltaTime
            );

            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        // Prioritize the player when they are nearby.
        if (player != null && !player.IsDead)
        {
            float playerDistance = Vector2.Distance(
                rb.position,
                player.transform.position
            );

            if (playerDistance <= playerAggroRange)
            {
                MoveTowardsPlayer();
                return;
            }
        }

        TowerHealth tower = FindNearbyTower();


        if (tower != null)
        {
            MoveTowardsTower(tower);
        }
        else
        {
            MoveTowardsCore();
        }
    }

    private TowerHealth FindNearbyTower()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            rb.position,
            towerAggroRange
        );

        TowerHealth closestTower = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            TowerHealth tower =
                hit.GetComponentInParent<TowerHealth>();

            if (tower == null || tower.CurrentHealth <= 0)
                continue;

            float distance =
                ((Vector2)tower.transform.position - rb.position)
                .sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTower = tower;
            }
        }

        return closestTower;
    }
    private void MoveTowardsPlayer()
    {
        Vector2 direction =
            (Vector2)player.transform.position - rb.position;

        float distance = direction.magnitude;

        // Attack when close enough.
        if (distance <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                player.TakeDamage(damage);

                nextAttackTime = Time.time + attackInterval;
            }

            return;
        }

        // Otherwise chase the player.
        direction.Normalize();

        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.fixedDeltaTime
        );
    }

    private void MoveTowardsTower(TowerHealth tower)
    {
        Vector2 direction =
            (Vector2)tower.transform.position - rb.position;

        float distance = direction.magnitude;

        if (distance <= attackRange)
        {
            AttackTower(tower);
            return;
        }

        direction.Normalize();

        rb.MovePosition(
            rb.position + direction * moveSpeed * Time.fixedDeltaTime
        );
    }

    private void MoveTowardsCore()
    {
        Vector2 direction =
            (Vector2)core.transform.position - rb.position;

        float distance = direction.magnitude;

        if (distance <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                core.TakeDamage(damage);
                nextAttackTime = Time.time + attackInterval;
            }

            return;
        }

        direction.Normalize();

        rb.MovePosition(
            rb.position + direction * moveSpeed * Time.fixedDeltaTime
        );
    }

    private void AttackTower(TowerHealth tower)
    {
        if (Time.time >= nextAttackTime)
        {
            tower.TakeDamage(damage);
            nextAttackTime = Time.time + attackInterval;
        }
    }

    public void ApplyKnockback(
    Vector2 direction,
    float force,
    float duration)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        knockbackDirection = direction.normalized;
        knockbackSpeed = force;
        knockbackTimer = duration;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, towerAggroRange);
    }
}
