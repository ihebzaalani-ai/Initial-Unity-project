using UnityEngine;

/// <summary>
/// Controls enemy behavior: patrols between two waypoints, chases the player when detected,
/// attacks with a cooldown within range, takes knockback and hit flash on damage,
/// and reports defeat on death.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("First patrol waypoint.")]
    public Transform pointA;

    [Tooltip("Second patrol waypoint.")]
    public Transform pointB;

    [Tooltip("Patrol movement speed.")]
    public float patrolSpeed = 2f;

    [Header("Combat Settings")]
    [Tooltip("Distance at which the enemy detects the player.")]
    public float detectionRadius = 5f;

    [Tooltip("Chase movement speed.")]
    public float chaseSpeed = 3.5f;

    [Tooltip("Distance at which the enemy can attack the player.")]
    public float attackRange = 1.2f;

    [Tooltip("Damage dealt per attack to the player's wax.")]
    public float attackDamage = 15f;

    [Tooltip("Cooldown between attacks in seconds.")]
    public float attackCooldown = 1.5f;

    [Tooltip("Impulse force pushing the enemy away from the player when struck.")]
    public float knockbackForce = 6f;

    [Header("Health Settings")]
    [Tooltip("Maximum enemy health.")]
    public float maxHealth = 30f;

    [SerializeField] private float currentHealth;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private HitFlash hitFlash;
    private Transform playerTransform;
    private WaxSystem playerWax;
    private Transform currentPatrolTarget;
    private float nextAttackTime = 0f;
    private float stunTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitFlash = GetComponent<HitFlash>();
        if (hitFlash == null)
        {
            hitFlash = GetComponentInChildren<HitFlash>();
        }
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Find player in the scene
        WaxSystem player = FindFirstObjectByType<WaxSystem>();
        if (player != null)
        {
            playerTransform = player.transform;
            playerWax = player;
        }

        // Initialize patrol destination
        if (pointA != null)
        {
            currentPatrolTarget = pointA;
        }
        else if (pointB != null)
        {
            currentPatrolTarget = pointB;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Briefly pause AI movement while recovering from knockback impulse
        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
            return;
        }

        // Check if player is detected
        if (playerTransform != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distToPlayer <= detectionRadius)
            {
                ChaseOrAttack(distToPlayer);
                return;
            }
        }

        // Fall back to patrolling if player is out of range
        Patrol();
    }

    private void ChaseOrAttack(float distToPlayer)
    {
        if (distToPlayer > attackRange)
        {
            // Move toward player
            Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
            rb.linearVelocity = direction * chaseSpeed;
            FlipSprite(direction.x);
        }
        else
        {
            // In attack range: stop moving and perform attack if off cooldown
            rb.linearVelocity = Vector2.zero;

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    private void AttackPlayer()
    {
        if (playerWax != null)
        {
            playerWax.TakeDamage(attackDamage);
        }
    }

    private void Patrol()
    {
        if (currentPatrolTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distToTarget = Vector2.Distance(transform.position, currentPatrolTarget.position);
        if (distToTarget < 0.2f)
        {
            // Switch patrol point
            currentPatrolTarget = (currentPatrolTarget == pointA && pointB != null) ? pointB : pointA;
        }

        if (currentPatrolTarget != null)
        {
            Vector2 direction = ((Vector2)currentPatrolTarget.position - rb.position).normalized;
            rb.linearVelocity = direction * patrolSpeed;
            FlipSprite(direction.x);
        }
    }

    private void FlipSprite(float directionX)
    {
        if (spriteRenderer != null)
        {
            if (directionX > 0.05f)
            {
                spriteRenderer.flipX = false;
            }
            else if (directionX < -0.05f)
            {
                spriteRenderer.flipX = true;
            }
        }
    }

    /// <summary>
    /// Reduces enemy health, flashes the sprite white, applies knockback away from the player,
    /// and destroys the GameObject when health reaches 0.
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Flash white on hit
        if (hitFlash != null)
        {
            hitFlash.Flash(Color.white, 0.1f);
        }

        // Apply knockback impulse away from the player
        if (rb != null && playerTransform != null)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            stunTimer = 0.15f;
        }

        if (currentHealth <= 0f)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EnemyDefeated();
            }

            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection radius in yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Visualize attack range in red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
