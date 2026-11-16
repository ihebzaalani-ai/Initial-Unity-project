using System.Collections;
using UnityEngine;

/// <summary>
/// Briefly tints a SpriteRenderer with a flash color and restores the original color.
/// Used for hit feedback on enemies (white flash) and the player (red flash).
/// </summary>
public class HitFlash : MonoBehaviour
{
    [Header("Flash Configuration")]
    [Tooltip("Default flash duration in seconds.")]
    public float defaultDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// Flashes the sprite with the specified color for duration seconds.
    /// </summary>
    public void Flash(Color color, float duration)
    {
        if (spriteRenderer == null) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            spriteRenderer.color = originalColor;
        }

        flashCoroutine = StartCoroutine(FlashRoutine(color, duration));
    }

    /// <summary>
    /// Flashes using the default duration.
    /// </summary>
    public void Flash(Color color)
    {
        Flash(color, defaultDuration);
    }

    private IEnumerator FlashRoutine(Color color, float duration)
    {
        spriteRenderer.color = color;
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    private void OnDisable()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        flashCoroutine = null;
    }
}
