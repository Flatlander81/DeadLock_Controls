using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for MissileBattery weapon.
/// Tests stats, ammo system, cooldown, arc validation.
/// </summary>
public class MissileTests
{
    private GameObject shipObject;
    private Ship ship;
    private WeaponManager weaponManager;

    // Test weapon hardpoint
    private GameObject missileHardpoint;
    private MissileBattery missileBattery;

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

        // Create missile hardpoint
        missileHardpoint = new GameObject("Missile_Hardpoint");
        missileHardpoint.transform.SetParent(shipObject.transform);
        missileHardpoint.transform.localPosition = new Vector3(0f, 1f, 0f); // Dorsal position
        missileBattery = missileHardpoint.AddComponent<MissileBattery>();

        // Add WeaponManager
        weaponManager = shipObject.AddComponent<WeaponManager>();

        // Create target ship (ahead, within range)
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
    /// Test 1: Verify MissileBattery default stats match GDD specs.
    /// damage=30, heat=20, arc=360, range=35, cooldown=1, ammo=20
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MissileBattery_DefaultStats()
    {
        yield return null; // Wait for Awake

        Assert.AreEqual("Missile Battery", missileBattery.WeaponName, "Name incorrect");
        Assert.AreEqual(30f, missileBattery.Damage, "Damage should be 30");
        Assert.AreEqual(20, missileBattery.HeatCost, "Heat cost should be 20");
        Assert.AreEqual(360f, missileBattery.FiringArc, "Arc should be 360 degrees (turret)");
        Assert.AreEqual(35f, missileBattery.MaxRange, "Range should be 35 units");
        Assert.AreEqual(1, missileBattery.MaxCooldown, "Cooldown should be 1 turn");
        Assert.AreEqual(20, missileBattery.AmmoCapacity, "Ammo capacity should be 20");
        Assert.AreEqual(0.4f, missileBattery.SpinUpTime, 0.01f, "Spin-up should be 0.4 seconds");
        Assert.AreEqual(15f, missileBattery.MissileSpeed, "Missile speed should be 15 units/sec");
        Assert.AreEqual(90f, missileBattery.MissileTurnRate, "Missile turn rate should be 90 deg/sec");
    }

    /// <summary>
    /// Test 2: Fire once, verify ammo decrements from 20 to 19.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MissileBattery_ConsumesAmmo()
    {
        yield return null; // Wait for initialization

        // Verify initial ammo
        Assert.AreEqual(20, missileBattery.CurrentAmmo, "Should start with 20 ammo");

        // Set target and fire
        missileBattery.SetTarget(targetShip);
        yield return missileBattery.FireWithSpinUp();

        // Verify ammo decremented
        Assert.AreEqual(19, missileBattery.CurrentAmmo, "Ammo should decrement to 19 after firing");
    }

    /// <summary>
    /// Test 3: Fire once, verify cooldown set to 1.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MissileBattery_SetsCooldown()
    {
        yield return null; // Wait for initialization

        // Verify initial cooldown
        Assert.AreEqual(0, missileBattery.CurrentCooldown, "Should start with 0 cooldown");

        // Set target and fire
        missileBattery.SetTarget(targetShip);
        yield return missileBattery.FireWithSpinUp();

        // Verify cooldown set
        Assert.AreEqual(1, missileBattery.CurrentCooldown, "Cooldown should be 1 after firing");

        // Verify CanFire returns false due to cooldown
        Assert.IsFalse(missileBattery.CanFire(), "Should not be able to fire while on cooldown");

        // Tick cooldown once
        missileBattery.TickCooldown();
        Assert.AreEqual(0, missileBattery.CurrentCooldown, "Cooldown should be 0 after tick");

        // Verify can fire again
        Assert.IsTrue(missileBattery.CanFire(), "Should be able to fire after cooldown expires");
    }

    /// <summary>
    /// Test 4: Test IsInArc at all cardinal directions for 360-degree arc.
    /// 0°, 90°, 180°, 270° should all pass (turret mount).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MissileBattery_FullArc()
    {
        yield return null; // Wait for initialization

        // Hardpoint is at (0, 1, 0)
        // 360° arc means all directions should be valid

        // 0° - directly ahead (+Z)
        targetObject.transform.position = new Vector3(0f, 1f, 20f);
        Assert.IsTrue(missileBattery.IsInArc(targetObject.transform.position),
            "Target at 0° (ahead) should be in 360° arc");

        // 90° - directly to the right (+X)
        targetObject.transform.position = new Vector3(20f, 1f, 0f);
        Assert.IsTrue(missileBattery.IsInArc(targetObject.transform.position),
            "Target at 90° (right) should be in 360° arc");

        // 180° - directly behind (-Z)
        targetObject.transform.position = new Vector3(0f, 1f, -20f);
        Assert.IsTrue(missileBattery.IsInArc(targetObject.transform.position),
            "Target at 180° (behind) should be in 360° arc");

        // 270° - directly to the left (-X)
        targetObject.transform.position = new Vector3(-20f, 1f, 0f);
        Assert.IsTrue(missileBattery.IsInArc(targetObject.transform.position),
            "Target at 270° (left) should be in 360° arc");

        // Also test above and below (vertical)
        targetObject.transform.position = new Vector3(0f, 21f, 0f);
        Assert.IsTrue(missileBattery.IsInArc(targetObject.transform.position),
            "Target above should be in 360° arc");

        targetObject.transform.position = new Vector3(0f, -19f, 0f);
        Assert.IsTrue(missileBattery.IsInArc(targetObject.transform.position),
            "Target below should be in 360° arc");
    }

    /// <summary>
    /// Test 5: Set ammo=0, verify CanFire() returns false.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MissileBattery_CannotFireEmpty()
    {
        yield return null; // Wait for initialization

        // Set target
        missileBattery.SetTarget(targetShip);

        // Verify can fire with ammo
        Assert.IsTrue(missileBattery.CanFire(), "Should be able to fire with ammo");

        // Deplete ammo using reflection to set directly (faster than firing 20 times)
        var ammoField = typeof(WeaponSystem).GetField("currentAmmo",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        ammoField.SetValue(missileBattery, 0);

        // Verify ammo depleted
        Assert.AreEqual(0, missileBattery.CurrentAmmo, "Ammo should be 0");

        // Verify cannot fire without ammo
        Assert.IsFalse(missileBattery.CanFire(), "Should NOT be able to fire with 0 ammo");
    }
}
