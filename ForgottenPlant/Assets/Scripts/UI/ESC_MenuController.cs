using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ESC_MenuController : MonoBehaviour
{
    [Header("Message Panel")]
    // Panel used to show temporary placeholder messages (for example Save / Quit).
    [SerializeField] private GameObject messagePanel;

    // Duration for which the message panel stays visible.
    [SerializeField] private float messageDuration = 2f;

    [Header("Options Panel")]
    // Main options panel inside the ESC menu.
    [SerializeField] private GameObject optionsPanel;

    // Optional panel that can be shown after saving settings.
    [SerializeField] private GameObject onSavePanel;

    [Header("Audio")]
    // Slider for master volume.
    [SerializeField] private Slider masterVolumeSlider;

    // Slider for sound effects volume.
    [SerializeField] private Slider sfxVolumeSlider;

    // Slider for speech / voice volume.
    [SerializeField] private Slider speechVolumeSlider;

    // Slider for music volume.
    [SerializeField] private Slider musicVolumeSlider;

    // Dropdown used to select the music track.
    [SerializeField] private TMP_Dropdown trackDropdown;

    [Header("Mouse")]
    // Slider for mouse sensitivity.
    [SerializeField] private Slider mouseSensitivitySlider;

    // Slider used as an invert Y on/off switch.
    [SerializeField] private Slider invertYSwitch;

    // Prevents UI events from applying settings while values are loaded programmatically.
    private bool isLoadingUI = false;

    private void Start()
    {
        // Hide all optional panels at startup.
        if (messagePanel != null)
            messagePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        // Register UI callbacks and populate the controls with the current settings.
        RegisterUIEvents();
        LoadSettingsToUI();
    }

    public void OpenOptions()
    {
        // Open the options panel.
        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        // Hide the save info panel when entering the options menu.
        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        // Refresh UI values from the current settings.
        LoadSettingsToUI();
    }

    public void CloseOptions()
    {
        // Revert all unsaved live settings when closing the options menu.
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.RevertLiveSettings();
        }

        // Reload reverted values into the UI.
        LoadSettingsToUI();

        // Hide the save info panel.
        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        // Close the options panel.
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void SaveAndCloseOptions()
    {
        // Stop if the settings manager is not available.
        if (GameSettingsManager.Instance == null)
            return;

        // Save the current live settings to disk.
        GameSettingsManager.Instance.SaveSettings();

        // Hide the save info panel.
        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        // Close the options panel.
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void OnSaveGameClicked()
    {
        // Show a temporary message panel when the save game button is pressed.
        StartCoroutine(ShowMessage());
    }

    public void OnQuitGameClicked()
    {
        // Show a temporary message panel when the quit game button is pressed.
        StartCoroutine(ShowMessage());
    }

    public void LoadScene(string sceneName)
    {
        // Cleanly leave the pause state before switching scenes.
        Time.timeScale = 1f;

        // Unlock and show the cursor, important when returning to menu scenes.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load the requested scene.
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private IEnumerator ShowMessage()
    {
        // Stop if no message panel is assigned.
        if (messagePanel == null)
            yield break;

        // Show the panel.
        messagePanel.SetActive(true);

        // Use realtime seconds so the message still works while the game is paused.
        yield return new WaitForSecondsRealtime(messageDuration);

        // Hide the panel again.
        messagePanel.SetActive(false);
    }

    private void RegisterUIEvents()
    {
        // Register a shared callback for all supported option controls.
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(_ => OnOptionChanged());

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(_ => OnOptionChanged());

        if (speechVolumeSlider != null)
            speechVolumeSlider.onValueChanged.AddListener(_ => OnOptionChanged());

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(_ => OnOptionChanged());

        if (trackDropdown != null)
            trackDropdown.onValueChanged.AddListener(_ => OnOptionChanged());

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(_ => OnOptionChanged());

        if (invertYSwitch != null)
            invertYSwitch.onValueChanged.AddListener(_ => OnOptionChanged());
    }

    private void LoadSettingsToUI()
    {
        // Stop if the settings manager instance does not exist.
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogWarning("GameSettingsManager instance not found.");
            return;
        }

        // Mark UI loading so callbacks do not instantly write values back.
        isLoadingUI = true;

        GameRuntimeSettings settings = GameSettingsManager.Instance.CurrentSettings;

        // Populate the UI controls with the current settings values.
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = settings.masterVolume;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = settings.sfxVolume;

        if (speechVolumeSlider != null)
            speechVolumeSlider.value = settings.speechVolume;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = settings.musicVolume;

        if (trackDropdown != null)
            trackDropdown.value = settings.trackIndex;

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = settings.mouseSensitivity + 0.5f;

        if (invertYSwitch != null)
            invertYSwitch.value = settings.invertY ? 1f : 0f;

        // Re-enable UI change handling.
        isLoadingUI = false;
    }

    private void OnOptionChanged()
    {
        // Ignore value changes while the UI is being filled programmatically.
        if (isLoadingUI)
            return;

        if (GameSettingsManager.Instance == null)
            return;

        GameRuntimeSettings settings = GameSettingsManager.Instance.LiveSettings;

        // Read current UI values into the live settings object.
        if (masterVolumeSlider != null)
            settings.masterVolume = masterVolumeSlider.value;

        if (sfxVolumeSlider != null)
            settings.sfxVolume = sfxVolumeSlider.value;

        if (speechVolumeSlider != null)
            settings.speechVolume = speechVolumeSlider.value;

        if (musicVolumeSlider != null)
            settings.musicVolume = musicVolumeSlider.value;

        if (trackDropdown != null)
            settings.trackIndex = trackDropdown.value;

        if (mouseSensitivitySlider != null)
            settings.mouseSensitivity = mouseSensitivitySlider.value - 0.5f;

        if (invertYSwitch != null)
            settings.invertY = invertYSwitch.value > 0.5f;

        // Apply the modified live settings immediately.
        GameSettingsManager.Instance.ApplyLiveSettings();

        // Hide the save feedback panel until settings are explicitly saved.
        if (onSavePanel != null)
            onSavePanel.SetActive(false);
    }
}