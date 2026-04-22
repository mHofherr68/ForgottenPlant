using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    // Panel shown briefly when the load game action is triggered.
    [SerializeField] private GameObject loadMessagePanel;

    // Time the load message panel stays visible.
    [SerializeField] private float messageDuration = 2f;

    [Header("Options Menu")]
    // Main options menu panel.
    [SerializeField] private GameObject optionsPanel;

    // Panel that can be shown after saving settings.
    [SerializeField] private GameObject onSavePanel;

    [Header("Graphics")]
    // Dropdown used to select the screen resolution preset.
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    // Slider used as a fullscreen on/off switch.
    [SerializeField] private Slider fullscreenSwitch;

    [Header("Audio")]
    // Slider for master volume.
    [SerializeField] private Slider masterVolumeSlider;

    // Slider for sound effects volume.
    [SerializeField] private Slider sfxVolumeSlider;

    // Slider for speech / voice volume.
    [SerializeField] private Slider speechVolumeSlider;

    // Slider for music volume.
    [SerializeField] private Slider musicVolumeSlider;

    [Header("Mouse")]
    // Slider for mouse sensitivity.
    [SerializeField] private Slider mouseSensitivitySlider;

    // Slider used as an invert Y on/off switch.
    [SerializeField] private Slider invertYSwitch;

    [Header("Misc")]
    // Dropdown used to select the difficulty level.
    [SerializeField] private TMP_Dropdown difficultyDropdown;

    // Dropdown used to select the music track.
    [SerializeField] private TMP_Dropdown trackDropdown;

    // Prevents option change events from being processed while UI values are being loaded.
    private bool isLoadingUI = false;

    private void Start()
    {
        // Hide all optional panels at startup.
        if (loadMessagePanel != null)
            loadMessagePanel.gameObject.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        // Register UI callbacks and populate UI with the current settings.
        RegisterUIEvents();
        LoadSettingsToUI();
    }

    public void OnLoadGameClicked()
    {
        // Show the temporary load message panel.
        StartCoroutine(ShowLoadMessage());
    }

    public void OpenOptions()
    {
        // Open the options panel.
        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        // Hide the save info panel when entering options.
        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        // Refresh all UI values from the current settings.
        LoadSettingsToUI();
    }

    public void CloseOptions()
    {
        // Revert all unsaved live settings before closing the options panel.
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.RevertLiveSettings();
        }

        // Reload reverted values into the UI.
        LoadSettingsToUI();

        // Hide additional save feedback panel.
        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        // Close the options panel.
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void SaveAndCloseOptions()
    {
        // Stop if no settings manager is available.
        if (GameSettingsManager.Instance == null)
            return;

        // Save current live settings to disk.
        GameSettingsManager.Instance.SaveSettings();

        // Hide save feedback panel.
        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        // Close the options panel.
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    private IEnumerator ShowLoadMessage()
    {
        // Stop if no load message panel is assigned.
        if (loadMessagePanel == null)
            yield break;

        // Show the message panel.
        loadMessagePanel.gameObject.SetActive(true);

        // Keep it visible for the configured duration.
        yield return new WaitForSeconds(messageDuration);

        // Hide it again afterward.
        loadMessagePanel.gameObject.SetActive(false);
    }

    public void LoadLevel(string sceneName)
    {
        // Start asynchronous scene loading for the selected scene.
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private void RegisterUIEvents()
    {
        // Register a common callback for every settings-related UI control.
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(_ => OnOptionChanged());

        if (fullscreenSwitch != null)
            fullscreenSwitch.onValueChanged.AddListener(_ => OnOptionChanged());

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(_ => OnOptionChanged());

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(_ => OnOptionChanged());

        if (speechVolumeSlider != null)
            speechVolumeSlider.onValueChanged.AddListener(_ => OnOptionChanged());

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(_ => OnOptionChanged());

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(_ => OnOptionChanged());

        if (invertYSwitch != null)
            invertYSwitch.onValueChanged.AddListener(_ => OnOptionChanged());

        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(_ => OnOptionChanged());

        if (trackDropdown != null)
            trackDropdown.onValueChanged.AddListener(_ => OnOptionChanged());
    }

    private void LoadSettingsToUI()
    {
        // Stop if the settings manager does not exist.
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogWarning("GameSettingsManager instance not found.");
            return;
        }

        // Mark the UI as currently loading so change callbacks do not apply settings again.
        isLoadingUI = true;

        GameRuntimeSettings settings = GameSettingsManager.Instance.CurrentSettings;

        // Populate all UI controls with the currently active settings values.
        if (resolutionDropdown != null)
            resolutionDropdown.value = settings.resolutionIndex;

        if (fullscreenSwitch != null)
            fullscreenSwitch.value = settings.fullscreen ? 1f : 0f;

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = settings.masterVolume;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = settings.sfxVolume;

        if (speechVolumeSlider != null)
            speechVolumeSlider.value = settings.speechVolume;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = settings.musicVolume;

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = settings.mouseSensitivity + 0.5f;

        if (invertYSwitch != null)
            invertYSwitch.value = settings.invertY ? 1f : 0f;

        if (difficultyDropdown != null)
            difficultyDropdown.value = settings.difficultyIndex;

        if (trackDropdown != null)
            trackDropdown.value = settings.trackIndex;

        // Re-enable settings change handling.
        isLoadingUI = false;
    }

    private void OnOptionChanged()
    {
        // Ignore change events while the UI is being populated programmatically.
        if (isLoadingUI)
            return;

        if (GameSettingsManager.Instance == null)
            return;

        GameRuntimeSettings settings = GameSettingsManager.Instance.LiveSettings;

        // Read all current UI values back into the live settings object.
        if (resolutionDropdown != null)
            settings.resolutionIndex = resolutionDropdown.value;

        if (fullscreenSwitch != null)
            settings.fullscreen = fullscreenSwitch.value > 0.5f;

        if (masterVolumeSlider != null)
            settings.masterVolume = masterVolumeSlider.value;

        if (sfxVolumeSlider != null)
            settings.sfxVolume = sfxVolumeSlider.value;

        if (speechVolumeSlider != null)
            settings.speechVolume = speechVolumeSlider.value;

        if (musicVolumeSlider != null)
            settings.musicVolume = musicVolumeSlider.value;

        if (mouseSensitivitySlider != null)
            settings.mouseSensitivity = mouseSensitivitySlider.value - 0.5f;

        if (invertYSwitch != null)
            settings.invertY = invertYSwitch.value > 0.5f;

        if (difficultyDropdown != null)
            settings.difficultyIndex = difficultyDropdown.value;

        if (trackDropdown != null)
            settings.trackIndex = trackDropdown.value;

        // Apply live settings immediately.
        GameSettingsManager.Instance.ApplyLiveSettings();

        // Hide the save feedback panel until settings are explicitly saved again.
        if (onSavePanel != null)
            onSavePanel.SetActive(false);
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Start loading the requested scene asynchronously.
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError("Scene could not be loaded.");
            yield break;
        }

        // Wait until loading is fully complete.
        while (!operation.isDone)
            yield return null;
    }

    public void QuitGame()
    {
        // Stop play mode in the Unity Editor or quit the application in a build.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}