using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Brazier checkpoint: restores the player's wax to full, updates their respawn checkpoint,
/// and loads the Ending scene after a delay if it is the final (or only) brazier in the level.
/// </summary>
public class Brazier : MonoBehaviour
{
    [Header("Brazier Settings")]
    [Tooltip("If true (or if this is the only brazier in the level), reaching this brazier loads the Ending scene.")]
    public bool isFinalBrazier = true;

    [Tooltip("Delay in seconds before loading the ending scene so the player sees the fire first.")]
    public float delay = 1.0f;

    [Tooltip("Name of the scene to load upon reaching the brazier.")]
    public string endingSceneName = "Ending";

    [Tooltip("Optional spawn position offset for the checkpoint.")]
    public Vector3 respawnOffset = Vector3.zero;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Guard so it only triggers once
        if (hasTriggered) return;

        // 2. Verify the collider belongs to the Player using CompareTag
        if (!other.CompareTag("Player")) return;

        // 3. Find WaxSystem on the Player (supports colliders on child objects)
        WaxSystem playerWax = other.GetComponentInParent<WaxSystem>();
        if (playerWax != null)
        {
            hasTriggered = true;

            Debug.Log($"[Brazier] Player reached the brazier at {transform.position}! Fully restoring wax and setting checkpoint.");

            // 4. Fully restore player's wax
            playerWax.Restore(playerWax.MaxWax);

            // 5. Set this brazier as the new respawn checkpoint
            playerWax.SetCheckpoint(transform.position + respawnOffset);

            // 6. Check if this is the final brazier or the only brazier in the level
            Brazier[] allBraziers = FindObjectsByType<Brazier>(FindObjectsSortMode.None);
            bool isOnlyBrazier = (allBraziers == null || allBraziers.Length <= 1);

            if (isFinalBrazier || isOnlyBrazier)
            {
                StartCoroutine(LoadEndingRoutine());
            }
        }
    }

    private IEnumerator LoadEndingRoutine()
    {
        yield return new WaitForSeconds(delay);

        // Ensure time is running normally before scene transition
        Time.timeScale = 1f;

        Debug.Log($"[Brazier] Loading ending scene: '{endingSceneName}'");
        SceneManager.LoadScene(endingSceneName);
    }
}
