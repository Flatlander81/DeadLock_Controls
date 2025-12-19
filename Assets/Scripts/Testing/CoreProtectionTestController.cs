using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime controller for testing Core protection mechanics.
/// Allows manual testing of section breaching, attack angles, and lucky shots.
/// </summary>
public class CoreProtectionTestController : MonoBehaviour
{
    [Header("References")]
    public Ship testShip;
    public SectionManager sectionManager;
    public CoreProtectionSystem coreProtection;
    public DamageRouter damageRouter;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 50f;
    [SerializeField] private Vector3 attackDirection = Vector3.forward;

    [Header("UI Settings")]
    [SerializeField] private bool showUI = true;

    // Angle presets (in degrees from forward)
    private readonly (string name, Vector3 dir)[] directionPresets = new[]
    {
        ("Forward (Fore)", Vector3.forward),
        ("Back (Aft)", Vector3.back),
        ("Left (Port)", Vector3.left),
        ("Right (Starboard)", Vector3.right),
        ("Up (Dorsal)", Vector3.up),
        ("Down (Ventral)", Vector3.down)
    };
    private int currentDirectionIndex = 0;

    // Last attack result
    private DamageReport lastDamageReport;
    private bool hasLastReport = false;

    void Start()
    {
        if (testShip == null)
        {
            testShip = FindObjectOfType<Ship>();
        }

        if (testShip != null)
        {
            if (sectionManager == null)
            {
                sectionManager = testShip.GetComponent<SectionManager>();
            }

            if (coreProtection == null)
            {
                coreProtection = testShip.GetComponent<CoreProtectionSystem>();
            }

            if (damageRouter == null)
            {
                damageRouter = testShip.GetComponent<DamageRouter>();
            }
        }

        Debug.Log("=== Core Protection Test Controller ===");
        Debug.Log("CONTROLS:");
        Debug.Log("  1-6: Breach specific sections");
        Debug.Log("  C: Attack Core directly");
        Debug.Log("  Arrow Keys: Change attack direction");
        Debug.Log("  L: Force lucky shot roll");
        Debug.Log("  R: Reset all sections");
        Debug.Log("  H: Toggle UI");
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Toggle UI
        if (Input.GetKeyDown(KeyCode.H))
        {
            showUI = !showUI;
        }

        // Breach specific sections (1-6 keys map to Fore, Aft, Port, Starboard, Dorsal, Ventral)
        if (Input.GetKeyDown(KeyCode.Alpha1)) BreachSection(SectionType.Fore);
        if (Input.GetKeyDown(KeyCode.Alpha2)) BreachSection(SectionType.Aft);
        if (Input.GetKeyDown(KeyCode.Alpha3)) BreachSection(SectionType.Port);
        if (Input.GetKeyDown(KeyCode.Alpha4)) BreachSection(SectionType.Starboard);
        if (Input.GetKeyDown(KeyCode.Alpha5)) BreachSection(SectionType.Dorsal);
        if (Input.GetKeyDown(KeyCode.Alpha6)) BreachSection(SectionType.Ventral);

        // Attack Core directly
        if (Input.GetKeyDown(KeyCode.C))
        {
            AttackCore();
        }

        // Change attack direction
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentDirectionIndex = (currentDirectionIndex + 1) % directionPresets.Length;
            attackDirection = directionPresets[currentDirectionIndex].dir;
            Debug.Log($"Attack direction: {directionPresets[currentDirectionIndex].name}");
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentDirectionIndex = (currentDirectionIndex - 1 + directionPresets.Length) % directionPresets.Length;
            attackDirection = directionPresets[currentDirectionIndex].dir;
            Debug.Log($"Attack direction: {directionPresets[currentDirectionIndex].name}");
        }

        // Force lucky shot
        if (Input.GetKeyDown(KeyCode.L))
        {
            ForceLuckyShot();
        }

        // Reset all sections
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllSections();
        }
    }

    void BreachSection(SectionType type)
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

        // Force breach by dealing massive damage
        if (!section.IsBreached)
        {
            float damageNeeded = section.CurrentArmor + section.CurrentStructure + 100f;
            section.ApplyDamage(damageNeeded);
            Debug.Log($"Section {type} BREACHED! Core is now exposed from {type} direction.");
        }
        else
        {
            Debug.Log($"Section {type} is already breached.");
        }

        UpdateCoreExposureStatus();
    }

    void AttackCore()
    {
        if (damageRouter == null)
        {
            Debug.LogError("No DamageRouter found!");
            return;
        }

        Debug.Log($"Attacking Core from {directionPresets[currentDirectionIndex].name} with {attackDamage} damage...");

        // Check if Core can be hit
        if (coreProtection != null)
        {
            bool canHit = coreProtection.CanHitCore(attackDirection);
            SectionType adjacent = coreProtection.GetAdjacentSection(
                testShip.transform.InverseTransformDirection(attackDirection.normalized));

            Debug.Log($"  Adjacent section: {adjacent}, Breached: {coreProtection.IsAdjacentSectionBreached(adjacent)}");
            Debug.Log($"  Can hit Core: {canHit}");
        }

        // Route the damage
        lastDamageReport = damageRouter.ProcessDamage(attackDamage, SectionType.Core, attackDirection);
        hasLastReport = true;

        // Log result
        Debug.Log($"  Result: {lastDamageReport}");

        if (lastDamageReport.CoreWasProtected)
        {
            Debug.Log($"  Core was PROTECTED - damage redirected to {lastDamageReport.SectionHit}");
        }

        if (lastDamageReport.WasLuckyShot)
        {
            Debug.Log($"  LUCKY SHOT hit Core!");
        }
    }

    void ForceLuckyShot()
    {
        if (coreProtection == null)
        {
            Debug.LogError("No CoreProtectionSystem found!");
            return;
        }

        // First attack a non-Core section to trigger structure damage
        if (damageRouter != null)
        {
            // Attack Fore section
            SectionType targetSection = SectionType.Fore;
            ShipSection section = sectionManager?.GetSection(targetSection);

            if (section != null && !section.IsBreached)
            {
                // Deal enough damage to reach structure
                float armorDamage = section.CurrentArmor + 10f;
                Debug.Log($"Dealing {armorDamage} damage to {targetSection} to reach structure...");

                // We can't directly force a lucky shot in ProcessDamage, but we can demonstrate the mechanic
                lastDamageReport = damageRouter.ProcessDamage(armorDamage, targetSection);
                hasLastReport = true;

                Debug.Log($"  Result: {lastDamageReport}");

                if (lastDamageReport.WasLuckyShot)
                {
                    Debug.Log("  LUCKY SHOT occurred!");
                }
                else
                {
                    Debug.Log($"  No lucky shot (5% chance). Structure damage: {lastDamageReport.StructureDamage}");
                    Debug.Log("  Try again or modify luckyShotChance to test.");
                }
            }
            else
            {
                Debug.Log($"{targetSection} is breached. Attack a different section.");
            }
        }
    }

    void ResetAllSections()
    {
        if (sectionManager == null)
        {
            Debug.LogError("No SectionManager found!");
            return;
        }

        sectionManager.ResetAllSections();
        hasLastReport = false;

        Debug.Log("All sections reset to full health.");
        UpdateCoreExposureStatus();
    }

    void UpdateCoreExposureStatus()
    {
        if (coreProtection == null) return;

        bool exposed = coreProtection.IsCoreExposed();
        var exposedAngles = coreProtection.GetExposedAngles();

        if (exposed)
        {
            Debug.Log($"Core is EXPOSED from: {string.Join(", ", exposedAngles)}");
        }
        else
        {
            Debug.Log("Core is fully PROTECTED.");
        }
    }

    void OnGUI()
    {
        if (!showUI) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 600));
        GUI.Box(new Rect(0, 0, 400, 600), "");

        GUILayout.Label("<b>CORE PROTECTION TEST CONTROLLER</b>");
        GUILayout.Space(5);

        // Attack direction
        GUILayout.Label($"<b>Attack Direction:</b> {directionPresets[currentDirectionIndex].name}");
        GUILayout.Label($"Attack Damage: {attackDamage}");
        GUILayout.Space(10);

        // Section status
        GUILayout.Label("<b>SECTION STATUS:</b>");
        if (sectionManager != null)
        {
            foreach (var section in sectionManager.GetAllSections())
            {
                if (section == null) continue;

                Color statusColor = section.IsBreached ? Color.red : Color.green;
                GUI.color = statusColor;

                string breached = section.IsBreached ? "[BREACHED]" : "[OK]";
                GUILayout.Label($"  {section.SectionType}: A:{section.CurrentArmor:F0}/{section.MaxArmor:F0} S:{section.CurrentStructure:F0}/{section.MaxStructure:F0} {breached}");

                GUI.color = Color.white;
            }
        }

        GUILayout.Space(10);

        // Core protection status
        GUILayout.Label("<b>CORE PROTECTION STATUS:</b>");
        if (coreProtection != null)
        {
            bool exposed = coreProtection.IsCoreExposed();
            var exposedAngles = coreProtection.GetExposedAngles();

            GUI.color = exposed ? Color.red : Color.green;
            GUILayout.Label($"  Core Exposed: {exposed}");
            GUI.color = Color.white;

            if (exposedAngles.Count > 0)
            {
                GUILayout.Label($"  Exposed From: {string.Join(", ", exposedAngles)}");
            }

            GUILayout.Label($"  Lucky Shot Chance: {coreProtection.LuckyShotChance * 100f:F0}%");

            // Can Core be hit from current direction?
            bool canHit = coreProtection.CanHitCore(attackDirection);
            GUI.color = canHit ? Color.red : Color.green;
            GUILayout.Label($"  Can Hit Core (current angle): {canHit}");
            GUI.color = Color.white;
        }

        GUILayout.Space(10);

        // Last damage report
        if (hasLastReport)
        {
            GUILayout.Label("<b>LAST ATTACK RESULT:</b>");
            GUILayout.Label($"  Total Damage: {lastDamageReport.TotalIncomingDamage:F1}");
            GUILayout.Label($"  Shield: {lastDamageReport.ShieldDamage:F1}");
            GUILayout.Label($"  Armor: {lastDamageReport.ArmorDamage:F1}");
            GUILayout.Label($"  Structure: {lastDamageReport.StructureDamage:F1}");
            GUILayout.Label($"  Section Hit: {lastDamageReport.SectionHit}");

            GUI.color = lastDamageReport.CoreWasProtected ? Color.yellow : Color.white;
            GUILayout.Label($"  Core Protected: {lastDamageReport.CoreWasProtected}");
            GUI.color = Color.white;

            GUI.color = lastDamageReport.WasLuckyShot ? Color.magenta : Color.white;
            GUILayout.Label($"  Lucky Shot: {lastDamageReport.WasLuckyShot}");
            GUI.color = Color.white;
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>CONTROLS:</b>");
        GUILayout.Label("1-6: Breach Fore/Aft/Port/Starboard/Dorsal/Ventral");
        GUILayout.Label("C: Attack Core | Arrows: Change direction");
        GUILayout.Label("L: Attack section (test lucky shot) | R: Reset | H: Toggle UI");

        GUILayout.EndArea();
    }
}
