using UnityEngine;

/// <summary>
/// Collectible wax shard pickup that restores wax to the player upon contact.
/// </summary>
public class WaxPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Amount of wax restored when collected.")]
    public float restoreAmount = 25f;

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Avoid duplicate triggers in the same physics frame before destruction
        if (isCollected) return;

        // Verify the colliding object is tagged Player
        if (other.CompareTag("Player"))
        {
            WaxSystem playerWax = other.GetComponentInParent<WaxSystem>();
            if (playerWax != null)
            {
                isCollected = true;

                // Restore wax to the player
                playerWax.Restore(restoreAmount);

                // Increment collected wax counter in GameManager if active
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddWaxCollected();
                }

                // Destroy this pickup object
                Destroy(gameObject);
            }
        }
    }
}
