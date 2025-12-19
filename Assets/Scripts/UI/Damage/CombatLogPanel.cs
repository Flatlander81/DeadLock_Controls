using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Scrolling log of combat events.
/// Categories: Hits, Criticals, Breaches, System Damage.
/// Color-coded by severity with timestamps.
/// </summary>
public class CombatLogPanel : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private bool isVisible = true;
    [SerializeField] private Vector2 panelPosition = new Vector2(10, 410);
    [SerializeField] private float panelWidth = 350f;
    [SerializeField] private float panelHeight = 200f;

    [Header("Log Settings")]
    [SerializeField] private int maxLogEntries = 100;
    [SerializeField] private bool showTimestamps = true;
    [SerializeField] private bool autoScroll = true;

    [Header("Filters")]
    [SerializeField] private bool showHits = true;
    [SerializeField] private bool showCriticals = true;
    [SerializeField] private bool showBreaches = true;
    [SerializeField] private bool showSystemDamage = true;
    [SerializeField] private bool showShieldEvents = true;

    // Log storage
    private List<CombatLogEntry> logEntries = new List<CombatLogEntry>();
    private Vector2 scrollPosition;
    private StringBuilder displayBuilder = new StringBuilder();

    // Cached filtered entries for display
    private List<CombatLogEntry> filteredEntries = new List<CombatLogEntry>();
    private bool filtersDirty = true;

    /// <summary>
    /// Combat log entry categories.
    /// </summary>
    public enum LogCategory
    {
        Hit,
        Critical,
        Breach,
        SystemDamage,
        Shield,
        Death
    }

    /// <summary>
    /// A single combat log entry.
    /// </summary>
    public struct CombatLogEntry
    {
        public float Timestamp;
        public LogCategory Category;
        public string Message;
        public Color MessageColor;

        public CombatLogEntry(LogCategory category, string message, Color color)
        {
            Timestamp = Time.time;
            Category = category;
            Message = message;
            MessageColor = color;
        }
    }

    // Properties
    public bool IsVisible => isVisible;
    public int EntryCount => logEntries.Count;

    /// <summary>
    /// Add a hit event to the log.
    /// </summary>
    public void LogHit(string shipName, SectionType section, float damage, float shieldDamage, float armorDamage, float structureDamage)
    {
        string msg;
        if (shieldDamage > 0 && armorDamage <= 0)
        {
            msg = $"{shipName} hit! {section}: {shieldDamage:F0} shield damage";
        }
        else if (structureDamage > 0)
        {
            msg = $"{shipName} hit! {section}: {armorDamage:F0} armor, {structureDamage:F0} structure";
        }
        else
        {
            msg = $"{shipName} hit! {section}: {armorDamage:F0} armor damage";
        }

        AddEntry(new CombatLogEntry(LogCategory.Hit, msg, Color.white));
    }

    /// <summary>
    /// Add a critical hit event to the log.
    /// </summary>
    public void LogCritical(string shipName, SectionType section, ShipSystemType systemType, bool wasDestroyed)
    {
        string systemName = ShipSystemData.GetName(systemType);
        string result = wasDestroyed ? "DESTROYED" : "DAMAGED";
        Color color = wasDestroyed ? Color.red : Color.yellow;

        string msg = $"CRITICAL! {shipName} {section}: {systemName} {result}!";
        AddEntry(new CombatLogEntry(LogCategory.Critical, msg, color));
    }

    /// <summary>
    /// Add a section breach event to the log.
    /// </summary>
    public void LogBreach(string shipName, SectionType section)
    {
        string msg = $"BREACH! {shipName} {section} section breached!";
        AddEntry(new CombatLogEntry(LogCategory.Breach, msg, new Color(1f, 0.3f, 0f))); // Orange-red
    }

    /// <summary>
    /// Add a system damage event to the log.
    /// </summary>
    public void LogSystemDamage(string shipName, ShipSystemType systemType, SystemState newState)
    {
        string systemName = ShipSystemData.GetName(systemType);
        Color color = newState == SystemState.Destroyed ? Color.red : Color.yellow;
        string stateText = newState.ToString().ToUpper();

        string msg = $"{shipName}: {systemName} now {stateText}";
        AddEntry(new CombatLogEntry(LogCategory.SystemDamage, msg, color));
    }

    /// <summary>
    /// Add a shield event to the log.
    /// </summary>
    public void LogShieldEvent(string shipName, float damage, bool depleted)
    {
        if (depleted)
        {
            string msg = $"{shipName}: SHIELDS DEPLETED!";
            AddEntry(new CombatLogEntry(LogCategory.Shield, msg, new Color(0f, 0.7f, 1f))); // Cyan
        }
        else
        {
            string msg = $"{shipName}: Shields absorbed {damage:F0} damage";
            AddEntry(new CombatLogEntry(LogCategory.Shield, msg, new Color(0.5f, 0.8f, 1f))); // Light blue
        }
    }

    /// <summary>
    /// Add a ship death event to the log.
    /// </summary>
    public void LogDeath(string shipName, ShipDeathController.DeathCause cause)
    {
        string causeText = cause.ToString();
        string msg = $"DESTROYED! {shipName} - {causeText}!";
        AddEntry(new CombatLogEntry(LogCategory.Death, msg, Color.red));
    }

    /// <summary>
    /// Add a ship disabled event to the log.
    /// </summary>
    public void LogDisabled(string shipName)
    {
        string msg = $"{shipName}: COMBAT INEFFECTIVE - All weapons and engines destroyed!";
        AddEntry(new CombatLogEntry(LogCategory.Death, msg, new Color(0.8f, 0.4f, 0f))); // Dark orange
    }

    /// <summary>
    /// Add a custom message to the log.
    /// </summary>
    public void LogMessage(string message, LogCategory category, Color color)
    {
        AddEntry(new CombatLogEntry(category, message, color));
    }

    /// <summary>
    /// Add an entry to the log.
    /// </summary>
    private void AddEntry(CombatLogEntry entry)
    {
        logEntries.Add(entry);

        // Trim if over max
        while (logEntries.Count > maxLogEntries)
        {
            logEntries.RemoveAt(0);
        }

        filtersDirty = true;

        // Auto-scroll to bottom
        if (autoScroll)
        {
            scrollPosition.y = float.MaxValue;
        }
    }

    /// <summary>
    /// Clear all log entries.
    /// </summary>
    public void Clear()
    {
        logEntries.Clear();
        filteredEntries.Clear();
        filtersDirty = true;
        scrollPosition = Vector2.zero;
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
    /// Toggle visibility.
    /// </summary>
    public void Toggle()
    {
        isVisible = !isVisible;
    }

    private void OnGUI()
    {
        if (!isVisible) return;

        Rect panelRect = new Rect(panelPosition.x, panelPosition.y, panelWidth, panelHeight);

        // Background
        GUI.Box(panelRect, "");

        // Header
        GUI.Label(new Rect(panelRect.x + 10, panelRect.y + 5, panelWidth - 80, 20), "<b>COMBAT LOG</b>");

        // Clear button
        if (GUI.Button(new Rect(panelRect.x + panelWidth - 60, panelRect.y + 5, 50, 18), "Clear"))
        {
            Clear();
        }

        // Filter toggles row
        float filterY = panelRect.y + 25;
        float filterX = panelRect.x + 10;
        float toggleWidth = 60f;

        bool oldShowHits = showHits;
        bool oldShowCriticals = showCriticals;
        bool oldShowBreaches = showBreaches;
        bool oldShowSystems = showSystemDamage;
        bool oldShowShields = showShieldEvents;

        GUI.color = showHits ? Color.white : Color.gray;
        showHits = GUI.Toggle(new Rect(filterX, filterY, toggleWidth, 16), showHits, "Hits");
        filterX += toggleWidth;

        GUI.color = showCriticals ? Color.yellow : Color.gray;
        showCriticals = GUI.Toggle(new Rect(filterX, filterY, toggleWidth, 16), showCriticals, "Crits");
        filterX += toggleWidth;

        GUI.color = showBreaches ? new Color(1f, 0.5f, 0f) : Color.gray;
        showBreaches = GUI.Toggle(new Rect(filterX, filterY, toggleWidth + 10, 16), showBreaches, "Breach");
        filterX += toggleWidth + 10;

        GUI.color = showSystemDamage ? Color.cyan : Color.gray;
        showSystemDamage = GUI.Toggle(new Rect(filterX, filterY, toggleWidth + 10, 16), showSystemDamage, "Systems");
        filterX += toggleWidth + 10;

        GUI.color = showShieldEvents ? new Color(0.5f, 0.8f, 1f) : Color.gray;
        showShieldEvents = GUI.Toggle(new Rect(filterX, filterY, toggleWidth, 16), showShieldEvents, "Shield");

        GUI.color = Color.white;

        // Check if filters changed
        if (showHits != oldShowHits || showCriticals != oldShowCriticals ||
            showBreaches != oldShowBreaches || showSystemDamage != oldShowSystems ||
            showShieldEvents != oldShowShields)
        {
            filtersDirty = true;
        }

        // Rebuild filtered list if needed
        if (filtersDirty)
        {
            RebuildFilteredList();
        }

        // Log scroll area
        float logY = filterY + 20;
        float logHeight = panelRect.height - (logY - panelRect.y) - 5;
        Rect logRect = new Rect(panelRect.x + 5, logY, panelWidth - 10, logHeight);

        // Calculate content height (16 pixels per line)
        float contentHeight = filteredEntries.Count * 16f;

        scrollPosition = GUI.BeginScrollView(
            logRect,
            scrollPosition,
            new Rect(0, 0, panelWidth - 30, Mathf.Max(contentHeight, logHeight))
        );

        float entryY = 0;
        foreach (var entry in filteredEntries)
        {
            DrawLogEntry(entry, entryY, panelWidth - 30);
            entryY += 16;
        }

        GUI.EndScrollView();
    }

    private void RebuildFilteredList()
    {
        filteredEntries.Clear();

        foreach (var entry in logEntries)
        {
            if (ShouldShowCategory(entry.Category))
            {
                filteredEntries.Add(entry);
            }
        }

        filtersDirty = false;
    }

    private bool ShouldShowCategory(LogCategory category)
    {
        switch (category)
        {
            case LogCategory.Hit: return showHits;
            case LogCategory.Critical: return showCriticals;
            case LogCategory.Breach: return showBreaches;
            case LogCategory.SystemDamage: return showSystemDamage;
            case LogCategory.Shield: return showShieldEvents;
            case LogCategory.Death: return true; // Always show deaths
            default: return true;
        }
    }

    private void DrawLogEntry(CombatLogEntry entry, float y, float width)
    {
        displayBuilder.Clear();

        if (showTimestamps)
        {
            int minutes = (int)(entry.Timestamp / 60f);
            int seconds = (int)(entry.Timestamp % 60f);
            displayBuilder.Append($"[{minutes:D2}:{seconds:D2}] ");
        }

        displayBuilder.Append(entry.Message);

        GUI.color = entry.MessageColor;
        GUI.Label(new Rect(0, y, width, 16), $"<size=11>{displayBuilder}</size>");
        GUI.color = Color.white;
    }

    /// <summary>
    /// Get recent entries (for testing).
    /// </summary>
    public List<CombatLogEntry> GetRecentEntries(int count)
    {
        int start = Mathf.Max(0, logEntries.Count - count);
        return logEntries.GetRange(start, logEntries.Count - start);
    }

    /// <summary>
    /// Check if log contains entry with text.
    /// </summary>
    public bool ContainsEntry(string text)
    {
        foreach (var entry in logEntries)
        {
            if (entry.Message.Contains(text))
                return true;
        }
        return false;
    }
}
