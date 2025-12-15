using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for projectile turn rate configuration.
/// Tests that different weapons can spawn projectiles with different turn rates.
/// </summary>
public class ProjectileTurnRateTests
{
    private GameObject managerObject;
    private ProjectileManager manager;

    private GameObject shipObject;
    private Ship ship;

    private GameObject targetObject;
    private Ship targetShip;

    [SetUp]
    public void Setup()
    {
        // Reset any existing ProjectileManager singleton
        ProjectileManager.ResetInstance();

        // Create ProjectileManager
        managerObject = new GameObject("ProjectileManager");
        manager = managerObject.AddComponent<ProjectileManager>();

        // Create owner ship
        shipObject = new GameObject("OwnerShip");
        ship = shipObject.AddComponent<Ship>();
        shipObject.AddComponent<HeatManager>();
        shipObject.transform.position = Vector3.zero;

        // Create target ship
        targetObject = new GameObject("TargetShip");
        targetShip = targetObject.AddComponent<Ship>();
        targetObject.AddComponent<HeatManager>();
        targetObject.transform.position = new Vector3(0f, 0f, 30f);
    }

    [TearDown]
    public void Teardown()
    {
        ProjectileManager.ResetInstance();

        if (managerObject != null) Object.DestroyImmediate(managerObject);
        if (shipObject != null) Object.DestroyImmediate(shipObject);
        if (targetObject != null) Object.DestroyImmediate(targetObject);
    }

    /// <summary>
    /// Test 1: Spawn homing projectile without turnRate param, verify default=90.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HomingProjectile_DefaultTurnRate()
    {
        yield return null;

        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Homing,
            SpawnPosition = Vector3.zero,
            SpawnRotation = Quaternion.identity,
            TargetPosition = targetObject.transform.position,
            TargetShip = targetShip,
            Damage = 30f,
            Speed = 10f,
            OwnerShip = ship
        };

        // Spawn without specifying turn rate (should default to 90)
        ProjectileManager.SpawnHomingProjectile(info);
        yield return null;

        HomingProjectile projectile = Object.FindFirstObjectByType<HomingProjectile>();
        Assert.IsNotNull(projectile, "Homing projectile should exist");
        Assert.AreEqual(90f, projectile.TurnRate, "Default turn rate should be 90 deg/sec");
    }

    /// <summary>
    /// Test 2: Spawn homing projectile with custom turnRate=45, verify set correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HomingProjectile_CustomTurnRate()
    {
        yield return null;

        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Homing,
            SpawnPosition = Vector3.zero,
            SpawnRotation = Quaternion.identity,
            TargetPosition = targetObject.transform.position,
            TargetShip = targetShip,
            Damage = 80f,
            Speed = 5f,
            OwnerShip = ship
        };

        // Spawn with custom turn rate of 45
        ProjectileManager.SpawnHomingProjectile(info, 45f);
        yield return null;

        HomingProjectile projectile = Object.FindFirstObjectByType<HomingProjectile>();
        Assert.IsNotNull(projectile, "Homing projectile should exist");
        Assert.AreEqual(45f, projectile.TurnRate, "Turn rate should be 45 deg/sec");
    }

    /// <summary>
    /// Test 3: Fire torpedo, verify projectile has slow turn rate (45 deg/sec).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_Torpedo_SlowTurnRate()
    {
        yield return null;

        // Create torpedo launcher on ship
        GameObject torpedoHardpoint = new GameObject("Torpedo_Hardpoint");
        torpedoHardpoint.transform.SetParent(shipObject.transform);
        torpedoHardpoint.transform.localPosition = Vector3.zero;
        TorpedoLauncher torpedoLauncher = torpedoHardpoint.AddComponent<TorpedoLauncher>();

        // Add WeaponManager to ship
        WeaponManager weaponManager = shipObject.AddComponent<WeaponManager>();

        yield return null; // Wait for initialization

        // Set target and fire
        torpedoLauncher.SetTarget(targetShip);
        yield return torpedoLauncher.FireWithSpinUp();

        // Find the spawned projectile
        HomingProjectile projectile = Object.FindFirstObjectByType<HomingProjectile>();
        Assert.IsNotNull(projectile, "Torpedo projectile should exist");
        Assert.AreEqual(45f, projectile.TurnRate, "Torpedo turn rate should be 45 deg/sec (slow)");

        // Cleanup
        Object.DestroyImmediate(torpedoHardpoint);
    }

    /// <summary>
    /// Test 4: Fire missile, verify projectile has fast turn rate (90 deg/sec).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_Missile_FastTurnRate()
    {
        yield return null;

        // Create missile battery on ship
        GameObject missileHardpoint = new GameObject("Missile_Hardpoint");
        missileHardpoint.transform.SetParent(shipObject.transform);
        missileHardpoint.transform.localPosition = Vector3.zero;
        MissileBattery missileBattery = missileHardpoint.AddComponent<MissileBattery>();

        // Add WeaponManager to ship
        WeaponManager weaponManager = shipObject.AddComponent<WeaponManager>();

        yield return null; // Wait for initialization

        // Set target and fire
        missileBattery.SetTarget(targetShip);
        yield return missileBattery.FireWithSpinUp();

        // Find the spawned projectile
        HomingProjectile projectile = Object.FindFirstObjectByType<HomingProjectile>();
        Assert.IsNotNull(projectile, "Missile projectile should exist");
        Assert.AreEqual(90f, projectile.TurnRate, "Missile turn rate should be 90 deg/sec (fast)");

        // Cleanup
        Object.DestroyImmediate(missileHardpoint);
    }
}
