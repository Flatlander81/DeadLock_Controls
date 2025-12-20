using UnityEngine;
using System.Text;

/// <summary>
/// Debug UI overlay for testing and development.
/// Displays ship status, heat system, abilities, and control buttons.
/// </summary>
public class DebugUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ship targetShip;
    [SerializeField] private MovementController movementController;

    // Cached StringBuilders to avoid per-frame allocations
    private readonly StringBuilder heatTextBuilder = new StringBuilder(64);
    private readonly StringBuilder abilityTextBuilder = new StringBuilder(64);

    // Accessors
    public Ship TargetShip => targetShip;

    /// <summary>
    /// Set the target ship to display.
    /// </summary>
    public void SetTargetShip(Ship ship)
    {
        targetShip = ship;
    }

    /// <summary>
    /// Set the movement controller reference.
    /// </summary>
    public void SetMovementController(MovementController controller)
    {
        movementController = controller;
    }

    /// <summary>
    /// Draw all debug UI panels.
    /// </summary>
    private void OnGUI()
    {
        DrawControlsPanel();
        DrawPhaseIndicator();

        if (targetShip != null)
        {
            DrawHeatDebugPanel();
        }
    }

    /// <summary>
    /// Draws the controls help panel (top-left).
    /// </summary>
    private void DrawControlsPanel()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 260));
        GUILayout.Box("Controls");

        if (movementController != null)
        {
            bool isMovementActive = movementController.IsMovementModeActive;
            string moveModeStatus = isMovementActive ? " (ACTIVE)" : "";
            GUILayout.Label($"M - Movement Mode{moveModeStatus}");

            if (isMovementActive)
            {
                GUILayout.Label("  Drag projection to move");
                GUILayout.Label("  E - Elevation mode");
                GUILayout.Label("  R - Rotation mode");
                GUILayout.Label("  Scroll - Adjust elevation");
                GUILayout.Label("  Arrows - Adjust rotation");
                GUILayout.Label("  Enter/Space - Confirm");
                GUILayout.Label("  Esc - Exit mode");
            }
        }

        GUILayout.Label("");
        GUILayout.Label("Camera Controls:");
        GUILayout.Label("  Shift+Drag - Orbit");
        GUILayout.Label("  Ctrl+Drag - Pan");
        GUILayout.Label("  Q/E - Orbit left/right");
        GUILayout.Label("  WASD - Pan");
        GUILayout.Label("  R/F - Zoom in/out");
        GUILayout.Label("  Scroll - Zoom");
        GUILayout.Label("");
        GUILayout.Label("Abilities:");
        GUILayout.Label("  1-6 - Activate ability");
        GUILayout.EndArea();

        // Current adjustment mode indicator
        if (movementController != null)
        {
            var adjustmentMode = movementController.CurrentAdjustmentMode;
            if (adjustmentMode != MovementController.AdjustmentMode.None && targetShip != null)
            {
                GUILayout.BeginArea(new Rect(10, 220, 300, 50));
                GUILayout.Box($"Mode: {adjustmentMode}");
                GUILayout.Label($"Ship: {targetShip.gameObject.name}");
                GUILayout.EndArea();
            }
        }
    }

    /// <summary>
    /// Draws the phase indicator and End Turn button (top-right).
    /// </summary>
    private void DrawPhaseIndicator()
    {
        if (TurnManager.Instance != null)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 160, 10, 150, 60));
            GUILayout.Box($"Phase: {TurnManager.Instance.CurrentPhase}");
            GUILayout.EndArea();

            // End Turn button (only visible during Command phase)
            if (TurnManager.Instance.CurrentPhase == TurnPhase.Command)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 160, 80, 150, 50));
                if (GUILayout.Button("End Turn", GUILayout.Height(40)))
                {
                    TurnManager.Instance.EndCommandPhase();

                    // Exit movement mode but keep ship selected
                    if (movementController != null)
                    {
                        movementController.ExitMovementMode();
                    }
                }
                GUILayout.EndArea();
            }
        }
    }

    /// <summary>
    /// Draws the Heat System debug panel for testing.
    /// </summary>
    private void DrawHeatDebugPanel()
    {
        if (targetShip == null || targetShip.HeatManager == null) return;

        HeatManager heatManager = targetShip.HeatManager;
        HeatManager.HeatPenalties penalties = heatManager.GetPenalties();

        int panelX = Screen.width - 220;
        int panelY = 140;
        int panelWidth = 210;

        // Heat status display
        GUILayout.BeginArea(new Rect(panelX, panelY, panelWidth, 240));
        GUILayout.Box("Heat System Debug");

        // Heat info - use StringBuilder to avoid allocations
        heatTextBuilder.Clear();
        heatTextBuilder.Append("Heat: ");
        heatTextBuilder.Append(heatManager.CurrentHeat.ToString("F0"));
        heatTextBuilder.Append("/");
        heatTextBuilder.Append(heatManager.MaxHeat.ToString("F0"));
        if (heatManager.PlannedHeat > 0.1f)
        {
            heatTextBuilder.Append(" +");
            heatTextBuilder.Append(heatManager.PlannedHeat.ToString("F0"));
        }
        GUILayout.Label(heatTextBuilder.ToString());
        GUILayout.Label($"Tier: {heatManager.GetCurrentTier()}");

        // Heat bar visualization with planned heat overlay
        Rect barRect = GUILayoutUtility.GetRect(panelWidth - 20, 20);

        // Background (gray)
        GUI.DrawTexture(barRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.gray, 0, 0);

        // Current heat (tier color - green/yellow/orange/red)
        float currentFillAmount = heatManager.CurrentHeat / heatManager.MaxHeat;
        Rect currentFillRect = new Rect(barRect.x, barRect.y, barRect.width * currentFillAmount, barRect.height);
        GUI.DrawTexture(currentFillRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, heatManager.GetTierColor(), 0, 0);

        // Planned heat (yellow overlay)
        if (heatManager.PlannedHeat > 0.1f)
        {
            float plannedFillAmount = (heatManager.CurrentHeat + heatManager.PlannedHeat) / heatManager.MaxHeat;
            Rect plannedFillRect = new Rect(barRect.x, barRect.y, barRect.width * plannedFillAmount, barRect.height);
            Color plannedColor = new Color(1.0f, 1.0f, 0.0f, 0.6f); // Yellow with 60% opacity
            GUI.DrawTexture(plannedFillRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, plannedColor, 0, 0);
        }

        GUILayout.Space(5);

        // Penalties
        GUILayout.Label($"Speed: {penalties.SpeedMultiplier * 100:F0}%");
        GUILayout.Label($"Accuracy: {penalties.AccuracyMultiplier * 100:F0}%");
        if (penalties.HullDamagePerTurn > 0)
            GUILayout.Label($"Heat Dmg: {penalties.HullDamagePerTurn:F0}/turn");

        GUILayout.Space(5);

        // Ship status
        GUILayout.Label($"Hull: {targetShip.CurrentHull:F0}/{targetShip.MaxHull:F0}");
        GUILayout.Label($"Shields: {targetShip.CurrentShields:F0}/{targetShip.MaxShields:F0}");

        GUILayout.EndArea();

        // Ability UI panel
        DrawAbilityPanel(panelX, panelY + 250);

        // Heat control buttons
        GUILayout.BeginArea(new Rect(panelX, panelY + 380, panelWidth, 220));

        // Check if we're in Command phase for debug buttons
        bool isCommandPhase = TurnManager.Instance != null && TurnManager.Instance.CurrentPhase == TurnPhase.Command;

        // Movement mode toggle button - only during Command phase
        if (movementController != null)
        {
            bool isMovementActive = movementController.IsMovementModeActive;
            string moveButtonText = isMovementActive ? "Exit Move (M)" : "Move Ship (M)";
            GUI.enabled = isCommandPhase;
            if (GUILayout.Button(moveButtonText, GUILayout.Height(30)))
            {
                movementController.ToggleMovementMode();
            }
            GUI.enabled = true;

            GUILayout.Space(5);
        }

        // Debug buttons - always enabled (they're for testing, not gameplay)
        if (GUILayout.Button("+20 Heat"))
        {
            heatManager.AddPlannedHeat(20f);
            heatManager.CommitPlannedHeat();
        }

        if (GUILayout.Button("+50 Heat"))
        {
            heatManager.AddPlannedHeat(50f);
            heatManager.CommitPlannedHeat();
        }

        if (GUILayout.Button("-50 Cooling"))
        {
            heatManager.InstantCooling(50f);
        }

        if (GUILayout.Button("Damage -100"))
        {
            targetShip.TakeDamage(100f);
        }

        if (GUILayout.Button("Reset Heat"))
        {
            heatManager.InstantCooling(heatManager.CurrentHeat);
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// Draws the Ability UI panel showing all abilities with their status.
    /// </summary>
    private void DrawAbilityPanel(int panelX, int panelY)
    {
        if (targetShip == null || targetShip.AbilitySystem == null) return;

        AbilitySystem abilitySystem = targetShip.AbilitySystem;
        var abilitySlots = abilitySystem.AbilitySlots;

        if (abilitySlots.Count == 0) return;

        // Check if we're in Command phase
        bool isCommandPhase = TurnManager.Instance != null && TurnManager.Instance.CurrentPhase == TurnPhase.Command;

        GUILayout.BeginArea(new Rect(panelX, panelY, 210, 190));
        GUILayout.Box("Abilities (Keys 1-6)");

        for (int i = 0; i < abilitySlots.Count && i < 6; i++)
        {
            AbilitySystem.AbilitySlot slot = abilitySlots[i];
            if (slot.abilityData == null) continue;

            // Determine button color based on state
            Color buttonColor = GUI.backgroundColor;
            if (slot.IsOnCooldown)
            {
                // On cooldown - gray
                GUI.backgroundColor = Color.gray;
            }
            else if (!slot.CanActivate)
            {
                // Can't activate - yellow
                GUI.backgroundColor = Color.yellow;
            }
            else if (slot.isQueued)
            {
                // Queued - green
                GUI.backgroundColor = Color.green;
            }

            // Build button text - use StringBuilder to avoid allocations
            abilityTextBuilder.Clear();
            abilityTextBuilder.Append(i + 1);
            abilityTextBuilder.Append(". ");
            abilityTextBuilder.Append(slot.abilityData.abilityName);
            if (slot.IsOnCooldown)
            {
                abilityTextBuilder.Append(" (CD:");
                abilityTextBuilder.Append(slot.currentCooldown);
                abilityTextBuilder.Append(")");
            }
            else if (slot.isQueued)
            {
                abilityTextBuilder.Append(" [QUEUED]");
            }
            else
            {
                abilityTextBuilder.Append(" (Heat:");
                abilityTextBuilder.Append(slot.abilityData.heatCost);
                abilityTextBuilder.Append(")");
            }
            string buttonText = abilityTextBuilder.ToString();

            // Button click - only allow during Command phase
            GUI.enabled = isCommandPhase;
            if (GUILayout.Button(buttonText, GUILayout.Height(25)))
            {
                abilitySystem.TryActivateAbilityByIndex(i);
            }
            GUI.enabled = true;

            // Restore color
            GUI.backgroundColor = buttonColor;
        }

        GUILayout.EndArea();
    }
}
