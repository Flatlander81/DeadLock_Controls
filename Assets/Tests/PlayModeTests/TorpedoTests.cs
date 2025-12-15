using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for TorpedoLauncher weapon.
/// Tests stats, ammo system, cooldown, arc validation.
/// </summary>
public class TorpedoTests
{
    private GameObject shipObject;
    private Ship ship;
    private WeaponManager weaponManager;

    // Test weapon hardpoint
    private GameObject torpedoHardpoint;
    private TorpedoLauncher torpedoLauncher;

    // Target ship for testing
    private GameObject targetObject;
    private Ship targetShip;

    [SetUp]
    public void Setup()
    {
        // Create test ship
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();

        // Add HeatManager (required for weapons)
        var heatManager = shipObject.AddComponent<HeatManager>();

        // Create torpedo hardpoint
        torpedoHardpoint = new GameObject("Torpedo_Hardpoint");
        torpedoHardpoint.transform.SetParent(shipObject.transform);
        torpedoHardpoint.transform.localPosition = new Vector3(0f, 0f, 2f); // Forward position
        torpedoLauncher = torpedoHardpoint.AddComponent<TorpedoLauncher>();

        // Add WeaponManager
        weaponManager = shipObject.AddComponent<WeaponManager>();

        // Create target ship (directly ahead, within range and arc)
        targetObject = new GameObject("TargetShip");
        targetObject.transform.position = new Vector3(0f, 0f, 20f); // 20 units ahead
        targetShip = targetObject.AddComponent<Ship>();
        targetObject.AddComponent<HeatManager>();
    }

    [TearDown]
    public void Teardown()
    {
        if (shipObject != null) Object.DestroyImmediate(shipObject);
        if (targetObject != null) Object.DestroyImmediate(targetObject);
    }

    /// <summary>
    /// Test 1: Verify TorpedoLauncher default stats match GDD specs.
    /// damage=80, heat=25, arc=30, range=50, cooldown=3, ammo=6
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TorpedoLauncher_DefaultStats()
    {
        yield return null; // Wait for Awake

        Assert.AreEqual("Torpedo Launcher", torpedoLauncher.WeaponName, "Name incorrect");
        Assert.AreEqual(80f, torpedoLauncher.Damage, "Damage should be 80");
        Assert.AreEqual(25, torpedoLauncher.HeatCost, "Heat cost should be 25");
        Assert.AreEqual(30f, torpedoLauncher.FiringArc, "Arc should be 30 degrees");
        Assert.AreEqual(50f, torpedoLauncher.MaxRange, "Range should be 50 units");
        Assert.AreEqual(3, torpedoLauncher.MaxCooldown, "Cooldown should be 3 turns");
        Assert.AreEqual(6, torpedoLauncher.AmmoCapacity, "Ammo capacity should be 6");
        Assert.AreEqual(1.0f, torpedoLauncher.SpinUpTime, 0.01f, "Spin-up should be 1.0 seconds");
        Assert.AreEqual(5f, torpedoLauncher.TorpedoSpeed, "Torpedo speed should be 5 units/sec");
        Assert.AreEqual(45f, torpedoLauncher.TorpedoTurnRate, "Torpedo turn rate should be 45 deg/sec");
    }

    /// <summary>
    /// Test 2: Fire once, verify ammo decrements from 6 to 5.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TorpedoLauncher_ConsumesAmmo()
    {
        yield return null; // Wait for initialization

        // Verify initial ammo
        Assert.AreEqual(6, torpedoLauncher.CurrentAmmo, "Should start with 6 ammo");

        // Set target and fire
        torpedoLauncher.SetTarget(targetShip);
        yield return torpedoLauncher.FireWithSpinUp();

        // Verify ammo decremented
        Assert.AreEqual(5, torpedoLauncher.CurrentAmmo, "Ammo should decrement to 5 after firing");
    }

    /// <summary>
    /// Test 3: Fire once, verify cooldown set to 3.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TorpedoLauncher_SetsCooldown()
    {
        yield return null; // Wait for initialization

        // Verify initial cooldown
        Assert.AreEqual(0, torpedoLauncher.CurrentCooldown, "Should start with 0 cooldown");

        // Set target and fire
        torpedoLauncher.SetTarget(targetShip);
        yield return torpedoLauncher.FireWithSpinUp();

        // Verify cooldown set
        Assert.AreEqual(3, torpedoLauncher.CurrentCooldown, "Cooldown should be 3 after firing");

        // Verify CanFire returns false due to cooldown
        Assert.IsFalse(torpedoLauncher.CanFire(), "Should not be able to fire while on cooldown");

        // Tick cooldown 3 times
        torpedoLauncher.TickCooldown();
        Assert.AreEqual(2, torpedoLauncher.CurrentCooldown, "Cooldown should be 2");
        torpedoLauncher.TickCooldown();
        Assert.AreEqual(1, torpedoLauncher.CurrentCooldown, "Cooldown should be 1");
        torpedoLauncher.TickCooldown();
        Assert.AreEqual(0, torpedoLauncher.CurrentCooldown, "Cooldown should be 0");

        // Verify can fire again (has target, ammo, no cooldown)
        Assert.IsTrue(torpedoLauncher.CanFire(), "Should be able to fire after cooldown expires");
    }

    /// <summary>
    /// Test 4: Test IsInArc at various angles for 30-degree arc.
    /// 0° = in arc, 14° = in arc, 16° = out of arc, 90° = out of arc
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TorpedoLauncher_NarrowArc()
    {
        yield return null; // Wait for initialization

        // Hardpoint is at (0, 0, 2) facing forward (+Z)
        // 30° arc means ±15° from forward (strict less-than, so <15° is in arc)
        // IsInArc uses Vector3.Angle from hardpoint position to target

        // Get hardpoint position for calculations
        Vector3 hardpointPos = torpedoHardpoint.transform.position; // (0, 0, 2)

        // 0° - directly ahead (in arc)
        // Target at (0, 0, 22) is directly ahead of hardpoint at (0, 0, 2)
        targetObject.transform.position = new Vector3(0f, 0f, 22f);
        Assert.IsTrue(torpedoLauncher.IsInArc(targetObject.transform.position),
            "Target at 0° should be in 30° arc");

        // ~10° - within arc
        // At 20 units ahead (z=22 from hardpoint at z=2), 10° offset = 20 * tan(10°) ≈ 3.5
        targetObject.transform.position = new Vector3(3.5f, 0f, 22f);
        Assert.IsTrue(torpedoLauncher.IsInArc(targetObject.transform.position),
            "Target at ~10° should be in 30° arc");

        // ~16° - outside arc (>15° half-arc)
        // At 20 units ahead, 16° offset = 20 * tan(16°) ≈ 5.7
        targetObject.transform.position = new Vector3(5.8f, 0f, 22f);
        Assert.IsFalse(torpedoLauncher.IsInArc(targetObject.transform.position),
            "Target at ~16° should be outside 30° arc");

        // 90° - directly to the side (definitely out of arc)
        targetObject.transform.position = new Vector3(20f, 0f, 2f);
        Assert.IsFalse(torpedoLauncher.IsInArc(targetObject.transform.position),
            "Target at 90° should be outside 30° arc");

        // Behind (180°) - definitely out of arc
        targetObject.transform.position = new Vector3(0f, 0f, -20f);
        Assert.IsFalse(torpedoLauncher.IsInArc(targetObject.transform.position),
            "Target behind should be outside 30° arc");
    }

    /// <summary>
    /// Test 5: Set ammo=0, verify CanFire() returns false.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TorpedoLauncher_CannotFireEmpty()
    {
        yield return null; // Wait for initialization

        // Set target
        torpedoLauncher.SetTarget(targetShip);

        // Verify can fire with ammo
        Assert.IsTrue(torpedoLauncher.CanFire(), "Should be able to fire with ammo");

        // Deplete ammo by firing 6 times
        for (int i = 0; i < 6; i++)
        {
            // Reset cooldown between shots using reflection
            var cooldownField = typeof(WeaponSystem).GetField("currentCooldown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cooldownField.SetValue(torpedoLauncher, 0);

            yield return torpedoLauncher.FireWithSpinUp();
        }

        // Verify ammo depleted
        Assert.AreEqual(0, torpedoLauncher.CurrentAmmo, "Ammo should be 0 after 6 shots");

        // Reset cooldown for final check
        var finalCooldownField = typeof(WeaponSystem).GetField("currentCooldown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        finalCooldownField.SetValue(torpedoLauncher, 0);

        // Verify cannot fire without ammo
        Assert.IsFalse(torpedoLauncher.CanFire(), "Should NOT be able to fire with 0 ammo");
    }
}
