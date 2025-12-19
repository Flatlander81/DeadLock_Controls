using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime controller for full combat integration testing.
/// Allows testing complete damage flow including projectiles, shields, sections, criticals, and ship death.
/// </summary>
public class FullCombatTestController : MonoBehaviour
{
    [Header("Ship References")]
    public Ship playerShip;
    public Ship enemyShip;

    [Header("System References")]
    public ProjectileManager projectileManager;
    public DamageUIManager damageUIManager;
    public TargetingController targetingController;

    [Header("Test Settings")]
    [SerializeField] private float testDamage = 50f;
    [SerializeField] private SectionType targetSection = SectionType.Fore;
    [SerializeField] private bool showUI = true;

    // Cached components
    private ShieldSystem playerShields;
    private ShieldSystem enemyShields;
    private SectionManager playerSections;
    private SectionManager enemySections;
    private DamageRouter playerDamageRouter;
    private DamageRouter enemyDamageRouter;
    private CombatLogPanel combatLog;

    private Vector2 scrollPosition;
    private Ship targetShip;

    void Start()
    {
        // Find ships if not assigned
        if (playerShip == null || enemyShip == null)
        {
            Ship[] ships = FindObjectsByType<Ship>(FindObjectsSortMode.None);
            foreach (var ship in ships)
            {
                if (playerShip == null)
                    playerShip = ship;
                else if (enemyShip == null)
                    enemyShip = ship;
            }
        }

        // Cache components
        if (playerShip != null)
        {
            playerShields = playerShip.ShieldSystem;
            playerSections = playerShip.SectionManager;
            playerDamageRouter = playerShip.DamageRouter;
        }

        if (enemyShip != null)
        {
            enemyShields = enemyShip.ShieldSystem;
            enemySections = enemyShip.SectionManager;
            enemyDamageRouter = enemyShip.DamageRouter;
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

        // Default to targeting enemy
        targetShip = enemyShip;

        Debug.Log("=== Full Combat Test Controller ===");
        Debug.Log("Press 'J' to toggle test UI");
        Debug.Log("Press 'K' to switch target between player/enemy");
    }

    void Update()
    {
        // Toggle UI
        if (Input.GetKeyDown(KeyCode.J))
        {
            showUI = !showUI;
        }

        // Switch target
        if (Input.GetKeyDown(KeyCode.K))
        {
            targetShip = (targetShip == enemyShip) ? playerShip : enemyShip;
            Debug.Log($"Target switched to: {targetShip?.gameObject.name}");
        }
    }

    void OnGUI()
    {
        if (!showUI) return;

        float panelWidth = 320f;
        float panelHeight = 550f;
        Rect panelRect = new Rect(Screen.width - panelWidth - 10, 10, panelWidth, panelHeight);

        GUI.Box(panelRect, "");

        GUILayout.BeginArea(new Rect(panelRect.x + 5, panelRect.y + 5, panelWidth - 10, panelHeight - 10));

        GUILayout.Label("<b>FULL COMBAT TEST CONTROLLER</b>");
        GUILayout.Space(5);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(panelHeight - 40));

        // Target selection
        GUILayout.Label("<b>Target Ship:</b>");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(targetShip == playerShip ? "[PLAYER]" : "Player"))
        {
            targetShip = playerShip;
        }
        if (GUILayout.Button(targetShip == enemyShip ? "[ENEMY]" : "Enemy"))
        {
            targetShip = enemyShip;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Section selection
        GUILayout.Label("<b>Target Section:</b>");
        GUILayout.BeginHorizontal();
        string[] sectionLabels = { "Fore", "Aft", "Port", "Stb", "Drs", "Vnt", "Core" };
        int selected = GUILayout.SelectionGrid((int)targetSection, sectionLabels, 4);
        if (selected < 7)
        {
            targetSection = (SectionType)selected;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Damage slider
        GUILayout.Label($"<b>Damage Amount: {testDamage:F0}</b>");
        testDamage = GUILayout.HorizontalSlider(testDamage, 10f, 200f);

        GUILayout.Space(10);

        // Shield damage tests
        GUILayout.Label("<b>Shield Tests:</b>");
        if (GUILayout.Button($"Hit {GetTargetName()} Shields ({testDamage:F0} dmg)"))
        {
            ApplyDamageToTarget(testDamage, targetSection);
        }

        if (GUILayout.Button($"Deplete {GetTargetName()} Shields"))
        {
            DepleteTargetShields();
        }

        if (GUILayout.Button($"Restore {GetTargetName()} Shields"))
        {
            RestoreTargetShields();
        }

        GUILayout.Space(10);

        // Section damage tests
        GUILayout.Label("<b>Section Damage Tests:</b>");
        if (GUILayout.Button($"Damage {targetSection} Armor ({testDamage:F0})"))
        {
            ApplyDirectSectionDamage(targetSection, testDamage);
        }

        if (GUILayout.Button($"Breach {targetSection}"))
        {
            BreachSection(targetSection);
        }

        if (GUILayout.Button("Breach All Outer Sections"))
        {
            BreachAllOuterSections();
        }

        GUILayout.Space(10);

        // Critical hit tests
        GUILayout.Label("<b>Critical Hit Tests:</b>");
        if (GUILayout.Button("Trigger Critical on Target Section"))
        {
            TriggerCriticalHit(targetSection);
        }

        if (GUILayout.Button("Damage Engine (Movement Penalty)"))
        {
            DamageTargetEngine();
        }

        if (GUILayout.Button("Damage Weapon (Cooldown Penalty)"))
        {
            DamageTargetWeapon();
        }

        GUILayout.Space(10);

        // Ship death tests
        GUILayout.Label("<b>Ship Death Tests:</b>");
        if (GUILayout.Button("Breach Core (Ship Destroyed)"))
        {
            BreachCoreSection();
        }

        if (GUILayout.Button("Destroy Reactor (Ship Destroyed)"))
        {
            DestroyTargetReactor();
        }

        if (GUILayout.Button("Make Combat Ineffective"))
        {
            MakeCombatIneffective();
        }

        GUILayout.Space(10);

        // Projectile tests
        GUILayout.Label("<b>Projectile Tests:</b>");
        if (GUILayout.Button("Fire Projectile at Target"))
        {
            FireProjectileAtTarget();
        }

        if (GUILayout.Button("Fire Missile at Target"))
        {
            FireMissileAtTarget();
        }

        GUILayout.Space(10);

        // Combat log tests
        GUILayout.Label("<b>Combat Log:</b>");
        if (GUILayout.Button("Log Test Hit"))
        {
            LogTestHit();
        }

        if (GUILayout.Button("Clear Combat Log"))
        {
            ClearCombatLog();
        }

        GUILayout.Space(10);

        // Reset buttons
        GUILayout.Label("<b>Reset:</b>");
        if (GUILayout.Button("Reset Target Ship"))
        {
            ResetTargetShip();
        }

        if (GUILayout.Button("Reset All Ships"))
        {
            ResetAllShips();
        }

        GUILayout.Space(10);

        // Status display
        DrawStatusDisplay();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private string GetTargetName()
    {
        return targetShip == playerShip ? "Player" : "Enemy";
    }

    private void ApplyDamageToTarget(float damage, SectionType section)
    {
        DamageRouter router = targetShip == playerShip ? playerDamageRouter : enemyDamageRouter;
        if (router == null)
        {
            Debug.LogError("No DamageRouter found for target!");
            return;
        }

        DamageReport report = router.ProcessDamage(damage, section);
        Debug.Log($"[Combat] {GetTargetName()} hit: {report}");

        if (damageUIManager != null)
        {
            damageUIManager.ProcessDamageReport(report, targetShip?.gameObject.name);
        }
    }

    private void DepleteTargetShields()
    {
        ShieldSystem shields = targetShip == playerShip ? playerShields : enemyShields;
        if (shields != null)
        {
            float damage = shields.CurrentShields + 10f;
            ApplyDamageToTarget(damage, SectionType.Fore);
        }
    }

    private void RestoreTargetShields()
    {
        ShieldSystem shields = targetShip == playerShip ? playerShields : enemyShields;
        if (shields != null)
        {
            shields.RestoreShields(shields.MaxShields);
            Debug.Log($"[Combat] {GetTargetName()} shields restored to {shields.CurrentShields}");
        }
    }

    private void ApplyDirectSectionDamage(SectionType type, float damage)
    {
        SectionManager sections = targetShip == playerShip ? playerSections : enemySections;
        if (sections == null)
        {
            Debug.LogError("No SectionManager found for target!");
            return;
        }

        ShipSection section = sections.GetSection(type);
        if (section == null)
        {
            Debug.LogError($"Section {type} not found!");
            return;
        }

        // Bypass shields for direct section damage
        DamageResult result = section.ApplyDamage(damage);
        Debug.Log($"[Combat] Direct damage to {GetTargetName()} {type}: {result}");

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

    private void BreachSection(SectionType type)
    {
        SectionManager sections = targetShip == playerShip ? playerSections : enemySections;
        if (sections == null) return;

        ShipSection section = sections.GetSection(type);
        if (section == null) return;

        if (!section.IsBreached)
        {
            float damageNeeded = section.CurrentArmor + section.CurrentStructure + 50f;
            ApplyDirectSectionDamage(type, damageNeeded);
        }
        else
        {
            Debug.Log($"[Combat] {type} is already breached.");
        }
    }

    private void BreachAllOuterSections()
    {
        SectionType[] outerSections = { SectionType.Fore, SectionType.Aft, SectionType.Port, SectionType.Starboard, SectionType.Dorsal, SectionType.Ventral };

        foreach (var sectionType in outerSections)
        {
            BreachSection(sectionType);
        }
    }

    private void BreachCoreSection()
    {
        // First breach an adjacent section to expose core
        BreachSection(SectionType.Fore);

        // Then breach core
        BreachSection(SectionType.Core);
    }

    private void TriggerCriticalHit(SectionType section)
    {
        SectionManager sections = targetShip == playerShip ? playerSections : enemySections;
        if (sections == null) return;

        ShipSection targetSec = sections.GetSection(section);
        if (targetSec == null) return;

        // Apply damage to breach armor and hit structure
        float armorDamage = targetSec.CurrentArmor + 30f;
        ApplyDirectSectionDamage(section, armorDamage);
    }

    private void DamageTargetEngine()
    {
        MountedEngine engine = targetShip?.GetComponentInChildren<MountedEngine>();
        if (engine != null)
        {
            engine.TakeCriticalHit();
            Debug.Log($"[Combat] {GetTargetName()} engine damaged! State: {engine.CurrentState}");

            if (combatLog != null)
            {
                combatLog.LogSystemDamage(targetShip?.gameObject.name, engine.SystemType, engine.CurrentState);
            }
        }
        else
        {
            Debug.LogError("No engine found on target!");
        }
    }

    private void DamageTargetWeapon()
    {
        MountedWeapon weapon = targetShip?.GetComponentInChildren<MountedWeapon>();
        if (weapon != null)
        {
            weapon.TakeCriticalHit();
            Debug.Log($"[Combat] {GetTargetName()} weapon damaged! State: {weapon.CurrentState}");

            if (combatLog != null)
            {
                combatLog.LogSystemDamage(targetShip?.gameObject.name, weapon.SystemType, weapon.CurrentState);
            }
        }
        else
        {
            Debug.LogError("No weapon found on target!");
        }
    }

    private void DestroyTargetReactor()
    {
        MountedReactor reactor = targetShip?.GetComponentInChildren<MountedReactor>();
        if (reactor != null)
        {
            reactor.TakeCriticalHit();
            reactor.TakeCriticalHit();
            Debug.Log($"[Combat] {GetTargetName()} reactor destroyed!");

            // Trigger death check
            ShipDeathController deathController = targetShip?.GetComponent<ShipDeathController>();
            if (deathController != null)
            {
                deathController.CheckDeathConditions();
            }
        }
        else
        {
            Debug.LogError("No reactor found on target!");
        }
    }

    private void MakeCombatIneffective()
    {
        // Destroy all weapons and engines
        MountedEngine[] engines = targetShip?.GetComponentsInChildren<MountedEngine>();
        MountedWeapon[] weapons = targetShip?.GetComponentsInChildren<MountedWeapon>();

        if (engines != null)
        {
            foreach (var engine in engines)
            {
                while (engine.CurrentState != SystemState.Destroyed)
                {
                    engine.TakeCriticalHit();
                }
            }
        }

        if (weapons != null)
        {
            foreach (var weapon in weapons)
            {
                while (weapon.CurrentState != SystemState.Destroyed)
                {
                    weapon.TakeCriticalHit();
                }
            }
        }

        // Trigger death check
        ShipDeathController deathController = targetShip?.GetComponent<ShipDeathController>();
        if (deathController != null)
        {
            deathController.CheckDeathConditions();
        }

        Debug.Log($"[Combat] {GetTargetName()} made combat ineffective!");
    }

    private void FireProjectileAtTarget()
    {
        Ship shooter = targetShip == enemyShip ? playerShip : enemyShip;
        if (shooter == null || targetShip == null)
        {
            Debug.LogError("Need both shooter and target ships!");
            return;
        }

        // Create projectile spawn info
        Vector3 spawnPos = shooter.transform.position + shooter.transform.forward * 2f;
        Vector3 targetPos = targetShip.transform.position;

        var spawnInfo = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
            SpawnPosition = spawnPos,
            SpawnRotation = Quaternion.LookRotation(targetPos - spawnPos),
            TargetPosition = targetPos,
            TargetShip = targetShip,
            Damage = testDamage,
            Speed = 50f,
            OwnerShip = shooter
        };

        ProjectileManager.SpawnBallisticProjectile(spawnInfo);
        Debug.Log($"[Combat] Fired projectile from {shooter.gameObject.name} at {targetShip.gameObject.name}");
    }

    private void FireMissileAtTarget()
    {
        Ship shooter = targetShip == enemyShip ? playerShip : enemyShip;
        if (shooter == null || targetShip == null)
        {
            Debug.LogError("Need both shooter and target ships!");
            return;
        }

        Vector3 spawnPos = shooter.transform.position + shooter.transform.forward * 2f;
        Vector3 targetPos = targetShip.transform.position;

        var spawnInfo = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Homing,
            SpawnPosition = spawnPos,
            SpawnRotation = Quaternion.LookRotation(targetPos - spawnPos),
            TargetPosition = targetPos,
            TargetShip = targetShip,
            Damage = testDamage * 1.5f,
            Speed = 30f,
            OwnerShip = shooter
        };

        ProjectileManager.SpawnHomingProjectile(spawnInfo, 90f);
        Debug.Log($"[Combat] Fired missile from {shooter.gameObject.name} at {targetShip.gameObject.name}");
    }

    private void LogTestHit()
    {
        if (combatLog != null)
        {
            combatLog.LogHit(targetShip?.gameObject.name ?? "Target", targetSection, testDamage, 20f, 15f, 15f);
        }
    }

    private void ClearCombatLog()
    {
        if (combatLog != null)
        {
            combatLog.Clear();
        }
    }

    private void ResetTargetShip()
    {
        SectionManager sections = targetShip == playerShip ? playerSections : enemySections;
        ShieldSystem shields = targetShip == playerShip ? playerShields : enemyShields;

        if (sections != null)
        {
            sections.ResetAllSections();
        }

        if (shields != null)
        {
            shields.RestoreShields(shields.MaxShields);
        }

        // Reset all mounted systems
        MountedSystem[] systems = targetShip?.GetComponentsInChildren<MountedSystem>();
        if (systems != null)
        {
            foreach (var system in systems)
            {
                system.Repair();
            }
        }

        Debug.Log($"[Combat] {GetTargetName()} ship reset.");
    }

    private void ResetAllShips()
    {
        Ship original = targetShip;

        targetShip = playerShip;
        ResetTargetShip();

        targetShip = enemyShip;
        ResetTargetShip();

        targetShip = original;

        ClearCombatLog();
        Debug.Log("[Combat] All ships reset.");
    }

    private void DrawStatusDisplay()
    {
        GUILayout.Label("<b>Ship Status:</b>");

        // Player status
        if (playerShip != null)
        {
            GUILayout.Label($"<color=cyan>Player Ship:</color>");
            GUILayout.Label($"  Shields: {playerShields?.CurrentShields:F0}/{playerShields?.MaxShields:F0}");
            GUILayout.Label($"  Breached: {playerSections?.GetBreachedSections().Count ?? 0}/7");
            GUILayout.Label($"  Destroyed: {playerShip.IsDestroyed}");
        }

        GUILayout.Space(5);

        // Enemy status
        if (enemyShip != null)
        {
            GUILayout.Label($"<color=red>Enemy Ship:</color>");
            GUILayout.Label($"  Shields: {enemyShields?.CurrentShields:F0}/{enemyShields?.MaxShields:F0}");
            GUILayout.Label($"  Breached: {enemySections?.GetBreachedSections().Count ?? 0}/7");
            GUILayout.Label($"  Destroyed: {enemyShip.IsDestroyed}");
        }
    }
}
