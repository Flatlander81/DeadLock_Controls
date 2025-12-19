using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Detailed view of a single ship section.
/// Shows all mounted systems with health, slot layout, and exact values.
/// </summary>
public class SectionDetailPopup : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private bool isVisible = false;
    [SerializeField] private Vector2 popupPosition = new Vector2(220, 120);
    [SerializeField] private float popupWidth = 250f;
    [SerializeField] private float popupHeight = 300f;

    // Current section being displayed
    private ShipSection currentSection;
    private Vector2 scrollPosition;

    // Properties
    public bool IsVisible => isVisible;
    public ShipSection CurrentSection => currentSection;

    /// <summary>
    /// Show popup for a specific section.
    /// </summary>
    public void ShowSection(ShipSection section)
    {
        currentSection = section;
        isVisible = true;
        scrollPosition = Vector2.zero;
    }

    /// <summary>
    /// Hide the popup.
    /// </summary>
    public void Hide()
    {
        isVisible = false;
        currentSection = null;
    }

    /// <summary>
    /// Toggle visibility.
    /// </summary>
    public void Toggle()
    {
        if (isVisible)
            Hide();
        else if (currentSection != null)
            isVisible = true;
    }

    private void OnGUI()
    {
        if (!isVisible || currentSection == null) return;

        Rect popupRect = new Rect(popupPosition.x, popupPosition.y, popupWidth, popupHeight);

        // Background
        GUI.Box(popupRect, "");

        // Header with close button
        GUI.Label(new Rect(popupRect.x + 10, popupRect.y + 5, popupWidth - 50, 20),
            $"<b>{currentSection.SectionType} SECTION</b>");

        if (GUI.Button(new Rect(popupRect.x + popupWidth - 25, popupRect.y + 5, 20, 20), "X"))
        {
            Hide();
            return;
        }

        float y = popupRect.y + 30;

        // Section status indicator
        Color statusColor = SectionStatusPanel.GetSectionColor(currentSection);
        GUI.color = statusColor;
        string statusText = currentSection.IsBreached ? "BREACHED" :
            (currentSection.GetArmorPercentage() <= 0 ? "ARMOR GONE" : "OPERATIONAL");
        GUI.Label(new Rect(popupRect.x + 10, y, popupWidth - 20, 20), $"<b>Status: {statusText}</b>");
        GUI.color = Color.white;
        y += 22;

        // Armor bar with values
        DrawLabeledBar(popupRect.x + 10, y, popupWidth - 20, "Armor",
            currentSection.CurrentArmor, currentSection.MaxArmor, Color.cyan);
        y += 35;

        // Structure bar with values
        DrawLabeledBar(popupRect.x + 10, y, popupWidth - 20, "Structure",
            currentSection.CurrentStructure, currentSection.MaxStructure, Color.green);
        y += 40;

        // Separator
        GUI.Box(new Rect(popupRect.x + 10, y, popupWidth - 20, 2), "");
        y += 8;

        // Mounted systems header
        GUI.Label(new Rect(popupRect.x + 10, y, popupWidth - 20, 20), "<b>MOUNTED SYSTEMS:</b>");
        y += 22;

        // Systems list with scroll
        if (currentSection.SlotLayout != null && currentSection.SlotLayout.MountedSystems.Count > 0)
        {
            float listHeight = popupRect.y + popupHeight - y - 10;
            Rect listRect = new Rect(popupRect.x + 10, y, popupWidth - 20, listHeight);

            // Calculate content height
            float contentHeight = currentSection.SlotLayout.MountedSystems.Count * 45f;

            // Scroll view
            scrollPosition = GUI.BeginScrollView(
                listRect,
                scrollPosition,
                new Rect(0, 0, popupWidth - 40, contentHeight)
            );

            float systemY = 0;
            foreach (var system in currentSection.SlotLayout.MountedSystems)
            {
                if (system == null) continue;
                DrawSystemEntry(0, systemY, popupWidth - 40, system);
                systemY += 45;
            }

            GUI.EndScrollView();
        }
        else
        {
            GUI.color = Color.gray;
            GUI.Label(new Rect(popupRect.x + 10, y, popupWidth - 20, 20), "No systems mounted");
            GUI.color = Color.white;
        }

        // Slot layout summary
        if (currentSection.SlotLayout != null)
        {
            float footerY = popupRect.y + popupHeight - 25;
            int usedSlots = currentSection.SlotLayout.GetOccupiedSlotCount();
            int totalSlots = currentSection.SlotLayout.TotalSlots;
            GUI.Label(new Rect(popupRect.x + 10, footerY, popupWidth - 20, 20),
                $"<size=10>Slots: {usedSlots}/{totalSlots} used</size>");
        }
    }

    private void DrawLabeledBar(float x, float y, float width, string label, float current, float max, Color barColor)
    {
        // Label
        GUI.Label(new Rect(x, y, width, 16), $"{label}: {current:F0}/{max:F0}");

        // Bar background
        Rect barRect = new Rect(x, y + 16, width, 14);
        GUI.color = new Color(0.2f, 0.2f, 0.2f);
        GUI.Box(barRect, "");

        // Bar fill
        float percent = max > 0 ? current / max : 0;
        if (percent > 0)
        {
            // Color based on percentage
            Color fillColor = percent > 0.5f ? barColor :
                (percent > 0.25f ? Color.yellow : Color.red);

            GUI.color = fillColor;
            Rect fillRect = new Rect(barRect.x + 1, barRect.y + 1, (barRect.width - 2) * percent, barRect.height - 2);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
        }

        GUI.color = Color.white;
    }

    private void DrawSystemEntry(float x, float y, float width, MountedSystem system)
    {
        // System name
        string systemName = ShipSystemData.GetName(system.SystemType);
        GUI.Label(new Rect(x, y, width, 18), $"<b>{systemName}</b>");

        // State indicator
        Color stateColor = GetSystemStateColor(system.CurrentState);
        GUI.color = stateColor;
        string stateText = system.CurrentState.ToString().ToUpper();
        GUI.Label(new Rect(x, y + 16, width, 16), stateText);
        GUI.color = Color.white;

        // Slot info
        GUI.Label(new Rect(x, y + 30, width, 14),
            $"<size=10>Slot Size: {system.Size} | Slots {system.SlotStart}-{system.SlotEnd}</size>");
    }

    /// <summary>
    /// Gets color for system state display.
    /// </summary>
    public static Color GetSystemStateColor(SystemState state)
    {
        switch (state)
        {
            case SystemState.Operational: return Color.green;
            case SystemState.Damaged: return Color.yellow;
            case SystemState.Destroyed: return Color.red;
            default: return Color.white;
        }
    }

    /// <summary>
    /// Set popup position.
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        popupPosition = position;
    }
}
