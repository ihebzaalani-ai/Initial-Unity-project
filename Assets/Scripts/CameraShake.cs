using UnityEngine;

/// <summary>
/// Shakes the Main Camera when events occur (e.g. player taking damage).
/// Works seamlessly with CameraFollow by supplying an additive offset.
/// </summary>
public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;

    [Header("Default Shake Settings")]
    [Tooltip("Default shake magnitude.")]
    public float defaultMagnitude = 0.25f;

    [Tooltip("Default shake duration.")]
    public float defaultDuration = 0.2f;

    private float shakeTimer = 0f;
    private float currentMagnitude = 0.2f;

    /// <summary>
    /// Current shake displacement vector to add to camera position.
    /// </summary>
    public static Vector3 CurrentShakeOffset { get; private set; } = Vector3.zero;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Triggers a screen shake with specified duration and magnitude.
    /// </summary>
    public static void Shake(float duration, float magnitude)
    {
        if (instance != null)
        {
            instance.shakeTimer = duration;
            instance.currentMagnitude = magnitude;
        }
    }

    /// <summary>
    /// Triggers a screen shake with default values.
    /// </summary>
    public static void Shake()
    {
        if (instance != null)
        {
            Shake(instance.defaultDuration, instance.defaultMagnitude);
        }
    }

    private void LateUpdate()
    {
        if (shakeTimer > 0f)
        {
            // Calculate a random 2D shake displacement on X and Y
            Vector2 randomOffset = Random.insideUnitCircle * currentMagnitude;
            CurrentShakeOffset = new Vector3(randomOffset.x, randomOffset.y, 0f);
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            CurrentShakeOffset = Vector3.zero;
        }
    }
}
