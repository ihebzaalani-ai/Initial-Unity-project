using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the UI buttons on the Ending scene:
/// PlayAgain (reloads "Game") and GoToMenu (loads "Menu").
/// </summary>
public class EndingMenu : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("Name of the gameplay scene to load when playing again.")]
    public string gameSceneName = "Game";

    [Tooltip("Name of the main menu scene to load.")]
    public string menuSceneName = "Menu";

    private void Start()
    {
        // Ensure time is running normally when entering the Ending scene
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Loads the main game scene to play again.
    /// </summary>
    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
