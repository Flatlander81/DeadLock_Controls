using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Phase 3 Integration Tests - Full damage system integration.
/// Tests complete damage flow from weapons through to ship destruction.
/// </summary>
public class Phase3IntegrationTests
{
    private GameObject testRoot;
    private Ship ship;
    private SectionManager sectionManager;
    private ShieldSystem shieldSystem;
    private DamageRouter damageRouter;
    private HeatManager heatManager;
    private CoreProtectionSystem coreProtection;
    private ShipDeathController deathController;
    private SystemDegradationManager degradationManager;
    private CriticalHitSystem criticalHitSystem;
    private DamageUIManager damageUIManager;
    private SectionStatusPanel sectionPanel;
    private CombatLogPanel combatLog;
    private ShieldStatusBar shieldBar;

    // Systems for integration testing
    private MountedEngine mainEngine;
    private MountedWeapon mainWeapon;
    private MountedReactor reactor;

    [SetUp]
    public void SetUp()
    {
        testRoot = new GameObject("TestRoot");

        // Create ship with all components
        GameObject shipObj = new GameObject("TestShip");
        shipObj.transform.SetParent(testRoot.transform);
        ship = shipObj.AddComponent<Ship>();
        heatManager = shipObj.AddComponent<HeatManager>();
        sectionManager = shipObj.AddComponent<SectionManager>();
        shieldSystem = shipObj.AddComponent<ShieldSystem>();
        damageRouter = shipObj.AddComponent<DamageRouter>();
        coreProtection = shipObj.AddComponent<CoreProtectionSystem>();
        deathController = shipObj.AddComponent<ShipDeathController>();
        degradationManager = shipObj.AddComponent<SystemDegradationManager>();
        criticalHitSystem = shipObj.AddComponent<CriticalHitSystem>();

        // Initialize shield system
        shieldSystem.Initialize(200f);

        // Create all 7 sections
        CreateSection(SectionType.Core, 75f, 150f);
        CreateSection(SectionType.Fore, 50f, 100f);
        CreateSection(SectionType.Aft, 50f, 100f);
        CreateSection(SectionType.Port, 50f, 100f);
        CreateSection(SectionType.Starboard, 50f, 100f);
        CreateSection(SectionType.Dorsal, 40f, 80f);
        CreateSection(SectionType.Ventral, 40f, 80f);

        // Set up component references
        coreProtection.SetReferences(sectionManager, ship);
        damageRouter.SetReferences(shieldSystem, sectionManager, ship, coreProtection);
        deathController.SetReferences(ship, sectionManager, degradationManager);
        degradationManager.SetReferences(ship, heatManager, sectionManager);
        deathController.SubscribeToEvents();
        ship.SetDeathController(deathController);

        // Create mounted systems for integration testing
        CreateMountedSystems();

        // Create UI components
        CreateUIComponents();
    }

    [TearDown]
    public void TearDown()
    {
        if (testRoot != null)
        {
            Object.Destroy(testRoot);
        }
    }

    private ShipSection CreateSection(SectionType type, float armor, float structure)
    {
        GameObject sectionObj = new GameObject($"Section_{type}");
        sectionObj.transform.SetParent(testRoot.transform);
        ShipSection section = sectionObj.AddComponent<ShipSection>();
        section.Initialize(type, armor, structure, ship);
        sectionManager.RegisterSection(section);
        return section;
    }

    private void CreateMountedSystems()
    {
        // Create main engine in Aft section
        ShipSection aftSection = sectionManager.GetSection(SectionType.Aft);
        GameObject engineObj = new GameObject("MainEngine");
        engineObj.transform.SetParent(testRoot.transform);
        mainEngine = engineObj.AddComponent<MountedEngine>();
        mainEngine.Initialize(ShipSystemType.MainEngine, 1, aftSection, ship);

        if (aftSection.SlotLayout != null)
        {
            aftSection.SlotLayout.AddSystem(mainEngine);
        }
        degradationManager.RegisterSystem(mainEngine);

        // Create weapon in Fore section
        ShipSection foreSection = sectionManager.GetSection(SectionType.Fore);
        GameObject weaponObj = new GameObject("NewtonianCannon");
        weaponObj.transform.SetParent(testRoot.transform);
        mainWeapon = weaponObj.AddComponent<MountedWeapon>();
        mainWeapon.Initialize(ShipSystemType.NewtonianCannon, 1, foreSection, ship);

        if (foreSection.SlotLayout != null)
        {
            foreSection.SlotLayout.AddSystem(mainWeapon);
        }
        degradationManager.RegisterSystem(mainWeapon);

        // Create reactor in Core section
        ShipSection coreSection = sectionManager.GetSection(SectionType.Core);
        GameObject reactorObj = new GameObject("ReactorCore");
        reactorObj.transform.SetParent(testRoot.transform);
        reactor = reactorObj.AddComponent<MountedReactor>();
        reactor.Initialize(ShipSystemType.ReactorCore, 1, coreSection, ship);

        if (coreSection.SlotLayout != null)
        {
            coreSection.SlotLayout.AddSystem(reactor);
        }
        degradationManager.RegisterSystem(reactor);
    }

    private void CreateUIComponents()
    {
        GameObject uiRoot = new GameObject("UIRoot");
        uiRoot.transform.SetParent(testRoot.transform);

        GameObject damageUIObj = new GameObject("DamageUIManager");
        damageUIObj.transform.SetParent(uiRoot.transform);
        damageUIManager = damageUIObj.AddComponent<DamageUIManager>();

        GameObject sectionPanelObj = new GameObject("SectionStatusPanel");
        sectionPanelObj.transform.SetParent(damageUIObj.transform);
        sectionPanel = sectionPanelObj.AddComponent<SectionStatusPanel>();

        GameObject combatLogObj = new GameObject("CombatLogPanel");
        combatLogObj.transform.SetParent(damageUIObj.transform);
        combatLog = combatLogObj.AddComponent<CombatLogPanel>();

        GameObject shieldBarObj = new GameObject("ShieldStatusBar");
        shieldBarObj.transform.SetParent(damageUIObj.transform);
        shieldBar = shieldBarObj.AddComponent<ShieldStatusBar>();

        // Initialize UI
        sectionPanel.Initialize(ship);
        shieldBar.SetShieldSystem(shieldSystem);
    }

    // Test 1: Full damage flow from shields to section to system
    [Test]
    public void Test_FullDamageFlow_ShieldsToSectionToSystem()
    {
        // Arrange
        float initialShields = shieldSystem.CurrentShields;
        ShipSection foreSection = sectionManager.GetSection(SectionType.Fore);
        float initialArmor = foreSection.CurrentArmor;

        // Verify weapon is operational
        Assert.AreEqual(SystemState.Operational, mainWeapon.CurrentState, "Weapon should start operational");

        // Act - Phase 1: Damage shields
        DamageReport report1 = damageRouter.ProcessDamage(100f, SectionType.Fore);

        // Assert - Shields absorbed damage
        Assert.AreEqual(100f, shieldSystem.CurrentShields, "Shields should be at 100 after 100 damage");
        Assert.AreEqual(100f, report1.ShieldDamage, "Report should show 100 shield damage");
        Assert.AreEqual(0f, report1.ArmorDamage, "No armor damage while shields up");

        // Act - Phase 2: Deplete shields and hit armor
        DamageReport report2 = damageRouter.ProcessDamage(150f, SectionType.Fore);

        // Assert - Shields depleted, armor hit
        Assert.AreEqual(0f, shieldSystem.CurrentShields, "Shields should be depleted");
        Assert.IsTrue(report2.ShieldsDepleted, "Report should indicate shields depleted");
        Assert.Greater(report2.ArmorDamage, 0f, "Armor should take damage");

        // Act - Phase 3: Damage structure (may trigger critical on weapon)
        float armorRemaining = foreSection.CurrentArmor;
        float structureDamage = armorRemaining + 50f;
        DamageReport report3 = damageRouter.ProcessDamage(structureDamage, SectionType.Fore);

        // Assert - Structure was damaged
        Assert.Greater(report3.StructureDamage, 0f, "Structure should take damage");
    }

    // Test 2: Projectile hit routes correctly through damage system
    [Test]
    public void Test_ProjectileHitRoutesCorrectly()
    {
        // Arrange
        float projectileDamage = 75f;
        SectionType hitSection = SectionType.Port;

        // Act - Simulate projectile hit via damage router
        DamageReport report = damageRouter.ProcessDamage(projectileDamage, hitSection);

        // Assert
        Assert.AreEqual(projectileDamage, report.TotalIncomingDamage, "Total incoming damage should match projectile damage");
        Assert.AreEqual(hitSection, report.SectionHit, "Target section should be Port");
        Assert.AreEqual(75f, report.ShieldDamage, "All damage should go to shields while active");

        // Deplete shields and verify armor routing
        shieldSystem.SetShields(0f);
        DamageReport report2 = damageRouter.ProcessDamage(30f, hitSection);

        Assert.AreEqual(30f, report2.ArmorDamage, "Damage should route to armor when shields down");
    }

    // Test 3: Shield depletion enables shield boost ability
    [Test]
    public void Test_ShieldDepletionEnablesShieldBoost()
    {
        // Arrange
        Assert.IsFalse(shieldSystem.CanRestoreShields(), "Shield restore should not be available when shields active");

        // Act - Deplete shields
        DamageReport report = damageRouter.ProcessDamage(250f, SectionType.Fore);

        // Assert
        Assert.IsTrue(shieldSystem.CanRestoreShields(), "Shield restore should be available when depleted");
        Assert.IsTrue(report.ShieldsDepleted, "Report should indicate depletion");

        // Shield boost should be usable when depleted
        // Verify restoration works
        bool restored = shieldSystem.RestoreShields(100f);
        Assert.IsTrue(restored, "RestoreShields should succeed when shields at zero");
        Assert.AreEqual(100f, shieldSystem.CurrentShields, "Shield boost should restore shields");
        Assert.IsFalse(shieldSystem.CanRestoreShields(), "Shield restore should not be available after restoration");
    }

    // Test 4: Critical damage to weapon affects cooldown
    [Test]
    public void Test_CriticalDamagesWeaponAffectsCooldown()
    {
        // Arrange
        Assert.AreEqual(SystemState.Operational, mainWeapon.CurrentState, "Weapon should start operational");
        float baseCooldownMultiplier = mainWeapon.GetCooldownMultiplier();
        Assert.AreEqual(1f, baseCooldownMultiplier, "Operational weapon should have 1x cooldown multiplier");

        // Act - Damage the weapon
        mainWeapon.TakeCriticalHit();

        // Assert
        Assert.AreEqual(SystemState.Damaged, mainWeapon.CurrentState, "Weapon should be damaged");

        // Damaged weapons may have increased cooldown (50% chance cooldown penalty)
        float damagedCooldownMultiplier = mainWeapon.GetCooldownMultiplier();
        // Either has cooldown penalty (2x) or damage penalty (1x cooldown)
        Assert.GreaterOrEqual(damagedCooldownMultiplier, 1f, "Damaged weapon should have same or higher cooldown multiplier");
    }

    // Test 5: Engine damage affects movement
    [Test]
    public void Test_EngineDamageAffectsMovement()
    {
        // Arrange
        Assert.AreEqual(SystemState.Operational, mainEngine.CurrentState, "Engine should start operational");
        float baseSpeedMultiplier = mainEngine.GetSpeedMultiplier();
        Assert.AreEqual(1f, baseSpeedMultiplier, "Operational engine should have 1x speed multiplier");

        // Act - Damage the engine
        mainEngine.TakeCriticalHit();

        // Assert
        Assert.AreEqual(SystemState.Damaged, mainEngine.CurrentState, "Engine should be damaged");

        // Damaged engine has reduced speed (0.5x multiplier)
        float damagedSpeedMultiplier = mainEngine.GetSpeedMultiplier();
        Assert.Less(damagedSpeedMultiplier, baseSpeedMultiplier, "Damaged engine should have reduced speed multiplier");
        Assert.AreEqual(0.5f, damagedSpeedMultiplier, "Damaged engine should have 0.5x speed multiplier");

        // Destroy engine
        mainEngine.TakeCriticalHit();
        Assert.AreEqual(SystemState.Destroyed, mainEngine.CurrentState, "Engine should be destroyed");
        Assert.AreEqual(0f, mainEngine.GetSpeedMultiplier(), "Destroyed engine should have zero speed multiplier");
    }

    // Test 6: Core access after adjacent section breach
    [Test]
    public void Test_CoreAccessAfterBreach()
    {
        // Arrange
        Assert.IsFalse(coreProtection.IsCoreExposed(), "Core should not be exposed initially");

        // Deplete shields first
        shieldSystem.SetShields(0f);

        // Act - Breach Fore section
        ShipSection foreSection = sectionManager.GetSection(SectionType.Fore);
        float damageToBreachFore = foreSection.CurrentArmor + foreSection.CurrentStructure + 10f;
        foreSection.ApplyDamage(damageToBreachFore);

        // Assert
        Assert.IsTrue(foreSection.IsBreached, "Fore section should be breached");
        Assert.IsTrue(coreProtection.IsCoreExposed(), "Core should be exposed after Fore breach");

        // Check exposed angles
        var exposedAngles = coreProtection.GetExposedAngles();
        Assert.Contains(SectionType.Fore, exposedAngles, "Fore should be in exposed angles");
    }

    // Test 7: Ship death on reactor destruction
    [Test]
    public void Test_ShipDeathOnReactorDestruction()
    {
        // Arrange
        Assert.IsFalse(ship.IsDestroyed, "Ship should not start destroyed");
        Assert.AreEqual(SystemState.Operational, reactor.CurrentState, "Reactor should start operational");

        // Expect the error logs from reactor destruction
        LogAssert.Expect(LogType.Error, "[MountedReactor] CORE BREACH! Reactor destroyed - Ship lost!");
        LogAssert.Expect(LogType.Error, "[SystemDegradationManager] CORE BREACH! Ship destroyed!");
        LogAssert.Expect(LogType.Error, "[ShipDeathController] SHIP DESTROYED! Cause: CoreBreach");
        LogAssert.Expect(LogType.Error, "TestShip has been destroyed!");

        // Act - Destroy reactor
        reactor.TakeCriticalHit(); // Damaged
        reactor.TakeCriticalHit(); // Destroyed

        // Trigger death check
        deathController.CheckDeathConditions();

        // Assert
        Assert.AreEqual(SystemState.Destroyed, reactor.CurrentState, "Reactor should be destroyed");
        Assert.IsTrue(ship.IsDestroyed, "Ship should be destroyed when reactor is destroyed");
        Assert.AreEqual(ShipDeathController.DeathCause.CoreBreach, deathController.Cause,
            "Death cause should be CoreBreach (reactor destroyed)");
    }

    // Test 8: Combat ineffective condition
    [Test]
    public void Test_CombatIneffectiveCondition()
    {
        // Arrange
        Assert.IsFalse(ship.IsDestroyed, "Ship should not start destroyed");
        Assert.IsFalse(deathController.IsDisabled, "Ship should not start disabled");

        // Act - Destroy both weapon and engine
        mainWeapon.TakeCriticalHit();
        mainWeapon.TakeCriticalHit();
        mainEngine.TakeCriticalHit();
        mainEngine.TakeCriticalHit();

        // Check death conditions
        deathController.CheckDeathConditions();

        // Assert
        Assert.AreEqual(SystemState.Destroyed, mainWeapon.CurrentState, "Weapon should be destroyed");
        Assert.AreEqual(SystemState.Destroyed, mainEngine.CurrentState, "Engine should be destroyed");
        // Ship becomes DISABLED (combat ineffective) but not destroyed
        Assert.IsTrue(deathController.IsDisabled, "Ship should be disabled when combat ineffective");
    }

    // Test 9: UI reflects damage state
    [Test]
    public void Test_UIReflectsDamageState()
    {
        // Arrange
        ShipSection foreSection = sectionManager.GetSection(SectionType.Fore);

        // Verify initial healthy color
        Color healthyColor = SectionStatusPanel.GetSectionColor(foreSection);
        Assert.AreEqual(Color.green, healthyColor, "Healthy section should be green");

        // Shield bar should show full
        Assert.AreEqual(200f, shieldBar.GetCurrentShields(), "Shield bar should show full shields");
        Assert.IsFalse(shieldBar.IsDepleted, "Shield bar should not show depleted");

        // Act - Damage the section
        foreSection.ApplyDamage(30f); // Partial armor damage

        // Assert - Color changes
        Color damagedColor = SectionStatusPanel.GetSectionColor(foreSection);
        Assert.AreEqual(Color.yellow, damagedColor, "Damaged section should be yellow");

        // Act - Deplete shields
        shieldSystem.SetShields(0f);

        // Assert - Shield bar updates
        Assert.AreEqual(0f, shieldBar.GetCurrentShields(), "Shield bar should show zero");
        Assert.IsTrue(shieldBar.IsDepleted, "Shield bar should show depleted");
    }

    // Test 10: Multiple hits to same section accumulate correctly
    [Test]
    public void Test_MultipleHitsSameSection()
    {
        // Arrange
        ShipSection portSection = sectionManager.GetSection(SectionType.Port);
        float initialArmor = portSection.CurrentArmor;
        float initialStructure = portSection.CurrentStructure;

        // Deplete shields first
        shieldSystem.SetShields(0f);

        // Act - Apply multiple small hits
        float hitDamage = 15f;
        int hitCount = 5;
        float totalExpectedArmorDamage = hitDamage * hitCount; // 75 damage total (exceeds 50 armor, so some goes to structure)

        for (int i = 0; i < hitCount; i++)
        {
            damageRouter.ProcessDamage(hitDamage, SectionType.Port);
        }

        // Assert - Damage accumulated correctly
        // Port section has 50 armor, 100 structure
        // 75 damage = 50 to armor + 25 to structure
        Assert.AreEqual(0f, portSection.CurrentArmor, "All armor should be depleted after 75 damage");
        Assert.AreEqual(75f, portSection.CurrentStructure, 0.01f, "Structure should have 25 damage (100-25=75)");

        // Continue hitting to breach
        while (!portSection.IsBreached)
        {
            damageRouter.ProcessDamage(25f, SectionType.Port);
        }

        // Assert - Section was breached through accumulated damage
        Assert.IsTrue(portSection.IsBreached, "Section should be breached after accumulated damage");
        Assert.AreEqual(0f, portSection.CurrentStructure, "Structure should be zero when breached");
    }
}
