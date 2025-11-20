using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

/// <summary>
/// Comprehensive test suite for the Heat System.
/// Tests heat accumulation, cooling, tiers, penalties, and integration with Ship.
/// </summary>
public class HeatSystemTests
{
    private GameObject testShipObject;
    private Ship testShip;
    private HeatManager heatManager;

    /// <summary>
    /// Setup before each test - creates a test ship with HeatManager.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Create test ship
        testShipObject = new GameObject("TestShip");
        testShip = testShipObject.AddComponent<Ship>();
        heatManager = testShipObject.AddComponent<HeatManager>();

        // Wait for Start() to be called
        Object.DontDestroyOnLoad(testShipObject);
    }

    /// <summary>
    /// Cleanup after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (testShipObject != null)
        {
            Object.DestroyImmediate(testShipObject);
        }
    }

    /// <summary>
    /// Test 1: Heat accumulation - Add heat, verify CurrentHeat increases.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HeatAccumulation()
    {
        yield return null; // Wait one frame for initialization

        float initialHeat = heatManager.CurrentHeat;
        Assert.AreEqual(0f, initialHeat, 0.01f, "Initial heat should be 0");

        // Add planned heat and commit
        heatManager.AddPlannedHeat(50f);
        Assert.AreEqual(50f, heatManager.PlannedHeat, 0.01f, "Planned heat should be 50");

        heatManager.CommitPlannedHeat();
        Assert.AreEqual(50f, heatManager.CurrentHeat, 0.01f, "Current heat should be 50 after commit");
        Assert.AreEqual(0f, heatManager.PlannedHeat, 0.01f, "Planned heat should be cleared after commit");

        Debug.Log("✓ Test_HeatAccumulation passed");
    }

    /// <summary>
    /// Test 2: Passive cooling - Apply cooling, verify heat decreases by 20.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_PassiveCooling()
    {
        yield return null;

        // Set heat to 100
        heatManager.AddPlannedHeat(100f);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(100f, heatManager.CurrentHeat, 0.01f);

        // Apply passive cooling
        heatManager.ApplyPassiveCooling();

        // Should be reduced by PassiveCooling amount (default 20)
        float expectedHeat = 100f - heatManager.PassiveCooling;
        Assert.AreEqual(expectedHeat, heatManager.CurrentHeat, 0.01f,
            $"Heat should be reduced by {heatManager.PassiveCooling}");

        Debug.Log("✓ Test_PassiveCooling passed");
    }

    /// <summary>
    /// Test 3: Heat tiers - Verify correct tier at each threshold.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HeatTiers()
    {
        yield return null;

        // Safe: 0-59
        heatManager.AddPlannedHeat(30f);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(HeatManager.HeatTier.Safe, heatManager.GetCurrentTier(), "30 heat should be Safe tier");

        // Minor: 60-79
        heatManager.AddPlannedHeat(35f);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(HeatManager.HeatTier.Minor, heatManager.GetCurrentTier(), "65 heat should be Minor tier");

        // Moderate: 80-99
        heatManager.AddPlannedHeat(20f);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(HeatManager.HeatTier.Moderate, heatManager.GetCurrentTier(), "85 heat should be Moderate tier");

        // Severe: 100-119
        heatManager.AddPlannedHeat(20f);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(HeatManager.HeatTier.Severe, heatManager.GetCurrentTier(), "105 heat should be Severe tier");

        // Critical: 120-149
        heatManager.AddPlannedHeat(20f);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(HeatManager.HeatTier.Critical, heatManager.GetCurrentTier(), "125 heat should be Critical tier");

        // Catastrophic: 150+
        heatManager.AddPlannedHeat(30f);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(HeatManager.HeatTier.Catastrophic, heatManager.GetCurrentTier(), "155 heat should be Catastrophic tier");

        Debug.Log("✓ Test_HeatTiers passed");
    }

    /// <summary>
    /// Test 4: Heat penalties - Verify penalty values at each tier.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HeatPenalties()
    {
        yield return null;

        // Safe tier - no penalties
        HeatManager.HeatPenalties penalties = heatManager.GetPenalties();
        Assert.AreEqual(1.0f, penalties.AccuracyMultiplier, 0.01f, "Safe tier should have 1.0 accuracy");
        Assert.AreEqual(1.0f, penalties.SpeedMultiplier, 0.01f, "Safe tier should have 1.0 speed");
        Assert.AreEqual(0f, penalties.HullDamagePerTurn, 0.01f, "Safe tier should have 0 hull damage");
        Assert.IsFalse(penalties.SensorFlicker, "Safe tier should have no sensor flicker");

        // Minor tier (60+)
        heatManager.AddPlannedHeat(65f);
        heatManager.CommitPlannedHeat();
        penalties = heatManager.GetPenalties();
        Assert.AreEqual(0.9f, penalties.AccuracyMultiplier, 0.01f, "Minor tier should have 0.9 accuracy");

        // Critical tier (120+) - has hull damage
        heatManager.AddPlannedHeat(60f);
        heatManager.CommitPlannedHeat();
        penalties = heatManager.GetPenalties();
        Assert.AreEqual(0.4f, penalties.AccuracyMultiplier, 0.01f, "Critical tier should have 0.4 accuracy");
        Assert.AreEqual(0.6f, penalties.SpeedMultiplier, 0.01f, "Critical tier should have 0.6 speed");
        Assert.AreEqual(5f, penalties.HullDamagePerTurn, 0.01f, "Critical tier should have 5 hull damage");

        // Catastrophic tier (150+)
        heatManager.AddPlannedHeat(30f);
        heatManager.CommitPlannedHeat();
        penalties = heatManager.GetPenalties();
        Assert.AreEqual(0.2f, penalties.AccuracyMultiplier, 0.01f, "Catastrophic tier should have 0.2 accuracy");
        Assert.AreEqual(0.5f, penalties.SpeedMultiplier, 0.01f, "Catastrophic tier should have 0.5 speed");
        Assert.AreEqual(20f, penalties.HullDamagePerTurn, 0.01f, "Catastrophic tier should have 20 hull damage");

        Debug.Log("✓ Test_HeatPenalties passed");
    }

    /// <summary>
    /// Test 5: Planned heat - Add planned heat, verify preview, commit, verify current.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_PlannedHeat()
    {
        yield return null;

        // Add planned heat without committing
        heatManager.AddPlannedHeat(30f);
        Assert.AreEqual(0f, heatManager.CurrentHeat, 0.01f, "Current heat should still be 0");
        Assert.AreEqual(30f, heatManager.PlannedHeat, 0.01f, "Planned heat should be 30");

        // Add more planned heat
        heatManager.AddPlannedHeat(20f);
        Assert.AreEqual(50f, heatManager.PlannedHeat, 0.01f, "Planned heat should accumulate to 50");

        // Commit planned heat
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(50f, heatManager.CurrentHeat, 0.01f, "Current heat should be 50 after commit");
        Assert.AreEqual(0f, heatManager.PlannedHeat, 0.01f, "Planned heat should be 0 after commit");

        // Test clearing planned heat
        heatManager.AddPlannedHeat(25f);
        heatManager.ClearPlannedHeat();
        Assert.AreEqual(0f, heatManager.PlannedHeat, 0.01f, "Planned heat should be 0 after clear");
        Assert.AreEqual(50f, heatManager.CurrentHeat, 0.01f, "Current heat should be unchanged");

        Debug.Log("✓ Test_PlannedHeat passed");
    }

    /// <summary>
    /// Test 6: Instant cooling - Apply instant cooling, verify immediate reduction.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_InstantCooling()
    {
        yield return null;

        // Set heat to 100
        heatManager.AddPlannedHeat(100f);
        heatManager.CommitPlannedHeat();
        Assert.AreEqual(100f, heatManager.CurrentHeat, 0.01f);

        // Apply instant cooling
        heatManager.InstantCooling(50f);
        Assert.AreEqual(50f, heatManager.CurrentHeat, 0.01f, "Heat should be reduced by 50");

        // Test that cooling doesn't go below 0
        heatManager.InstantCooling(100f);
        Assert.AreEqual(0f, heatManager.CurrentHeat, 0.01f, "Heat should not go below 0");

        Debug.Log("✓ Test_InstantCooling passed");
    }

    /// <summary>
    /// Test 7: Hull damage from heat - Set heat to 120, apply damage, verify hull decreased.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HullDamageFromHeat()
    {
        yield return null;

        float initialHull = testShip.CurrentHull;

        // Set heat to Critical tier (120+) which deals 5 damage/turn
        heatManager.AddPlannedHeat(125f);
        heatManager.CommitPlannedHeat();

        // Apply heat damage
        testShip.ApplyHeatDamage();

        // Hull should be reduced by 5
        float expectedHull = initialHull - 5f;
        Assert.AreEqual(expectedHull, testShip.CurrentHull, 0.01f, "Hull should be reduced by 5 from heat damage");

        // Set heat to Catastrophic tier (150+) which deals 20 damage/turn
        heatManager.AddPlannedHeat(30f);
        heatManager.CommitPlannedHeat();
        testShip.ApplyHeatDamage();

        expectedHull -= 20f;
        Assert.AreEqual(expectedHull, testShip.CurrentHull, 0.01f, "Hull should be reduced by 20 from catastrophic heat");

        Debug.Log("✓ Test_HullDamageFromHeat passed");
    }

    /// <summary>
    /// Test 8: Shield regeneration - Damage shields, regenerate, verify increase.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldRegeneration()
    {
        yield return null;

        float maxShields = testShip.MaxShields;
        float regenRate = testShip.ShieldRegenRate;

        // Damage shields
        testShip.TakeDamage(100f);
        float damagedShields = testShip.CurrentShields;
        Assert.AreEqual(maxShields - 100f, damagedShields, 0.01f, "Shields should be reduced by 100");

        // Regenerate shields
        testShip.RegenerateShields();
        float expectedShields = Mathf.Min(maxShields, damagedShields + regenRate);
        Assert.AreEqual(expectedShields, testShip.CurrentShields, 0.01f,
            $"Shields should regenerate by {regenRate}");

        // Test that shields don't regenerate above max
        for (int i = 0; i < 10; i++)
        {
            testShip.RegenerateShields();
        }
        Assert.AreEqual(maxShields, testShip.CurrentShields, 0.01f, "Shields should not exceed max");

        Debug.Log("✓ Test_ShieldRegeneration passed");
    }

    /// <summary>
    /// Test 9: Ship death - Reduce hull to 0, verify Die() called.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShipDeath()
    {
        yield return null;

        // Expect the death log error
        LogAssert.Expect(LogType.Error, new Regex(".*has been destroyed!"));

        // Deal massive damage to kill ship
        float massiveDamage = testShip.MaxShields + testShip.MaxHull + 100f;
        testShip.TakeDamage(massiveDamage);

        // Ship should be dead
        Assert.AreEqual(0f, testShip.CurrentHull, 0.01f, "Hull should be 0");
        Assert.IsFalse(testShipObject.activeSelf, "Ship GameObject should be deactivated");

        Debug.Log("✓ Test_ShipDeath passed");
    }

    /// <summary>
    /// Test 10: Heat events - Subscribe to events, verify they fire on heat changes.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HeatEvents()
    {
        yield return null;

        bool heatChangedFired = false;
        bool tierChangedFired = false;
        float receivedHeatValue = 0f;
        HeatManager.HeatTier receivedTier = HeatManager.HeatTier.Safe;

        // Subscribe to events
        heatManager.OnHeatChanged += (heat) =>
        {
            heatChangedFired = true;
            receivedHeatValue = heat;
        };

        heatManager.OnHeatTierChanged += (tier) =>
        {
            tierChangedFired = true;
            receivedTier = tier;
        };

        // Add heat and commit (should fire OnHeatChanged)
        heatManager.AddPlannedHeat(50f);
        heatManager.CommitPlannedHeat();

        Assert.IsTrue(heatChangedFired, "OnHeatChanged event should have fired");
        Assert.AreEqual(50f, receivedHeatValue, 0.01f, "Event should receive correct heat value");

        // Add enough heat to change tier (should fire OnHeatTierChanged)
        heatChangedFired = false;
        heatManager.AddPlannedHeat(15f);
        heatManager.CommitPlannedHeat();

        Assert.IsTrue(tierChangedFired, "OnHeatTierChanged event should have fired");
        Assert.AreEqual(HeatManager.HeatTier.Minor, receivedTier, "Event should receive correct tier");

        Debug.Log("✓ Test_HeatEvents passed");
    }
}
