using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Test controller for the critical hit system.
/// Provides GUI for testing critical rolls against sections with mounted systems.
/// </summary>
public class CriticalHitTestController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SectionManager sectionManager;
    [SerializeField] private CriticalHitSystem criticalHitSystem;
    [SerializeField] private SlotLayoutVisualizer slotVisualizer;

    [Header("UI Settings")]
    [SerializeField] private bool showGUI = true;
    [SerializeField] private Vector2 guiPosition = new Vector2(10, 10);
    [SerializeField] private float panelWidth = 400f;

    [Header("Test Configuration")]
    [SerializeField] private int criticalRollsPerBatch = 10;

    // Current selection
    private int selectedSectionIndex = 0;
    private int selectedSystemIndex = 0;
    private ShipSection selectedSection;

    // Log
    private List<string> criticalHitLog = new List<string>();
    private const int MAX_LOG_ENTRIES = 15;
    private Vector2 logScrollPosition;

    // GUI styles
    private GUIStyle headerStyle;
    private GUIStyle logStyle;
    private GUIStyle buttonStyle;
    private bool stylesInitialized = false;

    // Section names for dropdown
    private string[] sectionNames;

    // System selection
    private string[] systemNames;
    private MountedSystem[] currentSystems;

    private void Start()
    {
        // Auto-find references
        if (sectionManager == null)
        {
            sectionManager = FindAnyObjectByType<SectionManager>();
        }

        if (criticalHitSystem == null)
        {
            criticalHitSystem = FindAnyObjectByType<CriticalHitSystem>();
        }

        if (slotVisualizer == null)
        {
            slotVisualizer = FindAnyObjectByType<SlotLayoutVisualizer>();
        }

        // Build section names
        sectionNames = System.Enum.GetNames(typeof(SectionType));

        // Subscribe to critical hit events
        if (criticalHitSystem != null)
        {
            criticalHitSystem.OnCriticalRoll += HandleCriticalRoll;
        }

        UpdateSelectedSection();
        AddLog("Critical Hit Test Controller initialized");
    }

    private void OnDestroy()
    {
        if (criticalHitSystem != null)
        {
            criticalHitSystem.OnCriticalRoll -= HandleCriticalRoll;
        }
    }

    private void HandleCriticalRoll(CriticalHitResult result)
    {
        AddLog(result.ToString());
    }

    private void UpdateSelectedSection()
    {
        if (sectionManager == null) return;

        SectionType type = (SectionType)selectedSectionIndex;
        selectedSection = sectionManager.GetSection(type);

        // Update system list
        UpdateSystemList();

        // Update visualizer target
        if (slotVisualizer != null && selectedSection != null)
        {
            slotVisualizer.SetTargetSection(selectedSection);
        }
    }

    private void UpdateSystemList()
    {
        if (selectedSection == null || selectedSection.SlotLayout == null)
        {
            systemNames = new string[] { "No systems" };
            currentSystems = new MountedSystem[0];
            return;
        }

        var systems = selectedSection.SlotLayout.MountedSystems;
        if (systems == null || systems.Count == 0)
        {
            systemNames = new string[] { "No systems mounted" };
            currentSystems = new MountedSystem[0];
            return;
        }

        systemNames = new string[systems.Count];
        currentSystems = new MountedSystem[systems.Count];

        for (int i = 0; i < systems.Count; i++)
        {
            var sys = systems[i];
            currentSystems[i] = sys;
            systemNames[i] = $"{ShipSystemData.GetName(sys.SystemType)} [{sys.SlotStart}-{sys.SlotEnd}] ({sys.CurrentState})";
        }

        if (selectedSystemIndex >= currentSystems.Length)
        {
            selectedSystemIndex = 0;
        }
    }

    private void InitStyles()
    {
        if (stylesInitialized) return;

        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 14;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.white;

        logStyle = new GUIStyle(GUI.skin.label);
        logStyle.fontSize = 10;
        logStyle.wordWrap = true;
        logStyle.normal.textColor = Color.green;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 12;

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        if (!showGUI) return;

        InitStyles();

        float y = guiPosition.y;
        float x = guiPosition.x;

        // Title
        GUI.Label(new Rect(x, y, panelWidth, 25), "=== CRITICAL HIT TEST ===", headerStyle);
        y += 30;

        // Section selector
        GUI.Label(new Rect(x, y, 100, 20), "Section:");
        int newSectionIndex = GUI.SelectionGrid(
            new Rect(x + 70, y, panelWidth - 80, 50),
            selectedSectionIndex,
            sectionNames,
            4
        );
        if (newSectionIndex != selectedSectionIndex)
        {
            selectedSectionIndex = newSectionIndex;
            UpdateSelectedSection();
        }
        y += 55;

        // Section info
        if (selectedSection != null && selectedSection.SlotLayout != null)
        {
            var layout = selectedSection.SlotLayout;
            string info = $"Slots: {layout.GetOccupiedSlotCount()}/{layout.TotalSlots} occupied | " +
                         $"Systems: {layout.MountedSystems.Count}";
            GUI.Label(new Rect(x, y, panelWidth, 20), info);
            y += 22;

            // Armor/Structure status
            string status = $"Armor: {selectedSection.CurrentArmor:F0}/{selectedSection.MaxArmor:F0} | " +
                           $"Structure: {selectedSection.CurrentStructure:F0}/{selectedSection.MaxStructure:F0}";
            if (selectedSection.IsBreached)
            {
                GUI.color = Color.red;
                status += " [BREACHED]";
            }
            GUI.Label(new Rect(x, y, panelWidth, 20), status);
            GUI.color = Color.white;
            y += 25;
        }
        else
        {
            GUI.Label(new Rect(x, y, panelWidth, 20), "No section data available");
            y += 25;
        }

        // Critical roll buttons
        GUI.Label(new Rect(x, y, panelWidth, 20), "--- Critical Roll Actions ---", headerStyle);
        y += 22;

        if (GUI.Button(new Rect(x, y, 150, 25), "Roll Critical"))
        {
            RollCritical();
        }

        if (GUI.Button(new Rect(x + 160, y, 150, 25), $"Roll {criticalRollsPerBatch}x"))
        {
            for (int i = 0; i < criticalRollsPerBatch; i++)
            {
                RollCritical();
            }
        }
        y += 30;

        // System targeting
        GUI.Label(new Rect(x, y, panelWidth, 20), "--- Target Specific System ---", headerStyle);
        y += 22;

        if (currentSystems != null && currentSystems.Length > 0)
        {
            // System selector
            GUI.Label(new Rect(x, y, 60, 20), "System:");
            int newSystemIndex = GUI.SelectionGrid(
                new Rect(x + 70, y, panelWidth - 80, Mathf.Ceil(systemNames.Length / 2f) * 22),
                selectedSystemIndex,
                systemNames,
                2
            );
            if (newSystemIndex != selectedSystemIndex && newSystemIndex < currentSystems.Length)
            {
                selectedSystemIndex = newSystemIndex;
            }
            y += Mathf.Ceil(systemNames.Length / 2f) * 22 + 5;

            // Force hit buttons
            if (GUI.Button(new Rect(x, y, 150, 25), "Force Hit System"))
            {
                ForceHitSelectedSystem();
            }

            if (GUI.Button(new Rect(x + 160, y, 100, 25), "Damage"))
            {
                DamageSelectedSystem();
            }

            if (GUI.Button(new Rect(x + 270, y, 100, 25), "Destroy"))
            {
                DestroySelectedSystem();
            }
            y += 30;
        }
        else
        {
            GUI.Label(new Rect(x, y, panelWidth, 20), "No systems mounted in this section");
            y += 25;
        }

        // Reset buttons
        GUI.Label(new Rect(x, y, panelWidth, 20), "--- Reset ---", headerStyle);
        y += 22;

        if (GUI.Button(new Rect(x, y, 120, 25), "Reset Section"))
        {
            ResetSelectedSection();
        }

        if (GUI.Button(new Rect(x + 130, y, 120, 25), "Reset All"))
        {
            ResetAllSections();
        }

        if (GUI.Button(new Rect(x + 260, y, 120, 25), "Clear Log"))
        {
            criticalHitLog.Clear();
        }
        y += 35;

        // Statistics
        if (criticalHitSystem != null)
        {
            GUI.Label(new Rect(x, y, panelWidth, 20), "--- Statistics ---", headerStyle);
            y += 22;

            string stats = $"Total Rolls: {criticalHitSystem.TotalRolls} | " +
                          $"System Hits: {criticalHitSystem.SystemHits} | " +
                          $"Empty Hits: {criticalHitSystem.EmptyHits}";
            GUI.Label(new Rect(x, y, panelWidth, 20), stats);
            y += 22;

            float hitRate = criticalHitSystem.TotalRolls > 0 ?
                (float)criticalHitSystem.SystemHits / criticalHitSystem.TotalRolls * 100f : 0f;
            GUI.Label(new Rect(x, y, panelWidth, 20), $"Hit Rate: {hitRate:F1}%");
            y += 25;

            if (GUI.Button(new Rect(x, y, 120, 25), "Reset Stats"))
            {
                criticalHitSystem.ResetStatistics();
                AddLog("Statistics reset");
            }
            y += 30;
        }

        // Log display
        GUI.Label(new Rect(x, y, panelWidth, 20), "--- Critical Hit Log ---", headerStyle);
        y += 22;

        // Log area with scrollview
        float logHeight = 200;
        Rect logViewRect = new Rect(x, y, panelWidth, logHeight);
        Rect logContentRect = new Rect(0, 0, panelWidth - 20, criticalHitLog.Count * 18);

        GUI.Box(logViewRect, "");
        logScrollPosition = GUI.BeginScrollView(logViewRect, logScrollPosition, logContentRect);

        float logY = 0;
        for (int i = criticalHitLog.Count - 1; i >= 0; i--)
        {
            GUI.Label(new Rect(5, logY, panelWidth - 25, 18), criticalHitLog[i], logStyle);
            logY += 18;
        }

        GUI.EndScrollView();
    }

    private void RollCritical()
    {
        if (selectedSection == null)
        {
            AddLog("Error: No section selected");
            return;
        }

        if (selectedSection.SlotLayout == null)
        {
            AddLog("Error: Section has no slot layout");
            return;
        }

        CriticalHitResult result;
        if (criticalHitSystem != null)
        {
            result = criticalHitSystem.RollCritical(selectedSection);
        }
        else
        {
            // Fallback: direct roll
            var layout = selectedSection.SlotLayout;
            int slot = Random.Range(1, layout.TotalSlots + 1);
            var system = layout.GetSystemAtSlot(slot);

            if (system == null)
            {
                result = CriticalHitResult.EmptySlot(slot, layout.TotalSlots, selectedSection.SectionType);
            }
            else
            {
                SystemState prev = system.CurrentState;
                bool changed = system.TakeCriticalHit();
                result = CriticalHitResult.SystemHitResult(slot, layout.TotalSlots, system, prev, system.CurrentState, selectedSection.SectionType);
            }
        }

        // Result is logged via event handler
        UpdateSystemList();
    }

    private void ForceHitSelectedSystem()
    {
        if (currentSystems == null || selectedSystemIndex >= currentSystems.Length)
        {
            AddLog("Error: No system selected");
            return;
        }

        var system = currentSystems[selectedSystemIndex];
        int targetSlot = system.SlotStart;

        CriticalHitResult result;
        if (criticalHitSystem != null && selectedSection != null)
        {
            result = criticalHitSystem.ForceCritical(selectedSection.SlotLayout, targetSlot, selectedSection.SectionType);
        }
        else
        {
            SystemState prev = system.CurrentState;
            bool changed = system.TakeCriticalHit();
            result = CriticalHitResult.SystemHitResult(
                targetSlot,
                selectedSection?.SlotLayout?.TotalSlots ?? 0,
                system,
                prev,
                system.CurrentState,
                selectedSection?.SectionType ?? SectionType.Core
            );
        }

        AddLog($"Forced hit: {result}");
        UpdateSystemList();
    }

    private void DamageSelectedSystem()
    {
        if (currentSystems == null || selectedSystemIndex >= currentSystems.Length)
        {
            AddLog("Error: No system selected");
            return;
        }

        var system = currentSystems[selectedSystemIndex];
        if (system.CurrentState == SystemState.Operational)
        {
            system.TakeCriticalHit(); // Operational -> Damaged
            AddLog($"{ShipSystemData.GetName(system.SystemType)} damaged");
        }
        else
        {
            AddLog($"{ShipSystemData.GetName(system.SystemType)} already {system.CurrentState}");
        }
        UpdateSystemList();
    }

    private void DestroySelectedSystem()
    {
        if (currentSystems == null || selectedSystemIndex >= currentSystems.Length)
        {
            AddLog("Error: No system selected");
            return;
        }

        var system = currentSystems[selectedSystemIndex];
        while (system.CurrentState != SystemState.Destroyed)
        {
            system.TakeCriticalHit();
        }
        AddLog($"{ShipSystemData.GetName(system.SystemType)} DESTROYED");
        UpdateSystemList();
    }

    private void ResetSelectedSection()
    {
        if (selectedSection != null)
        {
            selectedSection.Reset();
            AddLog($"Section {selectedSection.SectionType} reset");
            UpdateSystemList();
        }
    }

    private void ResetAllSections()
    {
        if (sectionManager != null)
        {
            foreach (var section in sectionManager.GetAllSections())
            {
                if (section != null)
                {
                    section.Reset();
                }
            }
            AddLog("All sections reset");
            UpdateSystemList();
        }
    }

    private void AddLog(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        criticalHitLog.Add($"[{timestamp}] {message}");

        // Trim log
        while (criticalHitLog.Count > MAX_LOG_ENTRIES)
        {
            criticalHitLog.RemoveAt(0);
        }

        // Auto-scroll to bottom
        logScrollPosition.y = float.MaxValue;
    }

    /// <summary>
    /// Sets references for this controller.
    /// </summary>
    public void SetReferences(SectionManager manager, CriticalHitSystem critSystem, SlotLayoutVisualizer visualizer)
    {
        sectionManager = manager;
        criticalHitSystem = critSystem;
        slotVisualizer = visualizer;

        if (criticalHitSystem != null)
        {
            criticalHitSystem.OnCriticalRoll += HandleCriticalRoll;
        }

        UpdateSelectedSection();
    }
}
