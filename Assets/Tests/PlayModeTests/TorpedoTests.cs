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
    /// damage=80, heat=25, arc=360 (broadside), range=50, cooldown=3, ammo=6
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TorpedoLauncher_DefaultStats()
    {
        yield return null; // Wait for Awake

        Assert.AreEqual("Torpedo Launcher", torpedoLauncher.WeaponName, "Name incorrect");
        Assert.AreEqual(80f, torpedoLauncher.Damage, "Damage should be 80");
        Assert.AreEqual(25, torpedoLauncher.HeatCost, "Heat cost should be 25");
        Assert.AreEqual(360f, torpedoLauncher.FiringArc, "Arc should be 360 degrees (broadside weapon)");
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
    /// Test 4: Test IsInArc for 360-degree broadside weapon.
    /// All angles should be in arc for broadside weapons.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TorpedoLauncher_BroadsideArc()
    {
        yield return null; // Wait for initialization

        // Torpedo launcher has 360° arc - all positions within range should be in arc

        // 0° - directly ahead (in arc)
        targetObject.transform.position = new Vector3(0f, 0f, 22f);
        Assert.IsTrue(torpedoLauncher.IsInArc(targetObject.transform.position),
            "Target at 0° (ahead) should be in 360° arc");

        // ~45° - diagonal (in arc for broadside)
        targetObject.transform.position = new Vector3(15f, 0f, 17f);
        Assert.IsTrue(torpedoLauncher.IsInArc(targetObject.transform.position),
            "Target at ~45° should be in 360° arc");

        // 90° - directly to the side (in arc for broadside)
        targetObject.transform.position = new Vector3(20f, 0f, 2f);
        Assert.IsTrue(torpedoLauncher.IsInArc(targetObject.transform.position),
            "Target at 90° (side) should be in 360° arc");

        // 135° - diagonal behind (in arc for broadside)
        targetObject.transform.position = new Vector3(15f, 0f, -13f);
        Assert.IsTrue(torpedoLauncher.IsInArc(targetObject.transform.position),
            "Target at 135° should be in 360° arc");

        // 180° - directly behind (in arc for broadside)
        targetObject.transform.position = new Vector3(0f, 0f, -20f);
        Assert.IsTrue(torpedoLauncher.IsInArc(targetObject.transform.position),
            "Target at 180° (behind) should be in 360° arc");
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
