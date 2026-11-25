
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerResources player;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Core References")]
    [SerializeField] private CoreHealth core;

    [Header("Wave References")]
    [SerializeField] private WaveManager waveManager;

    [Header("Survival Mode")]
    [SerializeField] private SurvivalManager survivalManager;
    [SerializeField] private TMP_Text survivalTimerText;
    [SerializeField] private TMP_Text survivalPhaseText;


    [Header("Boss Health UI")]
    [SerializeField] private GameObject bossHealthPanel;
    [SerializeField] private Slider bossHealthSlider;
    [SerializeField] private TMP_Text bossHealthText;
    [SerializeField] private TMP_Text bossNameText;


    [Header("Survival Warnings")]
    [SerializeField] private TMP_Text survivalWarningText;

    [Header("HUD Text")]
    [SerializeField] private TMP_Text coreHealthText;
    [SerializeField] private TMP_Text playerStatsText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text waveText;

    [Header("Weapon Mode")]
    [SerializeField] private PlayerWeapon playerWeapon;
    [SerializeField] private TMP_Text weaponModeText;

    [Header("Tower Capacity")]
    [SerializeField] private TMP_Text towerCapacityText;

    [Header("XP Bar")]
    [SerializeField] private Slider xpBar;

    private void Update()
    {
        UpdateCoreHealth();
        UpdatePlayerStats();
        UpdateXP();
        UpdateWave();
        UpdateSurvival();
        UpdateWeaponMode();
        UpdateTowerCapacity();
        UpdateBossHealth();
    }

    private void UpdateCoreHealth()
    {
        if (coreHealthText == null)
            return;

        if (core == null)
        {
            coreHealthText.text = "CORE: --";
            return;
        }

        coreHealthText.text =
            $"CORE {Mathf.CeilToInt(core.CurrentHealth)} / {core.MaxHealth}";
    }

    private void UpdatePlayerStats()
    {
        if (playerStatsText == null)
            return;

        if (player == null)
        {
            playerStatsText.text = "PLAYER DATA MISSING";
            return;
        }

        string health = playerHealth != null
            ? $"{Mathf.CeilToInt(playerHealth.CurrentHealth)}/{playerHealth.MaxHealth}"
            : "--";

        playerStatsText.text =
            $"HP {health}    " +
            $"LV {player.CurrentLevel}    " +
            $"SCRAP {player.Scrap}    " +
            $"COMMAND POINTS: {player.CommandPoints:0.##}";
    }

    private void UpdateXP()
    {
        if (player == null)
            return;

        if (xpText != null)
        {
            xpText.text =
                $"XP {player.CurrentXP}/{player.XPToNextLevel}";
        }

        if (xpBar != null)
        {
            xpBar.maxValue = Mathf.Max(1, player.XPToNextLevel);
            xpBar.value = player.CurrentXP;
        }
    }

    private void UpdateWeaponMode()
    {
        if (playerWeapon == null || weaponModeText == null)
            return;

        if (playerWeapon.CurrentMode == PlayerWeapon.WeaponMode.Manual)
        {
            weaponModeText.text = "MODE: MANUAL  [T]";
            weaponModeText.color = new Color(0.3f, 0.7f, 1f);
        }
        else
        {
            weaponModeText.text = "MODE: AUTO  [T]";
            weaponModeText.color = new Color(0.3f, 1f, 0.7f);
        }
    }

    private void UpdateTowerCapacity()
    {
        if (towerCapacityText == null)
            return;

        TowerPlacementManager manager =
            TowerPlacementManager.Instance;

        if (manager == null)
        {
            towerCapacityText.text = "TOWERS: --";
            return;
        }

        int filled = manager.TowersPlaced;
        int capacity = manager.TowerCapacity;
        int available = Mathf.Max(0, capacity - filled);

        towerCapacityText.text =
            $"TOWERS: {filled}/{capacity}  |  FREE: {available}";
    }

    private void UpdateWave()
    {
        if (waveText == null)
            return;

        if (waveManager == null)
        {
            waveText.text = "WAVE --";
            return;
        }

        if (waveManager.CurrentWave == 0)
        {
            waveText.text = "GET READY";
        }
        else if (waveManager.IsBossActive)
        {
            waveText.text =
                $"WAVE {waveManager.CurrentWave} | SIEGEBREAKER";
        }
        else if (waveManager.IsWaveActive)
        {
            waveText.text =
                $"WAVE {waveManager.CurrentWave} | " +
                $"ENEMIES {waveManager.EnemiesRemaining}";
        }
        else
        {
            waveText.text =
                $"WAVE {waveManager.CurrentWave} CLEARED";
        }
    }

    private void UpdateSurvival()
    {
        if (survivalManager == null)
            return;

        if (survivalWarningText != null)
        {
            string warning = survivalManager.WarningMessage;

            bool shouldShow = !string.IsNullOrEmpty(warning);

            if (survivalWarningText.gameObject.activeSelf != shouldShow)
            {
                survivalWarningText.gameObject.SetActive(shouldShow);
            }

            if (shouldShow)
            {
                int warningSeconds = Mathf.CeilToInt(
                survivalManager.WarningCountdown
            );

                survivalWarningText.text =
                $"{warning}\nIN 00:{warningSeconds:00}";

                survivalWarningText.color =
                    warning.StartsWith("STORM")
                    ? new Color(1f, 0.8f, 0.2f)
                    : new Color(1f, 0.25f, 0.25f);
            }
        }

        int totalSeconds =
            Mathf.FloorToInt(survivalManager.ElapsedTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (survivalTimerText != null)
        {
            survivalTimerText.text =
                $"TIME {minutes:00}:{seconds:00}";
        }

        if (survivalPhaseText != null)
        {
            survivalPhaseText.text =
                survivalManager.PhaseName;
        }
    }


    private void UpdateBossHealth()
    {
        if (bossHealthPanel == null)
            return;

        // Timed Survival boss.
        BossHealth boss = survivalManager != null
            ? survivalManager.ActiveBossHealth
            : null;

        // Endless Survival boss.
        if (boss == null && waveManager != null)
        {
            boss = waveManager.ActiveBossHealth;
        }

        bool bossIsActive = boss != null && boss.IsAlive;

        if (bossHealthPanel.activeSelf != bossIsActive)
        {
            bossHealthPanel.SetActive(bossIsActive);
        }

        if (!bossIsActive)
            return;

        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = boss.MaxHealth;
            bossHealthSlider.value = boss.CurrentHealth;
        }

        if (bossHealthText != null)
        {
            bossHealthText.text =
                $"{Mathf.CeilToInt(boss.CurrentHealth)} / " +
                $"{Mathf.CeilToInt(boss.MaxHealth)}";
        }

        if (bossNameText != null)
        {
            bossNameText.text = "THE SIEGEBREAKER";
        }
    }

}
