using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettingsManager : MonoBehaviour
{
    // Global singleton instance for accessing the settings manager from other scripts.
    public static GameSettingsManager Instance { get; private set; }

    [Header("Default Settings")]
    // Default settings used when no valid saved settings file exists.
    [SerializeField] private GameDefaultSettings defaultSettings;

    [Header("Saved Settings")]
    // Settings currently stored on disk.
    [SerializeField] private GameRuntimeSettings savedSettings = new GameRuntimeSettings();

    [Header("Live Settings")]
    // Settings currently active in memory and used by the game.
    [SerializeField] private GameRuntimeSettings liveSettings = new GameRuntimeSettings();

    // Folder path used to store the settings files.
    private string folderPath;

    // Full path to the JSON settings file.
    private string settingsFilePath;

    // Full path to the checksum file.
    private string checksumFilePath;

    // Cached reference to the audio settings target.
    private AudioMixManager musicPlayer;

    // Cached reference to the mouse/look settings target.
    private FirstPersonLook firstPersonLook;

    // True if live settings differ from the saved settings.
    public bool HasUnsavedChanges { get; private set; } = false;

    // Public read-only access to the currently active settings.
    public GameRuntimeSettings CurrentSettings => liveSettings;

    // Public read-only access to the saved settings.
    public GameRuntimeSettings SavedSettings => savedSettings;

    // Public read-only access to the live settings.
    public GameRuntimeSettings LiveSettings => liveSettings;

    private void Awake()
    {
        // Enforce singleton behavior.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Build the save folder path inside the shared public documents folder.
        folderPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonDocuments),
            "ForgottenPlant"
        );

        // Build the full file paths for the settings and checksum files.
        settingsFilePath = Path.Combine(folderPath, "GameSettings.json");
        checksumFilePath = Path.Combine(folderPath, "GameSettings.chk");

        // Load settings immediately on startup.
        LoadSettings();
    }

    private void OnEnable()
    {
        // Register for scene loaded events so settings can be reapplied after scene changes.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unregister from scene loaded events.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Apply settings one frame later so scene objects are ready.
        StartCoroutine(ApplySettingsNextFrame());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reapply settings after every scene load.
        StartCoroutine(ApplySettingsNextFrame());
    }

    private IEnumerator ApplySettingsNextFrame()
    {
        // Wait one frame so new scene references can be found safely.
        yield return null;

        RefreshSceneReferences();
        ApplySettings(liveSettings);
    }

    private void RefreshSceneReferences()
    {
        // Refresh scene-local references after scene changes.
        musicPlayer = FindFirstObjectByType<AudioMixManager>();
        firstPersonLook = FindFirstObjectByType<FirstPersonLook>();
    }

    public void LoadSettings()
    {
        // Make sure the save folder exists before accessing files.
        EnsureFolderExists();

        // If either file is missing, create fresh default settings.
        if (!File.Exists(settingsFilePath) || !File.Exists(checksumFilePath))
        {
            Debug.Log("Settings file or checksum file missing. Creating default settings.");
            CreateDefaultSettingsFiles();
            return;
        }

        // Read both the JSON content and the stored checksum.
        string json = File.ReadAllText(settingsFilePath);
        string storedChecksumText = File.ReadAllText(checksumFilePath);

        // Validate that the checksum file contains a valid integer.
        if (!int.TryParse(storedChecksumText, out int storedChecksum))
        {
            Debug.LogWarning("Checksum file invalid. Recreating default settings.");
            CreateDefaultSettingsFiles();
            return;
        }

        // Recalculate the checksum from the JSON file content.
        int calculatedChecksum = CalculateChecksum(json);

        // Recreate defaults if checksum validation fails.
        if (storedChecksum != calculatedChecksum)
        {
            Debug.LogWarning("Checksum mismatch. Recreating default settings.");
            CreateDefaultSettingsFiles();
            return;
        }

        // Deserialize the saved settings from JSON.
        GameRuntimeSettings loadedSettings = JsonUtility.FromJson<GameRuntimeSettings>(json);

        // Recreate defaults if the JSON could not be parsed.
        if (loadedSettings == null)
        {
            Debug.LogWarning("GameSettings.json could not be read. Recreating default settings.");
            CreateDefaultSettingsFiles();
            return;
        }

        // Copy loaded settings into the saved settings object.
        savedSettings = CopySettings(loadedSettings);

        // Validate and correct invalid values if necessary.
        bool settingsWereCorrected = ValidateSettings(savedSettings);

        // Write corrected values back to disk if any values had to be fixed.
        if (settingsWereCorrected)
        {
            WriteSettingsToDisk(savedSettings);
        }

        // Use the saved settings as the initial live settings.
        liveSettings = CopySettings(savedSettings);
        HasUnsavedChanges = false;
    }

    public void SaveSettings()
    {
        // Ensure current live settings are valid before writing them to disk.
        ValidateSettings(liveSettings);

        // Save the current live settings as the new saved settings.
        savedSettings = CopySettings(liveSettings);
        WriteSettingsToDisk(savedSettings);

        HasUnsavedChanges = false;

        Debug.Log("Game settings saved to: " + settingsFilePath);
    }

    public void RevertLiveSettings()
    {
        // Restore live settings from the last saved version.
        liveSettings = CopySettings(savedSettings);
        HasUnsavedChanges = false;
        ApplySettings(liveSettings);
    }

    public void ApplyLiveSettings()
    {
        // Validate and apply the current live settings to all connected systems.
        ValidateSettings(liveSettings);
        ApplySettings(liveSettings);
        UpdateUnsavedChangesState();
    }

    public void MarkAsChanged()
    {
        // Recalculate whether unsaved changes are present.
        UpdateUnsavedChangesState();
    }

    private void CreateDefaultSettingsFiles()
    {
        // Build settings from the configured defaults.
        savedSettings = CreateSettingsFromDefaults();
        ValidateSettings(savedSettings);

        // Store default settings on disk immediately.
        WriteSettingsToDisk(savedSettings);

        // Copy defaults into the current live settings.
        liveSettings = CopySettings(savedSettings);
        HasUnsavedChanges = false;
    }

    private GameRuntimeSettings CreateSettingsFromDefaults()
    {
        // Create a runtime settings object from the default settings asset.
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
        // Make sure the save folder exists before writing files.
        EnsureFolderExists();

        // Convert settings to formatted JSON and generate a checksum for integrity validation.
        string json = JsonUtility.ToJson(settings, true);
        int checksum = CalculateChecksum(json);

        File.WriteAllText(settingsFilePath, json);
        File.WriteAllText(checksumFilePath, checksum.ToString());
    }

    private void EnsureFolderExists()
    {
        // Create the settings folder if it does not already exist.
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    private int CalculateChecksum(string text)
    {
        // Simple checksum calculation based on the sum of all character values.
        int checksum = 0;

        foreach (char c in text)
        {
            checksum += c;
        }

        return checksum;
    }

    private bool ValidateSettings(GameRuntimeSettings settings)
    {
        // Clamp all supported settings values to valid ranges.
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
        // Apply all supported settings categories.
        ApplyResolution(settings);
        ApplyMusic(settings);
        ApplyMouse(settings);
        ApplyDifficulty(settings);
    }

    private void ApplyResolution(GameRuntimeSettings settings)
    {
        // Convert the fullscreen bool into the correct Unity fullscreen mode.
        FullScreenMode screenMode = settings.fullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        // Apply the selected resolution preset.
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
        // Refresh the audio manager reference if needed.
        if (musicPlayer == null)
            musicPlayer = FindFirstObjectByType<AudioMixManager>();

        // Forward audio-related settings to the audio system.
        if (musicPlayer != null)
            musicPlayer.ApplySettings(settings);
    }

    private void ApplyMouse(GameRuntimeSettings settings)
    {
        // Refresh the look controller reference if needed.
        if (firstPersonLook == null)
            firstPersonLook = FindFirstObjectByType<FirstPersonLook>();

        // Forward mouse-related settings to the first-person look system.
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
        // Difficulty is currently read by other gameplay systems directly from the settings manager.
        Debug.Log("ApplyDifficulty → Level: " + settings.difficultyIndex);
    }

    private void UpdateUnsavedChangesState()
    {
        // Compare live and saved settings to determine whether unsaved changes exist.
        HasUnsavedChanges = !AreSettingsEqual(liveSettings, savedSettings);
    }

    private bool AreSettingsEqual(GameRuntimeSettings a, GameRuntimeSettings b)
    {
        // Null values are treated as not equal.
        if (a == null || b == null)
            return false;

        // Compare all supported settings fields.
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
        // Create a deep copy so saved and live settings remain independent.
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