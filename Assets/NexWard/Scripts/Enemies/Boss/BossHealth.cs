
using UnityEngine;
using System;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("Boss Health")]
    [SerializeField] private float maxHealth = 1000f;
    [SerializeField] private float currentHealth;

    private bool isDead;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public Transform DamageableTransform => transform;
    public bool IsAlive => !isDead && currentHealth > 0f;

    public event Action<BossHealth> OnBossDefeated;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || !IsAlive)
            return;

        currentHealth = Mathf.Max(
            0f,
            currentHealth - amount
        );

        Debug.Log(
            $"SIEGEBREAKER HP: {currentHealth:F0}/{maxHealth}"
        );

        if (currentHealth <= 0f)
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
            $"SIEGEBREAKER SPAWNED WITH {currentHealth:F0} HP."
        );
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("SIEGEBREAKER DEFEATED!");

        // Play boss explosion sound.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossExplosion();
        }

        // Notify the Survival system.
        OnBossDefeated?.Invoke(this);

        Destroy(gameObject);
    }
}
