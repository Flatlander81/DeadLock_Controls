using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for the System Degradation System (Step 3.5).
/// Tests degradation effects on movement, weapons, cooling, sensors, and reactor.
/// </summary>
public class SystemDegradationTests
{
    private GameObject shipObject;
    private Ship ship;
    private HeatManager heatManager;
    private SystemDegradationManager degradationManager;
    private ShipSection testSection;

    [SetUp]
    public void SetUp()
    {
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();
        heatManager = shipObject.AddComponent<HeatManager>();
        degradationManager = shipObject.AddComponent<SystemDegradationManager>();

        // Create a test section
        GameObject sectionObj = new GameObject("TestSection");
        sectionObj.transform.SetParent(shipObject.transform);
        testSection = sectionObj.AddComponent<ShipSection>();
        testSection.Initialize(SectionType.Core, 100f, 40f, ship);

        // Wire references
        degradationManager.SetReferences(ship, heatManager, null);
    }

    [TearDown]
    public void TearDown()
    {
        if (shipObject != null)
        {
            Object.Destroy(shipObject);
        }
    }

    /// <summary>
    /// Helper to create a MountedEngine.
    /// </summary>
    private MountedEngine CreateEngine()
    {
        GameObject engineObj = new GameObject("TestEngine");
        engineObj.transform.SetParent(shipObject.transform);
        MountedEngine engine = engineObj.AddComponent<MountedEngine>();
        engine.Initialize(ShipSystemType.MainEngine, 1, testSection, ship);
        degradationManager.RegisterSystem(engine);
        degradationManager.RecalculateAllMultipliers();
        return engine;
    }

    /// <summary>
    /// Helper to create a MountedRadiator.
    /// </summary>
    private MountedRadiator CreateRadiator()
    {
        GameObject radObj = new GameObject("TestRadiator");
        radObj.transform.SetParent(shipObject.transform);
        MountedRadiator radiator = radObj.AddComponent<MountedRadiator>();
        radiator.Initialize(ShipSystemType.Radiator, 1, testSection, ship);
        degradationManager.RegisterSystem(radiator);
        degradationManager.RecalculateAllMultipliers();
        return radiator;
    }

    /// <summary>
    /// Helper to create a MountedSensors.
    /// </summary>
    private MountedSensors CreateSensors()
    {
        GameObject sensorObj = new GameObject("TestSensors");
        sensorObj.transform.SetParent(shipObject.transform);
        MountedSensors sensors = sensorObj.AddComponent<MountedSensors>();
        sensors.Initialize(ShipSystemType.Sensors, 1, testSection, ship);
        degradationManager.RegisterSystem(sensors);
        degradationManager.RecalculateAllMultipliers();
        return sensors;
    }

    /// <summary>
    /// Helper to create a MountedReactor.
    /// </summary>
    private MountedReactor CreateReactor()
    {
        GameObject reactorObj = new GameObject("TestReactor");
        reactorObj.transform.SetParent(shipObject.transform);
        MountedReactor reactor = reactorObj.AddComponent<MountedReactor>();
        reactor.Initialize(ShipSystemType.ReactorCore, 1, testSection, ship);
        reactor.SetLinkedReferences(ship, heatManager);
        degradationManager.RegisterSystem(reactor);
        degradationManager.RecalculateAllMultipliers();
        return reactor;
    }

    /// <summary>
    /// Helper to create a MountedMagazine.
    /// </summary>
    private MountedMagazine CreateMagazine(MountedMagazine.MagazineType type)
    {
        GameObject magObj = new GameObject("TestMagazine");
        magObj.transform.SetParent(shipObject.transform);
        MountedMagazine magazine = magObj.AddComponent<MountedMagazine>();
        magazine.Initialize(ShipSystemType.TorpedoMagazine, 1, testSection, ship);
        magazine.SetMagazineType(type);
        degradationManager.RegisterSystem(magazine);
        degradationManager.RecalculateAllMultipliers();
        return magazine;
    }

    // Test 1: Damaged engine reduces speed multiplier
    [Test]
    public void DamagedEngine_ReducesSpeedMultiplier()
    {
        // Arrange
        MountedEngine engine = CreateEngine();
        Assert.AreEqual(1f, degradationManager.SpeedMultiplier, "Initial speed should be 1.0");

        // Act
        engine.TakeCriticalHit(); // Operational -> Damaged

        // Assert
        Assert.AreEqual(SystemState.Damaged, engine.CurrentState);
        Assert.Less(degradationManager.SpeedMultiplier, 1f, "Speed should be reduced when engine is damaged");
        Assert.AreEqual(0.5f, degradationManager.SpeedMultiplier, 0.01f, "Damaged engine should give 50% speed");
    }

    // Test 2: Destroyed engine makes ship unable to move (when it's the only engine)
    [Test]
    public void DestroyedEngine_MakesShipUnableToMove()
    {
        // Arrange
        MountedEngine engine = CreateEngine();
        Assert.IsTrue(degradationManager.CanShipMove(), "Ship should be able to move initially");

        // Act - Destroy engine
        engine.TakeCriticalHit(); // Operational -> Damaged
        engine.TakeCriticalHit(); // Damaged -> Destroyed

        // Assert
        Assert.AreEqual(SystemState.Destroyed, engine.CurrentState);
        Assert.IsFalse(degradationManager.CanShipMove(), "Ship should not be able to move with all engines destroyed");
        Assert.AreEqual(0f, degradationManager.SpeedMultiplier, "Speed multiplier should be 0 with destroyed engine");
    }

    // Test 3: Multiple engines provide redundancy (best engine used)
    [Test]
    public void MultipleEngines_ProvideRedundancy()
    {
        // Arrange - Create two engines
        MountedEngine engine1 = CreateEngine();
        MountedEngine engine2 = CreateEngine();

        Assert.AreEqual(1f, degradationManager.SpeedMultiplier);

        // Act - Damage first engine
        engine1.TakeCriticalHit();

        // Assert - Should use the undamaged engine
        Assert.AreEqual(1f, degradationManager.SpeedMultiplier, "Should use best (undamaged) engine");
        Assert.IsTrue(degradationManager.CanShipMove(), "Ship can still move with one working engine");
    }

    // Test 4: Damaged radiator reduces cooling multiplier
    [Test]
    public void DamagedRadiator_ReducesCoolingMultiplier()
    {
        // Arrange
        MountedRadiator radiator = CreateRadiator();
        Assert.AreEqual(1f, degradationManager.CoolingMultiplier, 0.01f, "Initial cooling should be 1.0");

        // Act
        radiator.TakeCriticalHit(); // Operational -> Damaged

        // Assert
        Assert.AreEqual(SystemState.Damaged, radiator.CurrentState);
        Assert.Less(degradationManager.CoolingMultiplier, 1f, "Cooling should be reduced when radiator is damaged");
    }

    // Test 5: Damaged sensors reduce targeting range multiplier
    [Test]
    public void DamagedSensors_ReduceTargetingRange()
    {
        // Arrange
        MountedSensors sensors = CreateSensors();
        Assert.AreEqual(1f, degradationManager.TargetingRangeMultiplier, "Initial targeting range should be 1.0");

        // Act
        sensors.TakeCriticalHit(); // Operational -> Damaged

        // Assert
        Assert.AreEqual(SystemState.Damaged, sensors.CurrentState);
        Assert.AreEqual(0.5f, degradationManager.TargetingRangeMultiplier, 0.01f, "Damaged sensors should halve targeting range");
    }

    // Test 6: Destroyed sensors severely impair targeting
    [Test]
    public void DestroyedSensors_SeverelyImpairTargeting()
    {
        // Arrange
        MountedSensors sensors = CreateSensors();

        // Act - Destroy sensors
        sensors.TakeCriticalHit(); // Operational -> Damaged
        sensors.TakeCriticalHit(); // Damaged -> Destroyed

        // Assert
        Assert.AreEqual(SystemState.Destroyed, sensors.CurrentState);
        Assert.AreEqual(0.1f, degradationManager.TargetingRangeMultiplier, 0.01f, "Destroyed sensors should give 10% targeting range");
    }

    // Test 7: Damaged reactor affects heat capacity or generates passive heat
    [Test]
    public void DamagedReactor_AffectsHeatSystem()
    {
        // Arrange
        MountedReactor reactor = CreateReactor();
        float initialCapacityMult = degradationManager.HeatCapacityMultiplier;
        float initialPassiveHeat = degradationManager.PassiveHeatGeneration;

        // Act
        reactor.TakeCriticalHit(); // Operational -> Damaged

        // Assert - One of these should have changed (random selection)
        bool heatCapacityChanged = degradationManager.HeatCapacityMultiplier < initialCapacityMult;
        bool passiveHeatChanged = degradationManager.PassiveHeatGeneration > initialPassiveHeat;

        Assert.IsTrue(heatCapacityChanged || passiveHeatChanged,
            "Damaged reactor should either reduce heat capacity or generate passive heat");
    }

    // Test 8: Magazine explosion deals damage on destruction
    [UnityTest]
    public IEnumerator MagazineExplosion_DealsDamageOnDestruction()
    {
        // Arrange
        MountedMagazine magazine = CreateMagazine(MountedMagazine.MagazineType.Torpedo);

        bool explosionOccurred = false;
        float explosionDamage = 0f;

        magazine.OnMagazineExplosion += (mag, damage) =>
        {
            explosionOccurred = true;
            explosionDamage = damage;
        };

        // Act - Destroy magazine
        magazine.TakeCriticalHit(); // Operational -> Damaged
        yield return null;
        magazine.TakeCriticalHit(); // Damaged -> Destroyed
        yield return null;

        // Assert
        Assert.IsTrue(explosionOccurred, "Magazine should have exploded");
        Assert.AreEqual(40f, explosionDamage, 0.01f, "Torpedo magazine should deal 40 damage");
    }

    // Test 9: Torpedo vs Missile magazine have different explosion damage
    [Test]
    public void MagazineExplosion_DifferentDamageByType()
    {
        // Arrange
        MountedMagazine torpedoMag = CreateMagazine(MountedMagazine.MagazineType.Torpedo);
        MountedMagazine missileMag = CreateMagazine(MountedMagazine.MagazineType.Missile);

        // Assert
        Assert.AreEqual(40f, torpedoMag.GetExplosionDamage(), "Torpedo magazine should deal 40 damage");
        Assert.AreEqual(25f, missileMag.GetExplosionDamage(), "Missile magazine should deal 25 damage");
    }

    // Test 10: SystemDegradationManager aggregates all system effects
    [Test]
    public void DegradationManager_AggregatesAllEffects()
    {
        // Arrange - Create multiple systems
        MountedEngine engine = CreateEngine();
        MountedRadiator radiator = CreateRadiator();
        MountedSensors sensors = CreateSensors();

        // Verify initial state
        Assert.AreEqual(1f, degradationManager.SpeedMultiplier);
        Assert.AreEqual(1f, degradationManager.CoolingMultiplier);
        Assert.AreEqual(1f, degradationManager.TargetingRangeMultiplier);

        // Act - Damage all systems
        engine.TakeCriticalHit();
        radiator.TakeCriticalHit();
        sensors.TakeCriticalHit();

        // Assert - All multipliers should be affected
        Assert.Less(degradationManager.SpeedMultiplier, 1f, "Speed should be reduced");
        Assert.Less(degradationManager.CoolingMultiplier, 1f, "Cooling should be reduced");
        Assert.Less(degradationManager.TargetingRangeMultiplier, 1f, "Targeting range should be reduced");

        // Verify summary includes all stats
        string summary = degradationManager.GetDegradationSummary();
        Assert.IsTrue(summary.Contains("Speed"), "Summary should include speed");
        Assert.IsTrue(summary.Contains("Cooling"), "Summary should include cooling");
        Assert.IsTrue(summary.Contains("Range"), "Summary should include range");
    }
}
