using UnityEngine;

/// <summary>
/// Brazier checkpoint: restores the player's wax to full, updates their respawn checkpoint,
/// and triggers victory when reached.
/// </summary>
public class Brazier : MonoBehaviour
{
    [Header("Brazier Settings")]
    [Tooltip("If true, reaching this brazier triggers victory immediately. If false, victory triggers only when all enemies are defeated.")]
    public bool isFinalBrazier = true;

    [Tooltip("Optional direct victory panel reference. If unassigned, GameManager.Instance.winPanel is used.")]
    public GameObject winPanel;

    [Tooltip("Optional spawn position offset for the checkpoint.")]
    public Vector3 respawnOffset = Vector3.zero;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verify the collider belongs to the Player using CompareTag
        if (!other.CompareTag("Player")) return;

        // 2. Find WaxSystem on the Player (supports colliders on child objects)
        WaxSystem playerWax = other.GetComponentInParent<WaxSystem>();
        if (playerWax != null)
        {
            Debug.Log($"[Brazier] Player reached the brazier at {transform.position}! Fully restoring wax and setting checkpoint.");

            // 3. Fully restore player's wax
            playerWax.Restore(playerWax.MaxWax);

            // 4. Set this brazier as the new respawn checkpoint
            playerWax.SetCheckpoint(transform.position + respawnOffset);

            // 5. Show the win panel
            bool winConditionMet = isFinalBrazier || (GameManager.Instance != null && GameManager.Instance.AreAllEnemiesDefeated());
            if (winConditionMet)
            {
                if (winPanel != null)
                {
                    winPanel.SetActive(true);
                }

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerWin();
                }
            }
        }
    }
}
