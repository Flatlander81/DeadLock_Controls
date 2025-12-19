using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for the Critical Hit System (Step 3.4).
/// Tests slot-based targeting, system state transitions, and critical hit integration.
/// </summary>
public class CriticalHitTests
{
    private GameObject shipObject;
    private Ship ship;
    private ShipSection section;
    private SlotLayout layout;
    private CriticalHitSystem criticalHitSystem;

    [SetUp]
    public void SetUp()
    {
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();

        // Create a section with 20 total slots
        GameObject sectionObj = new GameObject("TestSection");
        sectionObj.transform.SetParent(shipObject.transform);
        section = sectionObj.AddComponent<ShipSection>();
        section.Initialize(SectionType.Core, 100f, 20f, ship); // 20 structure = 20 slots

        layout = section.SlotLayout;

        // Add critical hit system
        criticalHitSystem = shipObject.AddComponent<CriticalHitSystem>();
        section.SetCriticalHitSystem(criticalHitSystem);
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
    /// Helper to create a MountedSystem as a component.
    /// </summary>
    private MountedSystem CreateMountedSystem(ShipSystemType type, int startSlot)
    {
        GameObject systemObj = new GameObject($"System_{type}");
        systemObj.transform.SetParent(shipObject.transform);
        MountedSystem system = systemObj.AddComponent<MountedSystem>();
        system.Initialize(type, startSlot, section, ship);
        return system;
    }

    // Test 1: SlotLayout correctly assigns system to multiple slots
    [Test]
    public void SlotLayout_AssignsSystemToMultipleSlots()
    {
        // Arrange - Need bigger layout for reactor
        SlotLayout bigLayout = new SlotLayout(50, SectionType.Core);

        // Create reactor system
        MountedSystem reactor = CreateMountedSystem(ShipSystemType.ReactorCore, 1);
        int size = ShipSystemData.GetSize(ShipSystemType.ReactorCore); // 20 slots

        // Act
        bool added = bigLayout.AddSystem(reactor);

        // Assert
        Assert.IsTrue(added, "System should be added successfully");
        Assert.AreEqual(1, reactor.SlotStart);
        Assert.AreEqual(20, reactor.SlotEnd);

        // Verify all slots 1-20 return the reactor
        for (int i = 1; i <= 20; i++)
        {
            Assert.AreEqual(reactor, bigLayout.GetSystemAtSlot(i), $"Slot {i} should return reactor");
        }

        // Slot 21 should be empty
        Assert.IsNull(bigLayout.GetSystemAtSlot(21));
    }

    // Test 2: SlotLayout correctly reports occupied vs empty slots
    [Test]
    public void SlotLayout_ReportsOccupiedAndEmptySlots()
    {
        // Arrange - 20 slot layout
        MountedSystem cannon = CreateMountedSystem(ShipSystemType.NewtonianCannon, 1); // 8 slots (1-8)
        MountedSystem pd = CreateMountedSystem(ShipSystemType.PDTurret, 10); // 4 slots (10-13)

        // Act
        layout.AddSystem(cannon);
        layout.AddSystem(pd);

        // Assert
        Assert.AreEqual(12, layout.GetOccupiedSlotCount(), "Should have 12 occupied slots");
        Assert.AreEqual(8, layout.GetEmptySlotCount(), "Should have 8 empty slots");
    }

    // Test 3: Critical roll on empty slot returns EmptySlot result
    [Test]
    public void CriticalRoll_EmptySlot_ReturnsMiss()
    {
        // Arrange - layout with some empty slots
        MountedSystem small = CreateMountedSystem(ShipSystemType.PDTurret, 1); // 4 slots (1-4)
        layout.AddSystem(small);

        // Act - Force roll on empty slot
        CriticalHitResult result = criticalHitSystem.ForceCritical(layout, 10, SectionType.Core);

        // Assert
        Assert.IsTrue(result.WasEmptySlot, "Should be empty slot result");
        Assert.IsNull(result.SystemHit, "No system should be hit");
        Assert.IsFalse(result.SystemWasDamaged);
        Assert.IsFalse(result.SystemWasDestroyed);
    }

    // Test 4: Critical roll on occupied slot damages system
    [Test]
    public void CriticalRoll_OccupiedSlot_DamagesSystem()
    {
        // Arrange
        MountedSystem cannon = CreateMountedSystem(ShipSystemType.NewtonianCannon, 1);
        layout.AddSystem(cannon);

        Assert.AreEqual(SystemState.Operational, cannon.CurrentState);

        // Act - Force roll on occupied slot
        CriticalHitResult result = criticalHitSystem.ForceCritical(layout, 3, SectionType.Core);

        // Assert
        Assert.IsFalse(result.WasEmptySlot);
        Assert.AreEqual(cannon, result.SystemHit);
        Assert.IsTrue(result.SystemWasDamaged, "System should be damaged");
        Assert.IsFalse(result.SystemWasDestroyed, "System should not be destroyed yet");
        Assert.AreEqual(SystemState.Damaged, cannon.CurrentState);
    }

    // Test 5: Second critical hit destroys damaged system
    [Test]
    public void CriticalRoll_DamagedSystem_DestroysIt()
    {
        // Arrange
        MountedSystem cannon = CreateMountedSystem(ShipSystemType.NewtonianCannon, 1);
        layout.AddSystem(cannon);

        // First hit - damages
        cannon.TakeCriticalHit();
        Assert.AreEqual(SystemState.Damaged, cannon.CurrentState);

        // Act - Second hit
        CriticalHitResult result = criticalHitSystem.ForceCritical(layout, 3, SectionType.Core);

        // Assert
        Assert.IsTrue(result.SystemWasDestroyed, "System should be destroyed");
        Assert.AreEqual(SystemState.Destroyed, cannon.CurrentState);
    }

    // Test 6: Critical hit on already destroyed system does nothing
    [Test]
    public void CriticalRoll_DestroyedSystem_NoEffect()
    {
        // Arrange
        MountedSystem cannon = CreateMountedSystem(ShipSystemType.NewtonianCannon, 1);
        layout.AddSystem(cannon);

        // Destroy it
        cannon.TakeCriticalHit(); // Operational -> Damaged
        cannon.TakeCriticalHit(); // Damaged -> Destroyed
        Assert.AreEqual(SystemState.Destroyed, cannon.CurrentState);

        // Act - Hit destroyed system
        CriticalHitResult result = criticalHitSystem.ForceCritical(layout, 3, SectionType.Core);

        // Assert
        Assert.IsFalse(result.SystemWasDamaged, "Destroyed system can't be damaged again");
        Assert.IsFalse(result.SystemWasDestroyed, "Destruction already happened");
        Assert.AreEqual(SystemState.Destroyed, cannon.CurrentState);
    }

    // Test 7: Structure damage triggers critical roll
    [UnityTest]
    public IEnumerator StructureDamage_TriggersCriticalRoll()
    {
        // Arrange
        MountedSystem system = CreateMountedSystem(ShipSystemType.Sensors, 1);
        layout.AddSystem(system);

        bool criticalRolled = false;
        section.OnCriticalHit += (s, result) => criticalRolled = true;

        // Act - Apply damage that penetrates armor to structure
        // First, deplete armor
        section.ApplyDamage(100f); // Depletes 100 armor
        yield return null;

        // Now apply damage that hits structure
        section.ApplyDamage(10f);
        yield return null;

        // Assert
        Assert.IsTrue(criticalRolled, "Critical roll should have occurred when structure took damage");
    }

    // Test 8: MountedSystem state transitions work correctly
    [Test]
    public void MountedSystem_StateTransitions_WorkCorrectly()
    {
        // Arrange
        MountedSystem system = CreateMountedSystem(ShipSystemType.MainEngine, 1);

        // Assert initial state
        Assert.AreEqual(SystemState.Operational, system.CurrentState);
        Assert.IsFalse(system.IsDamaged);
        Assert.IsFalse(system.IsDestroyed);

        // Act & Assert - First hit
        bool changed1 = system.TakeCriticalHit();
        Assert.IsTrue(changed1);
        Assert.AreEqual(SystemState.Damaged, system.CurrentState);
        Assert.IsTrue(system.IsDamaged);
        Assert.IsFalse(system.IsDestroyed);

        // Act & Assert - Second hit
        bool changed2 = system.TakeCriticalHit();
        Assert.IsTrue(changed2);
        Assert.AreEqual(SystemState.Destroyed, system.CurrentState);
        Assert.IsFalse(system.IsDamaged);
        Assert.IsTrue(system.IsDestroyed);

        // Act & Assert - Third hit (no change)
        bool changed3 = system.TakeCriticalHit();
        Assert.IsFalse(changed3);
        Assert.AreEqual(SystemState.Destroyed, system.CurrentState);
    }

    // Test 9: DamageResult includes critical hit information
    [Test]
    public void DamageResult_IncludesCriticalHitInfo()
    {
        // Arrange
        MountedSystem system = CreateMountedSystem(ShipSystemType.PDTurret, 1);
        layout.AddSystem(system);

        // Deplete armor first
        section.ApplyDamage(100f);

        // Act - Apply damage to structure
        DamageResult result = section.ApplyDamage(5f);

        // Assert
        Assert.IsTrue(result.HadCritical, "Result should indicate critical occurred");
        Assert.IsNotNull(result.CriticalResult);
    }

    // Test 10: ShipSystemData returns correct sizes for all system types
    [Test]
    public void ShipSystemData_ReturnsCorrectSizes()
    {
        // From GDD specifications
        Assert.AreEqual(20, ShipSystemData.GetSize(ShipSystemType.ReactorCore), "Reactor Core size");
        Assert.AreEqual(15, ShipSystemData.GetSize(ShipSystemType.MainEngine), "Main Engine size");
        Assert.AreEqual(8, ShipSystemData.GetSize(ShipSystemType.NewtonianCannon), "Newtonian Cannon size");
        Assert.AreEqual(8, ShipSystemData.GetSize(ShipSystemType.TorpedoLauncher), "Torpedo Launcher size");
        Assert.AreEqual(8, ShipSystemData.GetSize(ShipSystemType.TorpedoMagazine), "Torpedo Magazine size");
        Assert.AreEqual(6, ShipSystemData.GetSize(ShipSystemType.MissileBattery), "Missile Battery size");
        Assert.AreEqual(6, ShipSystemData.GetSize(ShipSystemType.MissileMagazine), "Missile Magazine size");
        Assert.AreEqual(5, ShipSystemData.GetSize(ShipSystemType.RailGun), "Rail Gun size");
        Assert.AreEqual(4, ShipSystemData.GetSize(ShipSystemType.PDTurret), "PD Turret size");
        Assert.AreEqual(6, ShipSystemData.GetSize(ShipSystemType.Sensors), "Sensors size");
        Assert.AreEqual(5, ShipSystemData.GetSize(ShipSystemType.Radiator), "Radiator size");
    }
}
