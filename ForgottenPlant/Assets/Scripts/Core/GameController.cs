using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Input actions instance used for pause input handling.
    private InputSystem_Actions controls;

    [Header("Pause UI")]
    // Reference to the main ESC pause menu panel.
    [SerializeField] private GameObject escMenuPanel;

    // Reference to the mini options panel inside the pause menu.
    [SerializeField] private GameObject miniOptionsPanel;

    [Header("Game Over")]
    // Name of the scene that will be loaded after the player dies.
    [SerializeField] private string gameOverSceneName = "GameOverScene";

    // Tracks whether the game is currently paused.
    private bool isPaused = false;

    // Prevents pause input or repeated loading while the game over scene is already loading.
    private bool isLoadingGameOver = false;

    private void Awake()
    {
        // Create a new input actions instance.
        controls = new InputSystem_Actions();

        // Register pause input callback.
        controls.Player.Pause.performed += ctx => TogglePause();
    }

    private void Start()
    {
        // Make sure the pause menu is hidden when the scene starts.
        if (escMenuPanel != null)
            escMenuPanel.SetActive(false);

        // Make sure the mini options panel is also hidden at startup.
        if (miniOptionsPanel != null)
            miniOptionsPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // Enable the input action map when this component becomes active.
        controls.Enable();
    }

    private void OnDisable()
    {
        // Disable the input action map when this component becomes inactive.
        controls.Disable();
    }

    private void TogglePause()
    {
        // Ignore pause input while the game over scene is loading.
        if (isLoadingGameOver)
            return;

        // Toggle between pause and resume depending on the current state.
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
        // Mark the game as paused.
        isPaused = true;

        // Freeze the game.
        Time.timeScale = 0f;

        // Unlock and show the cursor for menu interaction.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show the main pause menu panel.
        if (escMenuPanel != null)
            escMenuPanel.SetActive(true);

        // Ensure the mini options panel is hidden when opening pause menu.
        if (miniOptionsPanel != null)
            miniOptionsPanel.SetActive(false);

        Debug.Log("Game Paused");
    }

    private void ResumeGame()
    {
        // Revert unsaved live settings before returning to gameplay.
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.RevertLiveSettings();
        }

        // Mark the game as no longer paused.
        isPaused = false;

        // Resume normal game time.
        Time.timeScale = 1f;

        // Lock and hide the cursor for gameplay.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hide the mini options panel.
        if (miniOptionsPanel != null)
            miniOptionsPanel.SetActive(false);

        // Hide the main pause menu panel.
        if (escMenuPanel != null)
            escMenuPanel.SetActive(false);

        Debug.Log("Game Resumed");
    }

    public void ResumeGameFromButton()
    {
        // Called by a UI button to resume the game.
        ResumeGame();
    }

    public void OnPlayerDeath()
    {
        // Prevent multiple game over scene load requests.
        if (isLoadingGameOver)
            return;

        // Mark game over loading as active.
        isLoadingGameOver = true;

        // Ensure paused state is cleared.
        isPaused = false;

        // Reset time scale so the next scene loads normally.
        Time.timeScale = 1f;

        // Unlock and show cursor for the game over scene UI.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hide mini options panel if it is open.
        if (miniOptionsPanel != null)
            miniOptionsPanel.SetActive(false);

        // Hide pause menu if it is open.
        if (escMenuPanel != null)
            escMenuPanel.SetActive(false);

        // Load the configured game over scene asynchronously.
        SceneManager.LoadSceneAsync(gameOverSceneName);
    }
}