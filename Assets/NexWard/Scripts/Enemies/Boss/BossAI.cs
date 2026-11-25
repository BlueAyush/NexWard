
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.7f;

    [Header("Core Attack")]
    [SerializeField] private float attackRange = 1.3f;
    [SerializeField] private float damage = 12f;
    [SerializeField] private float attackInterval = 2.5f;

    [Header("Target")]
    [SerializeField] private CoreHealth core;

    [Header("Minion Summoning")]
    [SerializeField] private EnemyAI minionPrefab;
    [SerializeField] private int minionsPerSummon = 2;
    [SerializeField] private int maxMinionsAlive = 5;
    [SerializeField] private int maxTotalMinions = 12;
    [SerializeField] private float minionSpawnRadius = 2f;

    [Header("Minion Health")]
    [SerializeField] private float minionHealthMultiplier = 1f;

    [Header("Summon Intervals")]
    [SerializeField] private float highHealthInterval = 14f;
    [SerializeField] private float mediumHealthInterval = 9f;
    [SerializeField] private float lowHealthInterval = 5f;

    [Range(0f, 1f)]
    [SerializeField] private float mediumHealthThreshold = 0.66f;

    [Range(0f, 1f)]
    [SerializeField] private float lowHealthThreshold = 0.33f;

    private Rigidbody2D rb;
    private BossHealth bossHealth;

    private float nextAttackTime;
    private float nextSummonTime;

    private int totalMinionsSummoned;

    private readonly List<EnemyHealth> activeMinions =
        new List<EnemyHealth>();

    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bossHealth = GetComponent<BossHealth>();
    }

    private void Start()
    {
        nextSummonTime = Time.time + GetSummonInterval();
    }

    public void SetTarget(CoreHealth target)
    {
        core = target;
    }

    private void FixedUpdate()
    {
        if (core == null || core.CurrentHealth <= 0)
            return;

        Vector2 direction =
            (Vector2)core.transform.position - rb.position;

        float distance = direction.magnitude;

        if (distance <= attackRange)
        {
            AttackCore();
            return;
        }

        direction.Normalize();

        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.fixedDeltaTime
        );
    }

    private void Update()
    {
        if (bossHealth == null || !bossHealth.IsAlive)
            return;

        CleanupMinions();

        if (Time.time < nextSummonTime)
            return;

        if (minionPrefab == null ||
            totalMinionsSummoned >= maxTotalMinions ||
            activeMinions.Count >= maxMinionsAlive)
        {
            // Recheck shortly if the alive limit is full.
            nextSummonTime = Time.time + 1f;
            return;
        }

        SummonMinions();

        nextSummonTime = Time.time + GetSummonInterval();
    }

    // ---------------- CORE ATTACK ----------------

    private void AttackCore()
    {
        if (Time.time < nextAttackTime)
            return;

        core.TakeDamage(damage);

        nextAttackTime = Time.time + attackInterval;

        Debug.Log(
            $"SIEGEBREAKER ATTACKED CORE! Damage: {damage}"
        );
    }

    // ---------------- MINION SUMMONING ----------------

    private void SummonMinions()
    {
        CleanupMinions();

        int availableSlots =
            maxMinionsAlive - activeMinions.Count;

        int remainingTotal =
            maxTotalMinions - totalMinionsSummoned;

        int summonCount = Mathf.Min(
            minionsPerSummon,
            availableSlots,
            remainingTotal
        );

        for (int i = 0; i < summonCount; i++)
        {
            Vector2 direction =
                Random.insideUnitCircle.normalized;

            if (direction.sqrMagnitude < 0.001f)
                direction = Vector2.right;

            Vector2 spawnPosition =
                (Vector2)transform.position +
                direction * minionSpawnRadius;

            EnemyAI minion = Instantiate(
                minionPrefab,
                spawnPosition,
                minionPrefab.transform.rotation
            );

            minion.SetTarget(core);

            EnemyHealth minionHealth =
                minion.GetComponent<EnemyHealth>();

            if (minionHealth == null)
            {
                Debug.LogError(
                    "Boss minion prefab needs EnemyHealth!"
                );

                Destroy(minion.gameObject);
                continue;
            }

            // Boss minions give Scrap but never XP.
            minionHealth.SetDropRewards(
                false,
                true
            );

            minionHealth.ApplyHealthMultiplier(
                minionHealthMultiplier
            );

            activeMinions.Add(minionHealth);

            totalMinionsSummoned++;
        }

        if (summonCount > 0)
        {
            Debug.Log(
                $"SIEGEBREAKER SUMMONED {summonCount} MINIONS! " +
                $"Total summoned: {totalMinionsSummoned}/{maxTotalMinions}"
            );
        }
    }

    private float GetSummonInterval()
    {
        if (bossHealth == null || bossHealth.MaxHealth <= 0)
            return highHealthInterval;

        float healthPercentage =
            bossHealth.CurrentHealth / bossHealth.MaxHealth;

        if (healthPercentage <= lowHealthThreshold)
            return lowHealthInterval;

        if (healthPercentage <= mediumHealthThreshold)
            return mediumHealthInterval;

        return highHealthInterval;
    }

    private void CleanupMinions()
    {
        activeMinions.RemoveAll(minion => minion == null);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minionSpawnRadius);
    }
}
