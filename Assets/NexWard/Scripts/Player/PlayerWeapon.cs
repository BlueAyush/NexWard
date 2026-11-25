
using UnityEngine;
using System.Collections.Generic;

public class PlayerWeapon : MonoBehaviour
{
    public enum WeaponMode
    {
        Manual,
        Automatic
    }

    [Header("Weapon Settings")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float fireRate = 0.35f;
    [SerializeField] private float projectileSpawnDistance = 0.4f;

    [Header("Automatic Targeting")]
    [SerializeField] private float autoTargetRange = 6f;

    [Header("Weapon Upgrades")]
    [SerializeField] private float damage = 10f;

    private bool piercingUnlocked;

    private Camera mainCamera;
    private float nextFireTime;

    private WeaponMode currentMode = WeaponMode.Manual;

    public float Damage => damage;
    public WeaponMode CurrentMode => currentMode;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (TowerPlacementManager.IsPlacing ||
            TowerUpgradePanel.IsOpen ||
            CoreUpgradePanel.IsOpen ||
            PlayerUpgradePanel.IsOpen)
        {
            return;
        }

        if (mainCamera == null || projectilePrefab == null)
            return;

        // Switch between Manual and Automatic.
        if (Input.GetKeyDown(KeyCode.T))
        {
            SwitchMode();
        }

        if (currentMode == WeaponMode.Manual)
        {
            HandleManualShooting();
        }
        else
        {
            HandleAutomaticShooting();
        }
    }

    // ---------------- MODE SWITCHING ----------------

    private void SwitchMode()
    {
        currentMode = currentMode == WeaponMode.Manual
            ? WeaponMode.Automatic
            : WeaponMode.Manual;

        Debug.Log($"Weapon Mode: {currentMode}");
    }

    // ---------------- MANUAL SHOOTING ----------------

    private void HandleManualShooting()
    {
        if (!Input.GetMouseButton(0))
            return;

        if (Time.time < nextFireTime)
            return;

        Vector2 direction = GetMouseDirection();

        if (direction.sqrMagnitude < 0.001f)
            return;

        Fire(direction);
    }

    private Vector2 GetMouseDirection()
    {
        Vector3 screenPosition = Input.mousePosition;

        screenPosition.z =
            Mathf.Abs(mainCamera.transform.position.z);

        Vector3 mouseWorld =
            mainCamera.ScreenToWorldPoint(screenPosition);

        Vector2 direction =
            (Vector2)(mouseWorld - transform.position);

        return direction.normalized;
    }

    // ---------------- AUTOMATIC SHOOTING ----------------

    private void HandleAutomaticShooting()
    {
        if (Time.time < nextFireTime)
            return;

        IDamageable target = FindNearestEnemy();

        if (target == null)
            return;

        Vector2 direction =
            (Vector2)(target.DamageableTransform.position -
                      transform.position);

        if (direction.sqrMagnitude < 0.001f)
            return;

        Fire(direction.normalized);
    }


    private IDamageable FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            autoTargetRange
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


    // ---------------- PROJECTILE FIRING ----------------

    private void Fire(Vector2 direction)
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
            damage,
            piercingUnlocked
        );

        // Play shooting sound.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerShoot();
        }

        nextFireTime = Time.time + fireRate;
    }

    // ---------------- WEAPON UPGRADES ----------------

    public void IncreaseDamage(float amount)
    {
        damage += amount;

        Debug.Log($"Player Weapon Damage: {damage}");
    }

    public void UnlockPiercing()
    {
        piercingUnlocked = true;

        Debug.Log("Player unlocked Piercing Projectiles!");
    }

    private void OnDrawGizmosSelected()
    {
        if (currentMode == WeaponMode.Automatic)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(
                transform.position,
                autoTargetRange
            );
        }
    }
}
