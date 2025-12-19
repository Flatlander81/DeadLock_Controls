using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Debug visualizer for slot layouts.
/// Shows slot layout as a bar with color-coded systems.
/// </summary>
public class SlotLayoutVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipSection targetSection;
    [SerializeField] private SectionManager sectionManager;

    [Header("Display Settings")]
    [SerializeField] private bool showInGUI = true;
    [SerializeField] private Vector2 guiPosition = new Vector2(420, 10);
    [SerializeField] private float barWidth = 300f;
    [SerializeField] private float barHeight = 25f;
    [SerializeField] private float slotSpacing = 1f;

    [Header("Colors")]
    [SerializeField] private Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color damagedOverlay = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private Color destroyedOverlay = new Color(0f, 0f, 0f, 0.7f);

    private GUIStyle labelStyle;
    private GUIStyle boxStyle;

    /// <summary>
    /// Sets the target section to visualize.
    /// </summary>
    public void SetTargetSection(ShipSection section)
    {
        targetSection = section;
    }

    /// <summary>
    /// Sets the section manager for multi-section visualization.
    /// </summary>
    public void SetSectionManager(SectionManager manager)
    {
        sectionManager = manager;
    }

    private void OnGUI()
    {
        if (!showInGUI) return;

        InitStyles();

        float yOffset = guiPosition.y;

        // If we have a section manager, show all sections
        if (sectionManager != null)
        {
            GUI.Label(new Rect(guiPosition.x, yOffset, barWidth, 20), "=== SLOT LAYOUTS ===", labelStyle);
            yOffset += 25;

            foreach (var section in sectionManager.GetAllSections())
            {
                if (section != null && section.SlotLayout != null)
                {
                    yOffset = DrawSectionLayout(section, guiPosition.x, yOffset);
                    yOffset += 10;
                }
            }
        }
        // Otherwise just show the target section
        else if (targetSection != null && targetSection.SlotLayout != null)
        {
            GUI.Label(new Rect(guiPosition.x, yOffset, barWidth, 20), "=== SLOT LAYOUT ===", labelStyle);
            yOffset += 25;
            DrawSectionLayout(targetSection, guiPosition.x, yOffset);
        }
    }

    private float DrawSectionLayout(ShipSection section, float x, float y)
    {
        SlotLayout layout = section.SlotLayout;
        if (layout == null || layout.TotalSlots <= 0) return y;

        // Section header
        string header = $"{section.SectionType} ({layout.GetOccupiedSlotCount()}/{layout.TotalSlots} slots)";
        GUI.Label(new Rect(x, y, barWidth, 20), header, labelStyle);
        y += 22;

        // Calculate slot width
        float totalSpacing = (layout.TotalSlots - 1) * slotSpacing;
        float slotWidth = (barWidth - totalSpacing) / layout.TotalSlots;

        // Draw each slot
        float slotX = x;
        int currentSlot = 1;

        while (currentSlot <= layout.TotalSlots)
        {
            MountedSystem system = layout.GetSystemAtSlot(currentSlot);

            if (system != null)
            {
                // Draw system spanning its slots
                int systemSlots = system.Size;
                float systemWidth = systemSlots * slotWidth + (systemSlots - 1) * slotSpacing;

                // Base color
                Color baseColor = ShipSystemData.GetColor(system.SystemType);

                // Apply state overlay
                if (system.IsDestroyed)
                {
                    GUI.color = Color.Lerp(baseColor, Color.black, 0.7f);
                }
                else if (system.IsDamaged)
                {
                    GUI.color = Color.Lerp(baseColor, Color.yellow, 0.4f);
                }
                else
                {
                    GUI.color = baseColor;
                }

                GUI.Box(new Rect(slotX, y, systemWidth, barHeight), "");

                // Draw system name
                GUI.color = Color.white;
                string shortName = GetShortName(system.SystemType);
                string stateChar = system.IsDestroyed ? "X" : (system.IsDamaged ? "!" : "");
                GUI.Label(new Rect(slotX + 2, y + 3, systemWidth - 4, barHeight - 6),
                    $"{shortName}{stateChar}", labelStyle);

                slotX += systemWidth + slotSpacing;
                currentSlot += systemSlots;
            }
            else
            {
                // Empty slot
                GUI.color = emptySlotColor;
                GUI.Box(new Rect(slotX, y, slotWidth, barHeight), "");

                slotX += slotWidth + slotSpacing;
                currentSlot++;
            }
        }

        GUI.color = Color.white;

        // Draw system list below
        y += barHeight + 5;

        foreach (var system in layout.MountedSystems)
        {
            string state = system.CurrentState.ToString();
            string line = $"  {ShipSystemData.GetName(system.SystemType)} [{system.SlotStart}-{system.SlotEnd}]: {state}";

            Color textColor = Color.white;
            if (system.IsDestroyed) textColor = Color.gray;
            else if (system.IsDamaged) textColor = Color.yellow;

            GUI.color = textColor;
            GUI.Label(new Rect(x, y, barWidth, 18), line, labelStyle);
            y += 18;
        }

        GUI.color = Color.white;
        return y;
    }

    private string GetShortName(ShipSystemType type)
    {
        switch (type)
        {
            case ShipSystemType.ReactorCore: return "Reactor";
            case ShipSystemType.MainEngine: return "Engine";
            case ShipSystemType.NewtonianCannon: return "Cannon";
            case ShipSystemType.TorpedoLauncher: return "Torp.L";
            case ShipSystemType.TorpedoMagazine: return "Torp.M";
            case ShipSystemType.MissileBattery: return "Missile";
            case ShipSystemType.MissileMagazine: return "Miss.M";
            case ShipSystemType.RailGun: return "Rail";
            case ShipSystemType.PDTurret: return "PD";
            case ShipSystemType.Sensors: return "Sensors";
            case ShipSystemType.Radiator: return "Rad";
            default: return type.ToString().Substring(0, 4);
        }
    }

    private void InitStyles()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 10;
            labelStyle.alignment = TextAnchor.MiddleLeft;
        }

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
        }
    }

    /// <summary>
    /// Draw gizmos showing slot layout in Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (targetSection == null || targetSection.SlotLayout == null) return;

        SlotLayout layout = targetSection.SlotLayout;
        Vector3 basePos = targetSection.transform.position + Vector3.up * 2f;

        float totalWidth = 5f;
        float slotWidth = totalWidth / layout.TotalSlots;
        float startX = -totalWidth / 2f;

        for (int slot = 1; slot <= layout.TotalSlots; slot++)
        {
            MountedSystem system = layout.GetSystemAtSlot(slot);

            if (system != null)
            {
                Gizmos.color = system.GetDebugColor();
            }
            else
            {
                Gizmos.color = emptySlotColor;
            }

            Vector3 slotPos = basePos + targetSection.transform.right * (startX + (slot - 0.5f) * slotWidth);
            Gizmos.DrawCube(slotPos, new Vector3(slotWidth * 0.9f, 0.3f, 0.3f));
        }
    }
}
