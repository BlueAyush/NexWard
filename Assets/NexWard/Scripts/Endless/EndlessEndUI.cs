
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndlessEndUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaveManager waveManager;

    [Header("Results Panel")]
    [SerializeField] private GameObject resultsPanel;

    [Header("UI Text")]
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultStatsText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Research Data Rewards")]
    [SerializeField] private int researchDataPer30Seconds = 1;
    [SerializeField] private int enemiesPerResearchData = 5;
    [SerializeField] private int bossRewardPerDefeatedBoss = 20;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool resultsShown;

    private void Awake()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartRun);
            restartButton.onClick.AddListener(RestartRun);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void Start()
    {
        if (waveManager == null)
        {
            waveManager = FindAnyObjectByType<WaveManager>();
        }

        if (waveManager == null)
        {
            Debug.LogError(
                "EndlessEndUI: WaveManager reference missing!"
            );

            return;
        }

        waveManager.RunEnded += ShowDefeat;
    }

    // ---------------- RESULTS ----------------

    private void ShowDefeat(string reason)
    {
        if (resultsShown)
            return;

        resultsShown = true;

        // Calculate Research Data rewards.
        int timeReward =
            Mathf.FloorToInt(
                waveManager.ElapsedTime / 30f
            ) * researchDataPer30Seconds;

        int killReward =
            Mathf.FloorToInt(
                (float)waveManager.EnemiesDefeated /
                Mathf.Max(1, enemiesPerResearchData)
            );

        int bossReward =
            waveManager.BossesDefeated *
            bossRewardPerDefeatedBoss;

        int totalResearchReward =
            timeReward + killReward + bossReward;

        // Save Research Data permanently.
        ResearchDataManager.Add(totalResearchReward);

        // Make sure gameplay is paused.
        Time.timeScale = 0f;

        if (resultsPanel == null)
        {
            Debug.LogError(
                "EndlessEndUI: Results Panel missing!"
            );

            return;
        }

        resultsPanel.SetActive(true);

        // Result title.
        if (resultTitleText != null)
        {
            resultTitleText.text = "DEFEAT";
            resultTitleText.color =
                new Color(1f, 0.3f, 0.3f);
        }

        // Format survival time.
        int totalSeconds =
            Mathf.FloorToInt(waveManager.ElapsedTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        // Display run statistics.
        if (resultStatsText != null)
        {
            resultStatsText.text =
                $"DEFEAT REASON: {reason}\n\n" +

                $"WAVE REACHED: {waveManager.CurrentWave}\n\n" +

                $"TIME SURVIVED: {minutes:00}:{seconds:00}\n\n" +

                $"ENEMIES DEFEATED: {waveManager.EnemiesDefeated}\n\n" +

                $"BOSSES DEFEATED: {waveManager.BossesDefeated}\n\n" +

                $"SCRAP EARNED: {waveManager.ScrapEarned}\n\n" +

                $"RESEARCH DATA EARNED: +{totalResearchReward}\n\n" +

                $"TOTAL RESEARCH DATA: {ResearchDataManager.Total}";
        }

        Debug.Log(
            "ENDLESS SURVIVAL RESULTS DISPLAYED."
        );

        Debug.Log(
            $"Research Data earned: {totalResearchReward}"
        );
    }

    // ---------------- BUTTONS ----------------

    public void RestartRun()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ---------------- CLEANUP ----------------

    private void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.RunEnded -= ShowDefeat;
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartRun);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        }
    }
}

