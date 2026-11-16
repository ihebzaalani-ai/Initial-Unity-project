using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Singleton game manager tracking level progress, enemy counts, victory/game-over states,
/// and menu/retry scene transitions.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Text displaying defeated enemies progress, e.g. 'Enemies: 0/5'.")]
    [SerializeField] private TextMeshProUGUI enemiesText;

    [Tooltip("Optional text displaying collected wax shards.")]
    [SerializeField] private TextMeshProUGUI waxText;

    [Tooltip("Panel activated upon reaching the brazier to win the game.")]
    public GameObject winPanel;

    [Tooltip("Panel activated when player wax reaches 0.")]
    public GameObject gameOverPanel;

    [Header("Menu Configuration")]
    [Tooltip("Name of the main menu scene.")]
    public string menuSceneName = "Menu";

    [Header("Counters")]
    public int enemiesDefeated = 0;
    public int totalEnemies = 0;
    public int waxCollected = 0;

    private void Awake()
    {
        // Singleton pattern enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Ensure time is running normally on start
        Time.timeScale = 1f;

        // Count all enemies currently in the scene at startup
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        totalEnemies = enemies != null ? enemies.Length : 0;

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateUI();
    }

    /// <summary>
    /// Called by EnemyAI when an enemy is killed.
    /// </summary>
    public void EnemyDefeated()
    {
        enemiesDefeated++;
        UpdateUI();
    }

    /// <summary>
    /// Called by WaxPickup when a shard is collected.
    /// </summary>
    public void AddWaxCollected()
    {
        waxCollected++;
        UpdateUI();
    }

    /// <summary>
    /// Activates the victory panel when the goal is reached.
    /// </summary>
    public void TriggerWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Activates the game over panel when wax is depleted.
    /// </summary>
    public void TriggerGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Restores timeScale and reloads the current active scene.
    /// </summary>
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Restores timeScale and loads the Main Menu scene.
    /// </summary>
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    /// <summary>
    /// Returns true if all enemies in the level have been defeated.
    /// </summary>
    public bool AreAllEnemiesDefeated()
    {
        return enemiesDefeated >= totalEnemies;
    }

    private void UpdateUI()
    {
        if (enemiesText != null)
        {
            enemiesText.text = $"Enemies: {enemiesDefeated}/{totalEnemies}";
        }

        if (waxText != null)
        {
            waxText.text = $"Wax Shards: {waxCollected}";
        }
    }
}
