using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for the Core Protection & Ship Death System (Step 3.6).
/// Tests Core access rules, lucky shots, and ship death conditions.
/// </summary>
public class CoreProtectionTests
{
    private GameObject shipObject;
    private Ship ship;
    private SectionManager sectionManager;
    private CoreProtectionSystem coreProtection;
    private DamageRouter damageRouter;
    private ShieldSystem shieldSystem;
    private ShipDeathController deathController;
    private SystemDegradationManager degradationManager;
    private HeatManager heatManager;

    [SetUp]
    public void SetUp()
    {
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();
        heatManager = shipObject.AddComponent<HeatManager>();
        sectionManager = shipObject.AddComponent<SectionManager>();
        coreProtection = shipObject.AddComponent<CoreProtectionSystem>();
        damageRouter = shipObject.AddComponent<DamageRouter>();
        shieldSystem = shipObject.AddComponent<ShieldSystem>();
        deathController = shipObject.AddComponent<ShipDeathController>();
        degradationManager = shipObject.AddComponent<SystemDegradationManager>();

        // Initialize shield system with 0 shields for direct section damage
        shieldSystem.Initialize(0f);

        // Set up references
        coreProtection.SetReferences(sectionManager, ship);
        damageRouter.SetReferences(shieldSystem, sectionManager, ship, coreProtection);
        deathController.SetReferences(ship, sectionManager, degradationManager);
        degradationManager.SetReferences(ship, heatManager, sectionManager);

        // Create all sections
        CreateSection(SectionType.Core);
        CreateSection(SectionType.Fore);
        CreateSection(SectionType.Aft);
        CreateSection(SectionType.Port);
        CreateSection(SectionType.Starboard);
        CreateSection(SectionType.Dorsal);
        CreateSection(SectionType.Ventral);

        // Subscribe death controller to section events (since Start() doesn't run in tests)
        deathController.SubscribeToEvents();

        // Set the death controller reference on ship (since Start() doesn't run in tests)
        ship.SetDeathController(deathController);
    }

    [TearDown]
    public void TearDown()
    {
        if (shipObject != null)
        {
            Object.Destroy(shipObject);
        }
    }

    private ShipSection CreateSection(SectionType type)
    {
        GameObject sectionObj = new GameObject($"Section_{type}");
        sectionObj.transform.SetParent(shipObject.transform);
        ShipSection section = sectionObj.AddComponent<ShipSection>();
        section.Initialize(type, 50f, 100f, ship); // 50 armor, 100 structure
        sectionManager.RegisterSection(section);
        return section;
    }

    private MountedEngine CreateEngine()
    {
        ShipSection coreSection = sectionManager.GetSection(SectionType.Core);
        GameObject engineObj = new GameObject("TestEngine");
        engineObj.transform.SetParent(shipObject.transform);
        MountedEngine engine = engineObj.AddComponent<MountedEngine>();
        engine.Initialize(ShipSystemType.MainEngine, 1, coreSection, ship);
        degradationManager.RegisterSystem(engine);
        return engine;
    }

    private MountedWeapon CreateWeapon()
    {
        ShipSection portSection = sectionManager.GetSection(SectionType.Port);
        GameObject weaponObj = new GameObject("TestWeapon");
        weaponObj.transform.SetParent(shipObject.transform);
        MountedWeapon weapon = weaponObj.AddComponent<MountedWeapon>();
        weapon.Initialize(ShipSystemType.NewtonianCannon, 1, portSection, ship);
        degradationManager.RegisterSystem(weapon);
        return weapon;
    }

    private MountedReactor CreateReactor()
    {
        ShipSection coreSection = sectionManager.GetSection(SectionType.Core);
        GameObject reactorObj = new GameObject("TestReactor");
        reactorObj.transform.SetParent(shipObject.transform);
        MountedReactor reactor = reactorObj.AddComponent<MountedReactor>();
        reactor.Initialize(ShipSystemType.ReactorCore, 1, coreSection, ship);
        reactor.SetLinkedReferences(ship, heatManager);
        degradationManager.RegisterSystem(reactor);
        return reactor;
    }

    // Test 1: Core cannot be hit when adjacent section is not breached
    [Test]
    public void CoreProtected_WhenAdjacentSectionNotBreached()
    {
        // Arrange - Fore section is intact
        ShipSection foreSection = sectionManager.GetSection(SectionType.Fore);
        Assert.IsFalse(foreSection.IsBreached, "Fore section should not be breached initially");

        // Act - Try to attack Core from forward direction
        bool canHitCore = coreProtection.CanHitCore(Vector3.forward);

        // Assert
        Assert.IsFalse(canHitCore, "Core should be protected when Fore section is intact");
    }

    // Test 2: Core can be hit when adjacent section IS breached
    [Test]
    public void CoreAccessible_WhenAdjacentSectionBreached()
    {
        // Arrange - Breach Fore section
        ShipSection foreSection = sectionManager.GetSection(SectionType.Fore);
        float damageToBreachFore = foreSection.CurrentArmor + foreSection.CurrentStructure + 10f;
        foreSection.ApplyDamage(damageToBreachFore);
        Assert.IsTrue(foreSection.IsBreached, "Fore section should be breached");

        // Act - Try to attack Core from forward direction
        bool canHitCore = coreProtection.CanHitCore(Vector3.forward);

        // Assert
        Assert.IsTrue(canHitCore, "Core should be accessible when Fore section is breached");
    }

    // Test 3: DamageRouter redirects Core damage when protected
    [Test]
    public void DamageRouter_RedirectsCoreDamage_WhenProtected()
    {
        // Arrange - Fore section is intact (protecting Core from forward)
        ShipSection foreSection = sectionManager.GetSection(SectionType.Fore);
        float foreArmorBefore = foreSection.CurrentArmor;
        Assert.IsFalse(foreSection.IsBreached);

        // Act - Attack Core from forward (should redirect to Fore)
        DamageReport report = damageRouter.ProcessDamage(30f, SectionType.Core, Vector3.forward);

        // Assert
        Assert.IsTrue(report.CoreWasProtected, "Core should have been protected");
        Assert.AreEqual(SectionType.Fore, report.SectionHit, "Damage should have hit Fore section");
        Assert.Less(foreSection.CurrentArmor, foreArmorBefore, "Fore section should have taken damage");
    }

    // Test 4: GetAdjacentSection returns correct section for each direction
    [Test]
    public void GetAdjacentSection_ReturnsCorrectSectionPerDirection()
    {
        // Attack traveling forward (+Z) hits Fore section
        Assert.AreEqual(SectionType.Fore, coreProtection.GetAdjacentSection(Vector3.forward));

        // Attack traveling backward (-Z) hits Aft section
        Assert.AreEqual(SectionType.Aft, coreProtection.GetAdjacentSection(Vector3.back));

        // Attack traveling left (-X) hits Port section
        Assert.AreEqual(SectionType.Port, coreProtection.GetAdjacentSection(Vector3.left));

        // Attack traveling right (+X) hits Starboard section
        Assert.AreEqual(SectionType.Starboard, coreProtection.GetAdjacentSection(Vector3.right));

        // Attack traveling up (+Y) hits Dorsal section
        Assert.AreEqual(SectionType.Dorsal, coreProtection.GetAdjacentSection(Vector3.up));

        // Attack traveling down (-Y) hits Ventral section
        Assert.AreEqual(SectionType.Ventral, coreProtection.GetAdjacentSection(Vector3.down));
    }

    // Test 5: IsCoreExposed returns true when any section is breached
    [Test]
    public void IsCoreExposed_ReturnsTrue_WhenAnySectionBreached()
    {
        // Arrange - Initially Core is not exposed
        Assert.IsFalse(coreProtection.IsCoreExposed(), "Core should not be exposed initially");

        // Act - Breach Port section
        ShipSection portSection = sectionManager.GetSection(SectionType.Port);
        portSection.ApplyDamage(portSection.CurrentArmor + portSection.CurrentStructure + 10f);

        // Assert
        Assert.IsTrue(coreProtection.IsCoreExposed(), "Core should be exposed after Port breach");
        var exposedAngles = coreProtection.GetExposedAngles();
        Assert.Contains(SectionType.Port, exposedAngles);
    }

    // Test 6: Ship is destroyed when reactor is destroyed (Core Breach)
    [Test]
    public void Ship_Destroyed_WhenReactorDestroyed()
    {
        // Arrange
        MountedReactor reactor = CreateReactor();

        // Subscribe death controller to reactor events (since Start() doesn't run in tests)
        deathController.SubscribeToReactor(reactor);

        Assert.IsFalse(deathController.IsDestroyed, "Ship should not be destroyed initially");

        bool destroyedEventFired = false;
        ShipDeathController.DeathCause reportedCause = ShipDeathController.DeathCause.None;
        deathController.OnShipDestroyed += (s, cause) =>
        {
            destroyedEventFired = true;
            reportedCause = cause;
        };

        // Expect the error logs from reactor destruction (multiple logs fired)
        LogAssert.ignoreFailingMessages = true;

        // Act - Destroy reactor
        reactor.TakeCriticalHit(); // Operational -> Damaged
        reactor.TakeCriticalHit(); // Damaged -> Destroyed

        // Assert
        Assert.IsTrue(reactor.IsDestroyed, "Reactor should be destroyed");
        Assert.IsTrue(destroyedEventFired, "Ship destroyed event should have fired");
        Assert.AreEqual(ShipDeathController.DeathCause.CoreBreach, reportedCause, "Death cause should be CoreBreach");
        Assert.IsTrue(deathController.IsDestroyed, "Ship should be destroyed");
    }

    // Test 7: Ship is disabled when combat ineffective (all weapons AND engines destroyed)
    [Test]
    public void Ship_Disabled_WhenCombatIneffective()
    {
        // Arrange
        MountedEngine engine = CreateEngine();
        MountedWeapon weapon = CreateWeapon();

        bool disabledEventFired = false;
        deathController.OnShipDisabled += (s) =>
        {
            disabledEventFired = true;
        };

        Assert.IsFalse(deathController.IsDisabled, "Ship should not be disabled initially");

        // Act - Destroy weapon only first
        weapon.TakeCriticalHit();
        weapon.TakeCriticalHit();
        deathController.CheckDeathConditions();
        Assert.IsFalse(disabledEventFired, "Ship should not be disabled with only weapons destroyed");

        // Now destroy engine as well
        engine.TakeCriticalHit();
        engine.TakeCriticalHit();
        deathController.CheckDeathConditions();

        // Assert
        Assert.IsTrue(engine.IsDestroyed, "Engine should be destroyed");
        Assert.IsTrue(weapon.IsDestroyed, "Weapon should be destroyed");
        Assert.IsTrue(disabledEventFired, "Ship disabled event should have fired");
        Assert.IsTrue(deathController.IsDisabled, "Ship should be disabled");
    }

    // Test 8: Ship is destroyed when Core section is breached
    [Test]
    public void Ship_Destroyed_WhenCoreSectionBreached()
    {
        // Arrange
        ShipSection coreSection = sectionManager.GetSection(SectionType.Core);
        Assert.IsFalse(coreSection.IsBreached, "Core should not be breached initially");

        bool destroyedEventFired = false;
        ShipDeathController.DeathCause reportedCause = ShipDeathController.DeathCause.None;
        deathController.OnShipDestroyed += (s, cause) =>
        {
            destroyedEventFired = true;
            reportedCause = cause;
        };

        // Expect the warning logs from ship destruction
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("SHIP DESTROYED"));
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("has been destroyed"));

        // Act - Breach Core section
        float damageToBreachCore = coreSection.CurrentArmor + coreSection.CurrentStructure + 10f;
        coreSection.ApplyDamage(damageToBreachCore);

        // Assert
        Assert.IsTrue(coreSection.IsBreached, "Core section should be breached");
        Assert.IsTrue(destroyedEventFired, "Ship destroyed event should have fired");
        Assert.AreEqual(ShipDeathController.DeathCause.CoreBreached, reportedCause, "Death cause should be CoreBreached");
    }

    // Test 9: Ship is destroyed when ALL sections are breached
    [Test]
    public void Ship_Destroyed_WhenAllSectionsBreached()
    {
        // Arrange
        bool destroyedEventFired = false;
        ShipDeathController.DeathCause reportedCause = ShipDeathController.DeathCause.None;
        deathController.OnShipDestroyed += (s, cause) =>
        {
            destroyedEventFired = true;
            reportedCause = cause;
        };

        // Act - Breach all sections except Core first
        foreach (var section in sectionManager.GetAllSections())
        {
            if (section != null && section.SectionType != SectionType.Core)
            {
                section.ApplyDamage(section.CurrentArmor + section.CurrentStructure + 10f);
            }
        }
        deathController.CheckDeathConditions();

        // Not destroyed yet because Core is intact
        Assert.IsFalse(destroyedEventFired, "Ship should not be destroyed until all sections including Core are breached");

        // Expect the warning logs from ship destruction
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("SHIP DESTROYED"));
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("has been destroyed"));

        // Now breach Core
        ShipSection core = sectionManager.GetSection(SectionType.Core);
        core.ApplyDamage(core.CurrentArmor + core.CurrentStructure + 10f);

        // Assert
        Assert.IsTrue(destroyedEventFired, "Ship destroyed event should have fired");
        // Could be CoreBreached or AllSectionsBreached depending on check order
        Assert.IsTrue(
            reportedCause == ShipDeathController.DeathCause.CoreBreached ||
            reportedCause == ShipDeathController.DeathCause.AllSectionsBreached,
            "Death cause should be CoreBreached or AllSectionsBreached");
    }

    // Test 10: Ship.CanMove and CanAct respond correctly to death states
    [Test]
    public void Ship_CanMoveAndCanAct_RespondToDeathStates()
    {
        // Arrange - Create a reactor to test destruction
        MountedReactor reactor = CreateReactor();
        deathController.SubscribeToReactor(reactor);

        // Initially can move and act
        Assert.IsTrue(ship.CanMove(), "Ship should be able to move initially");
        Assert.IsTrue(ship.CanAct(), "Ship should be able to act initially");

        // Expect the error logs from ship destruction via reactor
        LogAssert.ignoreFailingMessages = true;

        // Act - Destroy the reactor (Core Breach = instant death)
        reactor.TakeCriticalHit();
        reactor.TakeCriticalHit();

        // Assert - Ship is destroyed and cannot move or act
        Assert.IsTrue(deathController.IsDestroyed, "Ship should be destroyed");
        Assert.IsFalse(ship.CanMove(), "Destroyed ship should not be able to move");
        Assert.IsFalse(ship.CanAct(), "Destroyed ship should not be able to act");
    }
}
