
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Results Panel")]
    [SerializeField] private GameObject resultsPanel;

    [Header("Game Managers")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private SurvivalManager survivalManager;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Connect button listeners.
        AddListener(pauseButton, TogglePause);
        AddListener(resumeButton, ResumeGame);
        AddListener(restartButton, RestartGame);
        AddListener(mainMenuButton, ReturnToMainMenu);
    }

    private void Start()
    {
        if (waveManager == null)
            waveManager = FindAnyObjectByType<WaveManager>();

        if (survivalManager == null)
            survivalManager = FindAnyObjectByType<SurvivalManager>();
    }

    private void Update()
    {
        if (!PauseKeyPressed())
            return;

        if (isPaused)
        {
            ResumeGame();
        }
        else if (CanPause())
        {
            PauseGame();
        }
    }

    // ---------------- INPUT ----------------

    private bool PauseKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
            return false;

        return Keyboard.current.escapeKey.wasPressedThisFrame ||
               Keyboard.current.pKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape) ||
               Input.GetKeyDown(KeyCode.P);
#endif
    }

    // ---------------- PAUSE CONDITIONS ----------------

    private bool CanPause()
    {
        // Don't open pause menu after Game Over.
        if (resultsPanel != null &&
            resultsPanel.activeInHierarchy)
        {
            return false;
        }

        if (waveManager != null && waveManager.IsGameOver)
            return false;

        if (survivalManager != null &&
            (survivalManager.IsDefeated ||
             survivalManager.IsVictory))
        {
            return false;
        }

        return true;
    }

    // ---------------- PAUSE / RESUME ----------------

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else if (CanPause())
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused || !CanPause())
            return;

        isPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Debug.Log("GAME PAUSED!");
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Debug.Log("GAME RESUMED!");
    }

    // ---------------- SCENE MANAGEMENT ----------------

    public void RestartGame()
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

    // ---------------- BUTTON LISTENERS ----------------

    private void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void OnDestroy()
    {
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(TogglePause);

        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);

        Time.timeScale = 1f;
    }
}

