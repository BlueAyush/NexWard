
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 30f;

    [Header("Enemy Drops")]
    [SerializeField] private ResourcePickup xpPickupPrefab;
    [SerializeField] private ResourcePickup scrapPickupPrefab;

    [Header("Drop Settings")]
    [SerializeField] private bool dropXPOnDeath = true;
    [SerializeField] private bool dropScrapOnDeath = true;

    public Transform DamageableTransform => transform;

    public bool IsAlive => currentHealth > 0f;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0 || currentHealth <= 0)
            return;

        currentHealth -= damage;

        Debug.Log($"Enemy Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyHealthMultiplier(float multiplier)
    {
        multiplier = Mathf.Max(0.1f, multiplier);

        maxHealth = Mathf.Max(1f, maxHealth * multiplier);
        currentHealth = maxHealth;

        Debug.Log(
            $"{name} spawned with {currentHealth:F0} HP."
        );
    }

    public void SetDropRewards(bool dropXP, bool dropScrap)
    {
        dropXPOnDeath = dropXP;
        dropScrapOnDeath = dropScrap;
    }


    private void Die()
    {
        Debug.Log("Enemy Destroyed!");

        // Play enemy death sound.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDie();
        }

        SurvivalManager.Instance?.RegisterEnemyDefeated();
        WaveManager.Instance?.EnemyDefeated();

        if (dropXPOnDeath && xpPickupPrefab != null)
        {
            Instantiate(
                xpPickupPrefab,
                transform.position + new Vector3(-0.15f, 0f, 0f),
                Quaternion.identity
            );
        }

        if (dropScrapOnDeath && scrapPickupPrefab != null)
        {
            Instantiate(
                scrapPickupPrefab,
                transform.position + new Vector3(0.15f, 0f, 0f),
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }

}
