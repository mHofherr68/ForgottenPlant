/*using System.IO;
using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    [Header("Default Settings")]
    [SerializeField] private GameDefaultSettings defaultSettings;

    [Header("Runtime Settings")]
    [SerializeField] private GameRuntimeSettings currentSettings = new GameRuntimeSettings();

    private string folderPath;
    private string settingsFilePath;
    private string checksumFilePath;

    public bool HasUnsavedChanges { get; private set; } = false;

    public GameRuntimeSettings CurrentSettings => currentSettings;

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

        currentSettings = JsonUtility.FromJson<GameRuntimeSettings>(json);

        if (currentSettings == null)
        {
            Debug.LogWarning("GameSettings.json could not be read. Recreating default settings.");
            CreateDefaultSettingsFiles();
            return;
        }

        bool settingsWereCorrected = ValidateSettings();

        if (settingsWereCorrected)
        {
            SaveSettings();
        }
        else
        {
            HasUnsavedChanges = false;
        }
    }

    public void SaveSettings()
    {
        EnsureFolderExists();

        string json = JsonUtility.ToJson(currentSettings, true);
        int checksum = CalculateChecksum(json);

        File.WriteAllText(settingsFilePath, json);
        File.WriteAllText(checksumFilePath, checksum.ToString());

        HasUnsavedChanges = false;

        Debug.Log("Game settings saved to: " + settingsFilePath);
    }

    private void CreateDefaultSettingsFiles()
    {
        currentSettings = new GameRuntimeSettings
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

        SaveSettings();
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

    private bool ValidateSettings()
    {
        bool changed = false;

        int validatedResolutionIndex = Mathf.Clamp(currentSettings.resolutionIndex, 0, 3);
        if (validatedResolutionIndex != currentSettings.resolutionIndex)
        {
            currentSettings.resolutionIndex = validatedResolutionIndex;
            changed = true;
        }

        float validatedMasterVolume = Mathf.Clamp(currentSettings.masterVolume, 0f, 1f);
        if (!Mathf.Approximately(validatedMasterVolume, currentSettings.masterVolume))
        {
            currentSettings.masterVolume = validatedMasterVolume;
            changed = true;
        }

        float validatedSfxVolume = Mathf.Clamp(currentSettings.sfxVolume, 0f, 1f);
        if (!Mathf.Approximately(validatedSfxVolume, currentSettings.sfxVolume))
        {
            currentSettings.sfxVolume = validatedSfxVolume;
            changed = true;
        }

        float validatedSpeechVolume = Mathf.Clamp(currentSettings.speechVolume, 0f, 1f);
        if (!Mathf.Approximately(validatedSpeechVolume, currentSettings.speechVolume))
        {
            currentSettings.speechVolume = validatedSpeechVolume;
            changed = true;
        }

        float validatedMusicVolume = Mathf.Clamp(currentSettings.musicVolume, 0f, 1f);
        if (!Mathf.Approximately(validatedMusicVolume, currentSettings.musicVolume))
        {
            currentSettings.musicVolume = validatedMusicVolume;
            changed = true;
        }

        float validatedMouseSensitivity = Mathf.Clamp(currentSettings.mouseSensitivity, -0.5f, 0.5f);
        if (!Mathf.Approximately(validatedMouseSensitivity, currentSettings.mouseSensitivity))
        {
            currentSettings.mouseSensitivity = validatedMouseSensitivity;
            changed = true;
        }

        int validatedDifficultyIndex = Mathf.Clamp(currentSettings.difficultyIndex, 0, 2);
        if (validatedDifficultyIndex != currentSettings.difficultyIndex)
        {
            currentSettings.difficultyIndex = validatedDifficultyIndex;
            changed = true;
        }

        int validatedTrackIndex = Mathf.Clamp(currentSettings.trackIndex, 0, 3);
        if (validatedTrackIndex != currentSettings.trackIndex)
        {
            currentSettings.trackIndex = validatedTrackIndex;
            changed = true;
        }

        return changed;
    }

    public void MarkAsChanged()
    {
        HasUnsavedChanges = true;
    }
}*/
using System.IO;
using UnityEngine;

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

    public bool HasUnsavedChanges { get; private set; } = false;

    // Für bestehenden Code: CurrentSettings = LiveSettings
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

        ApplySettings(liveSettings);
    }

    public void SaveSettings()
    {
        // Live wird vor dem Speichern validiert
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

        ApplySettings(liveSettings);
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

        // Später ergänzen:
        // ApplyAudio(settings);
        // ApplyMouse(settings);
        // ApplyDifficulty(settings);
        // ApplyMusicTrack(settings);
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
