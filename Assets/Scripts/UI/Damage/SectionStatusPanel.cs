using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Visual ship diagram showing all 7 sections with damage state.
/// Uses OnGUI for rendering. Color-coded sections with armor/structure bars.
/// Click section to open detail popup.
/// </summary>
public class SectionStatusPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ship targetShip;
    [SerializeField] private SectionManager sectionManager;
    [SerializeField] private CoreProtectionSystem coreProtection;

    [Header("UI Settings")]
    [SerializeField] private bool isVisible = true;
    [SerializeField] private Vector2 panelPosition = new Vector2(10, 120);
    [SerializeField] private float panelWidth = 200f;
    [SerializeField] private float panelHeight = 280f;

    // Section layout positions (relative to panel center)
    private readonly Dictionary<SectionType, Vector2> sectionPositions = new Dictionary<SectionType, Vector2>
    {
        { SectionType.Fore, new Vector2(0, -60) },      // Top
        { SectionType.Aft, new Vector2(0, 60) },        // Bottom
        { SectionType.Port, new Vector2(-55, 0) },      // Left
        { SectionType.Starboard, new Vector2(55, 0) },  // Right
        { SectionType.Dorsal, new Vector2(-35, -30) },  // Top-left
        { SectionType.Ventral, new Vector2(35, -30) },  // Top-right
        { SectionType.Core, new Vector2(0, 0) }         // Center
    };

    // Section sizes
    private readonly Vector2 outerSectionSize = new Vector2(45, 35);
    private readonly Vector2 coreSectionSize = new Vector2(40, 40);

    // Events
    /// <summary>Fired when a section is clicked.</summary>
    public event Action<ShipSection> OnSectionClicked;

    // Properties
    public bool IsVisible => isVisible;
    public Ship TargetShip => targetShip;

    /// <summary>
    /// Initialize the panel with a target ship.
    /// </summary>
    public void Initialize(Ship ship)
    {
        targetShip = ship;

        if (targetShip != null)
        {
            sectionManager = targetShip.SectionManager;
            coreProtection = targetShip.CoreProtection;
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
    /// Set target ship for display.
    /// </summary>
    public void SetTarget(Ship ship)
    {
        Initialize(ship);
    }

    private void OnGUI()
    {
        if (!isVisible || sectionManager == null) return;

        // Panel background
        Rect panelRect = new Rect(panelPosition.x, panelPosition.y, panelWidth, panelHeight);
        GUI.Box(panelRect, "");

        // Title
        GUI.Label(new Rect(panelRect.x + 10, panelRect.y + 5, panelWidth - 20, 20), "<b>SECTION STATUS</b>");

        // Core exposure indicator
        bool coreExposed = coreProtection != null && coreProtection.IsCoreExposed();
        if (coreExposed)
        {
            GUI.color = new Color(1f, 0.5f, 0f); // Orange
            GUI.Label(new Rect(panelRect.x + 10, panelRect.y + 22, panelWidth - 20, 18), "<b>! CORE EXPOSED !</b>");
            GUI.color = Color.white;
        }

        // Calculate diagram center
        Vector2 diagramCenter = new Vector2(
            panelRect.x + panelWidth / 2,
            panelRect.y + 40 + panelHeight / 2 - 20
        );

        // Draw each section
        foreach (var section in sectionManager.GetAllSections())
        {
            if (section == null) continue;
            DrawSection(section, diagramCenter);
        }

        // Legend at bottom
        float legendY = panelRect.y + panelHeight - 45;
        GUI.Label(new Rect(panelRect.x + 5, legendY, panelWidth - 10, 40),
            "<size=9>Armor [---] Structure [---]\n" +
            "<color=#00FF00>OK</color> <color=#FFFF00>Damaged</color> " +
            "<color=#FF8800>Critical</color> <color=#FF0000>Breached</color></size>");
    }

    private void DrawSection(ShipSection section, Vector2 center)
    {
        if (!sectionPositions.TryGetValue(section.SectionType, out Vector2 offset))
            return;

        Vector2 size = section.SectionType == SectionType.Core ? coreSectionSize : outerSectionSize;
        Rect sectionRect = new Rect(
            center.x + offset.x - size.x / 2,
            center.y + offset.y - size.y / 2,
            size.x,
            size.y
        );

        // Get section color based on damage state
        Color sectionColor = GetSectionColor(section);
        GUI.color = sectionColor;

        // Draw section box
        GUI.Box(sectionRect, "");

        // Section label
        GUI.color = Color.white;
        string label = GetSectionAbbreviation(section.SectionType);
        GUI.Label(new Rect(sectionRect.x + 2, sectionRect.y + 1, sectionRect.width - 4, 14),
            $"<size=9><b>{label}</b></size>");

        // Armor bar
        float armorPercent = section.GetArmorPercentage();
        DrawMiniBar(new Rect(sectionRect.x + 2, sectionRect.y + 14, sectionRect.width - 4, 6),
            armorPercent, GetBarColor(armorPercent, false));

        // Structure bar
        float structurePercent = section.GetStructurePercentage();
        DrawMiniBar(new Rect(sectionRect.x + 2, sectionRect.y + 21, sectionRect.width - 4, 6),
            structurePercent, GetBarColor(structurePercent, true));

        // Handle click
        GUI.color = Color.white;
        if (GUI.Button(sectionRect, "", GUIStyle.none))
        {
            OnSectionClicked?.Invoke(section);
        }

        // Draw system count indicator if section has mounted systems
        if (section.SlotLayout != null && section.SlotLayout.MountedSystems.Count > 0)
        {
            int operational = 0;
            int damaged = 0;
            int destroyed = 0;

            foreach (var system in section.SlotLayout.MountedSystems)
            {
                if (system == null) continue;
                switch (system.CurrentState)
                {
                    case SystemState.Operational: operational++; break;
                    case SystemState.Damaged: damaged++; break;
                    case SystemState.Destroyed: destroyed++; break;
                }
            }

            // Draw small indicators
            float indicatorX = sectionRect.x + sectionRect.width - 10;
            float indicatorY = sectionRect.y + sectionRect.height - 10;

            if (destroyed > 0)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(indicatorX, indicatorY, 10, 10), "<size=8>X</size>");
            }
            else if (damaged > 0)
            {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(indicatorX, indicatorY, 10, 10), "<size=8>!</size>");
            }
            GUI.color = Color.white;
        }
    }

    private void DrawMiniBar(Rect rect, float percent, Color barColor)
    {
        // Background
        GUI.color = new Color(0.2f, 0.2f, 0.2f);
        GUI.Box(rect, "", GUI.skin.box);

        // Fill
        if (percent > 0)
        {
            GUI.color = barColor;
            Rect fillRect = new Rect(rect.x + 1, rect.y + 1, (rect.width - 2) * percent, rect.height - 2);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
        }

        GUI.color = Color.white;
    }

    /// <summary>
    /// Gets color for section based on damage state.
    /// Green -> Yellow -> Orange -> Red -> Dark Red (breached)
    /// </summary>
    public static Color GetSectionColor(ShipSection section)
    {
        if (section.IsBreached)
        {
            return new Color(0.3f, 0f, 0f); // Dark red/black for breached
        }

        float armorPercent = section.GetArmorPercentage();
        float structurePercent = section.GetStructurePercentage();

        // Structure critical (< 25%)
        if (structurePercent < 0.25f && structurePercent > 0)
        {
            return Color.red;
        }

        // Armor breached (structure taking damage)
        if (armorPercent <= 0)
        {
            return new Color(1f, 0.5f, 0f); // Orange
        }

        // Armor damaged (< 50%)
        if (armorPercent < 0.5f)
        {
            return Color.yellow;
        }

        // Healthy
        return Color.green;
    }

    private Color GetBarColor(float percent, bool isStructure)
    {
        if (percent <= 0) return Color.red;
        if (percent < 0.25f) return Color.red;
        if (percent < 0.5f) return isStructure ? new Color(1f, 0.5f, 0f) : Color.yellow;
        if (percent < 0.75f) return Color.yellow;
        return Color.green;
    }

    private string GetSectionAbbreviation(SectionType type)
    {
        switch (type)
        {
            case SectionType.Fore: return "FWD";
            case SectionType.Aft: return "AFT";
            case SectionType.Port: return "PRT";
            case SectionType.Starboard: return "STB";
            case SectionType.Dorsal: return "DRS";
            case SectionType.Ventral: return "VNT";
            case SectionType.Core: return "CORE";
            default: return "???";
        }
    }

    /// <summary>
    /// Gets section status summary for external display.
    /// </summary>
    public string GetStatusSummary()
    {
        if (sectionManager == null) return "No ship";

        int breached = 0;
        int damaged = 0;
        int healthy = 0;

        foreach (var section in sectionManager.GetAllSections())
        {
            if (section.IsBreached) breached++;
            else if (section.GetArmorPercentage() < 1f) damaged++;
            else healthy++;
        }

        return $"Sections: {healthy} OK, {damaged} Damaged, {breached} Breached";
    }
}
