
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerAttack : MonoBehaviour
{
    public enum TowerType
    {
        Laser,
        Tesla
    }

    public enum UpgradePath
    {
        Power,
        Speed,
        Range
    }

    [Header("Tower Type")]
    [SerializeField] private TowerType towerType = TowerType.Laser;

    [Header("Targeting")]
    [SerializeField] private float attackRange = 5f;

    [Header("Attack")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float fireInterval = 1f;
    [SerializeField] private float projectileSpawnDistance = 0.4f;

    [Header("Tesla Settings")]
    [SerializeField] private float chainRadius = 2f;
    [SerializeField] private int maxChainTargets = 3;

    [Range(0f, 1f)]
    [SerializeField] private float chainDamageMultiplier = 0.7f;

    [SerializeField] private LineRenderer teslaArcRenderer;

    [Header("Upgrade Progress")]
    private int powerTier;
    private int speedTier;
    private int rangeTier;

    private int tier3Path = -1;

    private bool chargedShotUnlocked;
    private bool doubleShotUnlocked;
    private bool piercingUnlocked;

    private int shotsFired;
    private float nextFireTime;

    private Coroutine arcCoroutine;

    private static Material sharedArcMaterial;

    public float Damage => damage;
    public float FireInterval => fireInterval;
    public float AttackRange => attackRange;

    public int TotalCommandPointsSpent =>
        powerTier + speedTier + rangeTier;

    private void Awake()
    {
        if (teslaArcRenderer != null)
        {
            teslaArcRenderer.useWorldSpace = true;
            teslaArcRenderer.loop = false;
            teslaArcRenderer.enabled = false;

            teslaArcRenderer.startWidth = 0.07f;
            teslaArcRenderer.endWidth = 0.025f;

            teslaArcRenderer.sortingOrder = 10;

            if (sharedArcMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");

                if (shader != null)
                    sharedArcMaterial = new Material(shader);
            }

            if (teslaArcRenderer.sharedMaterial == null &&
                sharedArcMaterial != null)
            {
                teslaArcRenderer.sharedMaterial = sharedArcMaterial;
            }

            teslaArcRenderer.startColor =
                new Color(0.2f, 0.9f, 1f, 1f);

            teslaArcRenderer.endColor =
                new Color(0.5f, 0.7f, 1f, 0.8f);
        }
    }

    // ---------------- UPGRADE SYSTEM ----------------

    public int GetUpgradeTier(UpgradePath path)
    {
        switch (path)
        {
            case UpgradePath.Power:
                return powerTier;

            case UpgradePath.Speed:
                return speedTier;

            case UpgradePath.Range:
                return rangeTier;

            default:
                return 0;
        }
    }

    private int GetSelectedPathCount()
    {
        int count = 0;

        if (powerTier > 0) count++;
        if (speedTier > 0) count++;
        if (rangeTier > 0) count++;

        return count;
    }

    public bool IsPathLocked(UpgradePath path)
    {
        if (GetUpgradeTier(path) > 0)
            return false;

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
            case UpgradePath.Power:

                powerTier = tier;

                if (towerType == TowerType.Laser)
                {
                    if (tier <= 2)
                        damage += 5f;
                    else
                        chargedShotUnlocked = true;
                }
                else
                {
                    if (tier == 1)
                        damage += 5f;
                    else if (tier == 2)
                        chainDamageMultiplier =
                            Mathf.Min(1f, chainDamageMultiplier + 0.15f);
                    else
                        chargedShotUnlocked = true;
                }

                break;

            case UpgradePath.Speed:

                speedTier = tier;

                if (tier <= 2)
                {
                    fireInterval = Mathf.Max(
                        0.25f,
                        fireInterval - 0.15f
                    );
                }
                else
                {
                    doubleShotUnlocked = true;
                }

                break;

            case UpgradePath.Range:

                rangeTier = tier;

                if (towerType == TowerType.Laser)
                {
                    if (tier <= 2)
                        attackRange += 1f;
                    else
                        piercingUnlocked = true;
                }
                else
                {
                    if (tier == 1)
                    {
                        attackRange += 0.75f;
                        chainRadius += 0.25f;
                    }
                    else if (tier == 2)
                    {
                        attackRange += 0.75f;
                        chainRadius += 0.5f;
                    }
                    else
                    {
                        piercingUnlocked = true;
                    }
                }

                break;
        }

        if (tier == 3)
            tier3Path = (int)path;

        Debug.Log(
            $"{name}: {path} upgraded to Tier {tier}"
        );

        return true;
    }

    // Compatibility with the earlier upgrade panel.

    public int DamageUpgradeLevel => powerTier;
    public int FireRateUpgradeLevel => speedTier;

    public bool CanUpgradeFireRate =>
        speedTier < 3 &&
        !IsPathLocked(UpgradePath.Speed);

    public void UpgradeDamage()
    {
        TryPurchaseUpgrade(UpgradePath.Power, powerTier + 1);
    }

    public void UpgradeFireRate()
    {
        TryPurchaseUpgrade(UpgradePath.Speed, speedTier + 1);
    }

    // ---------------- TOWER ATTACK ----------------

    private void Update()
    {
        IDamageable target = FindTarget();

        if (target == null)
            return;

        if (Time.time < nextFireTime)
            return;

        if (towerType == TowerType.Laser)
        {
            if (projectilePrefab == null)
                return;

            Vector2 direction =
                ((Vector2)target.DamageableTransform.position -
                 (Vector2)transform.position).normalized;

            Shoot(direction);

            // Laser shooting sound.
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLaserShoot();
            }
        }
        else if (towerType == TowerType.Tesla)
        {
            FireTesla(target);

            // Tesla electric discharge sound.
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTeslaShoot();
            }
        }

        nextFireTime = Time.time + fireInterval;
    }

    private IDamageable FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange
        );

        IDamageable closestTarget = null;
        float closestDistance = float.MaxValue;

        HashSet<IDamageable> checkedTargets =
            new HashSet<IDamageable>();

        foreach (Collider2D hit in hits)
        {
            IDamageable target =
                hit.GetComponentInParent<IDamageable>();

            if (target == null ||
                !target.IsAlive ||
                target.DamageableTransform == null ||
                !checkedTargets.Add(target))
            {
                continue;
            }

            float distance =
                ((Vector2)target.DamageableTransform.position -
                 (Vector2)transform.position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }

    // ---------------- LASER ATTACK ----------------

    private void Shoot(Vector2 direction)
    {
        if (doubleShotUnlocked)
        {
            FireProjectile(
                RotateDirection(direction, -6f),
                damage
            );

            FireProjectile(
                RotateDirection(direction, 6f),
                damage
            );

            return;
        }

        shotsFired++;

        float shotDamage = damage;

        if (chargedShotUnlocked && shotsFired % 3 == 0)
        {
            shotDamage *= 2f;
        }

        FireProjectile(direction, shotDamage);
    }

    private void FireProjectile(
        Vector2 direction,
        float shotDamage)
    {
        Vector2 spawnPosition =
            (Vector2)transform.position +
            direction * projectileSpawnDistance;

        Projectile projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        projectile.Launch(
            direction,
            shotDamage,
            piercingUnlocked
        );
    }

    private Vector2 RotateDirection(
        Vector2 direction,
        float angle)
    {
        return Quaternion.Euler(0f, 0f, angle) * direction;
    }

    // ---------------- TESLA ATTACK ----------------

    private void FireTesla(IDamageable firstTarget)
    {
        if (firstTarget == null || !firstTarget.IsAlive)
            return;

        HashSet<IDamageable> hitEnemies =
            new HashSet<IDamageable>();

        List<Vector3> arcPoints = new List<Vector3>();

        arcPoints.Add(transform.position);

        shotsFired++;

        float attackMultiplier = 1f;

        if (chargedShotUnlocked && shotsFired % 3 == 0)
        {
            attackMultiplier = 2f;
        }

        int targetLimit =
            maxChainTargets + (doubleShotUnlocked ? 2 : 0);

        float currentChainRadius =
            chainRadius + (piercingUnlocked ? 1f : 0f);

        IDamageable currentTarget = firstTarget;

        for (int i = 0;
             i < targetLimit && currentTarget != null;
             i++)
        {
            if (!currentTarget.IsAlive ||
                !hitEnemies.Add(currentTarget))
                break;

            Vector3 targetPosition =
                currentTarget.DamageableTransform.position;

            float chainDamage =
                damage *
                Mathf.Pow(chainDamageMultiplier, i) *
                attackMultiplier;

            currentTarget.TakeDamage(chainDamage);

            arcPoints.Add(targetPosition);

            currentTarget = FindNextChainTarget(
                targetPosition,
                hitEnemies,
                currentChainRadius
            );
        }

        ShowTeslaArc(arcPoints);
    }

    private IDamageable FindNextChainTarget(
        Vector2 position,
        HashSet<IDamageable> alreadyHit,
        float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            position,
            radius
        );

        IDamageable closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IDamageable target =
                hit.GetComponentInParent<IDamageable>();

            if (target == null ||
                !target.IsAlive ||
                target.DamageableTransform == null ||
                alreadyHit.Contains(target))
            {
                continue;
            }

            float distance =
                ((Vector2)target.DamageableTransform.position -
                 position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }

    private void ShowTeslaArc(List<Vector3> points)
    {
        if (teslaArcRenderer == null || points.Count < 2)
            return;

        teslaArcRenderer.positionCount = points.Count;
        teslaArcRenderer.SetPositions(points.ToArray());

        teslaArcRenderer.enabled = true;

        if (arcCoroutine != null)
            StopCoroutine(arcCoroutine);

        arcCoroutine = StartCoroutine(HideTeslaArc());
    }

    public TowerType Type => towerType;

    public string GetTowerDisplayName()
    {
        return towerType == TowerType.Tesla
            ? "TESLA TOWER"
            : "LASER TOWER";
    }

    public string GetPathDisplayName(UpgradePath path)
    {
        if (towerType == TowerType.Laser)
        {
            switch (path)
            {
                case UpgradePath.Power: return "POWER";
                case UpgradePath.Speed: return "SPEED";
                case UpgradePath.Range: return "RANGE";
            }
        }
        else
        {
            switch (path)
            {
                case UpgradePath.Power: return "VOLTAGE";
                case UpgradePath.Speed: return "DISCHARGE";
                case UpgradePath.Range: return "CONDUCTION";
            }
        }

        return "";
    }

    public string GetUpgradeDisplayName(
        UpgradePath path,
        int tier)
    {
        if (towerType == TowerType.Laser)
        {
            switch (path)
            {
                case UpgradePath.Power:
                    return tier == 3 ? "Charged Shot" : "+5 Damage";

                case UpgradePath.Speed:
                    return tier == 3 ? "Double Shot" : "Faster Fire";

                case UpgradePath.Range:
                    return tier == 3 ? "Piercing Shot" : "+1 Range";
            }
        }
        else
        {
            switch (path)
            {
                case UpgradePath.Power:
                    if (tier == 1) return "Arc Damage +5";
                    if (tier == 2) return "Chain Damage +15%";
                    return "Overload Pulse";

                case UpgradePath.Speed:
                    if (tier == 1) return "Rapid Discharge";
                    if (tier == 2) return "Capacitor Overclock";
                    return "Chain +2 Targets";

                case UpgradePath.Range:
                    if (tier == 1) return "Reach +0.75 / Chain +0.25";
                    if (tier == 2) return "Reach +0.75 / Chain +0.5";
                    return "Chain Radius +1";
            }
        }

        return "Upgrade";
    }

    private IEnumerator HideTeslaArc()
    {
        yield return new WaitForSeconds(0.12f);

        if (teslaArcRenderer != null)
            teslaArcRenderer.enabled = false;

        arcCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (towerType == TowerType.Tesla)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, chainRadius);
        }
    }
}
