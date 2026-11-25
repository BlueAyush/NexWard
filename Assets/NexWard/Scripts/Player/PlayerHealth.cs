
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Damage Resistance")]
    [SerializeField, Range(0f, 0.8f)]
    private float damageReduction = 0f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float DamageReduction => damageReduction;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = maxHealth;
    }


    public void TakeDamage(float amount)
    {
        if (amount <= 0 || IsDead)
            return;

        float actualDamage =
            amount * (1f - damageReduction);

        currentHealth = Mathf.Max(
            0f,
            currentHealth - actualDamage
        );

        // Play player hurt sound.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHurt();
        }

        Debug.Log(
            $"Player Health: {currentHealth}/{maxHealth} " +
            $"(Received {actualDamage:F1} damage)"
        );

        if (IsDead)
            Die();
    }


    public void IncreaseMaxHealth(float amount)
    {
        if (amount <= 0 || IsDead)
            return;

        maxHealth += amount;

        // Also restore health by the same amount.
        currentHealth += amount;

        Debug.Log(
            $"Maximum Player Health increased to {maxHealth}"
        );
    }

    public void IncreaseDamageReduction(float amount)
    {
        if (amount <= 0)
            return;

        damageReduction = Mathf.Clamp(
            damageReduction + amount,
            0f,
            0.8f
        );

        Debug.Log(
            $"Player Damage Resistance: {damageReduction:P0}"
        );
    }

    public void RestoreHealth(float amount)
    {
        if (amount <= 0 || IsDead)
            return;

        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + amount
        );
    }

    private void Die()
    {
        Debug.Log("PLAYER DOWN!");
        DisableControls();
    }

    public void DisableControls()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        PlayerWeapon weapon = GetComponent<PlayerWeapon>();

        PlayerTowerInteraction towerInteraction =
            GetComponent<PlayerTowerInteraction>();

        PlayerCoreInteraction coreInteraction =
            GetComponent<PlayerCoreInteraction>();

        if (movement != null) movement.enabled = false;
        if (weapon != null) weapon.enabled = false;

        if (towerInteraction != null)
            towerInteraction.enabled = false;

        if (coreInteraction != null)
            coreInteraction.enabled = false;
    }
}
