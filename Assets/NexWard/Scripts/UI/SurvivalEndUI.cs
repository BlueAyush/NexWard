
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SurvivalEndUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SurvivalManager survivalManager;

    [Header("Panel")]
    [SerializeField] private GameObject resultsPanel;

    [Header("UI Text")]
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultStatsText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;

    [Header("Research Data Rewards")]
    [SerializeField] private int researchDataPer30Seconds = 1;
    [SerializeField] private int enemiesPerResearchData = 5;
    [SerializeField] private int bossVictoryBonus = 20;

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool resultsShown;

    private void Awake()
    {
        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartRun);
            restartButton.onClick.AddListener(RestartRun);
        }

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void Start()
    {
        if (survivalManager == null)
            survivalManager = FindAnyObjectByType<SurvivalManager>();

        if (survivalManager == null)
        {
            Debug.LogError(
                "SurvivalEndUI: SurvivalManager reference missing!"
            );
            return;
        }

        survivalManager.VictoryAchieved += ShowVictory;
        survivalManager.DefeatAchieved += ShowDefeat;
    }

    private void ShowVictory()
    {
        ShowResults(true);
    }

    private void ShowDefeat()
    {
        ShowResults(false);
    }

    private void ShowResults(bool victory)
    {
        if (resultsShown)
            return;

        resultsShown = true;

        // Calculate Research Data rewards.
        int timeReward =
            Mathf.FloorToInt(
                survivalManager.ElapsedTime / 30f
            ) * researchDataPer30Seconds;

        int killReward =
            Mathf.FloorToInt(
                (float)survivalManager.EnemiesDefeated /
                Mathf.Max(1, enemiesPerResearchData)
            );

        int bossReward = victory ? bossVictoryBonus : 0;

        int totalResearchReward =
            timeReward + killReward + bossReward;

        // Save the reward permanently.
        ResearchDataManager.Add(totalResearchReward);

        if (resultsPanel == null)
            return;

        // Freeze the game after the run ends.
        Time.timeScale = 0f;

        resultsPanel.SetActive(true);

        resultTitleText.text = victory
            ? "VICTORY!"
            : "DEFEAT";

        resultTitleText.color = victory
            ? new Color(0.3f, 1f, 0.6f)
            : new Color(1f, 0.3f, 0.3f);

        int totalSeconds =
            Mathf.FloorToInt(survivalManager.ElapsedTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        string bossResult = victory
            ? "DEFEATED"
            : "NOT DEFEATED";

        resultStatsText.text =
            $"SURVIVAL TIME: {minutes:00}:{seconds:00}\n\n" +
            $"ENEMIES DEFEATED: {survivalManager.EnemiesDefeated}\n\n" +
            $"SCRAP EARNED: {survivalManager.ScrapEarned}\n\n" +
            $"BOSS: {bossResult}\n\n" +
            $"RESEARCH DATA EARNED: +{totalResearchReward}\n\n" +
            $"TOTAL RESEARCH DATA: {ResearchDataManager.Total}";

        Debug.Log(
            victory
                ? "VICTORY RESULTS DISPLAYED."
                : "DEFEAT RESULTS DISPLAYED."
        );
    }

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

    private void OnDestroy()
    {
        if (survivalManager != null)
        {
            survivalManager.VictoryAchieved -= ShowVictory;
            survivalManager.DefeatAchieved -= ShowDefeat;
        }
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);

        Time.timeScale = 1f;
    }
}
