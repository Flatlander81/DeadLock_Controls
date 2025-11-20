using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI panel for firing weapon groups at selected target.
/// Shown when enemy ship is selected.
/// Displays 4 weapon group buttons + Alpha Strike button.
/// Shows heat costs, warnings for out-of-arc and cooldown weapons.
/// </summary>
public class WeaponGroupPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private Rect panelRect = new Rect(Screen.width - 360, 100, 350, 500);
    [SerializeField] private bool isVisible = false;

    [Header("References")]
    private Ship playerShip;
    private Ship currentTarget;
    private WeaponManager weaponManager;
    private TargetingController targetingController;

    // Public properties
    public bool IsVisible
    {
        get => isVisible;
        set => isVisible = value;
    }

    /// <summary>
    /// Initialize panel with references.
    /// </summary>
    public void Initialize(Ship ship, TargetingController controller)
    {
        playerShip = ship;
        weaponManager = ship != null ? ship.WeaponManager : null;
        targetingController = controller;

        if (weaponManager == null)
        {
            Debug.LogWarning("WeaponGroupPanel: No WeaponManager found on player ship");
        }

        if (targetingController == null)
        {
            Debug.LogWarning("WeaponGroupPanel: No TargetingController provided");
        }
    }

    /// <summary>
    /// Set the current target.
    /// </summary>
    public void SetTarget(Ship target)
    {
        currentTarget = target;
    }

    /// <summary>
    /// Show the panel.
    /// </summary>
    public void Show()
    {
        isVisible = true;
    }

    /// <summary>
    /// Hide the panel.
    /// </summary>
    public void Hide()
    {
        isVisible = false;
    }

    /// <summary>
    /// Render UI panel.
    /// </summary>
    void OnGUI()
    {
        if (!isVisible || weaponManager == null || currentTarget == null) return;

        // Update panel position to be on right side of screen
        panelRect.x = Screen.width - 360;

        GUI.Box(panelRect, "Weapon Groups");

        float y = panelRect.y + 25;
        float x = panelRect.x + 10;

        // Target info
        GUI.Label(new Rect(x, y, 330, 20), $"Target: {currentTarget.gameObject.name}");
        y += 25;

        GUI.Label(new Rect(x, y, 330, 20), "=== FIRE WEAPONS ===");
        y += 25;

        // Draw 4 weapon group buttons
        for (int groupNum = 1; groupNum <= 4; groupNum++)
        {
            DrawGroupButton(groupNum, ref x, ref y);
            y += 10; // Spacing between groups
        }

        // Alpha Strike button
        y += 10;
        DrawAlphaStrikeButton(ref x, ref y);

        // Instructions
        y += 20;
        GUI.Label(new Rect(x, y, 330, 20), "Hotkeys: 1-4 for groups, A for Alpha Strike");
    }

    /// <summary>
    /// Draw a single weapon group button with status.
    /// </summary>
    private void DrawGroupButton(int groupNum, ref float x, ref float y)
    {
        List<WeaponSystem> groupWeapons = weaponManager.GetWeaponsInGroup(groupNum);

        if (groupWeapons.Count == 0)
        {
            // Empty group - show grayed out
            GUI.color = Color.gray;
            GUI.Label(new Rect(x, y, 330, 20), $"Group {groupNum}: [Empty]");
            GUI.color = Color.white;
            y += 22;
            return;
        }

        // Check group status
        bool anyOnCooldown = false;
        bool anyOutOfArc = false;
        int totalHeat = 0;

        foreach (WeaponSystem weapon in groupWeapons)
        {
            if (weapon.CurrentCooldown > 0)
                anyOnCooldown = true;

            if (currentTarget != null && !weapon.IsInArc(currentTarget.transform.position))
                anyOutOfArc = true;

            if (weapon.CanFire())
                totalHeat += weapon.HeatCost;
        }

        // Group button with color coding
        Color groupColor = TargetingLineRenderer.GetGroupColor(groupNum);
        GUI.backgroundColor = groupColor;

        string buttonLabel = $"Group {groupNum}";
        if (GUI.Button(new Rect(x, y, 100, 20), buttonLabel))
        {
            // Fire this group
            if (targetingController != null)
            {
                targetingController.FireGroupAtCurrentTarget(groupNum);
            }
        }

        GUI.backgroundColor = Color.white;

        // Heat cost
        GUI.Label(new Rect(x + 105, y, 80, 20), $"Heat: {totalHeat}");

        // Warnings
        string warnings = "";
        if (anyOnCooldown)
            warnings += "[COOLDOWN] ";
        if (anyOutOfArc)
            warnings += "[OUT OF ARC]";

        if (warnings.Length > 0)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(x + 190, y, 140, 20), warnings);
            GUI.color = Color.white;
        }

        y += 22;

        // List weapons in group
        foreach (WeaponSystem weapon in groupWeapons)
        {
            string weaponStatus = $"  • {weapon.WeaponName}";

            // Show individual weapon status
            if (weapon.CurrentCooldown > 0)
            {
                GUI.color = Color.gray;
                weaponStatus += $" (CD:{weapon.CurrentCooldown})";
            }
            else if (currentTarget != null && !weapon.IsInArc(currentTarget.transform.position))
            {
                GUI.color = Color.yellow;
                weaponStatus += " (No Arc)";
            }
            else
            {
                GUI.color = Color.green;
                weaponStatus += " (Ready)";
            }

            GUI.Label(new Rect(x, y, 330, 20), weaponStatus);
            GUI.color = Color.white;
            y += 18;
        }
    }

    /// <summary>
    /// Draw Alpha Strike button.
    /// </summary>
    private void DrawAlphaStrikeButton(ref float x, ref float y)
    {
        GUI.Label(new Rect(x, y, 330, 20), "=== ALPHA STRIKE ===");
        y += 22;

        // Calculate total heat and readiness
        int totalWeapons = weaponManager.Weapons.Count;
        int readyWeapons = 0;
        int totalHeat = 0;

        foreach (WeaponSystem weapon in weaponManager.Weapons)
        {
            if (weapon.CanFire())
            {
                readyWeapons++;
                totalHeat += weapon.HeatCost;
            }
        }

        // Alpha Strike button - larger and more prominent
        GUI.backgroundColor = Color.red;
        string alphaLabel = $"FIRE ALL WEAPONS ({readyWeapons}/{totalWeapons} ready)";

        if (GUI.Button(new Rect(x, y, 330, 30), alphaLabel))
        {
            // Fire alpha strike
            if (targetingController != null)
            {
                targetingController.AlphaStrikeCurrentTarget();
            }
        }

        GUI.backgroundColor = Color.white;
        y += 35;

        // Heat warning
        GUI.Label(new Rect(x, y, 330, 20), $"Total Heat: {totalHeat}");
        y += 22;

        // Check heat capacity
        if (playerShip != null && playerShip.HeatManager != null)
        {
            float currentHeat = playerShip.HeatManager.CurrentHeat;
            float maxHeat = playerShip.HeatManager.MaxHeat;
            float projectedHeat = currentHeat + totalHeat;

            if (projectedHeat > maxHeat)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(x, y, 330, 20), $"WARNING: Will exceed heat capacity!");
                GUI.color = Color.white;
                y += 20;
            }
        }
    }
}
