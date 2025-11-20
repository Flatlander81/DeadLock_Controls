using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI panel for configuring weapon group assignments.
/// Shown when player ship (Hephaestus) is selected.
/// Displays all weapons and allows group assignment.
/// </summary>
public class WeaponConfigPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private Rect panelRect = new Rect(10, 100, 350, 400);
    [SerializeField] private bool isVisible = false;

    [Header("References")]
    private Ship playerShip;
    private WeaponManager weaponManager;

    // Public properties
    public bool IsVisible
    {
        get => isVisible;
        set => isVisible = value;
    }

    /// <summary>
    /// Initialize panel with player ship reference.
    /// </summary>
    public void Initialize(Ship ship)
    {
        playerShip = ship;
        weaponManager = ship != null ? ship.WeaponManager : null;

        if (weaponManager == null)
        {
            Debug.LogWarning("WeaponConfigPanel: No WeaponManager found on player ship");
        }
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
        if (!isVisible || weaponManager == null) return;

        GUI.Box(panelRect, "Weapon Configuration");

        float y = panelRect.y + 25;
        float x = panelRect.x + 10;

        GUI.Label(new Rect(x, y, 330, 20), "=== HEPHAESTUS WEAPONS ===");
        y += 25;

        // Get all weapons
        List<WeaponSystem> weapons = weaponManager.Weapons;

        if (weapons.Count == 0)
        {
            GUI.Label(new Rect(x, y, 330, 20), "No weapons installed");
            return;
        }

        // Display each weapon
        foreach (WeaponSystem weapon in weapons)
        {
            DrawWeaponEntry(weapon, ref x, ref y);
            y += 5; // Spacing
        }

        // Instructions at bottom
        y += 10;
        GUI.Label(new Rect(x, y, 330, 20), "Click dropdown to change group");
        y += 20;
        GUI.Label(new Rect(x, y, 330, 20), "Groups: 1=Blue, 2=Red, 3=Green, 4=Yellow");
    }

    /// <summary>
    /// Draw a single weapon entry with group dropdown.
    /// </summary>
    private void DrawWeaponEntry(WeaponSystem weapon, ref float x, ref float y)
    {
        // Weapon name
        GUI.Label(new Rect(x, y, 150, 20), weapon.WeaponName);

        // Group dropdown
        string[] groupOptions = { "Unassigned", "Group 1", "Group 2", "Group 3", "Group 4" };
        int currentGroup = weapon.AssignedGroup;

        // Create button that cycles through groups
        string buttonLabel = currentGroup == 0 ? "Unassigned" : $"Group {currentGroup}";
        Color buttonColor = TargetingLineRenderer.GetGroupColor(currentGroup);

        // Color the button
        GUI.backgroundColor = buttonColor;

        if (GUI.Button(new Rect(x + 155, y, 85, 20), buttonLabel))
        {
            // Cycle to next group
            int newGroup = (currentGroup + 1) % 5; // 0,1,2,3,4, back to 0
            weaponManager.AssignWeaponToGroup(weapon, newGroup);
            Debug.Log($"Assigned {weapon.WeaponName} to group {newGroup}");
        }

        GUI.backgroundColor = Color.white;

        // Weapon stats
        y += 22;
        string stats = $"  Arc: {weapon.FiringArc}°  Range: {weapon.MaxRange}  Heat: {weapon.HeatCost}";

        // Gray out if on cooldown
        if (weapon.CurrentCooldown > 0)
        {
            GUI.color = Color.gray;
            stats += $"  [COOLDOWN: {weapon.CurrentCooldown}]";
        }

        GUI.Label(new Rect(x, y, 330, 20), stats);
        GUI.color = Color.white;

        y += 22;
    }
}
