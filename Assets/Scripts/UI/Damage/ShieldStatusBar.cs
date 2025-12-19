using UnityEngine;

/// <summary>
/// Prominent shield display showing current/max.
/// Flash effect on damage, "DEPLETED" indicator when at 0.
/// </summary>
public class ShieldStatusBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ship targetShip;
    [SerializeField] private ShieldSystem shieldSystem;

    [Header("UI Settings")]
    [SerializeField] private bool isVisible = true;
    [SerializeField] private Vector2 barPosition = new Vector2(10, 80);
    [SerializeField] private float barWidth = 200f;
    [SerializeField] private float barHeight = 30f;

    [Header("Visual Settings")]
    [SerializeField] private Color shieldColor = new Color(0f, 0.7f, 1f); // Cyan
    [SerializeField] private Color depletedColor = new Color(0.5f, 0.5f, 0.5f); // Gray
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.3f;

    // Flash effect state
    private float flashTimer = 0f;
    private float previousShields = -1f;
    private bool isFlashing = false;

    // Properties
    public bool IsVisible => isVisible;
    public bool IsDepleted => shieldSystem != null && shieldSystem.CurrentShields <= 0;

    /// <summary>
    /// Initialize with target ship.
    /// </summary>
    public void Initialize(Ship ship)
    {
        targetShip = ship;

        if (targetShip != null)
        {
            shieldSystem = targetShip.ShieldSystem;
            if (shieldSystem != null)
            {
                previousShields = shieldSystem.CurrentShields;
            }
        }
    }

    /// <summary>
    /// Set the shield system directly.
    /// </summary>
    public void SetShieldSystem(ShieldSystem system)
    {
        shieldSystem = system;
        if (shieldSystem != null)
        {
            previousShields = shieldSystem.CurrentShields;
        }
    }

    /// <summary>
    /// Show the bar.
    /// </summary>
    public void Show()
    {
        isVisible = true;
    }

    /// <summary>
    /// Hide the bar.
    /// </summary>
    public void Hide()
    {
        isVisible = false;
    }

    /// <summary>
    /// Trigger flash effect (called when shields take damage).
    /// </summary>
    public void Flash()
    {
        flashTimer = flashDuration;
        isFlashing = true;
    }

    private void Update()
    {
        // Update flash timer
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                isFlashing = false;
                flashTimer = 0f;
            }
        }

        // Detect shield changes
        if (shieldSystem != null)
        {
            float currentShields = shieldSystem.CurrentShields;
            if (previousShields >= 0 && currentShields < previousShields)
            {
                // Shields took damage
                Flash();
            }
            previousShields = currentShields;
        }
    }

    private void OnGUI()
    {
        if (!isVisible || shieldSystem == null) return;

        float currentShields = shieldSystem.CurrentShields;
        float maxShields = shieldSystem.MaxShields;
        float shieldPercent = maxShields > 0 ? currentShields / maxShields : 0;
        bool isDepleted = currentShields <= 0;

        // Position
        Rect barRect = new Rect(barPosition.x, barPosition.y, barWidth, barHeight);

        // Background
        GUI.Box(barRect, "");

        // Determine bar color
        Color displayColor;
        if (isFlashing)
        {
            // Flash between white and shield color
            float flashPercent = flashTimer / flashDuration;
            displayColor = Color.Lerp(shieldColor, flashColor, flashPercent);
        }
        else if (isDepleted)
        {
            displayColor = depletedColor;
        }
        else
        {
            displayColor = shieldColor;
        }

        // Bar fill
        if (shieldPercent > 0)
        {
            GUI.color = displayColor;
            float fillWidth = (barWidth - 4) * shieldPercent;
            Rect fillRect = new Rect(barRect.x + 2, barRect.y + 2, fillWidth, barRect.height - 4);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // Text overlay
        string shieldText;
        if (isDepleted)
        {
            shieldText = "<b>SHIELDS DEPLETED</b>";
            GUI.color = Color.red;
        }
        else
        {
            shieldText = $"<b>SHIELDS: {currentShields:F0}/{maxShields:F0}</b>";
            GUI.color = Color.white;
        }

        // Center text
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        centeredStyle.richText = true;
        GUI.Label(barRect, shieldText, centeredStyle);

        GUI.color = Color.white;

        // Low shield warning (pulsing when under 25%)
        if (!isDepleted && shieldPercent < 0.25f)
        {
            float pulse = (Mathf.Sin(Time.time * 8f) + 1f) / 2f;
            GUI.color = Color.Lerp(Color.white, Color.red, pulse);
            GUI.Label(new Rect(barRect.x + barRect.width + 5, barRect.y + 5, 50, 20), "LOW!");
            GUI.color = Color.white;
        }
    }

    /// <summary>
    /// Get shield percentage (0-1).
    /// </summary>
    public float GetShieldPercentage()
    {
        if (shieldSystem == null || shieldSystem.MaxShields <= 0) return 0;
        return shieldSystem.CurrentShields / shieldSystem.MaxShields;
    }

    /// <summary>
    /// Get current shield value.
    /// </summary>
    public float GetCurrentShields()
    {
        return shieldSystem != null ? shieldSystem.CurrentShields : 0;
    }

    /// <summary>
    /// Get max shield value.
    /// </summary>
    public float GetMaxShields()
    {
        return shieldSystem != null ? shieldSystem.MaxShields : 0;
    }
}
