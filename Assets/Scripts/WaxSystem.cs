using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages player wax, which acts as both health and light source.
/// Drains continuously over time, triggers red hit flashes and camera shake on damage,
/// and displays the GameOver panel when wax reaches 0.
/// </summary>
public class WaxSystem : MonoBehaviour
{
    [Header("Wax Settings")]
    [Tooltip("Maximum wax capacity.")]
    public float maxWax = 100f;

    [Tooltip("Passive wax loss per second over time.")]
    public float drainPerSecond = 3.0f;

    [Header("Current Status")]
    [SerializeField] private float currentWax;

    public float CurrentWax => currentWax;
    public float MaxWax => maxWax;

    // Event fired whenever wax changes: (currentWax, maxWax)
    public event Action<float, float> OnWaxChanged;

    private Vector3 checkpointPosition;
    private bool hasCheckpoint = false;
    private Rigidbody2D rb;
    private HitFlash hitFlash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitFlash = GetComponent<HitFlash>();
    }

    private void Start()
    {
        currentWax = maxWax;
        OnWaxChanged?.Invoke(currentWax, maxWax);
    }

    private void Update()
    {
        // Continuously drain wax over time at drainPerSecond independent of combat damage
        if (drainPerSecond > 0f && currentWax > 0f)
        {
            currentWax = Mathf.Max(0f, currentWax - (drainPerSecond * Time.deltaTime));
            OnWaxChanged?.Invoke(currentWax, maxWax);

            if (currentWax <= 0f)
            {
                HandleDepletion();
            }
        }
    }

    /// <summary>
    /// Reduces current wax from enemy damage, flashes the player sprite red, and shakes the camera.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentWax <= 0f) return;

        currentWax = Mathf.Max(0f, currentWax - amount);
        OnWaxChanged?.Invoke(currentWax, maxWax);

        // Flash player sprite red
        if (hitFlash != null)
        {
            hitFlash.Flash(Color.red, 0.15f);
        }

        // Trigger camera shake on damage
        CameraShake.Shake(0.2f, 0.25f);

        if (currentWax <= 0f)
        {
            HandleDepletion();
        }
    }

    /// <summary>
    /// Restores wax and clamps the result to maxWax.
    /// </summary>
    public void Restore(float amount)
    {
        if (amount <= 0f) return;

        currentWax = Mathf.Clamp(currentWax + amount, 0f, maxWax);
        OnWaxChanged?.Invoke(currentWax, maxWax);
    }

    /// <summary>
    /// Sets the respawn position when wax reaches 0.
    /// </summary>
    public void SetCheckpoint(Vector3 newCheckpointPosition)
    {
        checkpointPosition = newCheckpointPosition;
        hasCheckpoint = true;
    }

    private void HandleDepletion()
    {
        // 1. Show GameOverPanel via GameManager if configured
        if (GameManager.Instance != null && GameManager.Instance.gameOverPanel != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
        else if (hasCheckpoint)
        {
            // Fallback: Respawn at the saved checkpoint
            transform.position = checkpointPosition;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            currentWax = maxWax;
            OnWaxChanged?.Invoke(currentWax, maxWax);
        }
        else
        {
            // Fallback: Reload the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
