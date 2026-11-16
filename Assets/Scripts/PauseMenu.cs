using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the pause menu. Pressing Escape toggles the pause panel and pauses/resumes time.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("The panel GameObject displayed when the game is paused.")]
    public GameObject pausePanel;

    [Header("Menu Scene")]
    [Tooltip("The name of the main menu scene to load.")]
    public string menuSceneName = "Menu";

    private bool isPaused = false;

    private void Start()
    {
        // Ensure pause panel starts hidden and time runs normally
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Toggle pause on Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    /// <summary>
    /// Pauses the game and shows the pause panel.
    /// </summary>
    public void Pause()
    {
        isPaused = true;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Resumes the game and hides the pause panel.
    /// </summary>
    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Restores timeScale and returns to the main menu.
    /// </summary>
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
