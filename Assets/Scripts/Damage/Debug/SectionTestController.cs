using UnityEngine;

/// <summary>
/// Test controller for applying damage to sections via Inspector.
/// Used for manual testing and debugging of the damage system.
/// </summary>
public class SectionTestController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SectionManager targetSectionManager;

    [Header("Damage Settings")]
    [SerializeField] private SectionType targetSection = SectionType.Fore;
    [SerializeField] private float damageAmount = 25f;

    [Header("Test Actions")]
    [SerializeField] private bool applyDamage = false;
    [SerializeField] private bool resetAllSections = false;
    [SerializeField] private bool logSectionStatus = false;

    [Header("Runtime Info (Read Only)")]
    [SerializeField] private float totalArmorRemaining;
    [SerializeField] private float totalStructureRemaining;
    [SerializeField] private int breachedSectionCount;

    private void Update()
    {
        // Handle test actions
        if (applyDamage)
        {
            applyDamage = false;
            ApplyDamageToSection();
        }

        if (resetAllSections)
        {
            resetAllSections = false;
            ResetSections();
        }

        if (logSectionStatus)
        {
            logSectionStatus = false;
            LogAllSectionStatus();
        }

        // Update runtime info
        UpdateRuntimeInfo();
    }

    /// <summary>
    /// Sets the target section manager (for editor automation).
    /// </summary>
    public void SetTargetSectionManager(SectionManager manager)
    {
        targetSectionManager = manager;
    }

    /// <summary>
    /// Applies damage to the specified section.
    /// </summary>
    public void ApplyDamageToSection()
    {
        if (targetSectionManager == null)
        {
            Debug.LogError("[SectionTestController] No target SectionManager assigned!");
            return;
        }

        ShipSection section = targetSectionManager.GetSection(targetSection);
        if (section == null)
        {
            Debug.LogError($"[SectionTestController] Section {targetSection} not found!");
            return;
        }

        DamageResult result = section.ApplyDamage(damageAmount);
        Debug.Log($"[SectionTestController] Applied {damageAmount:F1} damage to {targetSection}: {result}");
    }

    /// <summary>
    /// Applies damage to a specific section type.
    /// </summary>
    public void ApplyDamageToSection(SectionType type, float damage)
    {
        if (targetSectionManager == null)
        {
            Debug.LogError("[SectionTestController] No target SectionManager assigned!");
            return;
        }

        ShipSection section = targetSectionManager.GetSection(type);
        if (section == null)
        {
            Debug.LogError($"[SectionTestController] Section {type} not found!");
            return;
        }

        DamageResult result = section.ApplyDamage(damage);
        Debug.Log($"[SectionTestController] Applied {damage:F1} damage to {type}: {result}");
    }

    /// <summary>
    /// Resets all sections to full health.
    /// </summary>
    public void ResetSections()
    {
        if (targetSectionManager == null)
        {
            Debug.LogError("[SectionTestController] No target SectionManager assigned!");
            return;
        }

        targetSectionManager.ResetAllSections();
        Debug.Log("[SectionTestController] All sections reset to full health");
    }

    /// <summary>
    /// Logs the status of all sections.
    /// </summary>
    public void LogAllSectionStatus()
    {
        if (targetSectionManager == null)
        {
            Debug.LogError("[SectionTestController] No target SectionManager assigned!");
            return;
        }

        Debug.Log("=== SECTION STATUS ===");

        foreach (ShipSection section in targetSectionManager.GetAllSections())
        {
            string status = section.IsBreached ? "BREACHED" : "Operational";
            Debug.Log($"  {section.SectionType}: Armor {section.CurrentArmor:F0}/{section.MaxArmor:F0}, " +
                     $"Structure {section.CurrentStructure:F0}/{section.MaxStructure:F0} [{status}]");
        }

        Debug.Log($"Total Armor: {targetSectionManager.GetTotalArmorRemaining():F0}/{targetSectionManager.GetTotalMaxArmor():F0}");
        Debug.Log($"Total Structure: {targetSectionManager.GetTotalStructureRemaining():F0}/{targetSectionManager.GetTotalMaxStructure():F0}");
        Debug.Log($"Breached Sections: {targetSectionManager.GetBreachedSections().Count}/{targetSectionManager.SectionCount}");
        Debug.Log($"Core Breached: {targetSectionManager.IsCoreBreached()}");
        Debug.Log("=====================");
    }

    /// <summary>
    /// Updates runtime info displayed in Inspector.
    /// </summary>
    private void UpdateRuntimeInfo()
    {
        if (targetSectionManager == null) return;

        totalArmorRemaining = targetSectionManager.GetTotalArmorRemaining();
        totalStructureRemaining = targetSectionManager.GetTotalStructureRemaining();
        breachedSectionCount = targetSectionManager.GetBreachedSections().Count;
    }

    /// <summary>
    /// Keyboard shortcuts for testing.
    /// </summary>
    private void OnGUI()
    {
        if (targetSectionManager == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label("Section Test Controller", GUI.skin.box);

        GUILayout.Label($"Target: {targetSection}");
        GUILayout.Label($"Damage: {damageAmount:F0}");

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Damage [D]"))
        {
            ApplyDamageToSection();
        }

        if (GUILayout.Button("Reset All Sections [R]"))
        {
            ResetSections();
        }

        if (GUILayout.Button("Log Status [L]"))
        {
            LogAllSectionStatus();
        }

        GUILayout.Space(10);
        GUILayout.Label("Quick Section Select:");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fore")) targetSection = SectionType.Fore;
        if (GUILayout.Button("Aft")) targetSection = SectionType.Aft;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Port")) targetSection = SectionType.Port;
        if (GUILayout.Button("Starboard")) targetSection = SectionType.Starboard;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Dorsal")) targetSection = SectionType.Dorsal;
        if (GUILayout.Button("Ventral")) targetSection = SectionType.Ventral;
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Core"))
        {
            targetSection = SectionType.Core;
        }

        GUILayout.Space(10);
        GUILayout.Label($"Total Armor: {totalArmorRemaining:F0}");
        GUILayout.Label($"Total Structure: {totalStructureRemaining:F0}");
        GUILayout.Label($"Breached: {breachedSectionCount}");

        GUILayout.EndArea();

        // Keyboard shortcuts
        if (Event.current.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.D:
                    ApplyDamageToSection();
                    break;
                case KeyCode.R:
                    ResetSections();
                    break;
                case KeyCode.L:
                    LogAllSectionStatus();
                    break;
            }
        }
    }
}
