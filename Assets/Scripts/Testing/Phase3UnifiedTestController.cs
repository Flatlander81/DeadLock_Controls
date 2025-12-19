using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unified test controller for all Phase 3 damage system features.
/// Provides a comprehensive tabbed UI for testing shields, sections, criticals,
/// degradation, core protection, ship death, projectiles, and combat UI.
/// </summary>
public class Phase3UnifiedTestController : MonoBehaviour
{
    [Header("Ship References")]
    public Ship playerShip;
    public List<Ship> enemyShips = new List<Ship>();

    [Header("System References")]
    public ProjectileManager projectileManager;
    public DamageUIManager damageUIManager;
    public TargetingController targetingController;

    [Header("UI Settings")]
    [SerializeField] private bool showUI = true;
    [SerializeField] private float testDamage = 50f;
    [SerializeField] private SectionType targetSection = SectionType.Fore;

    // Current target
    private Ship targetShip;
    private int targetIndex = 0;

    // Cached components
    private ShieldSystem targetShields;
    private SectionManager targetSections;
    private DamageRouter targetDamageRouter;
    private CoreProtectionSystem targetCoreProtection;
    private SystemDegradationManager targetDegradation;
    private ShipDeathController targetDeathController;
    private CombatLogPanel combatLog;

    // UI state
    private enum TestTab { Combat, Sections, Systems, CoreDeath, Projectiles, Weapons, Status }
    private TestTab currentTab = TestTab.Combat;
    private Vector2 scrollPosition;
    private int selectedSystemIndex = 0;
    private List<MountedSystem> targetSystems = new List<MountedSystem>();

    // Attack direction for core protection testing
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

    // Phase 2.2 integration - weapon groups and UI panels
    private WeaponConfigPanel configPanel;
    private WeaponGroupPanel groupPanel;
    private bool weaponGroupsInitialized = false;

    void Start()
    {
        // Find player ship if not assigned
        if (playerShip == null)
        {
            Ship[] allShips = FindObjectsByType<Ship>(FindObjectsSortMode.None);
            foreach (var ship in allShips)
            {
                if (ship.gameObject.name.Contains("Player"))
                {
                    playerShip = ship;
                    break;
                }
            }
        }

        // Find enemy ships if not assigned
        if (enemyShips.Count == 0)
        {
            Ship[] allShips = FindObjectsByType<Ship>(FindObjectsSortMode.None);
            foreach (var ship in allShips)
            {
                if (ship != playerShip)
                {
                    enemyShips.Add(ship);
                }
            }
        }

        // Default to first enemy
        if (enemyShips.Count > 0)
        {
            targetShip = enemyShips[0];
            CacheTargetComponents();
        }

        // Find UI components
        if (damageUIManager == null)
        {
            damageUIManager = FindFirstObjectByType<DamageUIManager>();
        }

        if (damageUIManager != null)
        {
            combatLog = damageUIManager.CombatLog;
        }

        Debug.Log("=== Phase 3 Unified Test Controller ===");
        Debug.Log("CONTROLS:");
        Debug.Log("  J: Toggle test panel UI");
        Debug.Log("  K: Cycle target ships");
        Debug.Log("  Tab: Cycle UI tabs");
        Debug.Log("");
        Debug.Log("WEAPON CONTROLS (Phase 2.2):");
        Debug.Log("  1,2,3,4: Fire weapon groups");
        Debug.Log("  A: Alpha Strike (all weapons)");
        Debug.Log("  Space: Toggle weapon config panel");
        Debug.Log("  R: Reset cooldowns (cheat)");
        Debug.Log("  L: Reload all ammo (cheat)");

        // Initialize weapon systems after a short delay (let WeaponManager initialize)
        Invoke(nameof(InitializeWeaponSystems), 0.2f);
    }

    void InitializeWeaponSystems()
    {
        if (weaponGroupsInitialized) return;

        // Create weapon UI panels (from Phase 2.2)
        CreateWeaponUIPanels();

        // Assign weapons to groups automatically
        AssignWeaponGroups();

        // Set all weapons to target first enemy
        if (enemyShips.Count > 0)
        {
            SetAllWeaponsTarget(enemyShips[0]);
        }

        weaponGroupsInitialized = true;
        Debug.Log("✓ Weapon systems initialized with groups assigned");
    }

    void CreateWeaponUIPanels()
    {
        // Config Panel - only create if not already assigned
        if (configPanel == null)
        {
            var existingConfig = FindFirstObjectByType<WeaponConfigPanel>();
            if (existingConfig != null)
            {
                configPanel = existingConfig;
            }
            else
            {
                GameObject configObj = new GameObject("WeaponConfigPanel_Runtime");
                configObj.transform.SetParent(transform);
                configPanel = configObj.AddComponent<WeaponConfigPanel>();
            }
            configPanel.Initialize(playerShip);
        }

        // Group Panel - only create if not already assigned
        if (groupPanel == null)
        {
            var existingGroup = FindFirstObjectByType<WeaponGroupPanel>();
            if (existingGroup != null)
            {
                groupPanel = existingGroup;
            }
            else
            {
                GameObject groupObj = new GameObject("WeaponGroupPanel_Runtime");
                groupObj.transform.SetParent(transform);
                groupPanel = groupObj.AddComponent<WeaponGroupPanel>();
            }
            groupPanel.Initialize(playerShip, null);
            if (enemyShips.Count > 0)
            {
                groupPanel.SetTarget(enemyShips[0]);
            }
        }
    }

    void AssignWeaponGroups()
    {
        WeaponManager wm = playerShip?.WeaponManager;
        if (wm == null)
        {
            Debug.LogWarning("No WeaponManager found for weapon group assignment");
            return;
        }

        int assigned = 0;
        foreach (WeaponSystem weapon in wm.Weapons)
        {
            // Group 1: RailGuns (cyan)
            if (weapon is RailGun)
            {
                wm.AssignWeaponToGroup(weapon, 1);
                assigned++;
            }
            // Group 2: Newtonian Cannon (magenta)
            else if (weapon is NewtonianCannon)
            {
                wm.AssignWeaponToGroup(weapon, 2);
                assigned++;
            }
            // Group 3: Torpedoes (orange)
            else if (weapon is TorpedoLauncher)
            {
                wm.AssignWeaponToGroup(weapon, 3);
                assigned++;
            }
            // Group 4: Missiles (yellow)
            else if (weapon is MissileBattery)
            {
                wm.AssignWeaponToGroup(weapon, 4);
                assigned++;
            }
        }

        Debug.Log($"✓ Assigned {assigned} weapons to groups:");
        Debug.Log("  Group 1 (Blue/Cyan): RailGuns - instant hit, infinite ammo");
        Debug.Log("  Group 2 (Magenta): Newtonian Cannon - slow projectile, infinite ammo");
        Debug.Log("  Group 3 (Orange): Torpedo - slow homing, limited ammo");
        Debug.Log("  Group 4 (Yellow): Missiles - fast homing, limited ammo");
    }

    void SetAllWeaponsTarget(Ship target)
    {
        if (playerShip?.WeaponManager == null) return;

        foreach (WeaponSystem weapon in playerShip.WeaponManager.Weapons)
        {
            weapon.SetTarget(target);
        }

        if (groupPanel != null)
        {
            groupPanel.SetTarget(target);
        }

        Debug.Log($"All weapons targeting: {target?.gameObject.name ?? "None"}");
    }

    void CacheTargetComponents()
    {
        if (targetShip == null) return;

        targetShields = targetShip.ShieldSystem;
        targetSections = targetShip.SectionManager;
        targetDamageRouter = targetShip.DamageRouter;
        targetCoreProtection = targetShip.GetComponent<CoreProtectionSystem>();
        targetDegradation = targetShip.GetComponent<SystemDegradationManager>();
        targetDeathController = targetShip.GetComponent<ShipDeathController>();

        RefreshSystemList();
    }

    void RefreshSystemList()
    {
        targetSystems.Clear();
        if (targetShip == null) return;

        MountedSystem[] systems = targetShip.GetComponentsInChildren<MountedSystem>();
        targetSystems.AddRange(systems);
        selectedSystemIndex = 0;
    }

    void Update()
    {
        // Toggle test panel UI
        if (Input.GetKeyDown(KeyCode.J))
        {
            showUI = !showUI;
        }

        // Cycle targets
        if (Input.GetKeyDown(KeyCode.K))
        {
            CycleTarget();
            // Also update weapon targets
            if (targetShip != playerShip)
            {
                SetAllWeaponsTarget(targetShip);
            }
        }

        // Cycle tabs (when UI is visible)
        if (Input.GetKeyDown(KeyCode.Tab) && showUI)
        {
            currentTab = (TestTab)(((int)currentTab + 1) % System.Enum.GetValues(typeof(TestTab)).Length);
        }

        // Phase 2.2 weapon controls - these work regardless of UI visibility
        HandleWeaponInput();
    }

    void HandleWeaponInput()
    {
        WeaponManager wm = playerShip?.WeaponManager;
        if (wm == null) return;

        // Fire weapon groups 1-4
        if (Input.GetKeyDown(KeyCode.Alpha1)) FireWeaponGroup(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) FireWeaponGroup(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) FireWeaponGroup(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) FireWeaponGroup(4);

        // Alpha Strike - fire all weapons
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log(">>> ALPHA STRIKE <<<");
            int fired = 0;
            foreach (WeaponSystem weapon in wm.Weapons)
            {
                if (weapon.CanFire())
                {
                    StartCoroutine(weapon.FireWithSpinUp());
                    fired++;
                }
            }
            Debug.Log($"Alpha Strike: Fired {fired}/{wm.Weapons.Count} weapons");
        }

        // Toggle weapon config panel
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (configPanel != null)
            {
                configPanel.IsVisible = !configPanel.IsVisible;
            }
        }

        // Cheat: Reset all cooldowns
        if (Input.GetKeyDown(KeyCode.R))
        {
            var field = typeof(WeaponSystem).GetField("currentCooldown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                foreach (WeaponSystem weapon in wm.Weapons)
                {
                    field.SetValue(weapon, 0);
                }
                Debug.Log("CHEAT: All cooldowns reset to 0");
            }
        }

        // Cheat: Reload all ammo
        if (Input.GetKeyDown(KeyCode.L))
        {
            var field = typeof(WeaponSystem).GetField("currentAmmo",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                foreach (WeaponSystem weapon in wm.Weapons)
                {
                    if (weapon.AmmoCapacity > 0)
                    {
                        field.SetValue(weapon, weapon.AmmoCapacity);
                    }
                }
                Debug.Log("CHEAT: All ammo reloaded");
            }
        }
    }

    void FireWeaponGroup(int groupNum)
    {
        WeaponManager wm = playerShip?.WeaponManager;
        if (wm == null) return;

        List<WeaponSystem> weapons = wm.GetWeaponsInGroup(groupNum);

        if (weapons.Count == 0)
        {
            Debug.Log($"Group {groupNum}: Empty");
            return;
        }

        string[] groupNames = { "", "RailGuns", "Cannon", "Torpedoes", "Missiles" };
        string groupName = groupNum <= 4 ? groupNames[groupNum] : $"Group {groupNum}";

        Debug.Log($">>> Firing {groupName} (Group {groupNum}) <<<");
        int fired = 0;
        int blocked = 0;

        // Use first enemy as target
        Ship target = (enemyShips.Count > 0) ? enemyShips[0] : null;

        foreach (WeaponSystem weapon in weapons)
        {
            // Ensure weapon is initialized and has target
            if (weapon.OwnerShip == null)
            {
                weapon.Initialize(playerShip);
            }
            if (weapon.AssignedTarget == null && target != null)
            {
                weapon.SetTarget(target);
            }

            if (weapon.CanFire())
            {
                StartCoroutine(weapon.FireWithSpinUp());
                fired++;
            }
            else
            {
                blocked++;
                if (weapon.CurrentCooldown > 0)
                    Debug.Log($"  {weapon.WeaponName}: On cooldown ({weapon.CurrentCooldown} turns)");
                else if (weapon.AmmoCapacity > 0 && weapon.CurrentAmmo <= 0)
                    Debug.Log($"  {weapon.WeaponName}: Out of ammo!");
                else if (weapon.AssignedTarget == null)
                    Debug.Log($"  {weapon.WeaponName}: No target assigned");
                else
                {
                    // Detailed arc/range debug
                    Vector3 targetPos = weapon.AssignedTarget.transform.position;
                    float dist = Vector3.Distance(weapon.transform.position, targetPos);
                    Vector3 toTarget = (targetPos - weapon.transform.position).normalized;
                    float angle = Vector3.Angle(weapon.transform.forward, toTarget);
                    Debug.Log($"  {weapon.WeaponName}: Cannot fire - dist={dist:F1} (max={weapon.MaxRange}), angle={angle:F1}° (arc={weapon.FiringArc}°)");
                }
            }
        }

        Debug.Log($"  Result: Fired {fired}, Blocked {blocked}");
    }

    void CycleTarget()
    {
        List<Ship> allTargets = new List<Ship> { playerShip };
        allTargets.AddRange(enemyShips);

        targetIndex = (targetIndex + 1) % allTargets.Count;
        targetShip = allTargets[targetIndex];
        CacheTargetComponents();

        Debug.Log($"Target switched to: {targetShip?.gameObject.name}");
    }

    /// <summary>
    /// Draws the left-side status overlay showing target, distance, heat, shields, and ammo (from Phase 2.2)
    /// </summary>
    void DrawStatusOverlay()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 450));

        GUI.Box(new Rect(0, 0, 300, 430), "");

        GUILayout.Label("<size=12><b>PHASE 3 UNIFIED TEST</b></size>");
        GUILayout.Space(5);

        // Current weapon target info
        Ship weaponTarget = (enemyShips.Count > 0) ? enemyShips[0] : null;
        if (weaponTarget != null)
        {
            GUI.color = Color.red;
            GUILayout.Label($"<b>Weapon Target:</b> {weaponTarget.gameObject.name}");
            GUI.color = Color.white;

            float dist = Vector3.Distance(playerShip.transform.position, weaponTarget.transform.position);
            GUILayout.Label($"Distance: {dist:F1} units");

            // Show target shields
            var targetShieldSys = weaponTarget.ShieldSystem;
            if (targetShieldSys != null)
            {
                float pct = targetShieldSys.CurrentShields / targetShieldSys.MaxShields;
                GUI.color = pct > 0.5f ? Color.cyan : (pct > 0 ? Color.yellow : Color.red);
                GUILayout.Label($"Target Shields: {targetShieldSys.CurrentShields:F0}/{targetShieldSys.MaxShields:F0}");
                GUI.color = Color.white;
            }
        }

        GUILayout.Space(10);

        // Player heat
        if (playerShip?.HeatManager != null)
        {
            float heat = playerShip.HeatManager.CurrentHeat;
            float maxHeat = playerShip.HeatManager.MaxHeat;
            GUI.color = heat > maxHeat * 0.7f ? Color.red : (heat > maxHeat * 0.4f ? Color.yellow : Color.white);
            GUILayout.Label($"<b>Heat:</b> {heat:F0}/{maxHeat}");
            GUI.color = Color.white;
        }

        // Player shields
        if (playerShip?.ShieldSystem != null)
        {
            var shields = playerShip.ShieldSystem;
            float pct = shields.CurrentShields / shields.MaxShields;
            GUI.color = pct > 0.5f ? Color.cyan : (pct > 0 ? Color.yellow : Color.red);
            GUILayout.Label($"<b>Shields:</b> {shields.CurrentShields:F0}/{shields.MaxShields:F0}");
            GUI.color = Color.white;
        }

        GUILayout.Space(10);

        // Ammo status (from Phase 2.2)
        GUILayout.Label("<b>AMMO STATUS:</b>");
        if (playerShip?.WeaponManager != null)
        {
            foreach (WeaponSystem weapon in playerShip.WeaponManager.Weapons)
            {
                if (weapon.AmmoCapacity > 0)
                {
                    float pct = (float)weapon.CurrentAmmo / weapon.AmmoCapacity;
                    if (pct <= 0)
                        GUI.color = Color.red;
                    else if (pct < 0.25f)
                        GUI.color = Color.yellow;
                    else
                        GUI.color = Color.green;

                    string status = weapon.CurrentAmmo > 0
                        ? $"{weapon.CurrentAmmo}/{weapon.AmmoCapacity}"
                        : "EMPTY!";
                    GUILayout.Label($"  {weapon.WeaponName}: {status}");
                }
            }
        }
        GUI.color = Color.white;

        GUILayout.Space(10);

        // Weapon groups legend
        GUILayout.Label("<b>WEAPON GROUPS:</b>");
        GUI.color = Color.cyan;
        GUILayout.Label("  1: RailGuns");
        GUI.color = Color.magenta;
        GUILayout.Label("  2: Cannon");
        GUI.color = new Color(1f, 0.5f, 0f); // Orange
        GUILayout.Label("  3: Torpedoes");
        GUI.color = Color.yellow;
        GUILayout.Label("  4: Missiles");
        GUI.color = Color.white;

        GUILayout.Space(10);

        // Controls reminder
        GUILayout.Label("<size=10>A=Alpha Strike | K=Cycle Target</size>");
        GUILayout.Label("<size=10>R=Reset CD | L=Reload | J=Panel</size>");

        GUILayout.EndArea();
    }

    void DrawTargetGizmo()
    {
        // Draw line from player to weapon target in scene view
        // Note: This only works in OnDrawGizmos, not OnGUI
        // We'll handle this in OnDrawGizmos instead
    }

    void OnDrawGizmos()
    {
        // Draw line to weapon target
        if (playerShip != null && enemyShips.Count > 0 && enemyShips[0] != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(playerShip.transform.position, enemyShips[0].transform.position);
        }

        // Draw line to current test target (different color)
        if (playerShip != null && targetShip != null && targetShip != playerShip)
        {
            Gizmos.color = Color.yellow;
            Vector3 offset = Vector3.up * 0.5f; // Slight offset to not overlap
            Gizmos.DrawLine(playerShip.transform.position + offset, targetShip.transform.position + offset);
        }
    }

    void OnGUI()
    {
        // Always draw the status overlay on the left (from Phase 2.2)
        DrawStatusOverlay();

        // Draw gizmo line to current weapon target
        DrawTargetGizmo();

        if (!showUI) return;

        float panelWidth = 380f;
        float panelHeight = 650f;
        Rect panelRect = new Rect(Screen.width - panelWidth - 10, 10, panelWidth, panelHeight);

        GUI.Box(panelRect, "");

        GUILayout.BeginArea(new Rect(panelRect.x + 5, panelRect.y + 5, panelWidth - 10, panelHeight - 10));

        // Title
        GUILayout.Label("<size=14><b>PHASE 3 UNIFIED TEST</b></size>");

        // Target selector
        GUILayout.BeginHorizontal();
        GUILayout.Label($"<b>Target:</b> {targetShip?.gameObject.name ?? "None"}");
        if (GUILayout.Button("Switch (K)", GUILayout.Width(80)))
        {
            CycleTarget();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Tab buttons
        GUILayout.BeginHorizontal();
        DrawTabButton("Combat", TestTab.Combat);
        DrawTabButton("Sections", TestTab.Sections);
        DrawTabButton("Systems", TestTab.Systems);
        DrawTabButton("Core/Death", TestTab.CoreDeath);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        DrawTabButton("Projectiles", TestTab.Projectiles);
        DrawTabButton("Weapons", TestTab.Weapons);
        DrawTabButton("Status", TestTab.Status);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Damage slider (shared across tabs)
        GUILayout.Label($"<b>Damage: {testDamage:F0}</b>");
        testDamage = GUILayout.HorizontalSlider(testDamage, 10f, 300f);

        GUILayout.Space(5);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(panelHeight - 160));

        // Draw current tab content
        switch (currentTab)
        {
            case TestTab.Combat: DrawCombatTab(); break;
            case TestTab.Sections: DrawSectionsTab(); break;
            case TestTab.Systems: DrawSystemsTab(); break;
            case TestTab.CoreDeath: DrawCoreDeathTab(); break;
            case TestTab.Projectiles: DrawProjectilesTab(); break;
            case TestTab.Weapons: DrawWeaponsTab(); break;
            case TestTab.Status: DrawStatusTab(); break;
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void DrawTabButton(string label, TestTab tab)
    {
        string displayLabel = currentTab == tab ? $"[{label}]" : label;
        if (GUILayout.Button(displayLabel))
        {
            currentTab = tab;
        }
    }

    #region Combat Tab
    void DrawCombatTab()
    {
        GUILayout.Label("<b>SHIELD TESTS</b>");

        if (GUILayout.Button($"Hit Shields ({testDamage:F0} damage)"))
        {
            ApplyDamageToTarget(testDamage, targetSection);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Deplete Shields"))
        {
            DepleteTargetShields();
        }
        if (GUILayout.Button("Restore Shields"))
        {
            RestoreTargetShields();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("<b>QUICK DAMAGE</b>");

        // Section selection
        GUILayout.Label($"Target Section: {targetSection}");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fore")) targetSection = SectionType.Fore;
        if (GUILayout.Button("Aft")) targetSection = SectionType.Aft;
        if (GUILayout.Button("Port")) targetSection = SectionType.Port;
        if (GUILayout.Button("Stb")) targetSection = SectionType.Starboard;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Dorsal")) targetSection = SectionType.Dorsal;
        if (GUILayout.Button("Ventral")) targetSection = SectionType.Ventral;
        if (GUILayout.Button("Core")) targetSection = SectionType.Core;
        GUILayout.EndHorizontal();

        if (GUILayout.Button($"Apply {testDamage:F0} to {targetSection}"))
        {
            ApplyDirectSectionDamage(targetSection, testDamage);
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>RESET</b>");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Target"))
        {
            ResetTargetShip();
        }
        if (GUILayout.Button("Reset All"))
        {
            ResetAllShips();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Clear Combat Log"))
        {
            combatLog?.Clear();
        }
    }
    #endregion

    #region Sections Tab
    void DrawSectionsTab()
    {
        GUILayout.Label("<b>SECTION DAMAGE</b>");

        // Show all sections with status
        if (targetSections != null)
        {
            foreach (var section in targetSections.GetAllSections())
            {
                if (section == null) continue;

                Color statusColor = section.IsBreached ? Color.red :
                    (section.CurrentArmor < section.MaxArmor ? Color.yellow : Color.green);

                GUI.color = statusColor;
                string status = section.IsBreached ? "BREACHED" :
                    $"A:{section.CurrentArmor:F0}/{section.MaxArmor:F0} S:{section.CurrentStructure:F0}/{section.MaxStructure:F0}";

                GUILayout.BeginHorizontal();
                GUILayout.Label($"{section.SectionType}: {status}", GUILayout.Width(220));
                GUI.color = Color.white;

                if (GUILayout.Button("Dmg", GUILayout.Width(40)))
                {
                    ApplyDirectSectionDamage(section.SectionType, testDamage);
                }
                if (GUILayout.Button("Brch", GUILayout.Width(40)))
                {
                    BreachSection(section.SectionType);
                }
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>BULK OPERATIONS</b>");

        if (GUILayout.Button("Breach All Outer Sections"))
        {
            BreachAllOuterSections();
        }

        if (GUILayout.Button("Reset All Sections"))
        {
            targetSections?.ResetAllSections();
        }
    }
    #endregion

    #region Systems Tab
    void DrawSystemsTab()
    {
        GUILayout.Label("<b>SYSTEM DEGRADATION</b>");

        if (targetDegradation != null)
        {
            GUILayout.Label($"Speed: x{targetDegradation.SpeedMultiplier:F2}");
            GUILayout.Label($"Turn Rate: x{targetDegradation.TurnRateMultiplier:F2}");
            GUILayout.Label($"Cooling: x{targetDegradation.CoolingMultiplier:F2}");
            GUILayout.Label($"Targeting: x{targetDegradation.TargetingRangeMultiplier:F2}");
            GUILayout.Label($"Heat Cap: x{targetDegradation.HeatCapacityMultiplier:F2}");
            GUILayout.Label($"Passive Heat: +{targetDegradation.PassiveHeatGeneration:F1}");
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>MOUNTED SYSTEMS</b>");

        if (targetSystems.Count > 0)
        {
            for (int i = 0; i < targetSystems.Count; i++)
            {
                var system = targetSystems[i];
                if (system == null) continue;

                Color stateColor = system.CurrentState switch
                {
                    SystemState.Operational => Color.green,
                    SystemState.Damaged => Color.yellow,
                    SystemState.Destroyed => Color.red,
                    _ => Color.white
                };

                GUI.color = stateColor;
                string prefix = (i == selectedSystemIndex) ? ">" : " ";
                string typeName = system.GetType().Name.Replace("Mounted", "");

                GUILayout.BeginHorizontal();
                GUILayout.Label($"{prefix}{typeName}: {system.CurrentState}", GUILayout.Width(200));
                GUI.color = Color.white;

                if (GUILayout.Button("Dmg", GUILayout.Width(40)))
                {
                    system.TakeCriticalHit();
                    LogSystemDamage(system);
                }
                if (GUILayout.Button("Fix", GUILayout.Width(40)))
                {
                    system.Repair();
                }
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>QUICK ACTIONS</b>");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Damage Engine"))
        {
            DamageSystemByType<MountedEngine>();
        }
        if (GUILayout.Button("Damage Weapon"))
        {
            DamageSystemByType<MountedWeapon>();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Damage Reactor"))
        {
            DamageSystemByType<MountedReactor>();
        }
        if (GUILayout.Button("Damage Radiator"))
        {
            DamageSystemByType<MountedRadiator>();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Repair All Systems"))
        {
            foreach (var system in targetSystems)
            {
                system?.Repair();
            }
        }
    }
    #endregion

    #region Core/Death Tab
    void DrawCoreDeathTab()
    {
        GUILayout.Label("<b>CORE PROTECTION</b>");

        if (targetCoreProtection != null)
        {
            bool exposed = targetCoreProtection.IsCoreExposed();
            var exposedAngles = targetCoreProtection.GetExposedAngles();

            GUI.color = exposed ? Color.red : Color.green;
            GUILayout.Label($"Core Exposed: {exposed}");
            GUI.color = Color.white;

            if (exposedAngles.Count > 0)
            {
                GUILayout.Label($"Exposed From: {string.Join(", ", exposedAngles)}");
            }

            GUILayout.Label($"Lucky Shot Chance: {targetCoreProtection.LuckyShotChance * 100f:F0}%");
        }

        GUILayout.Space(5);
        GUILayout.Label($"Attack Direction: {directionPresets[currentDirectionIndex].name}");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<"))
        {
            currentDirectionIndex = (currentDirectionIndex - 1 + directionPresets.Length) % directionPresets.Length;
        }
        if (GUILayout.Button(">"))
        {
            currentDirectionIndex = (currentDirectionIndex + 1) % directionPresets.Length;
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Attack Core (test protection)"))
        {
            AttackCore();
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>SHIP DEATH CONDITIONS</b>");

        if (targetDeathController != null)
        {
            GUILayout.Label($"Is Destroyed: {targetShip?.IsDestroyed ?? false}");
            GUILayout.Label($"Is Disabled: {targetDeathController.IsDisabled}");
            GUILayout.Label($"Death Cause: {targetDeathController.Cause}");
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Breach Core (Core Breached Death)"))
        {
            BreachSection(SectionType.Fore);
            BreachSection(SectionType.Core);
            targetDeathController?.CheckDeathConditions();
        }

        if (GUILayout.Button("Destroy Reactor (Core Breach Death)"))
        {
            DestroyTargetReactor();
        }

        if (GUILayout.Button("Combat Ineffective (Disable)"))
        {
            MakeCombatIneffective();
        }

        if (GUILayout.Button("Breach All Sections (Death)"))
        {
            BreachAllOuterSections();
            BreachSection(SectionType.Core);
            targetDeathController?.CheckDeathConditions();
        }
    }
    #endregion

    #region Projectiles Tab
    void DrawProjectilesTab()
    {
        GUILayout.Label("<b>PROJECTILE TESTS</b>");

        Ship shooter = targetShip == playerShip ? (enemyShips.Count > 0 ? enemyShips[0] : null) : playerShip;

        GUILayout.Label($"Shooter: {shooter?.gameObject.name ?? "None"}");
        GUILayout.Label($"Target: {targetShip?.gameObject.name ?? "None"}");

        GUILayout.Space(10);

        if (GUILayout.Button($"Fire Ballistic ({testDamage:F0} damage)"))
        {
            FireProjectileAtTarget(false);
        }

        if (GUILayout.Button($"Fire Homing Missile ({testDamage * 1.5f:F0} damage)"))
        {
            FireProjectileAtTarget(true);
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>BURST FIRE</b>");

        if (GUILayout.Button("Fire 5 Projectiles"))
        {
            for (int i = 0; i < 5; i++)
            {
                FireProjectileAtTarget(false);
            }
        }

        if (GUILayout.Button("Fire 3 Missiles"))
        {
            for (int i = 0; i < 3; i++)
            {
                FireProjectileAtTarget(true);
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>COMBAT LOG</b>");

        if (GUILayout.Button("Log Test Hit"))
        {
            combatLog?.LogHit(targetShip?.gameObject.name ?? "Target", targetSection, testDamage, 20f, 15f, 15f);
        }

        if (GUILayout.Button("Log Test Critical"))
        {
            combatLog?.LogCritical(targetShip?.gameObject.name ?? "Target", targetSection, ShipSystemType.NewtonianCannon, false);
        }

        if (GUILayout.Button("Log Test Breach"))
        {
            combatLog?.LogBreach(targetShip?.gameObject.name ?? "Target", targetSection);
        }

        if (GUILayout.Button("Clear Log"))
        {
            combatLog?.Clear();
        }
    }
    #endregion

    #region Weapons Tab
    void DrawWeaponsTab()
    {
        GUILayout.Label("<b>WEAPON SYSTEM FIRE TESTS</b>");
        GUILayout.Label("Uses actual WeaponSystem components to fire projectiles");
        GUILayout.Label("that route through the full damage system.");
        GUILayout.Space(5);

        // Get weapon manager from player ship
        WeaponManager playerWeaponManager = playerShip?.GetComponent<WeaponManager>();
        if (playerWeaponManager == null)
        {
            GUILayout.Label("<color=red>No WeaponManager on player ship!</color>");
            return;
        }

        // Find all weapons on player ship
        WeaponSystem[] playerWeapons = playerShip.GetComponentsInChildren<WeaponSystem>();

        GUILayout.Label($"<b>Player Weapons: {playerWeapons.Length}</b>");

        // Select target for weapons
        Ship weaponTarget = (enemyShips.Count > 0) ? enemyShips[0] : null;
        GUILayout.Label($"Target: {weaponTarget?.gameObject.name ?? "None"}");

        if (weaponTarget == null)
        {
            GUILayout.Label("<color=yellow>No target available!</color>");
            return;
        }

        GUILayout.Space(5);

        // List each weapon with fire button
        foreach (var weapon in playerWeapons)
        {
            if (weapon == null) continue;

            GUILayout.BeginHorizontal();

            // Weapon info
            string weaponName = weapon.WeaponName;
            string status = weapon.CanFireSilent() ? "Ready" :
                           (weapon.CurrentCooldown > 0 ? $"CD:{weapon.CurrentCooldown}" : "Can't Fire");

            Color statusColor = weapon.CanFireSilent() ? Color.green : Color.yellow;
            GUI.color = statusColor;
            GUILayout.Label($"{weaponName}: {status}", GUILayout.Width(180));
            GUI.color = Color.white;

            // Set target and fire
            if (GUILayout.Button("Fire", GUILayout.Width(50)))
            {
                FireWeaponAtTarget(weapon, weaponTarget);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>QUICK FIRE ALL</b>");

        if (GUILayout.Button("Fire All Weapons at Target"))
        {
            foreach (var weapon in playerWeapons)
            {
                if (weapon != null)
                {
                    FireWeaponAtTarget(weapon, weaponTarget);
                }
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>ENEMY FIRES AT PLAYER</b>");

        // Get weapons from first enemy
        if (enemyShips.Count > 0)
        {
            WeaponSystem[] enemyWeapons = enemyShips[0].GetComponentsInChildren<WeaponSystem>();

            foreach (var weapon in enemyWeapons)
            {
                if (weapon == null) continue;

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Enemy {weapon.WeaponName}", GUILayout.Width(180));

                if (GUILayout.Button("Fire at Player", GUILayout.Width(100)))
                {
                    FireWeaponAtTarget(weapon, playerShip);
                }

                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Enemy Alpha Strike (all weapons)"))
            {
                foreach (var weapon in enemyWeapons)
                {
                    if (weapon != null)
                    {
                        FireWeaponAtTarget(weapon, playerShip);
                    }
                }
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>WEAPON STATS</b>");

        if (playerWeapons.Length > 0)
        {
            var firstWeapon = playerWeapons[0];
            GUILayout.Label($"Sample: {firstWeapon.WeaponName}");
            GUILayout.Label($"  Damage: {firstWeapon.Damage:F0} (Effective: {firstWeapon.EffectiveDamage:F0})");
            GUILayout.Label($"  Heat Cost: {firstWeapon.HeatCost}");
            GUILayout.Label($"  Cooldown: {firstWeapon.MaxCooldown} turns");
            GUILayout.Label($"  Range: {firstWeapon.MaxRange:F0} units");
            GUILayout.Label($"  Arc: {firstWeapon.FiringArc:F0}°");
        }
    }

    void FireWeaponAtTarget(WeaponSystem weapon, Ship target)
    {
        if (weapon == null || target == null)
        {
            Debug.LogWarning("[Weapons] Cannot fire - weapon or target is null");
            return;
        }

        // Initialize weapon if needed
        Ship ownerShip = weapon.GetComponentInParent<Ship>();
        if (weapon.OwnerShip == null && ownerShip != null)
        {
            weapon.Initialize(ownerShip);
        }

        // Set target
        weapon.SetTarget(target);

        // Check if weapon can fire
        if (!weapon.CanFire())
        {
            Debug.LogWarning($"[Weapons] {weapon.WeaponName} cannot fire at {target.gameObject.name}");
            return;
        }

        // Fire the weapon (this spawns the projectile)
        StartCoroutine(weapon.FireWithSpinUp());
        Debug.Log($"[Weapons] {weapon.WeaponName} fired at {target.gameObject.name}!");
    }
    #endregion

    #region Status Tab
    void DrawStatusTab()
    {
        GUILayout.Label("<b>ALL SHIPS STATUS</b>");

        // Player status
        DrawShipStatus(playerShip, "PLAYER", Color.cyan);

        GUILayout.Space(10);

        // Enemy statuses
        for (int i = 0; i < enemyShips.Count; i++)
        {
            DrawShipStatus(enemyShips[i], $"ENEMY {i + 1}", Color.red);
            GUILayout.Space(5);
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>QUICK REFERENCE</b>");
        GUILayout.Label("J: Toggle UI | K: Switch Target");
        GUILayout.Label("Tab: Switch Tab | 1-7: Select Section");
    }

    void DrawShipStatus(Ship ship, string label, Color labelColor)
    {
        if (ship == null)
        {
            GUILayout.Label($"{label}: NULL");
            return;
        }

        GUI.color = labelColor;
        GUILayout.Label($"<b>{label}: {ship.gameObject.name}</b>");
        GUI.color = Color.white;

        ShieldSystem shields = ship.ShieldSystem;
        SectionManager sections = ship.SectionManager;
        ShipDeathController death = ship.GetComponent<ShipDeathController>();

        if (shields != null)
        {
            float pct = shields.CurrentShields / shields.MaxShields;
            GUI.color = pct > 0.5f ? Color.green : (pct > 0 ? Color.yellow : Color.red);
            GUILayout.Label($"  Shields: {shields.CurrentShields:F0}/{shields.MaxShields:F0}");
            GUI.color = Color.white;
        }

        if (sections != null)
        {
            int breached = sections.GetBreachedSections().Count;
            GUI.color = breached == 0 ? Color.green : (breached < 4 ? Color.yellow : Color.red);
            GUILayout.Label($"  Breached: {breached}/7 sections");
            GUI.color = Color.white;
        }

        if (death != null)
        {
            GUI.color = ship.IsDestroyed ? Color.red : (death.IsDisabled ? Color.yellow : Color.green);
            string state = ship.IsDestroyed ? "DESTROYED" : (death.IsDisabled ? "DISABLED" : "OPERATIONAL");
            GUILayout.Label($"  State: {state}");
            GUI.color = Color.white;
        }
    }
    #endregion

    #region Action Methods
    void ApplyDamageToTarget(float damage, SectionType section)
    {
        if (targetDamageRouter == null) return;

        DamageReport report = targetDamageRouter.ProcessDamage(damage, section);
        Debug.Log($"[Combat] Damage to {targetShip?.gameObject.name}: {report}");

        damageUIManager?.ProcessDamageReport(report, targetShip?.gameObject.name);
    }

    void DepleteTargetShields()
    {
        if (targetShields == null) return;
        float damage = targetShields.CurrentShields + 10f;
        ApplyDamageToTarget(damage, SectionType.Fore);
    }

    void RestoreTargetShields()
    {
        if (targetShields == null) return;
        targetShields.RestoreShields(targetShields.MaxShields);
        Debug.Log($"[Combat] Shields restored to {targetShields.CurrentShields}");
    }

    void ApplyDirectSectionDamage(SectionType type, float damage)
    {
        if (targetSections == null) return;

        ShipSection section = targetSections.GetSection(type);
        if (section == null) return;

        DamageResult result = section.ApplyDamage(damage);
        Debug.Log($"[Combat] Direct damage to {targetShip?.gameObject.name} {type}: {result}");

        if (combatLog != null)
        {
            combatLog.LogHit(targetShip?.gameObject.name, type, damage, 0f, result.DamageToArmor, result.DamageToStructure);

            if (result.CriticalResult.HasValue)
            {
                var crit = result.CriticalResult.Value;
                if (crit.SystemWasDamaged || crit.SystemWasDestroyed)
                {
                    combatLog.LogCritical(targetShip?.gameObject.name, type, crit.SystemTypeHit, crit.SystemWasDestroyed);
                }
            }

            if (result.SectionBreached)
            {
                combatLog.LogBreach(targetShip?.gameObject.name, type);
            }
        }
    }

    void BreachSection(SectionType type)
    {
        if (targetSections == null) return;

        ShipSection section = targetSections.GetSection(type);
        if (section == null || section.IsBreached) return;

        float damageNeeded = section.CurrentArmor + section.CurrentStructure + 50f;
        ApplyDirectSectionDamage(type, damageNeeded);
    }

    void BreachAllOuterSections()
    {
        SectionType[] outer = { SectionType.Fore, SectionType.Aft, SectionType.Port,
                                SectionType.Starboard, SectionType.Dorsal, SectionType.Ventral };
        foreach (var type in outer)
        {
            BreachSection(type);
        }
    }

    void AttackCore()
    {
        if (targetDamageRouter == null) return;

        Vector3 attackDir = directionPresets[currentDirectionIndex].dir;
        Debug.Log($"Attacking Core from {directionPresets[currentDirectionIndex].name}...");

        DamageReport report = targetDamageRouter.ProcessDamage(testDamage, SectionType.Core, attackDir);

        if (report.CoreWasProtected)
        {
            Debug.Log($"Core PROTECTED - redirected to {report.SectionHit}");
        }
        if (report.WasLuckyShot)
        {
            Debug.Log("LUCKY SHOT hit Core!");
        }

        damageUIManager?.ProcessDamageReport(report, targetShip?.gameObject.name);
    }

    void DamageSystemByType<T>() where T : MountedSystem
    {
        foreach (var system in targetSystems)
        {
            if (system is T)
            {
                system.TakeCriticalHit();
                LogSystemDamage(system);
                return;
            }
        }
    }

    void LogSystemDamage(MountedSystem system)
    {
        Debug.Log($"[Combat] {system.GetType().Name} now {system.CurrentState}");
        combatLog?.LogSystemDamage(targetShip?.gameObject.name, system.SystemType, system.CurrentState);
    }

    void DestroyTargetReactor()
    {
        MountedReactor reactor = targetShip?.GetComponentInChildren<MountedReactor>();
        if (reactor != null)
        {
            while (reactor.CurrentState != SystemState.Destroyed)
            {
                reactor.TakeCriticalHit();
            }
            targetDeathController?.CheckDeathConditions();
        }
    }

    void MakeCombatIneffective()
    {
        MountedEngine[] engines = targetShip?.GetComponentsInChildren<MountedEngine>();
        MountedWeapon[] weapons = targetShip?.GetComponentsInChildren<MountedWeapon>();

        if (engines != null)
        {
            foreach (var e in engines)
            {
                while (e.CurrentState != SystemState.Destroyed)
                    e.TakeCriticalHit();
            }
        }

        if (weapons != null)
        {
            foreach (var w in weapons)
            {
                while (w.CurrentState != SystemState.Destroyed)
                    w.TakeCriticalHit();
            }
        }

        targetDeathController?.CheckDeathConditions();
        Debug.Log($"[Combat] {targetShip?.gameObject.name} made combat ineffective!");
    }

    void FireProjectileAtTarget(bool homing)
    {
        Ship shooter = targetShip == playerShip ? (enemyShips.Count > 0 ? enemyShips[0] : null) : playerShip;
        if (shooter == null || targetShip == null) return;

        Vector3 spawnPos = shooter.transform.position + shooter.transform.forward * 2f;
        Vector3 targetPos = targetShip.transform.position;

        var spawnInfo = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = homing ? WeaponSystem.ProjectileSpawnInfo.ProjectileType.Homing
                          : WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
            SpawnPosition = spawnPos,
            SpawnRotation = Quaternion.LookRotation(targetPos - spawnPos),
            TargetPosition = targetPos,
            TargetShip = targetShip,
            Damage = homing ? testDamage * 1.5f : testDamage,
            Speed = homing ? 30f : 50f,
            OwnerShip = shooter
        };

        if (homing)
        {
            ProjectileManager.SpawnHomingProjectile(spawnInfo, 90f);
            Debug.Log($"[Combat] Missile fired at {targetShip.gameObject.name}");
        }
        else
        {
            ProjectileManager.SpawnBallisticProjectile(spawnInfo);
            Debug.Log($"[Combat] Projectile fired at {targetShip.gameObject.name}");
        }
    }

    void ResetTargetShip()
    {
        targetSections?.ResetAllSections();
        targetShields?.RestoreShields(targetShields.MaxShields);

        foreach (var system in targetSystems)
        {
            system?.Repair();
        }

        Debug.Log($"[Combat] {targetShip?.gameObject.name} reset.");
    }

    void ResetAllShips()
    {
        Ship original = targetShip;

        // Reset player
        targetShip = playerShip;
        CacheTargetComponents();
        ResetTargetShip();

        // Reset all enemies
        foreach (var enemy in enemyShips)
        {
            targetShip = enemy;
            CacheTargetComponents();
            ResetTargetShip();
        }

        // Restore original target
        targetShip = original;
        CacheTargetComponents();

        combatLog?.Clear();
        Debug.Log("[Combat] All ships reset.");
    }
    #endregion
}
