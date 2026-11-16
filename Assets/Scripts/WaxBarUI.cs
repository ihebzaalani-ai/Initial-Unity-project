using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates a UI Slider to display the player's remaining wax and maximum capacity.
/// Automatically finds WaxSystem if unassigned, subscribes to OnWaxChanged, and handles missing references.
/// </summary>
public class WaxBarUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI Slider component representing wax. Auto-assigned if left empty.")]
    [SerializeField] private Slider waxSlider;

    [Tooltip("The player's WaxSystem. Auto-detected in the scene if left empty.")]
    [SerializeField] private WaxSystem waxSystem;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        // Ensure references are resolved when the component is enabled
        if (waxSystem == null)
        {
            waxSystem = FindFirstObjectByType<WaxSystem>();
        }

        if (waxSystem != null)
        {
            // Subscribe to wax changes (unsubscribing first prevents accidental duplicate subscriptions)
            waxSystem.OnWaxChanged -= UpdateWaxBar;
            waxSystem.OnWaxChanged += UpdateWaxBar;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled to prevent memory leaks or calling destroyed objects
        if (waxSystem != null)
        {
            waxSystem.OnWaxChanged -= UpdateWaxBar;
        }
    }

    private void Start()
    {
        ResolveReferences();

        // Warn if required components are missing
        if (waxSlider == null)
        {
            Debug.LogWarning($"[WaxBarUI] Slider component is missing on '{gameObject.name}'! Please attach a Slider component or assign it in the Inspector.", this);
        }

        if (waxSystem == null)
        {
            Debug.LogWarning("[WaxBarUI] WaxSystem was not found in the scene! Ensure the Player GameObject has a WaxSystem component attached.", this);
        }
        else
        {
            // Ensure subscription is active if waxSystem was resolved in Start
            waxSystem.OnWaxChanged -= UpdateWaxBar;
            waxSystem.OnWaxChanged += UpdateWaxBar;

            // Refresh immediately in Start so the slider displays the initial wax state
            UpdateWaxBar(waxSystem.CurrentWax, waxSystem.MaxWax);
        }
    }

    private void ResolveReferences()
    {
        if (waxSlider == null)
        {
            waxSlider = GetComponent<Slider>();
            if (waxSlider == null)
            {
                waxSlider = GetComponentInChildren<Slider>();
            }
        }

        if (waxSystem == null)
        {
            waxSystem = FindFirstObjectByType<WaxSystem>();
        }
    }

    /// <summary>
    /// Sets slider.maxValue from maxWax and updates slider.value from currentWax.
    /// </summary>
    private void UpdateWaxBar(float currentWax, float maxWax)
    {
        if (waxSlider != null)
        {
            waxSlider.maxValue = maxWax;
            waxSlider.value = currentWax;
        }
    }
}
