
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject researchLabPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject howToPlayPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button researchLabButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button quitButton;

    [Header("Level Select Buttons")]
    [SerializeField] private Button endlessArenaButton;
    [SerializeField] private Button survivalArenaButton;
    [SerializeField] private Button backButton;

    [Header("Research Lab")]
    [SerializeField] private Button researchLabCloseButton;
    [SerializeField] private ResearchLabUI researchLabUI;

    [Header("Settings")]
    [SerializeField] private Button settingsBackButton;

    [Header("How To Play")]
    [SerializeField] private Button howToPlayBackButton;

    private void Awake()
    {
        playButton.onClick.AddListener(OpenLevelSelect);
        researchLabButton.onClick.AddListener(OpenResearchLab);
        settingsButton.onClick.AddListener(OpenSettings);
        howToPlayButton.onClick.AddListener(OpenHowToPlay);
        quitButton.onClick.AddListener(QuitGame);

        endlessArenaButton.onClick.AddListener(LoadEndlessArena);
        survivalArenaButton.onClick.AddListener(LoadSurvivalArena);
        backButton.onClick.AddListener(ReturnToMainMenu);

        researchLabCloseButton.onClick.AddListener(ReturnToMainMenu);
        settingsBackButton.onClick.AddListener(ReturnToMainMenu);
        howToPlayBackButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void Start()
    {
        ShowPanel(mainMenuPanel);
    }

    // Only one menu panel can be active at a time.
    private void ShowPanel(GameObject panelToShow)
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        researchLabPanel.SetActive(false);
        settingsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);

        panelToShow.SetActive(true);
    }

    private void OpenLevelSelect()
    {
        ShowPanel(levelSelectPanel);
    }

    private void OpenResearchLab()
    {
        ShowPanel(researchLabPanel);

        if (researchLabUI != null)
            researchLabUI.RefreshUI();
    }

    private void OpenSettings()
    {
        ShowPanel(settingsPanel);
    }

    private void OpenHowToPlay()
    {
        ShowPanel(howToPlayPanel);
    }

    private void ReturnToMainMenu()
    {
        ShowPanel(mainMenuPanel);
    }

    private void LoadEndlessArena()
    {
        SceneManager.LoadScene("EndlessArena");
    }

    private void LoadSurvivalArena()
    {
        SceneManager.LoadScene("SurvivalArena");
    }

    private void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit button pressed.");
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(OpenLevelSelect);
        researchLabButton.onClick.RemoveListener(OpenResearchLab);
        settingsButton.onClick.RemoveListener(OpenSettings);
        howToPlayButton.onClick.RemoveListener(OpenHowToPlay);
        quitButton.onClick.RemoveListener(QuitGame);

        endlessArenaButton.onClick.RemoveListener(LoadEndlessArena);
        survivalArenaButton.onClick.RemoveListener(LoadSurvivalArena);
        backButton.onClick.RemoveListener(ReturnToMainMenu);

        researchLabCloseButton.onClick.RemoveListener(ReturnToMainMenu);
        settingsBackButton.onClick.RemoveListener(ReturnToMainMenu);
        howToPlayBackButton.onClick.RemoveListener(ReturnToMainMenu);
    }
}
