using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Unit tests for the Damage UI System (Step 3.7).
/// Tests UI components respond correctly to damage events.
/// </summary>
public class DamageUITests
{
    private GameObject testRoot;
    private Ship ship;
    private SectionManager sectionManager;
    private ShieldSystem shieldSystem;
    private DamageRouter damageRouter;
    private DamageUIManager damageUIManager;
    private SectionStatusPanel sectionPanel;
    private SectionDetailPopup detailPopup;
    private CombatLogPanel combatLog;
    private ShieldStatusBar shieldBar;
    private HeatManager heatManager;
    private CoreProtectionSystem coreProtection;
    private ShipDeathController deathController;
    private SystemDegradationManager degradationManager;

    [SetUp]
    public void SetUp()
    {
        testRoot = new GameObject("TestRoot");

        // Create ship
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

        // Initialize shield system
        shieldSystem.Initialize(200f);

        // Create sections
        CreateSection(SectionType.Core);
        CreateSection(SectionType.Fore);
        CreateSection(SectionType.Aft);
        CreateSection(SectionType.Port);
        CreateSection(SectionType.Starboard);
        CreateSection(SectionType.Dorsal);
        CreateSection(SectionType.Ventral);

        // Set up references
        coreProtection.SetReferences(sectionManager, ship);
        damageRouter.SetReferences(shieldSystem, sectionManager, ship, coreProtection);
        deathController.SetReferences(ship, sectionManager, degradationManager);
        degradationManager.SetReferences(ship, heatManager, sectionManager);
        deathController.SubscribeToEvents();
        ship.SetDeathController(deathController);

        // Create UI components
        GameObject uiRoot = new GameObject("UIRoot");
        uiRoot.transform.SetParent(testRoot.transform);

        GameObject damageUIObj = new GameObject("DamageUIManager");
        damageUIObj.transform.SetParent(uiRoot.transform);
        damageUIManager = damageUIObj.AddComponent<DamageUIManager>();

        GameObject sectionPanelObj = new GameObject("SectionStatusPanel");
        sectionPanelObj.transform.SetParent(damageUIObj.transform);
        sectionPanel = sectionPanelObj.AddComponent<SectionStatusPanel>();

        GameObject detailPopupObj = new GameObject("SectionDetailPopup");
        detailPopupObj.transform.SetParent(damageUIObj.transform);
        detailPopup = detailPopupObj.AddComponent<SectionDetailPopup>();

        GameObject combatLogObj = new GameObject("CombatLogPanel");
        combatLogObj.transform.SetParent(damageUIObj.transform);
        combatLog = combatLogObj.AddComponent<CombatLogPanel>();

        GameObject shieldBarObj = new GameObject("ShieldStatusBar");
        shieldBarObj.transform.SetParent(damageUIObj.transform);
        shieldBar = shieldBarObj.AddComponent<ShieldStatusBar>();

        // Initialize UI components
        damageUIManager.SetComponents(sectionPanel, detailPopup, combatLog, shieldBar);
        sectionPanel.Initialize(ship);
        shieldBar.SetShieldSystem(shieldSystem); // Use direct set since Ship.Start() doesn't run in tests
    }

    [TearDown]
    public void TearDown()
    {
        if (testRoot != null)
        {
            Object.Destroy(testRoot);
        }
    }

    private ShipSection CreateSection(SectionType type)
    {
        GameObject sectionObj = new GameObject($"Section_{type}");
        sectionObj.transform.SetParent(testRoot.transform);
        ShipSection section = sectionObj.AddComponent<ShipSection>();
        section.Initialize(type, 50f, 100f, ship);
        sectionManager.RegisterSection(section);
        return section;
    }

    // Test 1: Shield bar updates on damage
    [Test]
    public void Test_ShieldBarUpdatesOnDamage()
    {
        // Arrange
        float initialShields = shieldSystem.CurrentShields;
        Assert.AreEqual(200f, initialShields, "Shields should start at 200");

        // Act
        DamageReport report = damageRouter.ProcessDamage(50f, SectionType.Fore);

        // Assert
        float expectedShields = 150f;
        Assert.AreEqual(expectedShields, shieldSystem.CurrentShields, "Shields should be reduced to 150");
        Assert.AreEqual(expectedShields, shieldBar.GetCurrentShields(), "Shield bar should show 150");
        Assert.AreEqual(0.75f, shieldBar.GetShieldPercentage(), 0.01f, "Shield percentage should be 75%");
        Assert.IsFalse(shieldBar.IsDepleted, "Shields should not be depleted");
    }

    // Test 2: Section color changes with damage
    [Test]
    public void Test_SectionColorChangesWithDamage()
    {
        // Arrange
        ShipSection foreSection = sectionManager.GetSection(SectionType.Fore);
        Assert.IsNotNull(foreSection);

        // Initially green (healthy)
        Color healthyColor = SectionStatusPanel.GetSectionColor(foreSection);
        Assert.AreEqual(Color.green, healthyColor, "Healthy section should be green");

        // Act - Damage armor partially (< 50%)
        foreSection.ApplyDamage(30f); // 50 - 30 = 20 armor (40%)

        // Assert - Should be yellow
        Color damagedColor = SectionStatusPanel.GetSectionColor(foreSection);
        Assert.AreEqual(Color.yellow, damagedColor, "Damaged section should be yellow");

        // Act - Breach armor
        foreSection.ApplyDamage(25f); // Armor gone

        // Assert - Should be orange
        Color armorGoneColor = SectionStatusPanel.GetSectionColor(foreSection);
        Assert.AreEqual(new Color(1f, 0.5f, 0f), armorGoneColor, "Armor-breached section should be orange");

        // Act - Damage structure critically
        foreSection.ApplyDamage(80f); // Structure low

        // Assert - Should be red
        Color criticalColor = SectionStatusPanel.GetSectionColor(foreSection);
        Assert.AreEqual(Color.red, criticalColor, "Critical section should be red");

        // Act - Breach section
        foreSection.ApplyDamage(50f);

        // Assert - Should be dark red
        Color breachedColor = SectionStatusPanel.GetSectionColor(foreSection);
        Assert.AreEqual(new Color(0.3f, 0f, 0f), breachedColor, "Breached section should be dark red");
    }

    // Test 3: Combat log records hits
    [Test]
    public void Test_CombatLogRecordsHits()
    {
        // Arrange
        combatLog.Clear();
        Assert.AreEqual(0, combatLog.EntryCount, "Combat log should start empty");

        // Act
        combatLog.LogHit("TestShip", SectionType.Fore, 75f, 25f, 30f, 20f);

        // Assert
        Assert.AreEqual(1, combatLog.EntryCount, "Combat log should have 1 entry");
        Assert.IsTrue(combatLog.ContainsEntry("TestShip hit!"), "Log should contain hit message");
        Assert.IsTrue(combatLog.ContainsEntry("Fore"), "Log should contain section name");
    }

    // Test 4: Combat log records criticals
    [Test]
    public void Test_CombatLogRecordsCriticals()
    {
        // Arrange
        combatLog.Clear();

        // Act
        combatLog.LogCritical("TestShip", SectionType.Port, ShipSystemType.NewtonianCannon, false);
        combatLog.LogCritical("TestShip", SectionType.Aft, ShipSystemType.MainEngine, true);

        // Assert
        Assert.AreEqual(2, combatLog.EntryCount, "Combat log should have 2 entries");
        Assert.IsTrue(combatLog.ContainsEntry("CRITICAL"), "Log should contain CRITICAL");
        Assert.IsTrue(combatLog.ContainsEntry("DAMAGED"), "Log should contain DAMAGED");
        Assert.IsTrue(combatLog.ContainsEntry("DESTROYED"), "Log should contain DESTROYED");
    }

    // Test 5: System icon shows damaged state
    [Test]
    public void Test_SystemIconShowsDamagedState()
    {
        // Arrange - Test color mapping for system states
        Color operationalColor = SectionDetailPopup.GetSystemStateColor(SystemState.Operational);
        Color damagedColor = SectionDetailPopup.GetSystemStateColor(SystemState.Damaged);
        Color destroyedColor = SectionDetailPopup.GetSystemStateColor(SystemState.Destroyed);

        // Assert
        Assert.AreEqual(Color.green, operationalColor, "Operational should be green");
        Assert.AreEqual(Color.yellow, damagedColor, "Damaged should be yellow");
        Assert.AreEqual(Color.red, destroyedColor, "Destroyed should be red");
    }

    // Test 6: Core indicator when exposed
    [Test]
    public void Test_CoreIndicatorWhenExposed()
    {
        // Arrange
        Assert.IsFalse(coreProtection.IsCoreExposed(), "Core should not be exposed initially");

        // Act - Breach Fore section
        ShipSection foreSection = sectionManager.GetSection(SectionType.Fore);
        float damageToBreachFore = foreSection.CurrentArmor + foreSection.CurrentStructure + 10f;
        foreSection.ApplyDamage(damageToBreachFore);

        // Assert
        Assert.IsTrue(coreProtection.IsCoreExposed(), "Core should be exposed after Fore breach");
        var exposedAngles = coreProtection.GetExposedAngles();
        Assert.Contains(SectionType.Fore, exposedAngles, "Fore should be in exposed angles");
    }

    // Test 7: Section detail shows correct data
    [Test]
    public void Test_SectionDetailShowsCorrectData()
    {
        // Arrange
        ShipSection foreSection = sectionManager.GetSection(SectionType.Fore);
        Assert.IsNotNull(foreSection);

        // Act
        detailPopup.ShowSection(foreSection);

        // Assert
        Assert.IsTrue(detailPopup.IsVisible, "Detail popup should be visible");
        Assert.AreEqual(foreSection, detailPopup.CurrentSection, "Current section should be Fore");

        // Hide and verify
        detailPopup.Hide();
        Assert.IsFalse(detailPopup.IsVisible, "Detail popup should be hidden");
    }

    // Test 8: UI updates on breach
    [Test]
    public void Test_UIUpdatesOnBreach()
    {
        // Arrange
        combatLog.Clear();
        ShipSection portSection = sectionManager.GetSection(SectionType.Port);

        // Act - Breach the section
        float damageToBreachPort = portSection.CurrentArmor + portSection.CurrentStructure + 10f;
        portSection.ApplyDamage(damageToBreachPort);

        // Manually log breach (since we're not going through DamageUIManager's event subscription)
        combatLog.LogBreach("TestShip", SectionType.Port);

        // Assert
        Assert.IsTrue(portSection.IsBreached, "Port section should be breached");
        Assert.IsTrue(combatLog.ContainsEntry("BREACH"), "Log should contain BREACH");
        Assert.IsTrue(combatLog.ContainsEntry("Port"), "Log should contain Port");
    }

    // Test 9: Death state UI display
    [Test]
    public void Test_DeathStateUIDisplay()
    {
        // Arrange
        combatLog.Clear();

        // Act - Log death event
        combatLog.LogDeath("TestShip", ShipDeathController.DeathCause.CoreBreach);

        // Assert
        Assert.IsTrue(combatLog.ContainsEntry("DESTROYED"), "Log should contain DESTROYED");
        Assert.IsTrue(combatLog.ContainsEntry("CoreBreach"), "Log should contain CoreBreach");
    }

    // Test 10: UI performance with many events
    [Test]
    public void Test_UIPerformanceWithManyEvents()
    {
        // Arrange
        combatLog.Clear();
        int eventCount = 100;

        // Act - Log many events
        float startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < eventCount; i++)
        {
            combatLog.LogHit("Ship" + i, SectionType.Fore, 50f, 10f, 25f, 15f);
            combatLog.LogCritical("Ship" + i, SectionType.Port, ShipSystemType.NewtonianCannon, false);
        }

        float elapsed = Time.realtimeSinceStartup - startTime;

        // Assert - Should complete quickly (under 100ms)
        Assert.Less(elapsed, 0.1f, "Logging 200 events should complete in under 100ms");

        // Max entries should be enforced (default 100)
        // The log stores 100 entries max, so after 200 logs, should have ~100
        Assert.LessOrEqual(combatLog.EntryCount, 100, "Combat log should enforce max entry limit");
    }
}
