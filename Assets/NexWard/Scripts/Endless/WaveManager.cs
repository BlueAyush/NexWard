
using System;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private EnemyAI standardEnemyPrefab;
    [SerializeField] private EnemyAI runnerEnemyPrefab;
    [SerializeField] private BossAI bossPrefab;
    [SerializeField] private CoreHealth core;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Spawn Area")]
    [SerializeField] private float halfWidth = 7.5f;
    [SerializeField] private float halfHeight = 3.5f;

    [Header("Wave Settings")]
    [SerializeField] private int startingEnemies = 4;
    [SerializeField] private int additionalEnemiesPerWave = 2;
    [SerializeField] private float spawnInterval = 1.2f;
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Enemy Spawn Types")]
    [Range(0f, 1f)]
    [SerializeField] private float runnerSpawnChance = 0.2f;

    [Header("Enemy Scaling")]
    [SerializeField] private float healthIncreasePerTenWaves = 0.25f;

    [Header("Boss Encounter")]
    [SerializeField] private float bossSpawnInset = 0.5f;
    [SerializeField] private float bossHealthIncreasePerEncounter = 0.25f;

    private int currentWave;
    private int enemiesToSpawn;
    private int enemiesAlive;

    private float nextSpawnTime;
    private float nextWaveTime;

    private bool waveActive;
    private bool bossEncounterActive;
    private bool isGameOver;

    private BossAI activeBoss;

    // ---------------- PUBLIC DATA ----------------

    public int CurrentWave => currentWave;

    public int EnemiesRemaining =>
        Mathf.Max(0, enemiesToSpawn + enemiesAlive +
        (bossEncounterActive ? 1 : 0));

    public int EnemiesDefeated { get; private set; }

    public int BossesDefeated { get; private set; }

    [Header("Run Statistics")]
    [SerializeField] private int scrapEarned;

    public int ScrapEarned => scrapEarned;

    public float ElapsedTime { get; private set; }

    public bool IsWaveActive => waveActive;

    public bool IsBossActive => bossEncounterActive;

    public bool IsGameOver => isGameOver;

    public BossHealth ActiveBossHealth =>
        !isGameOver && activeBoss != null
            ? activeBoss.GetComponent<BossHealth>()
            : null;

    public event Action<string> RunEnded;

    // ---------------- INITIALIZATION ----------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        }

        if (core == null)
        {
            Debug.LogError(
                "WaveManager: Core reference missing!"
            );
        }

        if (standardEnemyPrefab == null &&
            runnerEnemyPrefab == null)
        {
            Debug.LogError(
                "WaveManager: Both enemy prefabs are missing!"
            );
        }

        nextWaveTime = Time.time + 2f;

        Debug.Log("ENDLESS SURVIVAL STARTED!");
    }

    // ---------------- MAIN UPDATE ----------------

    private void Update()
    {
        if (isGameOver)
            return;

        // Check Core destruction.
        if (core == null || core.CurrentHealth <= 0)
        {
            EndRun("CORE DESTROYED");
            return;
        }

        // Check player death.
        if (playerHealth != null && playerHealth.IsDead)
        {
            EndRun("PLAYER DOWN");
            return;
        }

        // Track survival time.
        ElapsedTime += Time.deltaTime;

        // Start the next wave after the countdown.
        if (!waveActive && Time.time >= nextWaveTime)
        {
            StartNextWave();
        }

        if (!waveActive)
            return;

        // Wait until SiegeBreaker is defeated.
        if (bossEncounterActive)
            return;

        // Spawn normal enemies at regular intervals.
        if (enemiesToSpawn > 0 &&
            Time.time >= nextSpawnTime)
        {
            SpawnEnemy();

            nextSpawnTime =
                Time.time + Mathf.Max(0.01f, spawnInterval);
        }

        // Complete the wave when all enemies are defeated.
        if (enemiesToSpawn <= 0 && enemiesAlive <= 0)
        {
            CompleteWave();
        }
    }

    // ---------------- WAVE PROGRESSION ----------------

    private void StartNextWave()
    {
        if (isGameOver)
            return;

        currentWave++;
        waveActive = true;

        // Every 10th wave is a SiegeBreaker encounter.
        if (currentWave % 10 == 0)
        {
            enemiesToSpawn = 0;

            StartBossEncounter();
            return;
        }

        long calculatedEnemies =
            (long)startingEnemies +
            (long)(currentWave - 1) *
            Math.Max(0, additionalEnemiesPerWave);

        enemiesToSpawn = (int)Math.Min(
            Math.Max(0L, calculatedEnemies),
            int.MaxValue
        );

        nextSpawnTime = Time.time;

        Debug.Log(
            $"WAVE {currentWave} STARTED! " +
            $"Enemies: {enemiesToSpawn}"
        );
    }

    private void CompleteWave()
    {
        if (isGameOver)
            return;

        waveActive = false;
        bossEncounterActive = false;

        enemiesToSpawn = 0;

        nextWaveTime =
            Time.time + Mathf.Max(0f, timeBetweenWaves);

        Debug.Log(
            $"WAVE {currentWave} COMPLETED! " +
            "Next wave incoming..."
        );
    }

    // ---------------- NORMAL ENEMY SPAWNING ----------------

    private void SpawnEnemy()
    {
        bool spawnRunner =
            runnerEnemyPrefab != null &&
            UnityEngine.Random.value < runnerSpawnChance;

        EnemyAI prefab = spawnRunner
            ? runnerEnemyPrefab
            : standardEnemyPrefab;

        // Fallback if one prefab is missing.
        if (prefab == null)
        {
            prefab = standardEnemyPrefab != null
                ? standardEnemyPrefab
                : runnerEnemyPrefab;
        }

        if (prefab == null)
        {
            Debug.LogError(
                "WaveManager: No enemy prefabs assigned!"
            );

            enemiesToSpawn--;
            return;
        }

        Vector2 spawnPosition = GetRandomSpawnPosition();

        EnemyAI enemy = Instantiate(
            prefab,
            spawnPosition,
            prefab.transform.rotation
        );

        enemy.SetTarget(core);

        EnemyHealth enemyHealth =
            enemy.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            Debug.LogError(
                $"{prefab.name} is missing EnemyHealth!"
            );

            Destroy(enemy.gameObject);
            enemiesToSpawn--;
            return;
        }

        // Apply wave-based health scaling.
        enemyHealth.ApplyHealthMultiplier(
            GetEnemyHealthMultiplier()
        );

        enemiesToSpawn--;
        enemiesAlive++;

        string enemyType =
            prefab == runnerEnemyPrefab
                ? "RUNNER"
                : "STANDARD";

        Debug.Log(
            $"{enemyType} ENEMY SPAWNED. " +
            $"Remaining to spawn: {enemiesToSpawn}"
        );
    }

    private float GetEnemyHealthMultiplier()
    {
        // Waves 1-10: 1x
        // Waves 11-20: 1.25x
        // Waves 21-30: 1.5x

        int scalingTier = (currentWave - 1) / 10;

        return 1f +
            scalingTier *
            Mathf.Max(0f, healthIncreasePerTenWaves);
    }

    // ---------------- BOSS ENCOUNTER ----------------

    private void StartBossEncounter()
    {
        bossEncounterActive = true;

        Debug.Log(
            $"SIEGEBREAKER APPROACHING! WAVE {currentWave}"
        );

        if (bossPrefab == null)
        {
            Debug.LogError(
                "WaveManager: Boss prefab is not assigned!"
            );

            bossEncounterActive = false;
            CompleteWave();
            return;
        }

        Vector2 spawnPosition = GetBossSpawnPosition();

        activeBoss = Instantiate(
            bossPrefab,
            spawnPosition,
            bossPrefab.transform.rotation
        );

        activeBoss.SetTarget(core);

        BossHealth bossHealth =
            activeBoss.GetComponent<BossHealth>();

        if (bossHealth == null)
        {
            Debug.LogError(
                "SiegeBreaker prefab is missing BossHealth!"
            );

            Destroy(activeBoss.gameObject);
            activeBoss = null;

            bossEncounterActive = false;
            CompleteWave();
            return;
        }

        // Increase boss HP with each boss encounter.
        int scalingTier = (currentWave / 10) - 1;

        float healthMultiplier =
            1f +
            scalingTier *
            Mathf.Max(0f, bossHealthIncreasePerEncounter);

        bossHealth.ApplyHealthMultiplier(healthMultiplier);

        // Listen for the boss defeat event.
        bossHealth.OnBossDefeated += HandleBossDefeated;

        Debug.Log(
            $"SIEGEBREAKER SPAWNED! " +
            $"Health multiplier: {healthMultiplier:F2}x"
        );
    }

    private void HandleBossDefeated(BossHealth defeatedBoss)
    {
        if (!bossEncounterActive || activeBoss == null)
            return;

        BossHealth currentBossHealth =
            activeBoss.GetComponent<BossHealth>();

        if (defeatedBoss != currentBossHealth)
            return;

        BossesDefeated++;

        defeatedBoss.OnBossDefeated -= HandleBossDefeated;

        activeBoss = null;
        bossEncounterActive = false;

        Debug.Log(
            $"SIEGEBREAKER DEFEATED! WAVE {currentWave} CLEARED!"
        );

        // Continue Endless Survival.
        CompleteWave();
    }

    // ---------------- SPAWN POSITIONS ----------------

    private Vector2 GetRandomSpawnPosition()
    {
        int side = UnityEngine.Random.Range(0, 4);

        switch (side)
        {
            case 0:
                return new Vector2(
                    UnityEngine.Random.Range(-halfWidth, halfWidth),
                    halfHeight
                );

            case 1:
                return new Vector2(
                    UnityEngine.Random.Range(-halfWidth, halfWidth),
                    -halfHeight
                );

            case 2:
                return new Vector2(
                    -halfWidth,
                    UnityEngine.Random.Range(-halfHeight, halfHeight)
                );

            default:
                return new Vector2(
                    halfWidth,
                    UnityEngine.Random.Range(-halfHeight, halfHeight)
                );
        }
    }

    private Vector2 GetBossSpawnPosition()
    {
        int side = UnityEngine.Random.Range(0, 4);

        float x = Mathf.Max(0f, halfWidth - bossSpawnInset);
        float y = Mathf.Max(0f, halfHeight - bossSpawnInset);

        switch (side)
        {
            case 0:
                return new Vector2(
                    UnityEngine.Random.Range(-x, x), y
                );

            case 1:
                return new Vector2(
                    UnityEngine.Random.Range(-x, x), -y
                );

            case 2:
                return new Vector2(
                    -x, UnityEngine.Random.Range(-y, y)
                );

            default:
                return new Vector2(
                    x, UnityEngine.Random.Range(-y, y)
                );
        }
    }

    // ---------------- ENEMY TRACKING ----------------

    public void EnemyDefeated()
    {
        EnemiesDefeated++;

        if (enemiesAlive > 0)
        {
            enemiesAlive--;
        }
    }

    public void RegisterScrapEarned(int amount)
    {
        if (amount <= 0 || isGameOver)
            return;

        scrapEarned += amount;

        Debug.Log($"Endless Scrap Earned: {scrapEarned}");
    }

    // ---------------- GAME OVER ----------------

    private void EndRun(string reason)
    {
        if (isGameOver)
            return;

        isGameOver = true;

        waveActive = false;
        enemiesToSpawn = 0;
        bossEncounterActive = false;

        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.DisableControls();
        }

        Debug.Log(
            $"ENDLESS SURVIVAL OVER: {reason}"
        );

        // Hide the boss bar through ActiveBossHealth.
        // Pause gameplay.
        Time.timeScale = 0f;

        // Notify the Endless results UI.
        RunEnded?.Invoke(reason);
    }

    // ---------------- CLEANUP ----------------

    private void OnDestroy()
    {
        if (activeBoss != null)
        {
            BossHealth bossHealth =
                activeBoss.GetComponent<BossHealth>();

            if (bossHealth != null)
            {
                bossHealth.OnBossDefeated -= HandleBossDefeated;
            }
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
