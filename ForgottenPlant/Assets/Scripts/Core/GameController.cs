/*using UnityEngine;

public class GameController : MonoBehaviour
{
    private InputSystem_Actions controls;

    private bool isPaused = false;

    private void Awake()
    {
        controls = new InputSystem_Actions();

        controls.Player.Pause.performed += ctx => TogglePause();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game Paused");
    }

    private void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Game Resumed");
    }
}*/
using UnityEngine;

public class GameController : MonoBehaviour
{
    private InputSystem_Actions controls;

    [Header("Pause UI")]
    [SerializeField] private GameObject escMenuPanel;
    [SerializeField] private GameObject miniOptionsPanel;

    private bool isPaused = false;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Player.Pause.performed += ctx => TogglePause();
    }

    private void Start()
    {
        if (escMenuPanel != null)
            escMenuPanel.SetActive(false);

        if (miniOptionsPanel != null)
            miniOptionsPanel.SetActive(false);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (escMenuPanel != null)
            escMenuPanel.SetActive(true);

        if (miniOptionsPanel != null)
            miniOptionsPanel.SetActive(false);

        Debug.Log("Game Paused");
    }

    private void ResumeGame()
    {
        // Ungespeicherte Änderungen verwerfen
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.RevertLiveSettings();
        }

        isPaused = false;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (miniOptionsPanel != null)
            miniOptionsPanel.SetActive(false);

        if (escMenuPanel != null)
            escMenuPanel.SetActive(false);

        Debug.Log("Game Resumed");
    }

    public void ResumeGameFromButton()
    {
        ResumeGame();
    }
}