
using UnityEngine;
using System.Collections.Generic;

public class CoreUpgradeSystem : MonoBehaviour
{
    public enum UpgradePath
    {
        DomeDefense,
        Expansion,
        Regeneration,
        Pulse
    }

    [Header("References")]
    [SerializeField] private CoreHealth core;
    [SerializeField] private CoreDome dome;

    [Header("Upgrade Progress")]
    [SerializeField] private int defenseTier;
    [SerializeField] private int expansionTier;
    [SerializeField] private int regenerationTier;
    [SerializeField] private int pulseTier;

    // -1 means no Tier 3 specialization selected.
    private int tier3Path = -1;

    [Header("Regeneration")]
    [SerializeField] private float regenerationPerSecond;

    [Header("Dome Pulse")]
    [SerializeField] private float pulseRadius = 4f;
    [SerializeField] private float pulseDamage = 10f;
    [SerializeField] private float pulseCooldown = 8f;

    private bool pulseUnlocked;
    private bool pulseKnockbackUnlocked;

    private float nextPulseTime;

    public CoreHealth Core => core;
    public CoreDome Dome => dome;

    public float RegenerationPerSecond => regenerationPerSecond;

    public bool PulseUnlocked => pulseUnlocked;
    public float PulseDamage => pulseDamage;
    public float PulseCooldown => pulseCooldown;

    public int GetUpgradeTier(UpgradePath path)
    {
        switch (path)
        {
            case UpgradePath.DomeDefense:
                return defenseTier;

            case UpgradePath.Expansion:
                return expansionTier;

            case UpgradePath.Regeneration:
                return regenerationTier;

            case UpgradePath.Pulse:
                return pulseTier;

            default:
                return 0;
        }
    }

    private int GetSelectedPathCount()
    {
        int count = 0;

        if (defenseTier > 0) count++;
        if (expansionTier > 0) count++;
        if (regenerationTier > 0) count++;
        if (pulseTier > 0) count++;

        return count;
    }

    public bool IsPathLocked(UpgradePath path)
    {
        // An already selected path stays available.
        if (GetUpgradeTier(path) > 0)
            return false;

        // Only two paths can be selected.
        return GetSelectedPathCount() >= 2;
    }

    public bool IsTier3LockedForPath(UpgradePath path)
    {
        return tier3Path != -1 &&
               tier3Path != (int)path;
    }

    public bool CanPurchaseUpgrade(UpgradePath path, int tier)
    {
        if (tier < 1 || tier > 3)
            return false;

        if (IsPathLocked(path))
            return false;

        if (tier != GetUpgradeTier(path) + 1)
            return false;

        if (tier == 3 && IsTier3LockedForPath(path))
            return false;

        return true;
    }

    public bool TryPurchaseUpgrade(UpgradePath path, int tier)
    {
        if (!CanPurchaseUpgrade(path, tier))
            return false;

        switch (path)
        {
            case UpgradePath.DomeDefense:

                defenseTier = tier;

                if (tier == 1)
                    dome.IncreaseDamageReduction(0.10f);
                else if (tier == 2)
                    dome.IncreaseDamageReduction(0.10f);
                else if (tier == 3)
                    dome.IncreaseDamageReduction(0.15f);

                break;

            case UpgradePath.Expansion:

                expansionTier = tier;

                if (tier == 1)
                    dome.IncreaseRadius(0.5f);
                else if (tier == 2)
                    dome.IncreaseRadius(0.5f);
                else if (tier == 3)
                    dome.IncreaseRadius(1f);

                break;

            case UpgradePath.Regeneration:

                regenerationTier = tier;

                if (tier == 1)
                    regenerationPerSecond += 0.5f;
                else if (tier == 2)
                    regenerationPerSecond += 0.75f;
                else if (tier == 3)
                    regenerationPerSecond += 1.25f;

                break;

            case UpgradePath.Pulse:

                pulseTier = tier;

                if (tier == 1)
                {
                    pulseUnlocked = true;
                }
                else if (tier == 2)
                {
                    pulseDamage += 10f;
                    pulseCooldown = 7f;
                }
                else if (tier == 3)
                {
                    pulseKnockbackUnlocked = true;
                }

                break;
        }

        if (tier == 3)
        {
            tier3Path = (int)path;
        }

        Debug.Log(
            $"Core upgraded: {path} Tier {tier}"
        );

        return true;
    }

    private void Update()
    {
        // Passive Core regeneration.
        if (core != null && regenerationPerSecond > 0f)
        {
            core.RestoreHealth(
                regenerationPerSecond * Time.deltaTime
            );
        }

        // Automatic Dome Pulse.
        if (pulseUnlocked && Time.time >= nextPulseTime)
        {
            PerformPulse();
            nextPulseTime = Time.time + pulseCooldown;
        }
    }

    private void PerformPulse()
    {
        Debug.Log("CORE PULSE ACTIVATED!");

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            pulseRadius
        );

        HashSet<EnemyHealth> damagedEnemies =
            new HashSet<EnemyHealth>();

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy =
                hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null || !damagedEnemies.Add(enemy))
                continue;

            enemy.TakeDamage(pulseDamage);

            if (pulseKnockbackUnlocked)
            {
                EnemyAI enemyAI =
                    enemy.GetComponentInParent<EnemyAI>();

                if (enemyAI != null)
                {
                    Vector2 direction =
                        (Vector2)enemy.transform.position -
                        (Vector2)transform.position;

                    enemyAI.ApplyKnockback(direction, 4f, 0.3f);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pulseUnlocked)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, pulseRadius);
        }
    }
}
