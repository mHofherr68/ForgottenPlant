using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ESC_MenuController : MonoBehaviour
{
    [Header("Message Panel")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private float messageDuration = 2f;

    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject onSavePanel;

    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider speechVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TMP_Dropdown trackDropdown;

    [Header("Mouse")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider invertYSwitch;

    private bool isLoadingUI = false;

    private void Start()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        RegisterUIEvents();
        LoadSettingsToUI();
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        LoadSettingsToUI();
    }

    public void CloseOptions()
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.RevertLiveSettings();
        }

        LoadSettingsToUI();

        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void SaveAndCloseOptions()
    {
        if (GameSettingsManager.Instance == null)
            return;

        GameSettingsManager.Instance.SaveSettings();

        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void OnSaveGameClicked()
    {
        StartCoroutine(ShowMessage());
    }

    public void OnQuitGameClicked()
    {
        StartCoroutine(ShowMessage());
    }

    public void LoadScene(string sceneName)
    {
        // Pause sauber beenden
        Time.timeScale = 1f;

        // Cursor wieder freigeben (wichtig fürs MainMenu)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Szene laden
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private IEnumerator ShowMessage()
    {
        if (messagePanel == null)
            yield break;

        messagePanel.SetActive(true);

        yield return new WaitForSecondsRealtime(messageDuration);

        messagePanel.SetActive(false);
    }

    private void RegisterUIEvents()
    {
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
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogWarning("GameSettingsManager instance not found.");
            return;
        }

        isLoadingUI = true;

        GameRuntimeSettings settings = GameSettingsManager.Instance.CurrentSettings;

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

        isLoadingUI = false;
    }

    private void OnOptionChanged()
    {
        if (isLoadingUI)
            return;

        if (GameSettingsManager.Instance == null)
            return;

        GameRuntimeSettings settings = GameSettingsManager.Instance.LiveSettings;

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

        GameSettingsManager.Instance.ApplyLiveSettings();

        if (onSavePanel != null)
            onSavePanel.SetActive(false);
    }
}
