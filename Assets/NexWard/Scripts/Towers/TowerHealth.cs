
using UnityEngine;

public class TowerHealth : MonoBehaviour
{
    [Header("Tower Health")]
    [SerializeField] private float maxHealth = 50f;

    [Header("Construction Cost")]
    [SerializeField] private int buildScrapCost;

    private float currentHealth;

    public float MissingHealth =>
    Mathf.Max(0f, maxHealth - currentHealth);

    public bool IsFullyRepaired =>
        currentHealth >= maxHealth;
    private CoreDome coreDome;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;

        coreDome = FindAnyObjectByType<CoreDome>();
    }

    public void TakeDamage(float baseDamage)
    {
        if (baseDamage <= 0 || currentHealth <= 0)
            return;

        float actualDamage = baseDamage;

        // Apply Core dome protection
        if (coreDome != null)
        {
            actualDamage = coreDome.GetIncomingDamage(
                baseDamage,
                transform.position
            );
        }

        currentHealth = Mathf.Max(0, currentHealth - actualDamage);

        Debug.Log(
            $"{name} Health: {currentHealth:F1}/{maxHealth} " +
            $"(Received {actualDamage:F1} damage)"
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{name} DESTROYED!");

        RemoveTower();
    }

    private void RemoveTower()
    {
        TowerPlacementManager.Instance?.OnTowerDestroyed();

        Destroy(gameObject);
    }

    public void Repair(float amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return;

        float previousHealth = currentHealth;

        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + amount
        );

        float restored = currentHealth - previousHealth;

        Debug.Log(
            $"{name} repaired! Restored {restored:F1} HP. " +
            $"Health: {currentHealth}/{maxHealth}"
        );
    }

    public void SetBuildCost(int cost)
    {
        buildScrapCost = Mathf.Max(0, cost);
    }

    public void Demolish(PlayerResources resources)
    {
        if (resources == null || currentHealth <= 0)
            return;

        TowerAttack attack = GetComponent<TowerAttack>();

        int investedCommandPoints =
            attack != null ? attack.TotalCommandPointsSpent : 0;

        int scrapRefund =
            Mathf.FloorToInt(buildScrapCost * 0.30f);

        float commandPointRefund =
            investedCommandPoints * 0.30f;

        resources.AddScrap(scrapRefund);

        resources.RefundCommandPoints(commandPointRefund);

        Debug.Log(
            $"{name} demolished! Refunded {scrapRefund} Scrap " +
            $"and {commandPointRefund:0.##} Command Points."
        );

        RemoveTower();
    }
}
