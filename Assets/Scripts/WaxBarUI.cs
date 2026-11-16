using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Updates and styles the UI Slider representing the player's wax.
/// Adds a prominent customizable border, high-contrast colors, full-height bar layout,
/// optional numeric text overlay, and low-wax warning feedback to maximize visibility.
/// </summary>
public class WaxBarUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI Slider component representing wax. Auto-assigned if left empty.")]
    [SerializeField] private Slider waxSlider;

    [Tooltip("The player's WaxSystem. Auto-detected in the scene if left empty.")]
    [SerializeField] private WaxSystem waxSystem;

    [Tooltip("The background Image of the slider. Auto-detected if left empty.")]
    [SerializeField] private Image backgroundImage;

    [Tooltip("The fill Image of the slider. Auto-detected if left empty.")]
    [SerializeField] private Image fillImage;

    [Header("Border & Frame")]
    [Tooltip("Enable to add a distinct border frame around the wax bar.")]
    public bool showBorder = true;

    [Tooltip("Color of the wax bar border frame (e.g. warm candle gold).")]
    public Color borderColor = new Color(0.95f, 0.78f, 0.25f, 1f);

    [Tooltip("Thickness of the border in pixels.")]
    public float borderThickness = 3f;

    [Tooltip("Add a dark outer outline/drop-shadow around the border for high contrast against dark and bright rooms.")]
    public bool useOuterOutline = true;

    [Tooltip("Color of the outer outline.")]
    public Color outerOutlineColor = new Color(0f, 0f, 0f, 0.85f);

    [Tooltip("Offset distance of the outer outline in pixels.")]
    public Vector2 outerOutlineDistance = new Vector2(1.5f, -1.5f);

    [Header("Wax Colors & Visibility")]
    [Tooltip("Color of the wax fill when healthy (luminous candle-flame gold).")]
    public Color waxFillColor = new Color(1f, 0.75f, 0.15f, 1f);

    [Tooltip("Color of the wax fill when critically low (warning crimson-orange).")]
    public Color lowWaxColor = new Color(0.95f, 0.22f, 0.15f, 1f);

    [Tooltip("Wax percentage (0 to 1) below which the low-wax warning color is triggered.")]
    [Range(0f, 1f)]
    public float lowWaxThreshold = 0.25f;

    [Tooltip("Smoothly blend from healthy wax color to warning red as wax depletes.")]
    public bool interpolateColor = true;

    [Tooltip("Gently pulse the bar brightness when wax is critically low.")]
    public bool pulseOnLowWax = true;

    [Tooltip("Speed of the low-wax warning pulse.")]
    public float pulseSpeed = 6f;

    [Tooltip("High-contrast background color for the empty portion of the bar (deep charcoal).")]
    public Color backgroundColor = new Color(0.12f, 0.12f, 0.15f, 0.95f);

    [Header("Bar Layout & Padding")]
    [Tooltip("Expands background and fill to occupy the full height of the bar rather than a squished 50% sliver.")]
    public bool expandToFullHeight = true;

    [Tooltip("Internal padding between the border and the inner bar in pixels.")]
    public float innerPadding = 0f;

    [Header("Text Display")]
    [Tooltip("Display wax numbers on top of the bar (e.g. 'Wax: 100 / 100').")]
    public bool showWaxText = true;

    [Tooltip("Automatically generate a centered TextMeshProUGUI text overlay if none is assigned.")]
    public bool autoCreateText = true;

    [Tooltip("TextMeshProUGUI component displaying wax. Auto-created if left empty.")]
    public TextMeshProUGUI waxText;

    [Tooltip("Format for wax text: {0} = current wax, {1} = max wax, {2} = percentage.")]
    public string textFormat = "Wax: {0:0} / {1:0}";

    [Tooltip("Color of the wax text.")]
    public Color textColor = Color.white;

    [Tooltip("Font size of the wax text.")]
    public float textFontSize = 14f;

    [Header("Bar Transform (Optional)")]
    [Tooltip("Check to apply customBarSize and customPosition to the RectTransform.")]
    public bool applyCustomTransform = false;

    [Tooltip("Custom dimensions (width, height) of the wax bar.")]
    public Vector2 customBarSize = new Vector2(300f, 32f);

    [Tooltip("Custom anchored position from top-left.")]
    public Vector2 customPosition = new Vector2(20f, -20f);

    private GameObject borderObject;
    private RectTransform borderRect;
    private Image borderImage;
    private bool wasLowWax = false;

    private void Awake()
    {
        ResolveReferences();
        if (waxSlider != null && GetComponent<RectTransform>() != null)
        {
            ApplyStyling();
        }
    }

    private void OnEnable()
    {
        if (waxSystem == null)
        {
            waxSystem = FindFirstObjectByType<WaxSystem>();
        }

        if (waxSystem != null)
        {
            waxSystem.OnWaxChanged -= UpdateWaxBar;
            waxSystem.OnWaxChanged += UpdateWaxBar;
        }
    }

    private void OnDisable()
    {
        if (waxSystem != null)
        {
            waxSystem.OnWaxChanged -= UpdateWaxBar;
        }
    }

    private void Start()
    {
        ResolveReferences();

        if (waxSlider == null || GetComponent<RectTransform>() == null)
        {
            Debug.LogWarning($"[WaxBarUI] Slider component is missing on '{gameObject.name}'! WaxBarUI should only be attached to the WaxBar UI object in the Canvas.", this);
            return;
        }

        ApplyStyling();

        if (waxSystem == null)
        {
            Debug.LogWarning("[WaxBarUI] WaxSystem was not found in the scene! Ensure the Player GameObject has a WaxSystem component attached.", this);
        }
        else
        {
            waxSystem.OnWaxChanged -= UpdateWaxBar;
            waxSystem.OnWaxChanged += UpdateWaxBar;

            UpdateWaxBar(waxSystem.CurrentWax, waxSystem.MaxWax);
        }
    }

    private void Update()
    {
        if (waxSlider == null || waxSystem == null || waxSystem.MaxWax <= 0f) return;

        bool isLow = (waxSystem.CurrentWax / waxSystem.MaxWax) <= lowWaxThreshold;
        if (isLow && pulseOnLowWax)
        {
            UpdateVisuals(waxSystem.CurrentWax, waxSystem.MaxWax);
            wasLowWax = true;
        }
        else if (wasLowWax)
        {
            // Restore standard visuals once wax has been restored above threshold
            UpdateVisuals(waxSystem.CurrentWax, waxSystem.MaxWax);
            wasLowWax = false;
        }
    }

    /// <summary>
    /// Configures the border, background, layout, and text styling for maximum visibility.
    /// </summary>
    public void ApplyStyling()
    {
        ResolveReferences();

        if (waxSlider == null || GetComponent<RectTransform>() == null)
        {
            return;
        }

        SetupBorder();
        SetupLayout();
        SetupText();

        if (waxSystem != null)
        {
            UpdateVisuals(waxSystem.CurrentWax, waxSystem.MaxWax);
        }
        else if (waxSlider != null)
        {
            UpdateVisuals(waxSlider.value, waxSlider.maxValue);
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

        if (backgroundImage == null)
        {
            Transform bg = transform.Find("Background");
            if (bg != null)
            {
                backgroundImage = bg.GetComponent<Image>();
            }
            else
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                foreach (Image img in images)
                {
                    if (waxSlider != null && waxSlider.fillRect != null && img.rectTransform == waxSlider.fillRect)
                    {
                        continue;
                    }
                    if (borderObject != null && img.gameObject == borderObject)
                    {
                        continue;
                    }
                    backgroundImage = img;
                    break;
                }
            }
        }

        if (fillImage == null)
        {
            if (waxSlider != null && waxSlider.fillRect != null)
            {
                fillImage = waxSlider.fillRect.GetComponent<Image>();
            }
            if (fillImage == null)
            {
                Transform fill = transform.Find("Fill Area/Fill");
                if (fill != null)
                {
                    fillImage = fill.GetComponent<Image>();
                }
            }
        }

        // Auto-assign fillRect if it was accidentally cleared
        if (waxSlider != null && waxSlider.fillRect == null && fillImage != null)
        {
            waxSlider.fillRect = fillImage.rectTransform;
        }
    }

    private void SetupBorder()
    {
        if (!showBorder)
        {
            if (borderObject != null) borderObject.SetActive(false);
            return;
        }

        if (borderObject == null)
        {
            Transform existing = transform.Find("WaxBar_Border");
            if (existing != null)
            {
                borderObject = existing.gameObject;
            }
            else
            {
                borderObject = new GameObject("WaxBar_Border", typeof(RectTransform), typeof(Image));
                borderObject.transform.SetParent(transform, false);
            }
        }

        // Place border at index 0 so it renders behind the background and fill
        borderObject.transform.SetSiblingIndex(0);
        borderObject.SetActive(true);

        borderRect = borderObject.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.pivot = new Vector2(0.5f, 0.5f);
        borderRect.offsetMin = new Vector2(-borderThickness, -borderThickness);
        borderRect.offsetMax = new Vector2(borderThickness, borderThickness);

        borderImage = borderObject.GetComponent<Image>();
        if (backgroundImage != null && backgroundImage.sprite != null)
        {
            borderImage.sprite = backgroundImage.sprite;
            borderImage.type = backgroundImage.type;
        }
        borderImage.color = borderColor;

        // Outer outline / drop shadow for extra edge separation
        if (useOuterOutline)
        {
            Outline outline = borderObject.GetComponent<Outline>();
            if (outline == null) outline = borderObject.AddComponent<Outline>();
            outline.effectColor = outerOutlineColor;
            outline.effectDistance = outerOutlineDistance;
            outline.enabled = true;
        }
        else
        {
            Outline outline = borderObject.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
        }
    }

    private void SetupLayout()
    {
        if (applyCustomTransform)
        {
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = customBarSize;
                rt.anchoredPosition = customPosition;
            }
        }

        if (expandToFullHeight)
        {
            if (backgroundImage != null)
            {
                RectTransform bgRect = backgroundImage.rectTransform;
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = new Vector2(innerPadding, innerPadding);
                bgRect.offsetMax = new Vector2(-innerPadding, -innerPadding);
            }

            if (waxSlider != null && waxSlider.fillRect != null)
            {
                RectTransform fillAreaRect = waxSlider.fillRect.parent as RectTransform;
                if (fillAreaRect != null && fillAreaRect != transform)
                {
                    fillAreaRect.anchorMin = Vector2.zero;
                    fillAreaRect.anchorMax = Vector2.one;
                    fillAreaRect.offsetMin = new Vector2(innerPadding, innerPadding);
                    fillAreaRect.offsetMax = new Vector2(-innerPadding, -innerPadding);
                }

                // Remove Unity Slider's default knob space margin
                if (waxSlider.handleRect == null)
                {
                    waxSlider.fillRect.sizeDelta = Vector2.zero;
                }
            }
        }
    }

    private void SetupText()
    {
        if (!showWaxText)
        {
            if (waxText != null) waxText.gameObject.SetActive(false);
            return;
        }

        if (waxText == null)
        {
            Transform existing = transform.Find("WaxBar_Text");
            if (existing != null)
            {
                waxText = existing.GetComponent<TextMeshProUGUI>();
            }
        }

        if (waxText == null && autoCreateText)
        {
            GameObject textObj = new GameObject("WaxBar_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(transform, false);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            waxText = textObj.GetComponent<TextMeshProUGUI>();
            waxText.alignment = TextAlignmentOptions.Center;
            waxText.fontSize = textFontSize;
            waxText.color = textColor;
            waxText.fontStyle = FontStyles.Bold;

            // Borrow font from any existing TMP text in the scene
            if (waxText.font == null)
            {
                TextMeshProUGUI otherTMP = FindFirstObjectByType<TextMeshProUGUI>();
                if (otherTMP != null && otherTMP != waxText && otherTMP.font != null)
                {
                    waxText.font = otherTMP.font;
                }
            }

            // Drop shadow for high readability over bright and dark fills
            Shadow shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(1f, -1f);
        }

        if (waxText != null)
        {
            waxText.gameObject.SetActive(true);
            waxText.transform.SetAsLastSibling(); // Ensure text is visible in front of fill
            waxText.color = textColor;
            waxText.fontSize = textFontSize;
        }
    }

    /// <summary>
    /// Sets slider values and refreshes all visual layers.
    /// </summary>
    private void UpdateWaxBar(float currentWax, float maxWax)
    {
        if (waxSlider != null)
        {
            waxSlider.maxValue = maxWax;
            waxSlider.value = currentWax;
        }

        UpdateVisuals(currentWax, maxWax);
    }

    private void UpdateVisuals(float currentWax, float maxWax)
    {
        float ratio = maxWax > 0f ? Mathf.Clamp01(currentWax / maxWax) : 0f;

        // 1. Update fill color with health lerp and low-wax pulse
        if (fillImage != null)
        {
            Color targetColor;
            if (interpolateColor)
            {
                if (ratio <= lowWaxThreshold)
                {
                    float t = ratio / Mathf.Max(0.001f, lowWaxThreshold);
                    targetColor = Color.Lerp(lowWaxColor, waxFillColor, t);
                }
                else
                {
                    targetColor = waxFillColor;
                }
            }
            else
            {
                targetColor = ratio <= lowWaxThreshold ? lowWaxColor : waxFillColor;
            }

            if (pulseOnLowWax && ratio <= lowWaxThreshold && Application.isPlaying)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0 to 1
                targetColor = Color.Lerp(targetColor, Color.white, pulse * 0.35f);
            }

            fillImage.color = targetColor;
        }

        // 2. Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }

        // 3. Update border color & pulse when critically low
        if (borderImage != null)
        {
            Color currentBorderColor = borderColor;
            if (pulseOnLowWax && ratio <= lowWaxThreshold && Application.isPlaying)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                currentBorderColor = Color.Lerp(borderColor, lowWaxColor, pulse * 0.5f);
            }
            borderImage.color = currentBorderColor;
        }

        // 4. Update wax numeric text
        if (showWaxText && waxText != null)
        {
            waxText.text = string.Format(textFormat, currentWax, maxWax, Mathf.RoundToInt(ratio * 100f));
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyStyling();
        }
    }
#endif
}
