
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Tracks")]
    [SerializeField] private AudioClip[] mainMenuMusic;
    [SerializeField] private AudioClip[] gameplayMusic;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip uiHoverClip;
    [SerializeField] private AudioClip uiSelectClip;

    [Header("Gameplay Sounds")]
    [SerializeField] private AudioClip bossExplosionClip;
    [SerializeField] private AudioClip coreHurtClip;
    [SerializeField] private AudioClip enemyDieClip;
    [SerializeField] private AudioClip playerShootClip;
    [SerializeField] private AudioClip playerHurtClip;
    [SerializeField] private AudioClip laserShootClip;
    [SerializeField] private AudioClip teslaShootClip;

    private int lastMainMenuTrack = -1;
    private int lastGameplayTrack = -1;

    private AudioClip[] activeTracks;
    private bool activeTracksAreMainMenu;

    // ---------------- INITIALIZATION ----------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = false;
        }

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForScene(
            SceneManager.GetActiveScene().name
        );
    }

    private void Update()
    {
        // Play another random track when the current one finishes.
        if (musicSource == null ||
            activeTracks == null ||
            musicSource.clip == null)
            return;

        if (!musicSource.isPlaying)
        {
            PlayNextTrack();
        }
    }

    // ---------------- SCENE MUSIC ----------------

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip[] tracks;
        bool isMainMenu;

        if (sceneName == "MainMenu")
        {
            tracks = mainMenuMusic;
            isMainMenu = true;
        }
        else if (sceneName == "TutorialArena" ||
                 sceneName == "EndlessArena" ||
                 sceneName == "SurvivalArena")
        {
            tracks = gameplayMusic;
            isMainMenu = false;
        }
        else
        {
            return;
        }

        if (tracks == null || tracks.Length == 0)
            return;

        // Don't restart music when moving between gameplay scenes.
        if (activeTracks == tracks &&
            musicSource != null &&
            musicSource.isPlaying)
        {
            return;
        }

        activeTracks = tracks;
        activeTracksAreMainMenu = isMainMenu;

        PlayNextTrack();
    }

    // ---------------- RANDOM MUSIC ----------------

    private void PlayNextTrack()
    {
        if (musicSource == null ||
            activeTracks == null)
            return;

        int previousTrack = activeTracksAreMainMenu
            ? lastMainMenuTrack
            : lastGameplayTrack;

        List<int> availableTracks = new List<int>();

        // Only include valid audio clips.
        for (int i = 0; i < activeTracks.Length; i++)
        {
            if (activeTracks[i] != null)
            {
                availableTracks.Add(i);
            }
        }

        if (availableTracks.Count == 0)
        {
            Debug.LogWarning(
                "AudioManager: No valid music tracks assigned!"
            );

            musicSource.Stop();
            musicSource.clip = null;
            return;
        }

        // Prevent the same track from playing twice consecutively.
        if (availableTracks.Count > 1)
        {
            availableTracks.Remove(previousTrack);
        }

        int randomPosition = Random.Range(
            0,
            availableTracks.Count
        );

        int selectedTrack = availableTracks[randomPosition];

        // Remember the selected track.
        if (activeTracksAreMainMenu)
        {
            lastMainMenuTrack = selectedTrack;
        }
        else
        {
            lastGameplayTrack = selectedTrack;
        }

        // Play selected track once.
        musicSource.Stop();

        musicSource.clip = activeTracks[selectedTrack];
        musicSource.loop = false;

        musicSource.Play();

        Debug.Log(
            $"Now Playing: {musicSource.clip.name}"
        );
    }

    // ---------------- SOUND EFFECTS ----------------

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // UI sounds.
    public void PlayUIHover()
    {
        PlaySFX(uiHoverClip);
    }

    public void PlayUISelect()
    {
        PlaySFX(uiSelectClip);
    }

    // Gameplay sounds.
    public void PlayBossExplosion()
    {
        PlaySFX(bossExplosionClip);
    }

    public void PlayCoreHurt()
    {
        PlaySFX(coreHurtClip);
    }

    public void PlayEnemyDie()
    {
        PlaySFX(enemyDieClip);
    }

    public void PlayPlayerShoot()
    {
        PlaySFX(playerShootClip);
    }

    public void PlayPlayerHurt()
    {
        PlaySFX(playerHurtClip);
    }

    public void PlayLaserShoot()
    {
        PlaySFX(laserShootClip);
    }

    public void PlayTeslaShoot()
    {
        PlaySFX(teslaShootClip);
    }

    // ---------------- CLEANUP ----------------

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
}
