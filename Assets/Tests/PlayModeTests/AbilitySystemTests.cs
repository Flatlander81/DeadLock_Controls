using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Comprehensive test suite for the Ability System.
/// Tests ability activation, cooldowns, heat costs, spin-up delays, and individual ability effects.
/// Updated to work with ScriptableObject-based AbilityData system.
/// </summary>
public class AbilitySystemTests
{
    private GameObject shipObject;
    private Ship ship;
    private HeatManager heatManager;
    private AbilitySystem abilitySystem;

    // Test ability data instances
    private EmergencyCoolingData emergencyCoolingData;
    private ShieldBoostData shieldBoostData;
    private EvasiveManeuverData evasiveManeuverData;

    /// <summary>
    /// Set up test environment before each test.
    /// Note: AbilitySystem is NOT added here - tests must call SetupAbilitySystemWithAbilities
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // Create test ship
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();
        heatManager = shipObject.AddComponent<HeatManager>();

        // Create test ability data instances
        CreateTestAbilityData();

        // DO NOT add AbilitySystem here - let tests add it after configuring abilities
    }

    /// <summary>
    /// Creates test ability data ScriptableObject instances.
    /// </summary>
    private void CreateTestAbilityData()
    {
        // Emergency Cooling - no heat cost, 4 turn cooldown
        emergencyCoolingData = ScriptableObject.CreateInstance<EmergencyCoolingData>();
        emergencyCoolingData.abilityName = "Emergency Cooling";
        emergencyCoolingData.description = "Test emergency cooling";
        emergencyCoolingData.heatCost = 0;
        emergencyCoolingData.maxCooldown = 4;
        emergencyCoolingData.spinUpTime = 0.1f;
        emergencyCoolingData.abilityColor = Color.cyan;

        // Shield Boost - 15 heat cost, 3 turn cooldown
        shieldBoostData = ScriptableObject.CreateInstance<ShieldBoostData>();
        shieldBoostData.abilityName = "Shield Boost";
        shieldBoostData.description = "Test shield boost";
        shieldBoostData.heatCost = 15;
        shieldBoostData.maxCooldown = 3;
        shieldBoostData.spinUpTime = 0.3f;
        shieldBoostData.abilityColor = Color.blue;

        // Evasive Maneuver - 10 heat cost, 2 turn cooldown
        evasiveManeuverData = ScriptableObject.CreateInstance<EvasiveManeuverData>();
        evasiveManeuverData.abilityName = "Evasive Maneuver";
        evasiveManeuverData.description = "Test evasive maneuver";
        evasiveManeuverData.heatCost = 10;
        evasiveManeuverData.maxCooldown = 2;
        evasiveManeuverData.spinUpTime = 0f;
        evasiveManeuverData.abilityColor = Color.green;
    }

    /// <summary>
    /// Helper to set up ability system with ability data.
    /// Adds the AbilitySystem component with pre-configured abilities.
    /// </summary>
    private void SetupAbilitySystemWithAbilities(params AbilityData[] abilities)
    {
        // Add AbilitySystem component
        abilitySystem = shipObject.AddComponent<AbilitySystem>();

        // Use reflection to set the private abilityDataList field BEFORE Start() runs
        var field = typeof(AbilitySystem).GetField("abilityDataList",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            var list = new List<AbilityData>(abilities);
            field.SetValue(abilitySystem, list);
        }
        else
        {
            Debug.LogError("Failed to find abilityDataList field via reflection");
        }
    }

    /// <summary>
    /// Clean up after each test.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        // Destroy ScriptableObject instances
        if (emergencyCoolingData != null) Object.DestroyImmediate(emergencyCoolingData);
        if (shieldBoostData != null) Object.DestroyImmediate(shieldBoostData);
        if (evasiveManeuverData != null) Object.DestroyImmediate(evasiveManeuverData);

        // Destroy ship
        if (shipObject != null)
        {
            Object.DestroyImmediate(shipObject);
        }
    }

    /// <summary>
    /// Test 1: Ability activation queues the ability.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilityActivation()
    {
        // Setup abilities
        SetupAbilitySystemWithAbilities(emergencyCoolingData);
        yield return null; // Wait for Start to be called

        // Try to activate
        bool activated = abilitySystem.TryActivateAbility("Emergency Cooling");

        // Verify
        Assert.IsTrue(activated, "Ability should activate successfully");
        Assert.AreEqual(1, abilitySystem.AbilityCount, "Should have 1 ability");

        var slot = abilitySystem.GetAbilitySlot("Emergency Cooling");
        Assert.IsNotNull(slot, "Ability slot should exist");
        Assert.IsTrue(slot.isQueued, "Ability should be queued");
    }

    /// <summary>
    /// Test 2: Ability cannot activate while on cooldown.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilityCannotActivateOnCooldown()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData);
        yield return null;

        // Get the slot and put it on cooldown manually
        var slot = abilitySystem.GetAbilitySlot("Emergency Cooling");
        slot.currentCooldown = 3;

        // Try to activate
        bool activated = abilitySystem.TryActivateAbility("Emergency Cooling");

        // Verify
        Assert.IsFalse(activated, "Ability should not activate while on cooldown");
        Assert.IsFalse(slot.isQueued, "Ability should not be queued");
    }

    /// <summary>
    /// Test 3: Ability cannot activate with insufficient heat.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilityCannotActivateInsufficientHeat()
    {
        SetupAbilitySystemWithAbilities(shieldBoostData);
        yield return null;

        // Max heat is 150, limit is 2x = 300
        // Shield Boost costs 15 heat
        // Set heat to 290, so 290 + 15 = 305 > 300 (should fail)
        for (int i = 0; i < 14; i++)
        {
            heatManager.AddPlannedHeat(20);  // 14 * 20 = 280
            heatManager.CommitPlannedHeat();
        }
        heatManager.AddPlannedHeat(10);  // 280 + 10 = 290
        heatManager.CommitPlannedHeat();

        // Try to activate (should fail: 290 + 15 = 305 > 300)
        bool activated = abilitySystem.TryActivateAbility("Shield Boost");

        // Verify
        Assert.IsFalse(activated, "Ability should not activate with insufficient heat capacity");

        var slot = abilitySystem.GetAbilitySlot("Shield Boost");
        Assert.IsFalse(slot.isQueued, "Ability should not be queued");
    }

    /// <summary>
    /// Test 4: Ability cooldown ticks down correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilityCooldownTick()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData);
        yield return null;

        var slot = abilitySystem.GetAbilitySlot("Emergency Cooling");

        // Start cooldown
        slot.currentCooldown = 4;
        Assert.AreEqual(4, slot.currentCooldown, "Cooldown should start at 4");

        // Tick once
        abilitySystem.TickAllCooldowns();
        Assert.AreEqual(3, slot.currentCooldown, "Cooldown should be 3 after one tick");

        // Tick again
        abilitySystem.TickAllCooldowns();
        Assert.AreEqual(2, slot.currentCooldown, "Cooldown should be 2 after two ticks");

        // Tick to completion
        abilitySystem.TickAllCooldowns();
        abilitySystem.TickAllCooldowns();
        Assert.AreEqual(0, slot.currentCooldown, "Cooldown should be 0 after four ticks");
        Assert.IsTrue(slot.CanActivate, "Ability should be ready");
    }

    /// <summary>
    /// Test 5: Ability executes after spin-up delay.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilitySpinUp()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData);
        yield return null;

        // Activate and queue
        abilitySystem.TryActivateAbility("Emergency Cooling");
        var slot = abilitySystem.GetAbilitySlot("Emergency Cooling");
        Assert.IsTrue(slot.isQueued, "Ability should be queued");

        // Record initial heat
        float initialHeat = heatManager.CurrentHeat;

        // Start execution
        yield return abilitySystem.ExecuteQueuedAbilities();

        // Wait for spin-up and execution
        yield return new WaitForSeconds(0.2f);

        // Verify execution occurred
        Assert.IsFalse(slot.isQueued, "Ability should no longer be queued");
        Assert.AreEqual(4, slot.currentCooldown, "Ability should be on cooldown");
    }

    /// <summary>
    /// Test 6: Emergency Cooling reduces heat by 50.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_EmergencyCooling()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData);
        yield return null;

        // Add heat to ship
        heatManager.AddPlannedHeat(100);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(100f, heatManager.CurrentHeat, "Heat should be 100");

        // Activate and execute
        abilitySystem.TryActivateAbility("Emergency Cooling");
        yield return abilitySystem.ExecuteQueuedAbilities();
        yield return new WaitForSeconds(0.2f);

        // Verify heat reduction (default cooling amount is 50)
        Assert.AreEqual(50f, heatManager.CurrentHeat, "Heat should be reduced to 50");
    }

    /// <summary>
    /// Test 7: Shield Boost increases shields.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldBoost()
    {
        SetupAbilitySystemWithAbilities(shieldBoostData);
        yield return null;

        // Damage shields first so there's room to boost
        ship.TakeDamage(250f); // Removes all shields (200) and some hull
        float initialShields = ship.CurrentShields; // Should be 0

        // Activate and execute
        abilitySystem.TryActivateAbility("Shield Boost");
        yield return abilitySystem.ExecuteQueuedAbilities();
        yield return new WaitForSeconds(0.4f);

        // Verify shields increased (default boost is 100)
        Assert.Greater(ship.CurrentShields, initialShields, "Shields should have increased");
        Assert.AreEqual(100f, ship.CurrentShields, "Shields should be 100 after boost");

        var slot = abilitySystem.GetAbilitySlot("Shield Boost");
        Assert.AreEqual(3, slot.currentCooldown, "Ability should be on cooldown (3 turns)");
    }

    /// <summary>
    /// Test 8: Evasive Maneuver executes successfully.
    /// Note: Turn rate boost is immediately reset by OnExecuteComplete in current implementation.
    /// Duration-based buffs would require additional buff management system.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_EvasiveManeuver()
    {
        SetupAbilitySystemWithAbilities(evasiveManeuverData);
        yield return null;

        // Activate and execute
        abilitySystem.TryActivateAbility("Evasive Maneuver");
        yield return abilitySystem.ExecuteQueuedAbilities();
        yield return new WaitForSeconds(0.1f);

        // Verify ability executed (check cooldown)
        var slot = abilitySystem.GetAbilitySlot("Evasive Maneuver");
        Assert.AreEqual(2, slot.currentCooldown, "Ability should be on cooldown (2 turns)");

        // Turn rate should be back to normal since OnExecuteComplete is called immediately
        // In a full implementation, this would require a buff/duration system
    }

    /// <summary>
    /// Test 9: Multiple abilities can be queued and executed.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MultipleAbilitiesQueue()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData, evasiveManeuverData);
        yield return null;

        // Queue both abilities
        bool activated1 = abilitySystem.TryActivateAbility("Emergency Cooling");
        bool activated2 = abilitySystem.TryActivateAbility("Evasive Maneuver");

        // Verify both queued
        Assert.IsTrue(activated1, "Emergency Cooling should activate");
        Assert.IsTrue(activated2, "Evasive Maneuver should activate");
        Assert.AreEqual(2, abilitySystem.AbilityCount, "Should have 2 abilities");

        // Execute both
        yield return abilitySystem.ExecuteQueuedAbilities();
        yield return new WaitForSeconds(0.3f);

        // Verify both executed
        var slot1 = abilitySystem.GetAbilitySlot("Emergency Cooling");
        var slot2 = abilitySystem.GetAbilitySlot("Evasive Maneuver");

        Assert.AreEqual(4, slot1.currentCooldown, "Emergency Cooling should be on cooldown");
        Assert.AreEqual(2, slot2.currentCooldown, "Evasive Maneuver should be on cooldown");
    }

    /// <summary>
    /// Test 10: Ability activation by index works correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilityActivationByIndex()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData, shieldBoostData);
        yield return null;

        // Activate first ability by index
        bool activated = abilitySystem.TryActivateAbilityByIndex(0);

        // Verify
        Assert.IsTrue(activated, "Ability should activate by index");

        var slot = abilitySystem.GetAbilitySlot("Emergency Cooling");
        Assert.IsTrue(slot.isQueued, "First ability should be queued");
    }

    /// <summary>
    /// Test 11: Heat cost is added as planned heat when queuing ability.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_PlannedHeatOnQueue()
    {
        SetupAbilitySystemWithAbilities(shieldBoostData);
        yield return null;

        float initialPlannedHeat = heatManager.PlannedHeat;

        // Queue ability (costs 15 heat)
        abilitySystem.TryActivateAbility("Shield Boost");

        // Verify planned heat increased
        Assert.AreEqual(initialPlannedHeat + 15, heatManager.PlannedHeat,
            "Planned heat should increase by ability cost");
    }

    /// <summary>
    /// Test 12: Queue can be cleared.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ClearQueue()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData, evasiveManeuverData);
        yield return null;

        // Queue abilities
        abilitySystem.TryActivateAbility("Emergency Cooling");
        abilitySystem.TryActivateAbility("Evasive Maneuver");

        // Clear queue
        abilitySystem.ClearQueue();

        // Verify
        var slot1 = abilitySystem.GetAbilitySlot("Emergency Cooling");
        var slot2 = abilitySystem.GetAbilitySlot("Evasive Maneuver");

        Assert.IsFalse(slot1.isQueued, "First ability should not be queued");
        Assert.IsFalse(slot2.isQueued, "Second ability should not be queued");
    }

    /// <summary>
    /// Test 13: Invalid ability index returns false.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_InvalidAbilityIndex()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData);
        yield return null;

        // Try to activate ability with invalid index
        bool activated = abilitySystem.TryActivateAbilityByIndex(99);

        // Verify
        Assert.IsFalse(activated, "Should not activate with invalid index");
    }
}
