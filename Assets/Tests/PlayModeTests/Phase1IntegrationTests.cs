using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Integration tests for Phase 1 - Heat System + Ability System integration.
/// Tests that the two systems work together harmoniously.
/// </summary>
public class Phase1IntegrationTests
{
    private GameObject shipObject;
    private Ship ship;
    private HeatManager heatManager;
    private AbilitySystem abilitySystem;

    // Test ability data
    private EmergencyCoolingData emergencyCoolingData;
    private ShieldBoostData shieldBoostData;
    private EvasiveManeuverData evasiveManeuverData;

    [SetUp]
    public void Setup()
    {
        // Create test ship
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();
        heatManager = shipObject.AddComponent<HeatManager>();

        // Create test ability data
        CreateTestAbilityData();
    }

    private void CreateTestAbilityData()
    {
        // Emergency Cooling - no heat cost, 4 turn cooldown
        emergencyCoolingData = ScriptableObject.CreateInstance<EmergencyCoolingData>();
        emergencyCoolingData.abilityName = "Emergency Cooling";
        emergencyCoolingData.heatCost = 0;
        emergencyCoolingData.maxCooldown = 4;
        emergencyCoolingData.spinUpTime = 0.1f;

        // Shield Boost - 15 heat cost, 3 turn cooldown
        shieldBoostData = ScriptableObject.CreateInstance<ShieldBoostData>();
        shieldBoostData.abilityName = "Shield Boost";
        shieldBoostData.heatCost = 15;
        shieldBoostData.maxCooldown = 3;
        shieldBoostData.spinUpTime = 0.3f;

        // Evasive Maneuver - 10 heat cost, 2 turn cooldown
        evasiveManeuverData = ScriptableObject.CreateInstance<EvasiveManeuverData>();
        evasiveManeuverData.abilityName = "Evasive Maneuver";
        evasiveManeuverData.heatCost = 10;
        evasiveManeuverData.maxCooldown = 2;
        evasiveManeuverData.spinUpTime = 0f;
    }

    private void SetupAbilitySystemWithAbilities(params AbilityData[] abilities)
    {
        abilitySystem = shipObject.AddComponent<AbilitySystem>();

        var field = typeof(AbilitySystem).GetField("abilityDataList",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            var list = new List<AbilityData>(abilities);
            field.SetValue(abilitySystem, list);
        }
    }

    [TearDown]
    public void Teardown()
    {
        if (emergencyCoolingData != null) Object.DestroyImmediate(emergencyCoolingData);
        if (shieldBoostData != null) Object.DestroyImmediate(shieldBoostData);
        if (evasiveManeuverData != null) Object.DestroyImmediate(evasiveManeuverData);

        if (shipObject != null)
        {
            Object.DestroyImmediate(shipObject);
        }
    }

    /// <summary>
    /// Test 1: Queue ability, verify HeatDisplay shows planned heat.
    /// Uses Evasive Maneuver (always available) instead of Shield Boost (requires shields depleted).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilityAddsPlannedHeat()
    {
        SetupAbilitySystemWithAbilities(evasiveManeuverData);
        yield return null;

        float initialPlannedHeat = heatManager.PlannedHeat;
        Assert.AreEqual(0f, initialPlannedHeat, "Planned heat should start at 0");

        // Queue ability
        abilitySystem.TryActivateAbility("Evasive Maneuver");

        // Verify planned heat increased
        Assert.AreEqual(10f, heatManager.PlannedHeat, "Planned heat should be 10 after queuing Evasive Maneuver");
        Assert.AreEqual(0f, heatManager.CurrentHeat, "Current heat should still be 0");
    }

    /// <summary>
    /// Test 2: Execute ability, verify heat added to current.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilityCommitsHeat()
    {
        SetupAbilitySystemWithAbilities(shieldBoostData);
        yield return null;

        // Damage shields first so ability can execute
        ship.TakeDamage(250f);

        // Queue and execute
        abilitySystem.TryActivateAbility("Shield Boost");
        Assert.AreEqual(15f, heatManager.PlannedHeat, "Planned heat should be 15");

        yield return abilitySystem.ExecuteQueuedAbilities();
        yield return new WaitForSeconds(0.4f);

        // Verify heat committed
        Assert.AreEqual(15f, heatManager.CurrentHeat, "Current heat should be 15 after execution");
        Assert.AreEqual(0f, heatManager.PlannedHeat, "Planned heat should be 0 after execution");
    }

    /// <summary>
    /// Test 3: Queue then cancel, verify planned cleared.
    /// Uses Evasive Maneuver (always available) instead of Shield Boost (requires shields depleted).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilityCancelClearsPlannedHeat()
    {
        SetupAbilitySystemWithAbilities(evasiveManeuverData);
        yield return null;

        // Queue ability
        abilitySystem.TryActivateAbility("Evasive Maneuver");
        Assert.AreEqual(10f, heatManager.PlannedHeat, "Planned heat should be 10");

        // Cancel queue
        abilitySystem.ClearQueue();

        // Verify planned heat cleared
        Assert.AreEqual(0f, heatManager.PlannedHeat, "Planned heat should be 0 after cancel");
    }

    /// <summary>
    /// Test 4: Queue 3 abilities, verify total heat correct.
    /// Note: Shield Boost requires shields to be depleted first.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MultipleAbilitiesHeatStacking()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData, shieldBoostData, evasiveManeuverData);
        yield return null;

        // Deplete shields so Shield Boost can activate
        ship.CurrentShields = 0f;

        // Queue all 3 abilities
        abilitySystem.TryActivateAbility("Emergency Cooling"); // 0 heat
        abilitySystem.TryActivateAbility("Shield Boost");      // 15 heat
        abilitySystem.TryActivateAbility("Evasive Maneuver");  // 10 heat

        // Verify total planned heat
        Assert.AreEqual(25f, heatManager.PlannedHeat, "Total planned heat should be 0+15+10 = 25");
    }

    /// <summary>
    /// Test 5: Heat at 140, use Emergency Cooling, verify drops to 90.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_EmergencyCoolingAtHighHeat()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData);
        yield return null;

        // Set heat to 140
        heatManager.AddPlannedHeat(140);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(140f, heatManager.CurrentHeat, "Heat should be 140");

        // Use Emergency Cooling
        abilitySystem.TryActivateAbility("Emergency Cooling");
        yield return abilitySystem.ExecuteQueuedAbilities();
        yield return new WaitForSeconds(0.2f);

        // Verify heat dropped by 50
        Assert.AreEqual(90f, heatManager.CurrentHeat, "Heat should be 90 after Emergency Cooling");
    }

    /// <summary>
    /// Test 6: Heat at 290, can't afford +15 ability (would exceed 300 limit).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilitiesPreventedByInsufficientHeat()
    {
        SetupAbilitySystemWithAbilities(shieldBoostData);
        yield return null;

        // Set heat to 290 (max is 150, limit is 2x = 300)
        for (int i = 0; i < 14; i++)
        {
            heatManager.AddPlannedHeat(20);
            heatManager.CommitPlannedHeat();
        }
        heatManager.AddPlannedHeat(10);
        heatManager.CommitPlannedHeat();

        Assert.AreEqual(290f, heatManager.CurrentHeat, "Heat should be 290");

        // Try to activate Shield Boost (15 heat, would be 305 total > 300)
        bool activated = abilitySystem.TryActivateAbility("Shield Boost");

        // Verify it failed
        Assert.IsFalse(activated, "Should not be able to activate ability (would exceed heat limit)");
        Assert.AreEqual(0f, heatManager.PlannedHeat, "Planned heat should still be 0");
    }

    /// <summary>
    /// Test 7: Verify turn order - abilities execute, then heat processes.
    /// Note: Full TurnManager integration would require TurnManager setup.
    /// This test verifies the ability system's part of the order.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AbilityExecutionOrder()
    {
        SetupAbilitySystemWithAbilities(emergencyCoolingData, shieldBoostData);
        yield return null;

        // Set initial heat
        heatManager.AddPlannedHeat(100);
        heatManager.CommitPlannedHeat();

        // Damage shields for Shield Boost
        ship.TakeDamage(250f);

        // Queue both abilities
        abilitySystem.TryActivateAbility("Emergency Cooling"); // Will reduce heat by 50
        abilitySystem.TryActivateAbility("Shield Boost");      // Will add 15 heat

        Assert.AreEqual(15f, heatManager.PlannedHeat, "Planned heat should be 15");
        Assert.AreEqual(100f, heatManager.CurrentHeat, "Current heat should still be 100");

        // Execute abilities
        yield return abilitySystem.ExecuteQueuedAbilities();
        yield return new WaitForSeconds(0.4f);

        // After execution:
        // - Started at 100 heat
        // - Emergency Cooling reduces by 50 → 50 heat
        // - Shield Boost adds 15 → 65 heat
        Assert.AreEqual(65f, heatManager.CurrentHeat, "Heat should be 65 after both abilities");
        Assert.AreEqual(0f, heatManager.PlannedHeat, "Planned heat should be 0");
    }

    /// <summary>
    /// Test 8: Use abilities to overheat, verify penalties apply.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HeatPenaltiesWithAbilities()
    {
        SetupAbilitySystemWithAbilities(shieldBoostData);
        yield return null;

        // Set heat to 100
        heatManager.AddPlannedHeat(100);
        heatManager.CommitPlannedHeat();

        // Damage shields
        ship.TakeDamage(250f);

        // Use Shield Boost (adds 15, total = 115 = Severe tier)
        abilitySystem.TryActivateAbility("Shield Boost");
        yield return abilitySystem.ExecuteQueuedAbilities();
        yield return new WaitForSeconds(0.4f);

        Assert.AreEqual(115f, heatManager.CurrentHeat, "Heat should be 115");

        // Verify penalties exist
        HeatManager.HeatPenalties penalties = heatManager.GetPenalties();
        Assert.Less(penalties.AccuracyMultiplier, 1f, "Accuracy should be penalized");
        Assert.Less(penalties.SpeedMultiplier, 1f, "Speed should be penalized");
    }

    /// <summary>
    /// Test 9: Multiple abilities commit heat correctly in sequence.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SequentialHeatCommit()
    {
        SetupAbilitySystemWithAbilities(shieldBoostData, evasiveManeuverData);
        yield return null;

        // Damage shields
        ship.TakeDamage(250f);

        // Queue both
        abilitySystem.TryActivateAbility("Shield Boost");      // 15 heat
        abilitySystem.TryActivateAbility("Evasive Maneuver");  // 10 heat

        Assert.AreEqual(25f, heatManager.PlannedHeat, "Planned should be 25");

        // Execute both
        yield return abilitySystem.ExecuteQueuedAbilities();
        yield return new WaitForSeconds(0.4f);

        // Verify both committed correctly
        Assert.AreEqual(25f, heatManager.CurrentHeat, "Current should be 25 (15+10)");
        Assert.AreEqual(0f, heatManager.PlannedHeat, "Planned should be 0");
    }

    /// <summary>
    /// Test 10: Verify shield boost works when shields are depleted.
    /// GDD: Shield Boost only works when shields are at 0.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldBoostWithRegen()
    {
        SetupAbilitySystemWithAbilities(shieldBoostData);
        yield return null;

        // Fully deplete shields (Shield Boost only works at 0)
        ship.CurrentShields = 0f;
        Assert.AreEqual(0f, ship.CurrentShields, "Shields should be 0");

        // Use Shield Boost (restores 100)
        abilitySystem.TryActivateAbility("Shield Boost");
        yield return abilitySystem.ExecuteQueuedAbilities();
        yield return new WaitForSeconds(0.4f);

        Assert.AreEqual(100f, ship.CurrentShields, "Shields should be 100 after boost");

        // Note: GDD specifies no shield regeneration - shields are a single bubble pool
        // Legacy regen test removed as it no longer applies to the new shield system
    }
}
