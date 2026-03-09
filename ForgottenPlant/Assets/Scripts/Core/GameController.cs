using UnityEngine;

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
}