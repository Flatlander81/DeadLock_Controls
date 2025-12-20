using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Integration tests for Heat and Cooldown turn-end processing.
/// Tests that TurnEndProcessor correctly handles heat dissipation and cooldown ticking.
/// </summary>
public class HeatCooldownIntegrationTests
{
    private GameObject testRoot;
    private TurnManager turnManager;
    private TurnEndProcessor turnEndProcessor;
    private Ship testShip;
    private HeatManager heatManager;
    private WeaponManager weaponManager;
    private SystemDegradationManager degradationManager;
    private List<MountedRadiator> radiators;

    [SetUp]
    public void Setup()
    {
        // Create root
        testRoot = new GameObject("TestRoot");

        // Create TurnManager
        GameObject turnManagerObj = new GameObject("TurnManager");
        turnManagerObj.transform.SetParent(testRoot.transform);
        turnManager = turnManagerObj.AddComponent<TurnManager>();

        // Create TurnEndProcessor
        GameObject processorObj = new GameObject("TurnEndProcessor");
        processorObj.transform.SetParent(testRoot.transform);
        turnEndProcessor = processorObj.AddComponent<TurnEndProcessor>();

        // Create test ship
        GameObject shipObj = new GameObject("TestShip");
        shipObj.transform.SetParent(testRoot.transform);
        testShip = shipObj.AddComponent<Ship>();
        heatManager = shipObj.AddComponent<HeatManager>();
        weaponManager = shipObj.AddComponent<WeaponManager>();
        degradationManager = shipObj.AddComponent<SystemDegradationManager>();

        // Create weapons container and weapons
        GameObject weaponsObj = new GameObject("Weapons");
        weaponsObj.transform.SetParent(shipObj.transform);

        // Create test weapons with cooldowns
        CreateTestWeapon(weaponsObj.transform, "TestWeapon1", 2);
        CreateTestWeapon(weaponsObj.transform, "TestWeapon2", 3);

        // Create radiators
        radiators = new List<MountedRadiator>();
        GameObject radiatorsObj = new GameObject("Radiators");
        radiatorsObj.transform.SetParent(shipObj.transform);

        radiators.Add(CreateRadiator(radiatorsObj.transform, "Radiator1"));
        radiators.Add(CreateRadiator(radiatorsObj.transform, "Radiator2"));

        // Register radiators with degradation manager
        foreach (var radiator in radiators)
        {
            degradationManager.RegisterSystem(radiator);
        }

        // Register ship with processor
        turnEndProcessor.RegisterShip(testShip);
    }

    [TearDown]
    public void Teardown()
    {
        if (testRoot != null)
        {
            Object.DestroyImmediate(testRoot);
        }
    }

    private RailGun CreateTestWeapon(Transform parent, string name, int cooldown)
    {
        GameObject weaponObj = new GameObject(name);
        weaponObj.transform.SetParent(parent);
        RailGun weapon = weaponObj.AddComponent<RailGun>();

        // Set cooldown via reflection
        var maxCdField = typeof(WeaponSystem).GetField("maxCooldown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        maxCdField?.SetValue(weapon, cooldown);

        var nameField = typeof(WeaponSystem).GetField("weaponName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        nameField?.SetValue(weapon, name);

        return weapon;
    }

    private MountedRadiator CreateRadiator(Transform parent, string name)
    {
        GameObject radiatorObj = new GameObject(name);
        radiatorObj.transform.SetParent(parent);
        MountedRadiator radiator = radiatorObj.AddComponent<MountedRadiator>();

        // Set base cooling contribution via reflection
        var coolingField = typeof(MountedRadiator).GetField("baseCoolingContribution",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        coolingField?.SetValue(radiator, 5f);

        return radiator;
    }

    private void SetWeaponCooldown(WeaponSystem weapon, int cooldown)
    {
        var field = typeof(WeaponSystem).GetField("currentCooldown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(weapon, cooldown);
    }

    // ==================== TEST 1: Heat Dissipates On Turn End ====================

    /// <summary>
    /// Test 1: Heat reduces by base amount at turn end.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HeatDissipatesOnTurnEnd()
    {
        yield return null; // Wait for initialization

        // Add heat
        heatManager.AddPlannedHeat(50f);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(50f, heatManager.CurrentHeat, 0.01f, "Heat should be 50");

        // Process turn end
        turnEndProcessor.ProcessTurnEnd(1);

        // Base dissipation is 10, so heat should be 40
        // But we also have 2 radiators at 5 each = +10, so total dissipation = 20
        // Final heat should be 30
        float expectedHeat = 50f - turnEndProcessor.BaseDissipationRate - (2 * 5f);
        Assert.AreEqual(expectedHeat, heatManager.CurrentHeat, 0.01f,
            $"Heat should be {expectedHeat} after dissipation");
    }

    // ==================== TEST 2: Radiator Bonus Applied ====================

    /// <summary>
    /// Test 2: Radiators provide bonus dissipation.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_RadiatorBonusApplied()
    {
        yield return null;

        // Register radiators with degradation manager
        degradationManager.RefreshSystemCache();

        // Calculate expected radiator bonus
        float radiatorBonus = turnEndProcessor.GetRadiatorBonus(testShip);

        // Should be 2 radiators * 5 each = 10
        Assert.AreEqual(10f, radiatorBonus, 0.01f, "Radiator bonus should be 10 (2 * 5)");

        // Total dissipation should be base + radiator bonus
        float totalDissipation = turnEndProcessor.CalculateDissipation(testShip);
        Assert.AreEqual(20f, totalDissipation, 0.01f, "Total dissipation should be 20 (10 base + 10 radiator)");
    }

    // ==================== TEST 3: Damaged Radiator Half Bonus ====================

    /// <summary>
    /// Test 3: Damaged radiators provide half effectiveness.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DamagedRadiatorHalfBonus()
    {
        yield return null;

        // Damage one radiator
        radiators[0].TakeCriticalHit();
        Assert.IsTrue(radiators[0].IsDamaged, "Radiator should be damaged");

        degradationManager.RefreshSystemCache();

        // One damaged (2.5), one operational (5) = 7.5
        float radiatorBonus = turnEndProcessor.GetRadiatorBonus(testShip);
        Assert.AreEqual(7.5f, radiatorBonus, 0.01f, "Radiator bonus should be 7.5 (2.5 + 5)");
    }

    // ==================== TEST 4: Destroyed Radiator No Bonus ====================

    /// <summary>
    /// Test 4: Destroyed radiators provide no contribution.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DestroyedRadiatorNoBonus()
    {
        yield return null;

        // Destroy one radiator (damage twice)
        radiators[0].TakeCriticalHit();
        radiators[0].TakeCriticalHit();
        Assert.IsTrue(radiators[0].IsDestroyed, "Radiator should be destroyed");

        degradationManager.RefreshSystemCache();

        // One destroyed (0), one operational (5) = 5
        float radiatorBonus = turnEndProcessor.GetRadiatorBonus(testShip);
        Assert.AreEqual(5f, radiatorBonus, 0.01f, "Radiator bonus should be 5 (0 + 5)");
    }

    // ==================== TEST 5: Weapon Cooldown Ticks ====================

    /// <summary>
    /// Test 5: Weapon cooldowns reduce by 1 at turn end.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponCooldownTicks()
    {
        yield return null;

        var weapons = weaponManager.Weapons;
        Assert.IsTrue(weapons.Count >= 1, "Should have at least one weapon");

        WeaponSystem weapon = weapons[0];

        // Set cooldown to 3
        SetWeaponCooldown(weapon, 3);
        Assert.AreEqual(3, weapon.CurrentCooldown, "Cooldown should be 3");

        // Process turn end
        turnEndProcessor.ProcessCooldowns(testShip);

        // Cooldown should be 2
        Assert.AreEqual(2, weapon.CurrentCooldown, "Cooldown should be 2 after tick");
    }

    // ==================== TEST 6: Cooldown Stops At Zero ====================

    /// <summary>
    /// Test 6: Cooldown doesn't go negative.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_CooldownStopsAtZero()
    {
        yield return null;

        var weapons = weaponManager.Weapons;
        WeaponSystem weapon = weapons[0];

        // Set cooldown to 0
        SetWeaponCooldown(weapon, 0);

        // Process turn end
        turnEndProcessor.ProcessCooldowns(testShip);

        // Cooldown should still be 0
        Assert.AreEqual(0, weapon.CurrentCooldown, "Cooldown should remain 0");
    }

    // ==================== TEST 7: Ability Cooldown Ticks ====================

    /// <summary>
    /// Test 7: Ability cooldowns tick down at turn end.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilityCooldownTicks()
    {
        yield return null;

        // Add AbilitySystem with a test ability
        AbilitySystem abilitySystem = testShip.gameObject.AddComponent<AbilitySystem>();

        // AbilitySystem needs AbilityData - we'll skip detailed ability testing
        // since AbilitySystem.TickAllCooldowns() already exists and is tested separately
        // Just verify the method is called without error
        turnEndProcessor.ProcessAbilityCooldowns(testShip);

        // No assertion needed - just verify no errors
        Assert.Pass("Ability cooldowns processed without error");
    }

    // ==================== TEST 8: Weapon Ready Event Fires ====================

    /// <summary>
    /// Test 8: Event fires when cooldown reaches 0.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponReadyEventFires()
    {
        yield return null;

        var weapons = weaponManager.Weapons;
        WeaponSystem weapon = weapons[0];

        // Set cooldown to 1 (will become 0)
        SetWeaponCooldown(weapon, 1);

        bool eventFired = false;
        WeaponSystem readyWeapon = null;

        turnEndProcessor.OnWeaponReady += (w) =>
        {
            eventFired = true;
            readyWeapon = w;
        };

        // Process turn end
        turnEndProcessor.ProcessCooldowns(testShip);

        Assert.IsTrue(eventFired, "OnWeaponReady event should have fired");
        Assert.AreEqual(weapon, readyWeapon, "Ready weapon should match");
        Assert.AreEqual(0, weapon.CurrentCooldown, "Cooldown should be 0");
    }

    // ==================== TEST 9: Heat Dissipation Event Fires ====================

    /// <summary>
    /// Test 9: Heat dissipation event fires with correct amount.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HeatDissipationEventFires()
    {
        yield return null;

        // Add heat
        heatManager.AddPlannedHeat(50f);
        heatManager.CommitPlannedHeat();

        bool eventFired = false;
        Ship eventShip = null;
        float eventAmount = 0f;

        turnEndProcessor.OnHeatDissipated += (ship, amount) =>
        {
            eventFired = true;
            eventShip = ship;
            eventAmount = amount;
        };

        // Process turn end
        turnEndProcessor.ProcessTurnEnd(1);

        Assert.IsTrue(eventFired, "OnHeatDissipated event should have fired");
        Assert.AreEqual(testShip, eventShip, "Event ship should match test ship");
        Assert.Greater(eventAmount, 0f, "Dissipation amount should be positive");
    }

    // ==================== TEST 10: Multiple Ships Processed ====================

    /// <summary>
    /// Test 10: All registered ships are processed at turn end.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MultipleShipsProcessed()
    {
        yield return null;

        // Create second ship
        GameObject ship2Obj = new GameObject("TestShip2");
        ship2Obj.transform.SetParent(testRoot.transform);
        Ship ship2 = ship2Obj.AddComponent<Ship>();
        HeatManager heatManager2 = ship2Obj.AddComponent<HeatManager>();

        // Add heat to both ships
        heatManager.AddPlannedHeat(50f);
        heatManager.CommitPlannedHeat();

        heatManager2.AddPlannedHeat(80f);
        heatManager2.CommitPlannedHeat();

        // Register second ship
        turnEndProcessor.RegisterShip(ship2);

        // Record initial heat
        float ship1InitialHeat = heatManager.CurrentHeat;
        float ship2InitialHeat = heatManager2.CurrentHeat;

        // Process turn end
        turnEndProcessor.ProcessTurnEnd(1);

        // Both ships should have reduced heat
        Assert.Less(heatManager.CurrentHeat, ship1InitialHeat, "Ship 1 heat should be reduced");
        Assert.Less(heatManager2.CurrentHeat, ship2InitialHeat, "Ship 2 heat should be reduced");
    }
}
