using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls the player's 2D point light radius and intensity proportional to remaining wax,
/// and automatically dims ambient scene lighting to create a dark, mysterious exploration atmosphere.
/// </summary>
public class WaxLight : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The WaxSystem to observe. Auto-assigned in Awake if left empty.")]
    [SerializeField] private WaxSystem waxSystem;

    [Tooltip("The Light2D component (usually on a child GameObject). Auto-assigned if left empty.")]
    [SerializeField] private Light2D playerLight;

    [Header("Candle Flicker")]
    [Tooltip("Magnitude of candle flicker on radius and intensity.")]
    public float flickerAmount = 0.08f;

    [Tooltip("Speed of the candle flicker.")]
    public float flickerSpeed = 10f;

    [Header("Point Light Radius (Exploration Glow)")]
    [Tooltip("Outer radius when wax is at 0 (dim glow right at feet).")]
    public float minRadius = 1.0f;

    [Tooltip("Outer radius when wax is at maximum (cozy circle around player).")]
    public float maxRadius = 3.5f;

    [Header("Light Intensity")]
    [Tooltip("Intensity when wax is at 0.")]
    public float minIntensity = 0.2f;

    [Tooltip("Intensity when wax is at maximum.")]
    public float maxIntensity = 1.0f;

    [Header("Ambient Darkness Control")]
    [Tooltip("Ambient global light intensity (0.02 is dark and moody, 0 is pitch black).")]
    public float ambientLightIntensity = 0.02f;

    [Tooltip("Optional reference to Global Light 2D. Automatically detected in the scene if unassigned.")]
    [SerializeField] private Light2D globalLight;

    private void Awake()
    {
        if (waxSystem == null) waxSystem = GetComponent<WaxSystem>();
        if (waxSystem == null) waxSystem = GetComponentInParent<WaxSystem>();
        if (playerLight == null) playerLight = GetComponentInChildren<Light2D>();
        if (playerLight == null) playerLight = GetComponent<Light2D>();
    }

    private void Start()
    {
        // 1. Darken the global ambient lighting so the player must explore using their own light
        if (globalLight == null)
        {
            Light2D[] allLights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
            foreach (Light2D l in allLights)
            {
                if (l.lightType == Light2D.LightType.Global)
                {
                    globalLight = l;
                    break;
                }
            }
        }

        if (globalLight != null)
        {
            globalLight.intensity = ambientLightIntensity;
        }
    }

    private void Update()
    {
        if (playerLight == null) return;

        float ratio = 1f;
        if (waxSystem != null && waxSystem.MaxWax > 0f)
        {
            ratio = Mathf.Clamp01(waxSystem.CurrentWax / waxSystem.MaxWax);
        }

        float baseRadius = Mathf.Lerp(minRadius, maxRadius, ratio);
        float baseIntensity = Mathf.Lerp(minIntensity, maxIntensity, ratio);

        // Perlin noise candle flicker
        float noise = (Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) - 0.5f) * 2f;
        float flicker = noise * flickerAmount;

        playerLight.pointLightOuterRadius = Mathf.Max(0.1f, baseRadius * (1f + flicker));
        playerLight.intensity = Mathf.Max(0.05f, baseIntensity * (1f + flicker));
    }
}
