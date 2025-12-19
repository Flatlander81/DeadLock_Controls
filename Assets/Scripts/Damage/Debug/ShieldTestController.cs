using UnityEngine;

/// <summary>
/// Test controller for the shield system and damage routing.
/// Provides GUI and hotkeys for testing damage flow through shields and sections.
/// </summary>
public class ShieldTestController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private DamageRouter targetDamageRouter;
    [SerializeField] private ShieldSystem targetShieldSystem;
    [SerializeField] private SectionManager targetSectionManager;
    [SerializeField] private Ship targetShip;

    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 50f;
    [SerializeField] private SectionType targetSection = SectionType.Fore;

    [Header("Test Actions")]
    [SerializeField] private bool applyDamage = false;
    [SerializeField] private bool resetAll = false;
    [SerializeField] private bool depleteShields = false;
    [SerializeField] private bool testShieldBoost = false;

    [Header("Runtime Info (Read Only)")]
    [SerializeField] private float currentShields;
    [SerializeField] private float totalArmor;
    [SerializeField] private float totalStructure;
    [SerializeField] private int breachedSections;

    private void Update()
    {
        // Handle test actions
        if (applyDamage)
        {
            applyDamage = false;
            ApplyDamageViaRouter();
        }

        if (resetAll)
        {
            resetAll = false;
            ResetAll();
        }

        if (depleteShields)
        {
            depleteShields = false;
            DepleteShields();
        }

        if (testShieldBoost)
        {
            testShieldBoost = false;
            TestShieldBoost();
        }

        // Update runtime info
        UpdateRuntimeInfo();
    }

    /// <summary>
    /// Sets targets from a ship reference.
    /// </summary>
    public void SetTargetShip(Ship ship)
    {
        targetShip = ship;
        if (ship != null)
        {
            targetDamageRouter = ship.DamageRouter;
            targetShieldSystem = ship.ShieldSystem;
            targetSectionManager = ship.SectionManager;
        }
    }

    /// <summary>
    /// Apply damage through the DamageRouter.
    /// </summary>
    public void ApplyDamageViaRouter()
    {
        if (targetDamageRouter == null)
        {
            Debug.LogError("[ShieldTestController] No DamageRouter assigned!");
            return;
        }

        DamageReport report = targetDamageRouter.ProcessDamage(damageAmount, targetSection);
        Debug.Log($"[ShieldTestController] {report}");
    }

    /// <summary>
    /// Apply damage directly to shields.
    /// </summary>
    public void ApplyDamageToShields(float damage)
    {
        if (targetShieldSystem == null)
        {
            Debug.LogError("[ShieldTestController] No ShieldSystem assigned!");
            return;
        }

        float overflow = targetShieldSystem.AbsorbDamage(damage);
        Debug.Log($"[ShieldTestController] Applied {damage:F1} to shields, {overflow:F1} overflow");
    }

    /// <summary>
    /// Deplete shields completely.
    /// </summary>
    public void DepleteShields()
    {
        if (targetShieldSystem == null)
        {
            Debug.LogError("[ShieldTestController] No ShieldSystem assigned!");
            return;
        }

        targetShieldSystem.SetShields(0f);
        Debug.Log("[ShieldTestController] Shields depleted");
    }

    /// <summary>
    /// Test the Shield Boost ability activation.
    /// </summary>
    public void TestShieldBoost()
    {
        if (targetShip == null)
        {
            Debug.LogError("[ShieldTestController] No Ship assigned!");
            return;
        }

        if (targetShip.AbilitySystem == null)
        {
            Debug.LogError("[ShieldTestController] Ship has no AbilitySystem!");
            return;
        }

        // Try to find Shield Boost ability
        var slot = targetShip.AbilitySystem.GetAbilitySlot("Shield Boost");
        if (slot == null)
        {
            Debug.LogError("[ShieldTestController] Shield Boost ability not found!");
            return;
        }

        // Check if it can activate
        bool canActivate = slot.abilityData.CanActivate(targetShip);
        string blockedReason = slot.abilityData.GetActivationBlockedReason(targetShip);

        if (canActivate)
        {
            Debug.Log("[ShieldTestController] Shield Boost CAN activate - shields are depleted");
        }
        else
        {
            Debug.Log($"[ShieldTestController] Shield Boost BLOCKED: {blockedReason}");
        }
    }

    /// <summary>
    /// Reset all systems.
    /// </summary>
    public void ResetAll()
    {
        if (targetShieldSystem != null)
        {
            targetShieldSystem.Reset();
        }

        if (targetSectionManager != null)
        {
            targetSectionManager.ResetAllSections();
        }

        Debug.Log("[ShieldTestController] All systems reset");
    }

    /// <summary>
    /// Updates runtime info displayed in Inspector.
    /// </summary>
    private void UpdateRuntimeInfo()
    {
        if (targetShieldSystem != null)
        {
            currentShields = targetShieldSystem.CurrentShields;
        }

        if (targetSectionManager != null)
        {
            totalArmor = targetSectionManager.GetTotalArmorRemaining();
            totalStructure = targetSectionManager.GetTotalStructureRemaining();
            breachedSections = targetSectionManager.GetBreachedSections().Count;
        }
    }

    /// <summary>
    /// GUI for testing.
    /// </summary>
    private void OnGUI()
    {
        if (targetDamageRouter == null && targetShieldSystem == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 350, 500));
        GUILayout.Label("Shield & Damage Test Controller", GUI.skin.box);

        // Shield status
        GUILayout.Space(5);
        GUILayout.Label("=== SHIELDS ===");
        if (targetShieldSystem != null)
        {
            float shieldPct = targetShieldSystem.GetShieldPercentage() * 100f;
            GUILayout.Label($"Shields: {targetShieldSystem.CurrentShields:F0}/{targetShieldSystem.MaxShields:F0} ({shieldPct:F0}%)");

            // Shield bar
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(300 * targetShieldSystem.GetShieldPercentage()));
            GUILayout.EndHorizontal();
            Rect lastRect = GUILayoutUtility.GetLastRect();
            GUI.color = targetShieldSystem.IsShieldActive ? Color.cyan : Color.gray;
            GUI.Box(new Rect(lastRect.x, lastRect.y, 300 * targetShieldSystem.GetShieldPercentage(), 20), "");
            GUI.color = Color.white;
            GUILayout.Space(20);
        }
        else
        {
            GUILayout.Label("No ShieldSystem");
        }

        // Section status
        GUILayout.Space(5);
        GUILayout.Label("=== SECTIONS ===");
        if (targetSectionManager != null)
        {
            GUILayout.Label($"Total Armor: {totalArmor:F0}");
            GUILayout.Label($"Total Structure: {totalStructure:F0}");
            GUILayout.Label($"Breached: {breachedSections}/{targetSectionManager.SectionCount}");
        }

        // Damage controls
        GUILayout.Space(10);
        GUILayout.Label("=== DAMAGE CONTROLS ===");
        GUILayout.Label($"Damage: {damageAmount:F0}");
        damageAmount = GUILayout.HorizontalSlider(damageAmount, 10f, 300f);

        GUILayout.Label($"Target Section: {targetSection}");

        // Section buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fore")) targetSection = SectionType.Fore;
        if (GUILayout.Button("Aft")) targetSection = SectionType.Aft;
        if (GUILayout.Button("Port")) targetSection = SectionType.Port;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Starboard")) targetSection = SectionType.Starboard;
        if (GUILayout.Button("Dorsal")) targetSection = SectionType.Dorsal;
        if (GUILayout.Button("Ventral")) targetSection = SectionType.Ventral;
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Core")) targetSection = SectionType.Core;

        GUILayout.Space(10);

        // Action buttons
        if (GUILayout.Button("Apply Damage via Router [D]"))
        {
            ApplyDamageViaRouter();
        }

        if (GUILayout.Button("Deplete Shields [S]"))
        {
            DepleteShields();
        }

        if (GUILayout.Button("Test Shield Boost [B]"))
        {
            TestShieldBoost();
        }

        if (GUILayout.Button("Reset All [R]"))
        {
            ResetAll();
        }

        GUILayout.EndArea();

        // Keyboard shortcuts
        if (Event.current.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.D:
                    ApplyDamageViaRouter();
                    break;
                case KeyCode.S:
                    DepleteShields();
                    break;
                case KeyCode.B:
                    TestShieldBoost();
                    break;
                case KeyCode.R:
                    ResetAll();
                    break;
            }
        }
    }
}
