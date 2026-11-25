
using UnityEngine;
using System;
using System.Collections.Generic;

public class SurvivalManager : MonoBehaviour
{
    public static SurvivalManager Instance { get; private set; }

    public event Action DefeatAchieved;

    public int EnemiesDefeated { get; private set; }
    public int ScrapEarned { get; private set; }


    [Header("Run Settings")]
    [SerializeField, Min(30f)]
    private float totalSurvivalTime = 300f;

    [SerializeField, Min(1f)]
    private float stormStartTime = 150f;


    [Header("Event Warnings")]
    [SerializeField] private float warningLeadTime = 8f;

    [Header("References")]
    [SerializeField] private EnemyAI standardEnemyPrefab;
    [SerializeField] private EnemyAI runnerEnemyPrefab;
    [SerializeField] private CoreHealth core;

    [Header("Boss Encounter")]
    [SerializeField] private BossAI bossPrefab;
    [SerializeField] private float bossSpawnInset = 0.5f;

    [Header("Arena Spawn Bounds")]
    [SerializeField] private float halfWidth = 7.5f;
    [SerializeField] private float halfHeight = 3.5f;

    [Header("INITIAL ASSAULT - 0 to 4 Minutes")]
    [SerializeField] private float initialSpawnInterval = 2f;
    [SerializeField] private int initialEnemiesPerSpawn = 1;
    [SerializeField] private int initialMaxAlive = 10;

    [Range(0.1f, 3f)]
    [SerializeField] private float initialStandardHealthMultiplier = 1f;

    [Range(0.1f, 3f)]
    [SerializeField] private float initialRunnerHealthMultiplier = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float initialRunnerSpawnChance = 0.1f;

    [Header("STORM WAVE - At 4 Minutes")]
    [SerializeField] private int stormEnemiesPerSide = 3;
    [SerializeField] private float stormIndicatorDuration = 12f;

    [Range(0.1f, 3f)]
    [SerializeField] private float stormStandardHealthMultiplier = 1f;

    [Range(0.1f, 3f)]
    [SerializeField] private float stormRunnerHealthMultiplier = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float stormRunnerSpawnChance = 0.2f;

    [Header("ESCALATION - 4 to 8 Minutes")]
    [SerializeField] private float lateSpawnInterval = 1.4f;
    [SerializeField] private int lateEnemiesPerSpawn = 2;
    [SerializeField] private int lateMaxAlive = 16;

    [Range(0.1f, 3f)]
    [SerializeField] private float lateStandardHealthMultiplier = 1.4f;

    [Range(0.1f, 3f)]
    [SerializeField] private float lateRunnerHealthMultiplier = 1.4f;

    [Range(0f, 1f)]
    [SerializeField] private float lateRunnerSpawnChance = 0.3f;


    private readonly List<EnemyHealth> activeEnemies =
        new List<EnemyHealth>();

    private float elapsedTime;
    private float nextSpawnTime;
    private float stormIndicatorEndTime;

    private bool isRunning;
    private bool stormTriggered;
    private bool isBossPhase;
    private bool isDefeated;

    public event Action StormStarted;
    public event Action BossPhaseStarted;

    private bool stormWarningTriggered;
    private bool bossWarningTriggered;

    private string warningMessage = "";
    private float warningTargetTime;

    private BossAI activeBoss;
    private bool isVictory;

    public BossHealth ActiveBossHealth =>
    activeBoss != null
        ? activeBoss.GetComponent<BossHealth>()
        : null;

    public bool IsVictory => isVictory;

    public event Action VictoryAchieved;

    public float ElapsedTime => elapsedTime;

    public float RemainingTime =>
        Mathf.Max(0f, totalSurvivalTime - elapsedTime);

    public float TotalSurvivalTime => totalSurvivalTime;

    public bool IsRunning => isRunning;
    public bool IsBossPhase => isBossPhase;
    public bool IsDefeated => isDefeated;

    public string WarningMessage => warningMessage;

    public float WarningCountdown =>
        Mathf.Max(0f, warningTargetTime - elapsedTime);

    public int EnemiesAlive
    {
        get
        {
            CleanupEnemyList();
            return activeEnemies.Count;
        }
    }

    public string PhaseName
    {
        get
        {
            if (isDefeated)
                return "CORE DESTROYED";

            if (isBossPhase)
                return "BOSS ENCOUNTER";

            if (elapsedTime < stormStartTime)
                return "INITIAL WAVE";

            if (elapsedTime < stormIndicatorEndTime)
                return "STORM WAVE";

            return "ESCALATION";
        }
    }

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
        if (core == null)
        {
            Debug.LogError("SurvivalManager: Core reference missing!");
            return;
        }

        if (standardEnemyPrefab == null)
        {
            Debug.LogError("SurvivalManager: Standard Enemy prefab missing!");
            return;
        }

        isRunning = true;
        nextSpawnTime = initialSpawnInterval;

        Debug.Log("SURVIVAL MODE STARTED!");
    }

    private void Update()
    {
        if (isDefeated || isVictory)
            return;

        // Core must still be checked during the boss encounter.
        if (core == null || core.CurrentHealth <= 0)
        {
            Defeat();
            return;
        }

        if (!isRunning)
            return;

        // End the run if the Core is destroyed.
        if (core == null || core.CurrentHealth <= 0)
        {
            Defeat();
            return;
        }

        elapsedTime = Mathf.Min(
            elapsedTime + Time.deltaTime,
            totalSurvivalTime
        );

        // 8-second warning before the storm.
        if (!stormWarningTriggered &&
            !stormTriggered &&
            elapsedTime >= stormStartTime - warningLeadTime)
        {
            stormWarningTriggered = true;

            ShowWarning(
                "STORM WAVE INCOMING!\nPREPARE YOUR DEFENSES",
                stormStartTime
            );
        }

        // 8-second warning before the boss.
        if (!bossWarningTriggered &&
            !isBossPhase &&
            elapsedTime >= totalSurvivalTime - warningLeadTime)
        {
            bossWarningTriggered = true;

            ShowWarning(
                "BOSS APPROACHING!\nPREPARE FOR BATTLE",
                totalSurvivalTime
            );
        }

        // Trigger the storm exactly once.
        if (!stormTriggered && elapsedTime >= stormStartTime)
        {
            TriggerStorm();
            return;
        }

        // Stop regular spawning at the boss time.
        if (elapsedTime >= totalSurvivalTime)
        {
            StartBossPhase();
            return;
        }

        CleanupEnemyList();

        bool isLatePhase = elapsedTime >= stormStartTime;

        float spawnInterval = isLatePhase
            ? lateSpawnInterval
            : initialSpawnInterval;

        int enemiesPerSpawn = isLatePhase
            ? lateEnemiesPerSpawn
            : initialEnemiesPerSpawn;

        int maxAlive = isLatePhase
            ? lateMaxAlive
            : initialMaxAlive;

        float standardHealthMultiplier = isLatePhase
            ? lateStandardHealthMultiplier
            : initialStandardHealthMultiplier;

        float runnerHealthMultiplier = isLatePhase
            ? lateRunnerHealthMultiplier
            : initialRunnerHealthMultiplier;

        float runnerChance = isLatePhase
            ? lateRunnerSpawnChance
            : initialRunnerSpawnChance;

        if (elapsedTime >= nextSpawnTime)
        {
            if (activeEnemies.Count < maxAlive)
            {
                SpawnGroup(
                    enemiesPerSpawn,
                    maxAlive,
                    runnerChance,
                    standardHealthMultiplier,
                    runnerHealthMultiplier
                );
            }

            nextSpawnTime = elapsedTime + spawnInterval;
        }
    }

    public void RegisterEnemyDefeated()
    {
        EnemiesDefeated++;
    }

    public void RegisterScrapEarned(int amount)
    {
        if (amount <= 0)
            return;

        ScrapEarned += amount;
    }

    // ---------------- NORMAL SPAWNING ----------------

    private void SpawnGroup(
        int count,
        int maxAlive,
        float runnerChance,
        float standardHealthMultiplier,
        float runnerHealthMultiplier)
    {
        for (int i = 0; i < count; i++)
        {
            CleanupEnemyList();

            if (activeEnemies.Count >= maxAlive)
                break;

            SpawnEnemy(
                GetRandomEdgePosition(),
                runnerChance,
                standardHealthMultiplier,
                runnerHealthMultiplier
            );
        }
    }

    // ---------------- STORM EVENT ----------------

    private void TriggerStorm()
    {
        stormTriggered = true;
        warningMessage = "";

        stormIndicatorEndTime =
            elapsedTime + stormIndicatorDuration;

        Debug.Log("STORM WAVE! ENEMIES APPROACHING FROM ALL SIDES!");

        StormStarted?.Invoke();

        // Spawn enemies from all four sides.
        for (int side = 0; side < 4; side++)
        {
            for (int i = 0; i < stormEnemiesPerSide; i++)
            {
                SpawnEnemy(
                    GetSideSpawnPosition(side),
                    stormRunnerSpawnChance,
                    stormStandardHealthMultiplier,
                    stormRunnerHealthMultiplier
                );
            }
        }

        // Schedule the first regular escalation spawn.
        nextSpawnTime = elapsedTime + lateSpawnInterval;
    }

    private void ShowWarning(string message, float eventTime)
    {
        warningMessage = message;
        warningTargetTime = eventTime;

        Debug.Log(message);
    }

    // ---------------- ENEMY CREATION ----------------

    private void SpawnEnemy(
        Vector2 position,
        float runnerChance,
        float standardHealthMultiplier,
        float runnerHealthMultiplier)
    {
        bool spawnRunner =
            runnerEnemyPrefab != null &&
            UnityEngine.Random.value < runnerChance;

        EnemyAI prefab = spawnRunner
            ? runnerEnemyPrefab
            : standardEnemyPrefab;

        EnemyAI enemy = Instantiate(
            prefab,
            position,
            prefab.transform.rotation
        );

        enemy.SetTarget(core);

        EnemyHealth health =
            enemy.GetComponent<EnemyHealth>();

        if (health != null)
        {
            float multiplier = spawnRunner
                ? runnerHealthMultiplier
                : standardHealthMultiplier;

            health.ApplyHealthMultiplier(multiplier);
            activeEnemies.Add(health);
        }
    }

    // ---------------- SPAWN POSITIONS ----------------

    private Vector2 GetRandomEdgePosition()
    {
        int side = UnityEngine.Random.Range(0, 4);

        return GetSideSpawnPosition(side);
    }

    // 0 = top, 1 = bottom, 2 = left, 3 = right.
    private Vector2 GetSideSpawnPosition(int side)
    {
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

    private void CleanupEnemyList()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    // ---------------- END CONDITIONS ----------------

    private void StartBossPhase()
    {
        isBossPhase = true;
        warningMessage = "";
        isRunning = false;

        Debug.Log("BOSS ENCOUNTER STARTED!");

        SpawnBoss();

        BossPhaseStarted?.Invoke();
    }


    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError(
                "SurvivalManager: Boss prefab is not assigned!"
            );
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
                "Boss prefab is missing BossHealth!"
            );

            Destroy(activeBoss.gameObject);
            return;
        }

        bossHealth.OnBossDefeated += HandleBossDefeated;

        Debug.Log(
            $"SIEGEBREAKER SPAWNED AT {spawnPosition}"
        );
    }

    private Vector2 GetBossSpawnPosition()
    {
        int side = UnityEngine.Random.Range(0, 4);

        float x = halfWidth - bossSpawnInset;
        float y = halfHeight - bossSpawnInset;

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

    private void HandleBossDefeated(BossHealth defeatedBoss)
    {
        if (isVictory || isDefeated)
            return;

        isVictory = true;

        Debug.Log("VICTORY! SIEGEBREAKER DEFEATED!");

        VictoryAchieved?.Invoke();
    }

    private void Defeat()
    {
        if (isDefeated || isVictory)
            return;

        isDefeated = true;
        isRunning = false;

        Debug.Log("SURVIVAL DEFEAT! CORE DESTROYED!");

        DefeatAchieved?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnValidate()
    {
        totalSurvivalTime = Mathf.Max(30f, totalSurvivalTime);

        stormStartTime = Mathf.Clamp(
            stormStartTime,
            1f,
            totalSurvivalTime - 1f
        );

        warningLeadTime = Mathf.Clamp(
            warningLeadTime,
            0f,
            totalSurvivalTime
        );
    }
}
