using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Smooth 2D top-down camera follow for the Main Camera.
/// Features target tracking with SmoothDamp, automatic or manual room boundary clamping,
/// and live orthographic zoom control.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target & Movement")]
    [Tooltip("The Transform to follow. If empty, automatically searches for the GameObject tagged 'Player'.")]
    public Transform target;

    [Tooltip("Smoothing duration in seconds (lower is faster, higher is smoother).")]
    public float smoothTime = 0.12f;

    [Tooltip("Position offset relative to the target (Z is typically -10 in 2D).")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Camera Settings")]
    [Tooltip("Orthographic size (zoom). Lower numbers zoom in closer; 2.2 provides a close, tense view.")]
    public float orthographicSize = 2.2f;

    [Header("Room Bounds")]
    [Tooltip("Enable to prevent the camera from revealing areas outside the room boundary.")]
    public bool useBounds = true;

    [Tooltip("Automatically calculates bounds from the Floor / Walls Tilemaps in the scene.")]
    public bool autoDetectFromTilemap = true;

    [Tooltip("Bottom-left corner of the level boundary (X = left wall, Y = bottom floor).")]
    public Vector2 minBounds = new Vector2(-12f, -5f);

    [Tooltip("Top-right corner of the level boundary (X = right wall, Y = top floor).")]
    public Vector2 maxBounds = new Vector2(12f, 5f);

    private Camera cam;
    private Vector3 currentVelocity = Vector3.zero;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
        }

        FindPlayerTarget();
        CheckAndDetectBounds();
    }

    private void Start()
    {
        if (target == null)
        {
            FindPlayerTarget();
        }

        CheckAndDetectBounds();

        // Snap camera immediately to target position on start
        if (target != null)
        {
            Vector3 initialPos = target.position + offset;
            if (useBounds && cam != null)
            {
                initialPos = ClampPosition(initialPos);
            }
            transform.position = initialPos;
        }
    }

    private void LateUpdate()
    {
        // Allow live zoom adjustments in the Inspector during Play Mode
        if (cam != null && Mathf.Abs(cam.orthographicSize - orthographicSize) > 0.001f)
        {
            cam.orthographicSize = orthographicSize;
        }

        if (target == null)
        {
            FindPlayerTarget();
            if (target == null) return;
        }

        // 1. Calculate desired target position with offset
        Vector3 desiredPosition = target.position + offset;

        // 2. Clamp desired position within room boundaries
        if (useBounds && cam != null)
        {
            desiredPosition = ClampPosition(desiredPosition);
        }

        // 3. Smoothly move toward the desired position and add camera shake offset
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime) + CameraShake.CurrentShakeOffset;
    }

    private void CheckAndDetectBounds()
    {
        // Auto-detect if enabled or if bounds are uninitialized/collapsed (e.g. min == max)
        if (autoDetectFromTilemap || minBounds.x >= maxBounds.x || minBounds.y >= maxBounds.y)
        {
            DetectTilemapBounds();
        }
    }

    private void DetectTilemapBounds()
    {
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        if (tilemaps != null && tilemaps.Length > 0)
        {
            bool found = false;
            Bounds totalBounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (Tilemap tm in tilemaps)
            {
                TilemapRenderer tr = tm.GetComponent<TilemapRenderer>();
                if (tr != null)
                {
                    if (!found)
                    {
                        totalBounds = tr.bounds;
                        found = true;
                    }
                    else
                    {
                        totalBounds.Encapsulate(tr.bounds);
                    }
                }
            }

            if (found && totalBounds.size.x > 0f && totalBounds.size.y > 0f)
            {
                minBounds = new Vector2(totalBounds.min.x, totalBounds.min.y);
                maxBounds = new Vector2(totalBounds.max.x, totalBounds.max.y);
                useBounds = true;
            }
        }
    }

    private Vector3 ClampPosition(Vector3 pos)
    {
        if (minBounds.x >= maxBounds.x || minBounds.y >= maxBounds.y)
        {
            DetectTilemapBounds();
        }

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        float minX = minBounds.x + horzExtent;
        float maxX = maxBounds.x - horzExtent;
        float minY = minBounds.y + vertExtent;
        float maxY = maxBounds.y - vertExtent;

        // If the room is smaller than the camera viewport on an axis, center it on that axis
        float clampedX = (minX > maxX) ? (minBounds.x + maxBounds.x) * 0.5f : Mathf.Clamp(pos.x, minX, maxX);
        float clampedY = (minY > maxY) ? (minBounds.y + maxBounds.y) * 0.5f : Mathf.Clamp(pos.y, minY, maxY);

        return new Vector3(clampedX, clampedY, pos.z);
    }

    private void FindPlayerTarget()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        // Draw level boundary box in Scene view
        Gizmos.color = Color.green;
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0f);
        Vector3 center = new Vector3(minBounds.x + size.x * 0.5f, minBounds.y + size.y * 0.5f, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}
