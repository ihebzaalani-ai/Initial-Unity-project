using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the UI buttons on the Ending scene:
/// PlayAgain (reloads "Game") and GoToMenu (loads "Menu"),
/// and automatically formats the ending background image to full screen.
/// </summary>
public class EndingMenu : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("Name of the gameplay scene to load when playing again.")]
    public string gameSceneName = "Game";

    [Tooltip("Name of the main menu scene to load.")]
    public string menuSceneName = "Menu";

    [Header("Full Screen Image Settings")]
    [Tooltip("The Image RectTransform to make full screen. Auto-detected in the scene if left empty.")]
    public RectTransform endingImage;

    [Tooltip("If true, automatically stretches endingImage to full screen on Start.")]
    public bool autoFullScreen = true;

    [Tooltip("Preserve aspect ratio so the ending artwork is never stretched or squished.")]
    public bool preserveAspect = true;

    private void Awake()
    {
        if (autoFullScreen)
        {
            ApplyFullScreen();
        }
    }

    private void Start()
    {
        // Ensure time is running normally when entering the Ending scene
        Time.timeScale = 1f;

        if (autoFullScreen)
        {
            ApplyFullScreen();
        }
    }

    /// <summary>
    /// Stretches the ending image RectTransform to fill the full screen and resets any distorted scale.
    /// </summary>
    public void ApplyFullScreen()
    {
        if (endingImage == null)
        {
            Image img = FindFirstObjectByType<Image>();
            if (img != null)
            {
                endingImage = img.rectTransform;
            }
        }

        if (endingImage == null) return;

        // 1. Reset local scale to (1, 1, 1) to remove distortion
        endingImage.localScale = Vector3.one;
        endingImage.localRotation = Quaternion.identity;

        // 2. Set anchors to stretch across the full parent Canvas (0,0 to 1,1)
        endingImage.anchorMin = Vector2.zero;
        endingImage.anchorMax = Vector2.one;

        // 3. Center pivot and zero out all pixel offsets
        endingImage.pivot = new Vector2(0.5f, 0.5f);
        endingImage.anchoredPosition = Vector2.zero;
        endingImage.sizeDelta = Vector2.zero;
        endingImage.offsetMin = Vector2.zero;
        endingImage.offsetMax = Vector2.zero;

        // 4. Configure Image component aspect ratio
        Image imageComp = endingImage.GetComponent<Image>();
        if (imageComp != null)
        {
            imageComp.preserveAspect = preserveAspect;
        }

        // 5. Disable AspectRatioFitter if it was left on None
        AspectRatioFitter fitter = endingImage.GetComponent<AspectRatioFitter>();
        if (fitter != null && fitter.aspectMode == AspectRatioFitter.AspectMode.None)
        {
            fitter.enabled = false;
        }
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (autoFullScreen && endingImage != null)
        {
            ApplyFullScreen();
        }
    }
#endif
}
