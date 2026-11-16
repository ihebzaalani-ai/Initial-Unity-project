using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the main menu UI buttons: Play, Options panel toggle, and Quit.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Scene Loading")]
    [Tooltip("The name of the gameplay scene to load when pressing Play.")]
    public string gameSceneName = "Game";

    [Header("Panels")]
    [Tooltip("The options popup panel to show or hide.")]
    public GameObject optionsPanel;

    private void Start()
    {
        // Ensure options panel starts closed
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Loads the main gameplay scene.
    /// </summary>
    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Shows the options menu panel.
    /// </summary>
    public void OpenOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the options menu panel.
    /// </summary>
    public void CloseOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Quits the standalone game application.
    /// </summary>
    public void Quit()
    {
        Debug.Log("Quit requested.");
        Application.Quit();
    }
}
