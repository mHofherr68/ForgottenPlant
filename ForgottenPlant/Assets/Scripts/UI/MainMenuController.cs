using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject loadMessagePanel;
    [SerializeField] private float messageDuration = 2f;

    [Header("Options Menu")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject onSavePanel;

    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Slider fullscreenSwitch;

    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider speechVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    [Header("Mouse")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider invertYSwitch;

    [Header("Misc")]
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private TMP_Dropdown trackDropdown;

    private bool isLoadingUI = false;

    private void Start()
    {
        if (loadMessagePanel != null)
            loadMessagePanel.gameObject.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (onSavePanel != null)
            onSavePanel.SetActive(false);

        RegisterUIEvents();
        LoadSettingsToUI();
    }

    public void OnLoadGameClicked()
    {
        StartCoroutine(ShowLoadMessage());
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

    private IEnumerator ShowLoadMessage()
    {
        if (loadMessagePanel == null)
            yield break;

        loadMessagePanel.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        loadMessagePanel.gameObject.SetActive(false);
    }

    public void LoadSandbox()
    {
        StartCoroutine(LoadSceneAsync("SC_Sandbox"));
    }

    private void RegisterUIEvents()
    {
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
        if (GameSettingsManager.Instance == null)
        {
            Debug.LogWarning("GameSettingsManager instance not found.");
            return;
        }

        isLoadingUI = true;

        GameRuntimeSettings settings = GameSettingsManager.Instance.CurrentSettings;

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

        isLoadingUI = false;
    }

    private void OnOptionChanged()
    {
        if (isLoadingUI)
            return;

        if (GameSettingsManager.Instance == null)
            return;

        GameRuntimeSettings settings = GameSettingsManager.Instance.LiveSettings;

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

        GameSettingsManager.Instance.ApplyLiveSettings();

        if (onSavePanel != null)
            onSavePanel.SetActive(false);
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError("Scene could not be loaded.");
            yield break;
        }

        while (!operation.isDone)
            yield return null;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}