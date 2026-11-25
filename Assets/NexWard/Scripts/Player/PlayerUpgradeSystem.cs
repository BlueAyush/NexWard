
using UnityEngine;

public class PlayerUpgradeSystem : MonoBehaviour
{
    public enum UpgradePath
    {
        Combat,
        Survival,
        Mobility
    }

    [Header("Upgrade Progress")]
    private int combatTier;
    private int survivalTier;
    private int mobilityTier;

    // -1 means no Tier 3 specialization selected.
    private int tier3Path = -1;

    private PlayerWeapon weapon;
    private PlayerHealth health;
    private PlayerMovement movement;

    public PlayerWeapon Weapon => weapon;
    public PlayerHealth Health => health;
    public PlayerMovement Movement => movement;

    private void Awake()
    {
        weapon = GetComponent<PlayerWeapon>();
        health = GetComponent<PlayerHealth>();
        movement = GetComponent<PlayerMovement>();

        if (weapon == null)
            Debug.LogError("PlayerWeapon missing from Player!");

        if (health == null)
            Debug.LogError("PlayerHealth missing from Player!");

        if (movement == null)
            Debug.LogError("PlayerMovement missing from Player!");
    }

    public int GetUpgradeTier(UpgradePath path)
    {
        switch (path)
        {
            case UpgradePath.Combat:
                return combatTier;

            case UpgradePath.Survival:
                return survivalTier;

            case UpgradePath.Mobility:
                return mobilityTier;

            default:
                return 0;
        }
    }

    private int GetSelectedPathCount()
    {
        int count = 0;

        if (combatTier > 0) count++;
        if (survivalTier > 0) count++;
        if (mobilityTier > 0) count++;

        return count;
    }

    public bool IsPathLocked(UpgradePath path)
    {
        // Already selected paths remain available.
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
            case UpgradePath.Combat:

                combatTier = tier;

                if (tier <= 2)
                    weapon.IncreaseDamage(5f);
                else
                    weapon.UnlockPiercing();

                break;

            case UpgradePath.Survival:

                survivalTier = tier;

                if (tier <= 2)
                    health.IncreaseMaxHealth(25f);
                else
                    health.IncreaseDamageReduction(0.20f);

                break;

            case UpgradePath.Mobility:

                mobilityTier = tier;

                if (tier <= 2)
                    movement.IncreaseMoveSpeed(0.5f);
                else
                    movement.UnlockDash();

                break;
        }

        // Only one path can reach Tier 3.
        if (tier == 3)
            tier3Path = (int)path;

        Debug.Log(
            $"Player upgraded: {path} Tier {tier}"
        );

        return true;
    }
}
