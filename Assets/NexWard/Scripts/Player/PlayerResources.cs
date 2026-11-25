
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public static PlayerResources Instance { get; private set; }

    [Header("Experience")]
    [SerializeField] private int currentXP;
    [SerializeField] private int xpToNextLevel = 30;
    [SerializeField] private int currentLevel = 1;

    [Header("Resources")]
    [SerializeField] private int scrap;

    [Header("Upgrade Points")]
    [SerializeField] private float commandPoints;


    public int CurrentXP => currentXP;
    public int XPToNextLevel => xpToNextLevel;
    public int CurrentLevel => currentLevel;
    public int Scrap => scrap;
    public float CommandPoints => commandPoints;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        currentXP += amount;

        Debug.Log($"XP: {currentXP}/{xpToNextLevel}");

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        commandPoints++;

        Debug.Log($"LEVEL UP! Level: {currentLevel}");
        Debug.Log($"Command Points: {commandPoints}");
    }

    public void AddScrap(int amount)
    {
        if (amount <= 0)
            return;

        scrap += amount;

        // Timed Survival statistics.
        SurvivalManager.Instance?.RegisterScrapEarned(amount);

        // Endless Survival statistics.
        WaveManager.Instance?.RegisterScrapEarned(amount);

        Debug.Log($"Scrap: {scrap}");
    }

    public bool SpendScrap(int amount)
    {
        if (amount <= 0 || scrap < amount)
            return false;

        scrap -= amount;
        Debug.Log($"Scrap remaining: {scrap}");

        return true;
    }

    public bool SpendCommandPoint()
    {
        if (commandPoints < 1f)
            return false;

        commandPoints -= 1f;

        return true;
    }

    public void RefundCommandPoints(float amount)
    {
        if (amount <= 0)
            return;

        commandPoints += amount;

        Debug.Log(
            $"Command Points after refund: {commandPoints:0.##}"
        );
    }
}
