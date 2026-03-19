using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    [Header("Default Settings")]
    [SerializeField] private GameDefaultSettings defaultSettings;

    [Header("Saved Settings")]
    [SerializeField] private GameRuntimeSettings savedSettings = new GameRuntimeSettings();

    [Header("Live Settings")]
    [SerializeField] private GameRuntimeSettings liveSettings = new GameRuntimeSettings();

    private string folderPath;
    private string settingsFilePath;
    private string checksumFilePath;

    private AudioMixManager musicPlayer;
    private FirstPersonLook firstPersonLook;

    public bool HasUnsavedChanges { get; private set; } = false;

    public GameRuntimeSettings CurrentSettings => liveSettings;
    public GameRuntimeSettings SavedSettings => savedSettings;
    public GameRuntimeSettings LiveSettings => liveSettings;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        folderPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonDocuments),
            "ForgottenPlant"
        );

        settingsFilePath = Path.Combine(folderPath, "GameSettings.json");
        checksumFilePath = Path.Combine(folderPath, "GameSettings.chk");

        LoadSettings();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(ApplySettingsNextFrame());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ApplySettingsNextFrame());
    }

    private IEnumerator ApplySettingsNextFrame()
    {
        yield return null;

        RefreshSceneReferences();
        ApplySettings(liveSettings);
    }

    private void RefreshSceneReferences()
    {
        musicPlayer = FindFirstObjectByType<AudioMixManager>();
        firstPersonLook = FindFirstObjectByType<FirstPersonLook>();
    }

    public void LoadSettings()
    {
        EnsureFolderExists();

        if (!File.Exists(settingsFilePath) || !File.Exists(checksumFilePath))
        {
            Debug.Log("Settings file or checksum file missing. Creating default settings.");
            CreateDefaultSettingsFiles();
            return;
        }

        string json = File.ReadAllText(settingsFilePath);
        string storedChecksumText = File.ReadAllText(checksumFilePath);

        if (!int.TryParse(storedChecksumText, out int storedChecksum))
        {
            Debug.LogWarning("Checksum file invalid. Recreating default settings.");
            CreateDefaultSettingsFiles();
            return;
        }

        int calculatedChecksum = CalculateChecksum(json);

        if (storedChecksum != calculatedChecksum)
        {
            Debug.LogWarning("Checksum mismatch. Recreating default settings.");
            CreateDefaultSettingsFiles();
            return;
        }

        GameRuntimeSettings loadedSettings = JsonUtility.FromJson<GameRuntimeSettings>(json);

        if (loadedSettings == null)
        {
            Debug.LogWarning("GameSettings.json could not be read. Recreating default settings.");
            CreateDefaultSettingsFiles();
            return;
        }

        savedSettings = CopySettings(loadedSettings);

        bool settingsWereCorrected = ValidateSettings(savedSettings);

        if (settingsWereCorrected)
        {
            WriteSettingsToDisk(savedSettings);
        }

        liveSettings = CopySettings(savedSettings);
        HasUnsavedChanges = false;
    }

    public void SaveSettings()
    {
        ValidateSettings(liveSettings);

        savedSettings = CopySettings(liveSettings);
        WriteSettingsToDisk(savedSettings);

        HasUnsavedChanges = false;

        Debug.Log("Game settings saved to: " + settingsFilePath);
    }

    public void RevertLiveSettings()
    {
        liveSettings = CopySettings(savedSettings);
        HasUnsavedChanges = false;
        ApplySettings(liveSettings);
    }

    public void ApplyLiveSettings()
    {
        ValidateSettings(liveSettings);
        ApplySettings(liveSettings);
        UpdateUnsavedChangesState();
    }

    public void MarkAsChanged()
    {
        UpdateUnsavedChangesState();
    }

    private void CreateDefaultSettingsFiles()
    {
        savedSettings = CreateSettingsFromDefaults();
        ValidateSettings(savedSettings);

        WriteSettingsToDisk(savedSettings);

        liveSettings = CopySettings(savedSettings);
        HasUnsavedChanges = false;
    }

    private GameRuntimeSettings CreateSettingsFromDefaults()
    {
        return new GameRuntimeSettings
        {
            resolutionIndex = defaultSettings.resolutionIndex,
            fullscreen = defaultSettings.fullscreen,

            masterVolume = defaultSettings.masterVolume,
            sfxVolume = defaultSettings.sfxVolume,
            speechVolume = defaultSettings.speechVolume,
            musicVolume = defaultSettings.musicVolume,

            mouseSensitivity = defaultSettings.mouseSensitivity,
            invertY = defaultSettings.invertY,

            difficultyIndex = defaultSettings.difficultyIndex,
            trackIndex = defaultSettings.trackIndex
        };
    }

    private void WriteSettingsToDisk(GameRuntimeSettings settings)
    {
        EnsureFolderExists();

        string json = JsonUtility.ToJson(settings, true);
        int checksum = CalculateChecksum(json);

        File.WriteAllText(settingsFilePath, json);
        File.WriteAllText(checksumFilePath, checksum.ToString());
    }

    private void EnsureFolderExists()
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    private int CalculateChecksum(string text)
    {
        int checksum = 0;

        foreach (char c in text)
        {
            checksum += c;
        }

        return checksum;
    }

    private bool ValidateSettings(GameRuntimeSettings settings)
    {
        bool changed = false;

        int validatedResolutionIndex = Mathf.Clamp(settings.resolutionIndex, 0, 3);
        if (validatedResolutionIndex != settings.resolutionIndex)
        {
            settings.resolutionIndex = validatedResolutionIndex;
            changed = true;
        }

        float validatedMasterVolume = Mathf.Clamp(settings.masterVolume, 0f, 1f);
        if (!Mathf.Approximately(validatedMasterVolume, settings.masterVolume))
        {
            settings.masterVolume = validatedMasterVolume;
            changed = true;
        }

        float validatedSfxVolume = Mathf.Clamp(settings.sfxVolume, 0f, 1f);
        if (!Mathf.Approximately(validatedSfxVolume, settings.sfxVolume))
        {
            settings.sfxVolume = validatedSfxVolume;
            changed = true;
        }

        float validatedSpeechVolume = Mathf.Clamp(settings.speechVolume, 0f, 1f);
        if (!Mathf.Approximately(validatedSpeechVolume, settings.speechVolume))
        {
            settings.speechVolume = validatedSpeechVolume;
            changed = true;
        }

        float validatedMusicVolume = Mathf.Clamp(settings.musicVolume, 0f, 1f);
        if (!Mathf.Approximately(validatedMusicVolume, settings.musicVolume))
        {
            settings.musicVolume = validatedMusicVolume;
            changed = true;
        }

        float validatedMouseSensitivity = Mathf.Clamp(settings.mouseSensitivity, -0.5f, 0.5f);
        if (!Mathf.Approximately(validatedMouseSensitivity, settings.mouseSensitivity))
        {
            settings.mouseSensitivity = validatedMouseSensitivity;
            changed = true;
        }

        int validatedDifficultyIndex = Mathf.Clamp(settings.difficultyIndex, 0, 2);
        if (validatedDifficultyIndex != settings.difficultyIndex)
        {
            settings.difficultyIndex = validatedDifficultyIndex;
            changed = true;
        }

        int validatedTrackIndex = Mathf.Clamp(settings.trackIndex, 0, 3);
        if (validatedTrackIndex != settings.trackIndex)
        {
            settings.trackIndex = validatedTrackIndex;
            changed = true;
        }

        return changed;
    }

    private void ApplySettings(GameRuntimeSettings settings)
    {
        ApplyResolution(settings);
        ApplyMusic(settings);
        ApplyMouse(settings);
        ApplyDifficulty(settings);
    }

    private void ApplyResolution(GameRuntimeSettings settings)
    {
        FullScreenMode screenMode = settings.fullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        switch (settings.resolutionIndex)
        {
            case 0:
                Screen.SetResolution(1920, 1080, screenMode);
                break;

            case 1:
                Screen.SetResolution(2560, 1080, screenMode);
                break;

            case 2:
                Screen.SetResolution(2560, 1440, screenMode);
                break;

            case 3:
                Screen.SetResolution(3440, 1440, screenMode);
                break;
        }
    }

    private void ApplyMusic(GameRuntimeSettings settings)
    {
        if (musicPlayer == null)
            musicPlayer = FindFirstObjectByType<AudioMixManager>();

        if (musicPlayer != null)
            musicPlayer.ApplySettings(settings);
    }

    private void ApplyMouse(GameRuntimeSettings settings)
    {
        if (firstPersonLook == null)
            firstPersonLook = FindFirstObjectByType<FirstPersonLook>();

        if (firstPersonLook != null)
        {
            firstPersonLook.ApplyMouseSettings(
                settings.mouseSensitivity,
                settings.invertY
            );
        }
    }

    private void ApplyDifficulty(GameRuntimeSettings settings)
    {
        Debug.Log("ApplyDifficulty → Level: " + settings.difficultyIndex);
    }

    private void UpdateUnsavedChangesState()
    {
        HasUnsavedChanges = !AreSettingsEqual(liveSettings, savedSettings);
    }

    private bool AreSettingsEqual(GameRuntimeSettings a, GameRuntimeSettings b)
    {
        if (a == null || b == null)
            return false;

        return
            a.resolutionIndex == b.resolutionIndex &&
            a.fullscreen == b.fullscreen &&
            Mathf.Approximately(a.masterVolume, b.masterVolume) &&
            Mathf.Approximately(a.sfxVolume, b.sfxVolume) &&
            Mathf.Approximately(a.speechVolume, b.speechVolume) &&
            Mathf.Approximately(a.musicVolume, b.musicVolume) &&
            Mathf.Approximately(a.mouseSensitivity, b.mouseSensitivity) &&
            a.invertY == b.invertY &&
            a.difficultyIndex == b.difficultyIndex &&
            a.trackIndex == b.trackIndex;
    }

    private GameRuntimeSettings CopySettings(GameRuntimeSettings source)
    {
        return new GameRuntimeSettings
        {
            resolutionIndex = source.resolutionIndex,
            fullscreen = source.fullscreen,

            masterVolume = source.masterVolume,
            sfxVolume = source.sfxVolume,
            speechVolume = source.speechVolume,
            musicVolume = source.musicVolume,

            mouseSensitivity = source.mouseSensitivity,
            invertY = source.invertY,

            difficultyIndex = source.difficultyIndex,
            trackIndex = source.trackIndex
        };
    }
}