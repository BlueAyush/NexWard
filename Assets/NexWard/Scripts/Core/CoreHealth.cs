
using UnityEngine;

public class CoreHealth : MonoBehaviour
{
    [Header("Core Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }


    public void TakeDamage(float amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        // Play Core hurt sound.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCoreHurt();
        }

        Debug.Log($"Core Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("CORE DESTROYED! GAME OVER!");
        }
    }


    public void RestoreHealth(float amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    [ContextMenu("TEST: Take 10 Damage")]
    private void TestDamage()
    {
        if (Application.isPlaying)
            TakeDamage(10f);
        else
            Debug.LogWarning("Enter Play mode to test Core damage.");
    }
}
