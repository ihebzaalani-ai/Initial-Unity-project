using System.Collections;
using UnityEngine;

/// <summary>
/// Allows the player to attack enemies in front of them using Space or J.
/// Shows a brief slash effect in the attack direction and damages enemies in range.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Radius of the attack circle.")]
    public float attackRange = 1.0f;

    [Tooltip("Distance in front of the player where the attack circle is centered.")]
    public float attackOffset = 0.8f;

    [Tooltip("Damage dealt to enemies per attack.")]
    public float attackDamage = 15f;

    [Tooltip("Minimum time between attacks in seconds.")]
    public float attackCooldown = 0.4f;

    [Tooltip("LayerMask assigned to enemies.")]
    public LayerMask enemyLayer;

    [Header("Attack Visual")]
    [Tooltip("Child GameObject or SpriteRenderer representing the slash. Auto-created if left empty.")]
    public GameObject slashVisual;

    [Tooltip("Duration in seconds that the slash remains visible.")]
    public float slashDuration = 0.1f;

    private Vector2 lastFacingDirection = Vector2.right;
    private float nextAttackTime = 0f;
    private Coroutine slashCoroutine;

    private void Awake()
    {
        // If no custom slash effect was assigned, generate a procedural crescent arc
        if (slashVisual == null)
        {
            CreateProceduralSlash();
        }
        else
        {
            slashVisual.SetActive(false);
        }
    }

    private void Update()
    {
        // 1. Track the last movement direction from WASD / arrow keys
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
        {
            lastFacingDirection = new Vector2(h, v).normalized;
        }

        // 2. Check for attack input (Space or J)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.J))
        {
            if (Time.time >= nextAttackTime)
            {
                PerformAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    private void PerformAttack()
    {
        Vector2 attackCenter = (Vector2)transform.position + (lastFacingDirection * attackOffset);

        // 1. Show slash effect aligned with the attack direction
        if (slashVisual != null)
        {
            slashVisual.transform.position = attackCenter;
            float angleDeg = Mathf.Atan2(lastFacingDirection.y, lastFacingDirection.x) * Mathf.Rad2Deg;
            slashVisual.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

            if (slashCoroutine != null)
            {
                StopCoroutine(slashCoroutine);
            }
            slashCoroutine = StartCoroutine(ShowSlashRoutine());
        }

        // 2. Detect and damage all enemies within range
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRange, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    private IEnumerator ShowSlashRoutine()
    {
        slashVisual.SetActive(true);
        yield return new WaitForSeconds(slashDuration);
        slashVisual.SetActive(false);
        slashCoroutine = null;
    }

    private void CreateProceduralSlash()
    {
        GameObject slashObj = new GameObject("ProceduralSlash");
        slashObj.transform.SetParent(transform, false);

        LineRenderer lr = slashObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.startWidth = 0.18f;
        lr.endWidth = 0.03f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(1f, 0.92f, 0.45f, 0.95f); // warm candle flame yellow
        lr.endColor = new Color(1f, 0.45f, 0.1f, 0.2f);

        int pointCount = 10;
        lr.positionCount = pointCount;
        float arcDegrees = 90f;
        float arcRadius = 0.65f;

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            float angleRad = Mathf.Lerp(-arcDegrees * 0.5f, arcDegrees * 0.5f, t) * Mathf.Deg2Rad;
            Vector3 point = new Vector3(Mathf.Cos(angleRad) * arcRadius, Mathf.Sin(angleRad) * arcRadius, 0f);
            lr.SetPosition(i, point);
        }

        slashObj.SetActive(false);
        slashVisual = slashObj;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the attack range in the Scene view when selected
        Vector2 center = (Vector2)transform.position + (lastFacingDirection * attackOffset);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, attackRange);
    }
}
