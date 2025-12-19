using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime controller for testing Damage UI.
/// Buttons to simulate various damage events and verify UI updates.
/// </summary>
public class DamageUITestController : MonoBehaviour
{
    [Header("References")]
    public Ship testShip;
    public DamageUIManager damageUIManager;
    public SectionManager sectionManager;
    public ShieldSystem shieldSystem;
    public DamageRouter damageRouter;
    public CombatLogPanel combatLog;

    [Header("Test Settings")]
    [SerializeField] private SectionType targetSection = SectionType.Fore;

    [Header("UI Settings")]
    [SerializeField] private bool showUI = true;

    private Vector2 scrollPosition;

    void Start()
    {
        if (testShip == null)
        {
            testShip = FindFirstObjectByType<Ship>();
        }

        if (testShip != null)
        {
            if (sectionManager == null)
                sectionManager = testShip.SectionManager;
            if (shieldSystem == null)
                shieldSystem = testShip.ShieldSystem;
            if (damageRouter == null)
                damageRouter = testShip.DamageRouter;
        }

        if (damageUIManager == null)
        {
            damageUIManager = FindFirstObjectByType<DamageUIManager>();
        }

        if (combatLog == null && damageUIManager != null)
        {
            combatLog = damageUIManager.CombatLog;
        }

        Debug.Log("=== Damage UI Test Controller ===");
        Debug.Log("Use buttons to test damage UI features");
    }

    void OnGUI()
    {
        if (!showUI) return;

        // Position on right side of screen
        float panelWidth = 280f;
        float panelHeight = 450f;
        Rect panelRect = new Rect(Screen.width - panelWidth - 10, 10, panelWidth, panelHeight);

        GUI.Box(panelRect, "");

        GUILayout.BeginArea(new Rect(panelRect.x + 5, panelRect.y + 5, panelWidth - 10, panelHeight - 10));

        GUILayout.Label("<b>DAMAGE UI TEST CONTROLLER</b>");
        GUILayout.Space(5);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(panelHeight - 40));

        // Shield tests
        GUILayout.Label("<b>Shield Tests:</b>");

        if (GUILayout.Button("Hit Shield (30 dmg)"))
        {
            ApplyDamageViaRouter(30f, SectionType.Fore);
        }

        if (GUILayout.Button("Hit Shield (100 dmg)"))
        {
            ApplyDamageViaRouter(100f, SectionType.Fore);
        }

        if (GUILayout.Button("Deplete Shields"))
        {
            if (shieldSystem != null)
            {
                ApplyDamageViaRouter(shieldSystem.CurrentShields + 10f, SectionType.Fore);
            }
        }

        if (GUILayout.Button("Restore Shields"))
        {
            if (shieldSystem != null)
            {
                shieldSystem.RestoreShields(shieldSystem.MaxShields);
            }
        }

        GUILayout.Space(10);

        // Section damage tests
        GUILayout.Label("<b>Section Damage Tests:</b>");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Target:", GUILayout.Width(50));
        string[] sectionNames = System.Enum.GetNames(typeof(SectionType));
        int currentIndex = (int)targetSection;
        currentIndex = GUILayout.SelectionGrid(currentIndex, new string[] { "Fore", "Aft", "Port", "Stb", "Drs", "Vnt", "Core" }, 4);
        if (currentIndex < 7)
        {
            targetSection = (SectionType)currentIndex;
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button($"Damage {targetSection} (Armor: 30)"))
        {
            ApplyDirectSectionDamage(targetSection, 30f);
        }

        if (GUILayout.Button($"Damage {targetSection} (Structure: 50)"))
        {
            ApplyDirectSectionDamage(targetSection, 150f); // Enough to breach armor
        }

        if (GUILayout.Button($"Breach {targetSection}"))
        {
            BreachSection(targetSection);
        }

        GUILayout.Space(10);

        // Quick section buttons
        GUILayout.Label("<b>Quick Section Damage:</b>");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fore")) ApplyDirectSectionDamage(SectionType.Fore, 50f);
        if (GUILayout.Button("Aft")) ApplyDirectSectionDamage(SectionType.Aft, 50f);
        if (GUILayout.Button("Port")) ApplyDirectSectionDamage(SectionType.Port, 50f);
        if (GUILayout.Button("Stb")) ApplyDirectSectionDamage(SectionType.Starboard, 50f);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Critical hit tests
        GUILayout.Label("<b>Critical Hit Tests:</b>");

        if (GUILayout.Button("Trigger Critical (via Structure Damage)"))
        {
            // Deal enough damage to reach structure and trigger critical
            if (sectionManager != null)
            {
                ShipSection section = sectionManager.GetSection(targetSection);
                if (section != null)
                {
                    float damageNeeded = section.CurrentArmor + 20f;
                    ApplyDirectSectionDamage(targetSection, damageNeeded);
                }
            }
        }

        GUILayout.Space(10);

        // Combat log tests
        GUILayout.Label("<b>Combat Log Tests:</b>");

        if (GUILayout.Button("Log Custom Hit"))
        {
            if (combatLog != null)
            {
                combatLog.LogHit("TestShip", SectionType.Fore, 75f, 25f, 30f, 20f);
            }
        }

        if (GUILayout.Button("Log Critical"))
        {
            if (combatLog != null)
            {
                combatLog.LogCritical("TestShip", SectionType.Port, ShipSystemType.NewtonianCannon, false);
            }
        }

        if (GUILayout.Button("Log System Destroyed"))
        {
            if (combatLog != null)
            {
                combatLog.LogCritical("TestShip", SectionType.Aft, ShipSystemType.MainEngine, true);
            }
        }

        if (GUILayout.Button("Log Breach"))
        {
            if (combatLog != null)
            {
                combatLog.LogBreach("TestShip", SectionType.Fore);
            }
        }

        if (GUILayout.Button("Clear Combat Log"))
        {
            if (combatLog != null)
            {
                combatLog.Clear();
            }
        }

        GUILayout.Space(10);

        // Death tests
        GUILayout.Label("<b>Death Tests:</b>");

        if (GUILayout.Button("Breach Core (Ship Destroyed)"))
        {
            BreachSection(SectionType.Core);
        }

        if (GUILayout.Button("Log Ship Disabled"))
        {
            if (combatLog != null)
            {
                combatLog.LogDisabled("TestShip");
            }
        }

        GUILayout.Space(10);

        // Reset
        GUILayout.Label("<b>Reset:</b>");

        if (GUILayout.Button("Reset All Sections"))
        {
            if (sectionManager != null)
            {
                sectionManager.ResetAllSections();
            }
        }

        if (GUILayout.Button("Reset Ship & Shields"))
        {
            ResetAll();
        }

        GUILayout.Space(10);

        // Status display
        GUILayout.Label("<b>Current Status:</b>");
        if (shieldSystem != null)
        {
            GUILayout.Label($"Shields: {shieldSystem.CurrentShields:F0}/{shieldSystem.MaxShields:F0}");
        }
        if (sectionManager != null)
        {
            int breached = sectionManager.GetBreachedSections().Count;
            GUILayout.Label($"Breached Sections: {breached}/7");
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void Update()
    {
        // Toggle UI with H key
        if (Input.GetKeyDown(KeyCode.H))
        {
            showUI = !showUI;
        }
    }

    private void ApplyDamageViaRouter(float damage, SectionType section)
    {
        if (damageRouter == null)
        {
            Debug.LogError("No DamageRouter found!");
            return;
        }

        DamageReport report = damageRouter.ProcessDamage(damage, section);
        Debug.Log($"Damage Report: {report}");

        // Process through UI
        if (damageUIManager != null)
        {
            damageUIManager.ProcessDamageReport(report, testShip?.gameObject.name ?? "TestShip");
        }
    }

    private void ApplyDirectSectionDamage(SectionType type, float damage)
    {
        if (sectionManager == null)
        {
            Debug.LogError("No SectionManager found!");
            return;
        }

        ShipSection section = sectionManager.GetSection(type);
        if (section == null)
        {
            Debug.LogError($"Section {type} not found!");
            return;
        }

        // Bypass shields for direct section damage
        DamageResult result = section.ApplyDamage(damage);
        Debug.Log($"Direct damage to {type}: {result}");

        // Log to combat log
        if (combatLog != null)
        {
            combatLog.LogHit(
                testShip?.gameObject.name ?? "TestShip",
                type,
                damage,
                0f,
                result.DamageToArmor,
                result.DamageToStructure
            );

            // Log critical if occurred
            if (result.CriticalResult.HasValue)
            {
                var crit = result.CriticalResult.Value;
                if (crit.SystemWasDamaged || crit.SystemWasDestroyed)
                {
                    combatLog.LogCritical(
                        testShip?.gameObject.name ?? "TestShip",
                        type,
                        crit.SystemTypeHit,
                        crit.SystemWasDestroyed
                    );
                }
            }

            // Log breach if occurred
            if (result.SectionBreached)
            {
                combatLog.LogBreach(testShip?.gameObject.name ?? "TestShip", type);
            }
        }
    }

    private void BreachSection(SectionType type)
    {
        if (sectionManager == null)
        {
            Debug.LogError("No SectionManager found!");
            return;
        }

        ShipSection section = sectionManager.GetSection(type);
        if (section == null)
        {
            Debug.LogError($"Section {type} not found!");
            return;
        }

        if (!section.IsBreached)
        {
            float damageNeeded = section.CurrentArmor + section.CurrentStructure + 50f;
            ApplyDirectSectionDamage(type, damageNeeded);
        }
        else
        {
            Debug.Log($"Section {type} is already breached.");
        }
    }

    private void ResetAll()
    {
        if (sectionManager != null)
        {
            sectionManager.ResetAllSections();
        }

        if (shieldSystem != null)
        {
            shieldSystem.RestoreShields(shieldSystem.MaxShields);
        }

        if (combatLog != null)
        {
            combatLog.Clear();
        }

        Debug.Log("All ship state reset.");
    }
}
