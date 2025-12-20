using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unified test controller for all Phase 3.5 integration features.
/// Extends Phase 3 testing with Turn System, Weapon Firing Queue, and Heat/Cooldown tabs.
/// </summary>
public class Phase35UnifiedTestController : MonoBehaviour
{
    [Header("Ship References")]
    public Ship playerShip;
    public List<Ship> enemyShips = new List<Ship>();

    [Header("Phase 3 System References")]
    public ProjectileManager projectileManager;
    public DamageUIManager damageUIManager;
    public TargetingController targetingController;

    [Header("Phase 3.5 Integration References")]
    public TurnManager turnManager;
    public TurnEndProcessor turnEndProcessor;
    public WeaponFiringQueue firingQueue;
    public MovementExecutor movementExecutor;
    public WeaponArcValidator arcValidator;

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
    private HeatManager playerHeatManager;
    private WeaponManager playerWeaponManager;
    private CombatLogPanel combatLog;

    // UI state - Extended tabs for Phase 3.5
    private enum TestTab { Combat, Sections, Systems, CoreDeath, Projectiles, Weapons, Status, Turns, HeatCooldown, FiringQueue, Movement }
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

    // Phase 2.2 integration
    private WeaponConfigPanel configPanel;
    private WeaponGroupPanel groupPanel;
    private bool weaponGroupsInitialized = false;

    // Phase 3.5 specific state
    private List<string> turnEventLog = new List<string>();
    private const int MAX_TURN_LOG_ENTRIES = 15;

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

        // Cache player components
        if (playerShip != null)
        {
            playerHeatManager = playerShip.GetComponent<HeatManager>();
            playerWeaponManager = playerShip.GetComponent<WeaponManager>();
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

        // Find Phase 3.5 systems if not assigned
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();
        if (turnEndProcessor == null)
            turnEndProcessor = FindFirstObjectByType<TurnEndProcessor>();
        if (firingQueue == null)
            firingQueue = FindFirstObjectByType<WeaponFiringQueue>();
        if (movementExecutor == null)
            movementExecutor = FindFirstObjectByType<MovementExecutor>();
        if (arcValidator == null)
            arcValidator = FindFirstObjectByType<WeaponArcValidator>();

        // Subscribe to turn events for logging
        SubscribeToTurnEvents();

        // Find UI components
        if (damageUIManager == null)
            damageUIManager = FindFirstObjectByType<DamageUIManager>();

        if (damageUIManager != null)
            combatLog = damageUIManager.CombatLog;

        Debug.Log("=== Phase 3.5 Unified Test Controller ===");
        Debug.Log("CONTROLS:");
        Debug.Log("  J: Toggle test panel UI");
        Debug.Log("  K: Cycle target ships");
        Debug.Log("  Tab: Cycle UI tabs");
        Debug.Log("  T: Advance turn");
        Debug.Log("");
        Debug.Log("WEAPON CONTROLS:");
        Debug.Log("  1,2,3,4: Fire weapon groups");
        Debug.Log("  A: Alpha Strike (all weapons)");
        Debug.Log("  Space: Toggle weapon config panel");
        Debug.Log("  R: Reset cooldowns (cheat)");
        Debug.Log("  L: Reload all ammo (cheat)");

        // Initialize weapon systems
        Invoke(nameof(InitializeWeaponSystems), 0.2f);
    }

    void SubscribeToTurnEvents()
    {
        if (turnManager != null)
        {
            turnManager.OnCommandPhaseStart += () => LogTurnEvent($"Command Phase Start (Turn {turnManager.CurrentTurn})");
            turnManager.OnSimulationPhaseStart += () => LogTurnEvent($"Simulation Phase Start (Turn {turnManager.CurrentTurn})");
            turnManager.OnTurnEnd += (turn) => LogTurnEvent($"Turn End (Turn {turn})");
        }

        if (turnEndProcessor != null)
        {
            turnEndProcessor.OnHeatDissipated += (ship, amount) => LogTurnEvent($"{ship.gameObject.name}: -{amount:F1} heat");
            turnEndProcessor.OnWeaponReady += (weapon) => LogTurnEvent($"{weapon.WeaponName} ready!");
        }
    }

    void LogTurnEvent(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        turnEventLog.Insert(0, $"[{timestamp}] {message}");

        while (turnEventLog.Count > MAX_TURN_LOG_ENTRIES)
        {
            turnEventLog.RemoveAt(turnEventLog.Count - 1);
        }
    }

    void InitializeWeaponSystems()
    {
        if (weaponGroupsInitialized) return;

        CreateWeaponUIPanels();
        AssignWeaponGroups();

        if (enemyShips.Count > 0)
        {
            SetAllWeaponsTarget(enemyShips[0]);
        }

        weaponGroupsInitialized = true;
    }

    void CreateWeaponUIPanels()
    {
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
        if (wm == null) return;

        foreach (WeaponSystem weapon in wm.Weapons)
        {
            if (weapon is RailGun) wm.AssignWeaponToGroup(weapon, 1);
            else if (weapon is NewtonianCannon) wm.AssignWeaponToGroup(weapon, 2);
            else if (weapon is TorpedoLauncher) wm.AssignWeaponToGroup(weapon, 3);
            else if (weapon is MissileBattery) wm.AssignWeaponToGroup(weapon, 4);
        }
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
            if (targetShip != playerShip)
            {
                SetAllWeaponsTarget(targetShip);
            }
        }

        // Cycle tabs
        if (Input.GetKeyDown(KeyCode.Tab) && showUI)
        {
            currentTab = (TestTab)(((int)currentTab + 1) % System.Enum.GetValues(typeof(TestTab)).Length);
        }

        // Advance turn (Phase 3.5)
        if (Input.GetKeyDown(KeyCode.T))
        {
            AdvanceTurn();
        }

        // Weapon controls
        HandleWeaponInput();
    }

    void HandleWeaponInput()
    {
        WeaponManager wm = playerShip?.WeaponManager;
        if (wm == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) FireWeaponGroup(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) FireWeaponGroup(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) FireWeaponGroup(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) FireWeaponGroup(4);

        if (Input.GetKeyDown(KeyCode.A))
        {
            int fired = 0;
            foreach (WeaponSystem weapon in wm.Weapons)
            {
                if (weapon.CanFire())
                {
                    StartCoroutine(weapon.FireWithSpinUp());
                    fired++;
                }
            }
            LogTurnEvent($"Alpha Strike: {fired} weapons fired");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (configPanel != null)
                configPanel.IsVisible = !configPanel.IsVisible;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllCooldowns();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ReloadAllAmmo();
        }
    }

    void FireWeaponGroup(int groupNum)
    {
        WeaponManager wm = playerShip?.WeaponManager;
        if (wm == null) return;

        List<WeaponSystem> weapons = wm.GetWeaponsInGroup(groupNum);
        if (weapons.Count == 0) return;

        Ship target = (enemyShips.Count > 0) ? enemyShips[0] : null;
        int fired = 0;

        foreach (WeaponSystem weapon in weapons)
        {
            if (weapon.OwnerShip == null)
                weapon.Initialize(playerShip);
            if (weapon.AssignedTarget == null && target != null)
                weapon.SetTarget(target);

            if (weapon.CanFire())
            {
                StartCoroutine(weapon.FireWithSpinUp());
                fired++;
            }
        }

        LogTurnEvent($"Group {groupNum}: {fired} fired");
    }

    void AdvanceTurn()
    {
        if (turnManager != null)
        {
            turnManager.ForceEndTurn();
        }
    }

    void CycleTarget()
    {
        List<Ship> allTargets = new List<Ship> { playerShip };
        allTargets.AddRange(enemyShips);

        targetIndex = (targetIndex + 1) % allTargets.Count;
        targetShip = allTargets[targetIndex];
        CacheTargetComponents();
    }

    void OnGUI()
    {
        // Always draw the status overlay on the left
        DrawStatusOverlay();

        if (!showUI) return;

        float panelWidth = 400f;
        float panelHeight = 700f;
        Rect panelRect = new Rect(Screen.width - panelWidth - 10, 10, panelWidth, panelHeight);

        GUI.Box(panelRect, "");

        GUILayout.BeginArea(new Rect(panelRect.x + 5, panelRect.y + 5, panelWidth - 10, panelHeight - 10));

        GUILayout.Label("<size=14><b>PHASE 3.5 UNIFIED TEST</b></size>");

        // Target selector
        GUILayout.BeginHorizontal();
        GUILayout.Label($"<b>Target:</b> {targetShip?.gameObject.name ?? "None"}");
        if (GUILayout.Button("Switch (K)", GUILayout.Width(80)))
        {
            CycleTarget();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Tab buttons - Row 1 (Phase 3)
        GUILayout.BeginHorizontal();
        DrawTabButton("Combat", TestTab.Combat);
        DrawTabButton("Sections", TestTab.Sections);
        DrawTabButton("Systems", TestTab.Systems);
        DrawTabButton("Core/Death", TestTab.CoreDeath);
        GUILayout.EndHorizontal();

        // Tab buttons - Row 2 (Phase 3)
        GUILayout.BeginHorizontal();
        DrawTabButton("Projectiles", TestTab.Projectiles);
        DrawTabButton("Weapons", TestTab.Weapons);
        DrawTabButton("Status", TestTab.Status);
        GUILayout.EndHorizontal();

        // Tab buttons - Row 3 (Phase 3.5)
        GUILayout.BeginHorizontal();
        GUI.color = Color.cyan;
        DrawTabButton("Turns", TestTab.Turns);
        DrawTabButton("Heat/CD", TestTab.HeatCooldown);
        DrawTabButton("FiringQ", TestTab.FiringQueue);
        DrawTabButton("Movement", TestTab.Movement);
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Damage slider
        GUILayout.Label($"<b>Damage: {testDamage:F0}</b>");
        testDamage = GUILayout.HorizontalSlider(testDamage, 10f, 300f);

        GUILayout.Space(5);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(panelHeight - 180));

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
            case TestTab.Turns: DrawTurnsTab(); break;
            case TestTab.HeatCooldown: DrawHeatCooldownTab(); break;
            case TestTab.FiringQueue: DrawFiringQueueTab(); break;
            case TestTab.Movement: DrawMovementTab(); break;
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

    void DrawStatusOverlay()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));

        GUI.Box(new Rect(0, 0, 300, 480), "");

        GUILayout.Label("<size=12><b>PHASE 3.5 UNIFIED TEST</b></size>");
        GUILayout.Space(5);

        // Turn info
        if (turnManager != null)
        {
            GUI.color = Color.cyan;
            GUILayout.Label($"<b>Turn {turnManager.CurrentTurn}</b> - {turnManager.CurrentPhase}");
            GUI.color = Color.white;
        }

        GUILayout.Space(5);

        // Weapon target info
        Ship weaponTarget = (enemyShips.Count > 0) ? enemyShips[0] : null;
        if (weaponTarget != null)
        {
            GUI.color = Color.red;
            GUILayout.Label($"<b>Target:</b> {weaponTarget.gameObject.name}");
            GUI.color = Color.white;

            float dist = Vector3.Distance(playerShip.transform.position, weaponTarget.transform.position);
            GUILayout.Label($"Distance: {dist:F1} units");

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
        if (playerHeatManager != null)
        {
            float heat = playerHeatManager.CurrentHeat;
            float maxHeat = playerHeatManager.MaxHeat;
            GUI.color = heat > maxHeat * 0.7f ? Color.red : (heat > maxHeat * 0.4f ? Color.yellow : Color.white);
            GUILayout.Label($"<b>Heat:</b> {heat:F0}/{maxHeat}");
            GUI.color = Color.white;

            // Show dissipation rate
            if (turnEndProcessor != null)
            {
                float dissipation = turnEndProcessor.CalculateDissipation(playerShip);
                GUILayout.Label($"Dissipation: {dissipation:F1}/turn");
            }
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

        // Weapon groups
        GUILayout.Label("<b>WEAPON GROUPS:</b>");
        GUI.color = Color.cyan;
        GUILayout.Label("  1: RailGuns");
        GUI.color = Color.magenta;
        GUILayout.Label("  2: Cannon");
        GUI.color = new Color(1f, 0.5f, 0f);
        GUILayout.Label("  3: Torpedoes");
        GUI.color = Color.yellow;
        GUILayout.Label("  4: Missiles");
        GUI.color = Color.white;

        GUILayout.Space(10);

        // Controls reminder
        GUILayout.Label("<size=10>A=Alpha | K=Target | T=Turn | J=Panel</size>");
        GUILayout.Label("<size=10>R=Reset CD | L=Reload</size>");

        GUILayout.EndArea();
    }

    #region Phase 3 Tabs (Combat, Sections, Systems, CoreDeath, Projectiles, Weapons, Status)

    void DrawCombatTab()
    {
        GUILayout.Label("<b>SHIELD TESTS</b>");

        if (GUILayout.Button($"Hit Shields ({testDamage:F0} damage)"))
        {
            ApplyDamageToTarget(testDamage, targetSection);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Deplete Shields")) DepleteTargetShields();
        if (GUILayout.Button("Restore Shields")) RestoreTargetShields();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("<b>QUICK DAMAGE</b>");

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
        if (GUILayout.Button("Reset Target")) ResetTargetShip();
        if (GUILayout.Button("Reset All")) ResetAllShips();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Clear Combat Log"))
        {
            combatLog?.Clear();
        }
    }

    void DrawSectionsTab()
    {
        GUILayout.Label("<b>SECTION DAMAGE</b>");

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
        if (GUILayout.Button("Breach All Outer Sections"))
        {
            BreachAllOuterSections();
        }

        if (GUILayout.Button("Reset All Sections"))
        {
            targetSections?.ResetAllSections();
        }
    }

    void DrawSystemsTab()
    {
        GUILayout.Label("<b>SYSTEM DEGRADATION</b>");

        if (targetDegradation != null)
        {
            GUILayout.Label($"Speed: x{targetDegradation.SpeedMultiplier:F2}");
            GUILayout.Label($"Turn Rate: x{targetDegradation.TurnRateMultiplier:F2}");
            GUILayout.Label($"Cooling: x{targetDegradation.CoolingMultiplier:F2}");
            GUILayout.Label($"Targeting: x{targetDegradation.TargetingRangeMultiplier:F2}");
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>MOUNTED SYSTEMS</b>");

        for (int i = 0; i < Mathf.Min(targetSystems.Count, 10); i++)
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
            string typeName = system.GetType().Name.Replace("Mounted", "");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{typeName}: {system.CurrentState}", GUILayout.Width(180));
            GUI.color = Color.white;

            if (GUILayout.Button("Dmg", GUILayout.Width(40)))
            {
                system.TakeCriticalHit();
            }
            if (GUILayout.Button("Fix", GUILayout.Width(40)))
            {
                system.Repair();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Repair All Systems"))
        {
            foreach (var system in targetSystems)
            {
                system?.Repair();
            }
        }
    }

    void DrawCoreDeathTab()
    {
        GUILayout.Label("<b>CORE PROTECTION</b>");

        if (targetCoreProtection != null)
        {
            bool exposed = targetCoreProtection.IsCoreExposed();
            GUI.color = exposed ? Color.red : Color.green;
            GUILayout.Label($"Core Exposed: {exposed}");
            GUI.color = Color.white;
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

        if (GUILayout.Button("Attack Core")) AttackCore();

        GUILayout.Space(10);
        GUILayout.Label("<b>SHIP DEATH CONDITIONS</b>");

        if (targetDeathController != null)
        {
            GUILayout.Label($"Is Destroyed: {targetShip?.IsDestroyed ?? false}");
            GUILayout.Label($"Is Disabled: {targetDeathController.IsDisabled}");
        }

        if (GUILayout.Button("Breach Core")) { BreachSection(SectionType.Fore); BreachSection(SectionType.Core); }
        if (GUILayout.Button("Destroy Reactor")) DestroyTargetReactor();
    }

    void DrawProjectilesTab()
    {
        GUILayout.Label("<b>PROJECTILE TESTS</b>");

        if (GUILayout.Button($"Fire Ballistic ({testDamage:F0} damage)"))
        {
            FireProjectileAtTarget(false);
        }

        if (GUILayout.Button($"Fire Homing Missile ({testDamage * 1.5f:F0} damage)"))
        {
            FireProjectileAtTarget(true);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Fire 5 Projectiles"))
        {
            for (int i = 0; i < 5; i++) FireProjectileAtTarget(false);
        }
    }

    void DrawWeaponsTab()
    {
        GUILayout.Label("<b>WEAPON TESTS</b>");

        WeaponSystem[] playerWeapons = playerShip?.GetComponentsInChildren<WeaponSystem>();
        if (playerWeapons == null || playerWeapons.Length == 0)
        {
            GUILayout.Label("No weapons found");
            return;
        }

        Ship weaponTarget = (enemyShips.Count > 0) ? enemyShips[0] : null;

        foreach (var weapon in playerWeapons)
        {
            GUILayout.BeginHorizontal();
            string status = weapon.CanFireSilent() ? "Ready" : (weapon.CurrentCooldown > 0 ? $"CD:{weapon.CurrentCooldown}" : "Can't");
            GUI.color = weapon.CanFireSilent() ? Color.green : Color.yellow;
            GUILayout.Label($"{weapon.WeaponName}: {status}", GUILayout.Width(180));
            GUI.color = Color.white;

            if (GUILayout.Button("Fire", GUILayout.Width(50)))
            {
                if (weapon.OwnerShip == null) weapon.Initialize(playerShip);
                if (weapon.AssignedTarget == null && weaponTarget != null) weapon.SetTarget(weaponTarget);
                if (weapon.CanFire()) StartCoroutine(weapon.FireWithSpinUp());
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Fire All Weapons")) { foreach (var w in playerWeapons) { if (w.OwnerShip == null) w.Initialize(playerShip); if (w.AssignedTarget == null && weaponTarget != null) w.SetTarget(weaponTarget); if (w.CanFire()) StartCoroutine(w.FireWithSpinUp()); } }
    }

    void DrawStatusTab()
    {
        GUILayout.Label("<b>ALL SHIPS STATUS</b>");

        DrawShipStatus(playerShip, "PLAYER", Color.cyan);
        GUILayout.Space(10);

        for (int i = 0; i < enemyShips.Count; i++)
        {
            DrawShipStatus(enemyShips[i], $"ENEMY {i + 1}", Color.red);
            GUILayout.Space(5);
        }
    }

    void DrawShipStatus(Ship ship, string label, Color labelColor)
    {
        if (ship == null) { GUILayout.Label($"{label}: NULL"); return; }

        GUI.color = labelColor;
        GUILayout.Label($"<b>{label}: {ship.gameObject.name}</b>");
        GUI.color = Color.white;

        if (ship.ShieldSystem != null)
        {
            float pct = ship.ShieldSystem.CurrentShields / ship.ShieldSystem.MaxShields;
            GUI.color = pct > 0.5f ? Color.green : (pct > 0 ? Color.yellow : Color.red);
            GUILayout.Label($"  Shields: {ship.ShieldSystem.CurrentShields:F0}/{ship.ShieldSystem.MaxShields:F0}");
            GUI.color = Color.white;
        }

        if (ship.SectionManager != null)
        {
            int breached = ship.SectionManager.GetBreachedSections().Count;
            GUI.color = breached == 0 ? Color.green : (breached < 4 ? Color.yellow : Color.red);
            GUILayout.Label($"  Breached: {breached}/7 sections");
            GUI.color = Color.white;
        }
    }

    #endregion

    #region Phase 3.5 Tabs (Turns, HeatCooldown, FiringQueue)

    void DrawTurnsTab()
    {
        GUILayout.Label("<b>TURN SYSTEM (Phase 3.5.1)</b>");

        if (turnManager != null)
        {
            GUILayout.Label($"Current Turn: {turnManager.CurrentTurn}");
            GUILayout.Label($"Current Phase: {turnManager.CurrentPhase}");

            GUILayout.Space(10);
            GUILayout.Label("<b>PHASE CONTROLS</b>");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("End Command (Start Sim)"))
            {
                turnManager.EndCommandPhase();
            }
            if (GUILayout.Button("Start Simulation"))
            {
                turnManager.StartSimulation();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Force End Turn (T)"))
            {
                turnManager.ForceEndTurn();
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>TURN EVENT LOG</b>");

        foreach (string entry in turnEventLog)
        {
            GUILayout.Label(entry, GUILayout.Height(18));
        }

        if (GUILayout.Button("Clear Log"))
        {
            turnEventLog.Clear();
        }
    }

    void DrawHeatCooldownTab()
    {
        GUILayout.Label("<b>HEAT & COOLDOWN (Phase 3.5.3)</b>");

        // Player heat status
        if (playerHeatManager != null)
        {
            float heat = playerHeatManager.CurrentHeat;
            float maxHeat = playerHeatManager.MaxHeat;
            float heatPct = heat / maxHeat * 100f;

            GUILayout.Label($"Heat: {heat:F0}/{maxHeat} ({heatPct:F0}%)");
            GUILayout.Label($"Tier: {playerHeatManager.GetCurrentTier()}");
        }

        // Dissipation info
        if (turnEndProcessor != null)
        {
            GUILayout.Space(5);
            float dissipation = turnEndProcessor.CalculateDissipation(playerShip);
            float radiatorBonus = turnEndProcessor.GetRadiatorBonus(playerShip);
            GUILayout.Label($"Base Dissipation: {turnEndProcessor.BaseDissipationRate}");
            GUILayout.Label($"Radiator Bonus: +{radiatorBonus:F1}");
            GUILayout.Label($"Total Dissipation: {dissipation:F1}/turn");
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>HEAT CONTROLS</b>");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+25")) AddHeat(25f);
        if (GUILayout.Button("+50")) AddHeat(50f);
        if (GUILayout.Button("+100")) AddHeat(100f);
        if (GUILayout.Button("Reset")) ResetHeat();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("<b>COOLDOWN CONTROLS</b>");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set All CD=3")) SetAllWeaponCooldowns(3);
        if (GUILayout.Button("Reset All CD")) ResetAllCooldowns();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("<b>RADIATOR STATUS</b>");

        var degradation = playerShip?.GetComponent<SystemDegradationManager>();
        if (degradation != null)
        {
            var radiators = degradation.GetRadiators();
            foreach (var radiator in radiators)
            {
                GUILayout.BeginHorizontal();
                string state = radiator.IsDestroyed ? "X" : radiator.IsDamaged ? "DMG" : "OK";
                GUILayout.Label($"{radiator.gameObject.name}: {state}", GUILayout.Width(150));
                if (!radiator.IsDestroyed)
                {
                    if (GUILayout.Button("Hit", GUILayout.Width(40)))
                    {
                        radiator.TakeCriticalHit();
                        degradation.RecalculateAllMultipliers();
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Process Turn End Only"))
        {
            turnEndProcessor?.ProcessTurnEnd(turnManager?.CurrentTurn ?? 0);
            LogTurnEvent("Manual turn end processed");
        }
    }

    void DrawFiringQueueTab()
    {
        GUILayout.Label("<b>WEAPON FIRING QUEUE (Phase 3.5.2)</b>");

        if (firingQueue != null)
        {
            GUILayout.Label($"Queued Commands: {firingQueue.QueuedCount}");
            GUILayout.Label($"Is Executing: {firingQueue.IsExecuting}");
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>QUEUE WEAPONS</b>");

        WeaponSystem[] weapons = playerShip?.GetComponentsInChildren<WeaponSystem>();
        Ship target = (enemyShips.Count > 0) ? enemyShips[0] : null;

        if (weapons != null && target != null)
        {
            foreach (var weapon in weapons)
            {
                GUILayout.BeginHorizontal();
                string status = weapon.CanFireSilent() ? "Ready" : "Busy";
                GUILayout.Label($"{weapon.WeaponName}: {status}", GUILayout.Width(180));

                if (GUILayout.Button("Queue", GUILayout.Width(60)))
                {
                    if (weapon.OwnerShip == null) weapon.Initialize(playerShip);
                    if (weapon.AssignedTarget == null) weapon.SetTarget(target);

                    firingQueue?.QueueFire(weapon, target);
                    LogTurnEvent($"Queued {weapon.WeaponName}");
                }
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>QUEUE CONTROLS</b>");

        if (GUILayout.Button("Queue All Ready Weapons"))
        {
            if (weapons != null && target != null)
            {
                foreach (var weapon in weapons)
                {
                    if (weapon.CanFireSilent())
                    {
                        if (weapon.OwnerShip == null) weapon.Initialize(playerShip);
                        if (weapon.AssignedTarget == null) weapon.SetTarget(target);
                        firingQueue?.QueueFire(weapon, target);
                    }
                }
                LogTurnEvent("Queued all ready weapons");
            }
        }

        if (GUILayout.Button("Clear Queue"))
        {
            firingQueue?.ClearQueue();
            LogTurnEvent("Queue cleared");
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Execute Queue Now"))
        {
            if (firingQueue != null)
            {
                StartCoroutine(firingQueue.ExecuteQueue());
                LogTurnEvent("Executing queue...");
            }
        }
    }

    void DrawMovementTab()
    {
        GUILayout.Label("<b>MOVEMENT & ARCS (Phase 3.5.4)</b>");

        // Movement status
        GUILayout.Label("<b>PLAYER MOVEMENT</b>");

        if (playerShip != null)
        {
            bool hasMove = playerShip.HasPlannedMove;
            bool isExecuting = playerShip.IsExecutingMove;
            bool canMove = playerShip.CanMove();

            GUI.color = canMove ? Color.green : Color.red;
            GUILayout.Label($"Can Move: {canMove}");
            GUI.color = Color.white;

            GUILayout.Label($"Has Planned Move: {hasMove}");
            GUILayout.Label($"Is Executing: {isExecuting}");

            if (isExecuting)
            {
                float progress = playerShip.GetMoveProgress();
                GUILayout.Label($"Progress: {progress * 100f:F0}%");
            }

            GUILayout.Space(5);
            GUILayout.Label($"Max Move Distance: {playerShip.GetEffectiveMaxMoveDistance():F1}");
            GUILayout.Label($"Max Turn Angle: {playerShip.GetEffectiveMaxTurnAngle():F1}°");
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>MOVEMENT CONTROLS</b>");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Plan +10 Forward"))
        {
            if (playerShip != null)
            {
                Vector3 target = playerShip.transform.position + playerShip.transform.forward * 10f;
                playerShip.PlanMove(target, Quaternion.identity);
                LogTurnEvent("Planned forward move");
            }
        }
        if (GUILayout.Button("Plan Turn Right"))
        {
            if (playerShip != null)
            {
                Vector3 target = playerShip.transform.position + (playerShip.transform.forward + playerShip.transform.right).normalized * 10f;
                playerShip.PlanMove(target, Quaternion.identity);
                LogTurnEvent("Planned right turn");
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Execute Move"))
        {
            if (playerShip != null && playerShip.HasPlannedMove)
            {
                playerShip.ExecuteMove();
                LogTurnEvent("Executing move");
            }
        }
        if (GUILayout.Button("Reset Move"))
        {
            playerShip?.ResetPlannedMove();
            LogTurnEvent("Move reset");
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("<b>MOVEMENT EXECUTOR</b>");

        if (movementExecutor != null)
        {
            GUILayout.Label($"Is Executing: {movementExecutor.IsExecuting}");
            GUILayout.Label($"Progress: {movementExecutor.ExecutionProgress * 100f:F0}%");

            if (GUILayout.Button("Execute All Planned Movements"))
            {
                movementExecutor.ExecuteAllPlannedMovements();
                LogTurnEvent("Executing all movements");
            }
        }
        else
        {
            GUI.color = Color.yellow;
            GUILayout.Label("MovementExecutor not found");
            GUI.color = Color.white;
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>WEAPON ARC VALIDATION</b>");

        if (arcValidator != null && playerShip != null)
        {
            Ship target = (enemyShips.Count > 0) ? enemyShips[0] : null;

            if (target != null)
            {
                WeaponSystem[] weapons = playerShip.GetComponentsInChildren<WeaponSystem>();
                if (weapons.Length > 0)
                {
                    foreach (var weapon in weapons)
                    {
                        if (weapon.OwnerShip == null) weapon.Initialize(playerShip);

                        bool inArcNow = arcValidator.IsInArcNow(weapon, target);
                        var result = arcValidator.ValidateArc(weapon, target);

                        GUILayout.BeginHorizontal();
                        GUI.color = inArcNow ? Color.green : Color.red;
                        string arcStatus = inArcNow ? "IN ARC" : "OUT";
                        GUILayout.Label($"{weapon.WeaponName}: {arcStatus}", GUILayout.Width(180));
                        GUI.color = Color.white;

                        if (result.WillBeInArc && !inArcNow)
                        {
                            GUI.color = Color.yellow;
                            GUILayout.Label($"@t={result.OptimalFiringTime:F2}", GUILayout.Width(80));
                            GUI.color = Color.white;
                        }
                        GUILayout.EndHorizontal();
                    }

                    if (GUILayout.Button("Clear Arc Cache"))
                    {
                        arcValidator.ClearCache();
                        LogTurnEvent("Arc cache cleared");
                    }
                }
            }
            else
            {
                GUILayout.Label("No target selected");
            }
        }
        else
        {
            if (arcValidator == null)
            {
                GUI.color = Color.yellow;
                GUILayout.Label("WeaponArcValidator not found");
                GUI.color = Color.white;
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>POSITION QUERIES</b>");

        if (playerShip != null && playerShip.HasPlannedMove)
        {
            GUILayout.Label("Position at time:");
            GUILayout.BeginHorizontal();
            for (float t = 0f; t <= 1f; t += 0.25f)
            {
                Vector3 pos = playerShip.GetPositionAtTime(t);
                GUILayout.Label($"t={t:F1}: {pos.x:F1},{pos.z:F1}", GUILayout.Width(90));
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("Plan a move to see position queries");
        }
    }

    #endregion

    #region Helper Methods

    void AddHeat(float amount)
    {
        if (playerHeatManager != null)
        {
            playerHeatManager.AddPlannedHeat(amount);
            playerHeatManager.CommitPlannedHeat();
            LogTurnEvent($"+{amount:F0} heat -> {playerHeatManager.CurrentHeat:F0}");
        }
    }

    void ResetHeat()
    {
        if (playerHeatManager != null)
        {
            playerHeatManager.Reset();
            LogTurnEvent("Heat reset");
        }
    }

    void SetAllWeaponCooldowns(int cooldown)
    {
        if (playerWeaponManager == null) return;

        var field = typeof(WeaponSystem).GetField("currentCooldown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        foreach (var weapon in playerWeaponManager.Weapons)
        {
            field?.SetValue(weapon, cooldown);
        }
        LogTurnEvent($"All CD={cooldown}");
    }

    void ResetAllCooldowns()
    {
        SetAllWeaponCooldowns(0);
    }

    void ReloadAllAmmo()
    {
        if (playerWeaponManager == null) return;

        var field = typeof(WeaponSystem).GetField("currentAmmo",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        foreach (var weapon in playerWeaponManager.Weapons)
        {
            if (weapon.AmmoCapacity > 0)
            {
                field?.SetValue(weapon, weapon.AmmoCapacity);
            }
        }
        LogTurnEvent("Ammo reloaded");
    }

    void ApplyDamageToTarget(float damage, SectionType section)
    {
        if (targetDamageRouter == null) return;
        DamageReport report = targetDamageRouter.ProcessDamage(damage, section);
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
        targetShields?.RestoreShields(targetShields.MaxShields);
    }

    void ApplyDirectSectionDamage(SectionType type, float damage)
    {
        if (targetSections == null) return;
        ShipSection section = targetSections.GetSection(type);
        section?.ApplyDamage(damage);
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
        foreach (var type in outer) BreachSection(type);
    }

    void AttackCore()
    {
        if (targetDamageRouter == null) return;
        Vector3 attackDir = directionPresets[currentDirectionIndex].dir;
        targetDamageRouter.ProcessDamage(testDamage, SectionType.Core, attackDir);
    }

    void DestroyTargetReactor()
    {
        MountedReactor reactor = targetShip?.GetComponentInChildren<MountedReactor>();
        if (reactor != null)
        {
            while (reactor.CurrentState != SystemState.Destroyed)
                reactor.TakeCriticalHit();
            targetDeathController?.CheckDeathConditions();
        }
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
            ProjectileManager.SpawnHomingProjectile(spawnInfo, 90f);
        else
            ProjectileManager.SpawnBallisticProjectile(spawnInfo);
    }

    void ResetTargetShip()
    {
        targetSections?.ResetAllSections();
        targetShields?.RestoreShields(targetShields.MaxShields);
        foreach (var system in targetSystems) system?.Repair();
    }

    void ResetAllShips()
    {
        Ship original = targetShip;

        targetShip = playerShip;
        CacheTargetComponents();
        ResetTargetShip();

        foreach (var enemy in enemyShips)
        {
            targetShip = enemy;
            CacheTargetComponents();
            ResetTargetShip();
        }

        targetShip = original;
        CacheTargetComponents();
        combatLog?.Clear();
    }

    #endregion
}
