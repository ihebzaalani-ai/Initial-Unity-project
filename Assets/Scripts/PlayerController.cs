using UnityEngine;

/// <summary>
/// Handles 4-direction top-down player movement, sprite flipping, and animation speed.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 4f;

    [Header("Optional Component Overrides")]
    [Tooltip("Auto-assigned in Awake if left empty.")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    // Stores normalized input direction from WASD / arrow keys
    private Vector2 movementInput;

    private void Awake()
    {
        // Automatically find components on this GameObject if not set manually
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // 1. Read input from the legacy Input Manager (WASD and Arrow keys)
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        // 2. Normalize diagonal movement so diagonal speed is not faster than cardinal speed
        movementInput = new Vector2(inputX, inputY).normalized;

        // 3. Flip sprite horizontally when moving left or right
        if (spriteRenderer != null)
        {
            if (inputX > 0f)
            {
                spriteRenderer.flipX = false; // Facing right (default)
            }
            else if (inputX < 0f)
            {
                spriteRenderer.flipX = true;  // Facing left
            }
        }

        // 4. Update Animator float parameter "Speed" if an Animator component exists
        if (animator != null)
        {
            animator.SetFloat("Speed", movementInput.magnitude);
        }
    }

    private void FixedUpdate()
    {
        // 5. Apply physics-based movement using Unity 6 linearVelocity
        if (rb != null)
        {
            rb.linearVelocity = movementInput * moveSpeed;
        }
    }
}
