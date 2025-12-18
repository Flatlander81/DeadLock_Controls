using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Integration tests for Phase 2.2 - Full combat flow with ammo weapons.
/// Tests TorpedoLauncher, MissileBattery, and UI integration.
/// </summary>
public class Phase22IntegrationTests
{
    private GameObject managerObject;
    private ProjectileManager manager;

    private GameObject playerShipObject;
    private Ship playerShip;
    private WeaponManager playerWeaponManager;

    private GameObject targetShipObject;
    private Ship targetShip;

    [SetUp]
    public void Setup()
    {
        // Reset ProjectileManager singleton
        ProjectileManager.ResetInstance();

        // Create ProjectileManager
        managerObject = new GameObject("ProjectileManager");
        manager = managerObject.AddComponent<ProjectileManager>();

        // Create player ship
        playerShipObject = new GameObject("PlayerShip");
        playerShip = playerShipObject.AddComponent<Ship>();
        playerShipObject.AddComponent<HeatManager>();
        playerShipObject.transform.position = Vector3.zero;

        // Create target ship
        targetShipObject = new GameObject("TargetShip");
        targetShip = targetShipObject.AddComponent<Ship>();
        targetShipObject.AddComponent<HeatManager>();
        targetShipObject.transform.position = new Vector3(0f, 0f, 30f);
    }

    [TearDown]
    public void Teardown()
    {
        ProjectileManager.ResetInstance();

        if (managerObject != null) Object.DestroyImmediate(managerObject);
        if (playerShipObject != null) Object.DestroyImmediate(playerShipObject);
        if (targetShipObject != null) Object.DestroyImmediate(targetShipObject);
    }

    /// <summary>
    /// Test 1: Full combat cycle with TorpedoLauncher.
    /// Verifies ammo decrements, cooldown sets, projectile spawns with correct speed/turnRate.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TorpedoFullCombatCycle()
    {
        yield return null;

        // Create torpedo launcher on ship
        GameObject torpedoHardpoint = new GameObject("Torpedo_Hardpoint");
        torpedoHardpoint.transform.SetParent(playerShipObject.transform);
        torpedoHardpoint.transform.localPosition = Vector3.zero;
        TorpedoLauncher torpedoLauncher = torpedoHardpoint.AddComponent<TorpedoLauncher>();

        // Add WeaponManager
        playerWeaponManager = playerShipObject.AddComponent<WeaponManager>();
        yield return null;

        // Verify initial state
        Assert.AreEqual(6, torpedoLauncher.CurrentAmmo, "Should start with 6 ammo");
        Assert.AreEqual(0, torpedoLauncher.CurrentCooldown, "Should start with no cooldown");

        // Set target before checking CanFire (CanFire requires target)
        torpedoLauncher.SetTarget(targetShip);
        Assert.IsTrue(torpedoLauncher.CanFire(), "Should be able to fire initially");

        // Fire
        yield return torpedoLauncher.FireWithSpinUp();

        // Verify ammo decremented
        Assert.AreEqual(5, torpedoLauncher.CurrentAmmo, "Ammo should decrement to 5 after firing");

        // Verify cooldown set
        Assert.AreEqual(3, torpedoLauncher.CurrentCooldown, "Cooldown should be set to 3 turns");

        // Verify projectile spawned
        HomingProjectile projectile = Object.FindFirstObjectByType<HomingProjectile>();
        Assert.IsNotNull(projectile, "Torpedo projectile should exist");

        // Verify projectile properties
        Assert.AreEqual(45f, projectile.TurnRate, "Torpedo turn rate should be 45 deg/sec (slow)");

        // Cleanup
        Object.DestroyImmediate(torpedoHardpoint);
    }

    /// <summary>
    /// Test 2: Full combat cycle with MissileBattery.
    /// Verifies ammo decrements, cooldown sets, projectile spawns with correct speed/turnRate.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MissileFullCombatCycle()
    {
        yield return null;

        // Create missile battery on ship
        GameObject missileHardpoint = new GameObject("Missile_Hardpoint");
        missileHardpoint.transform.SetParent(playerShipObject.transform);
        missileHardpoint.transform.localPosition = Vector3.zero;
        MissileBattery missileBattery = missileHardpoint.AddComponent<MissileBattery>();

        // Add WeaponManager
        playerWeaponManager = playerShipObject.AddComponent<WeaponManager>();
        yield return null;

        // Verify initial state
        Assert.AreEqual(20, missileBattery.CurrentAmmo, "Should start with 20 ammo");
        Assert.AreEqual(0, missileBattery.CurrentCooldown, "Should start with no cooldown");

        // Set target before checking CanFire (CanFire requires target)
        missileBattery.SetTarget(targetShip);
        Assert.IsTrue(missileBattery.CanFire(), "Should be able to fire initially");

        // Fire
        yield return missileBattery.FireWithSpinUp();

        // Verify ammo decremented
        Assert.AreEqual(19, missileBattery.CurrentAmmo, "Ammo should decrement to 19 after firing");

        // Verify cooldown set
        Assert.AreEqual(1, missileBattery.CurrentCooldown, "Cooldown should be set to 1 turn");

        // Verify projectile spawned
        HomingProjectile projectile = Object.FindFirstObjectByType<HomingProjectile>();
        Assert.IsNotNull(projectile, "Missile projectile should exist");

        // Verify projectile properties
        Assert.AreEqual(90f, projectile.TurnRate, "Missile turn rate should be 90 deg/sec (fast)");

        // Cleanup
        Object.DestroyImmediate(missileHardpoint);
    }

    /// <summary>
    /// Test 3: Ammo depletion prevents firing.
    /// Creates TorpedoLauncher with 1 ammo, fires, then verifies cannot fire again.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AmmoDepletionPreventsFiring()
    {
        yield return null;

        // Create torpedo launcher
        GameObject torpedoHardpoint = new GameObject("Torpedo_Hardpoint");
        torpedoHardpoint.transform.SetParent(playerShipObject.transform);
        torpedoHardpoint.transform.localPosition = Vector3.zero;
        TorpedoLauncher torpedoLauncher = torpedoHardpoint.AddComponent<TorpedoLauncher>();

        // Add WeaponManager
        playerWeaponManager = playerShipObject.AddComponent<WeaponManager>();
        yield return null;

        // Set ammo to 1 via reflection
        var ammoField = typeof(WeaponSystem).GetField("currentAmmo",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        ammoField.SetValue(torpedoLauncher, 1);

        Assert.AreEqual(1, torpedoLauncher.CurrentAmmo, "Should have 1 ammo");

        // Set target before checking CanFire (CanFire requires target)
        torpedoLauncher.SetTarget(targetShip);
        Assert.IsTrue(torpedoLauncher.CanFire(), "Should be able to fire with 1 ammo");

        // Fire
        yield return torpedoLauncher.FireWithSpinUp();

        // Verify ammo depleted
        Assert.AreEqual(0, torpedoLauncher.CurrentAmmo, "Ammo should be 0 after firing");

        // Reset cooldown to isolate ammo check
        var cooldownField = typeof(WeaponSystem).GetField("currentCooldown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        cooldownField.SetValue(torpedoLauncher, 0);

        // Verify cannot fire with no ammo
        Assert.IsFalse(torpedoLauncher.CanFire(), "Should NOT be able to fire with 0 ammo");

        // Cleanup
        Object.DestroyImmediate(torpedoHardpoint);
    }

    /// <summary>
    /// Test 4: Mixed weapon alpha strike.
    /// Ship with RailGun (infinite) + TorpedoLauncher (limited).
    /// Verifies both fire, torpedo ammo decrements, railgun unaffected.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MixedWeaponAlphaStrike()
    {
        yield return null;

        // Create rail gun (infinite ammo)
        GameObject railGunHardpoint = new GameObject("RailGun_Hardpoint");
        railGunHardpoint.transform.SetParent(playerShipObject.transform);
        railGunHardpoint.transform.localPosition = Vector3.zero;
        RailGun railGun = railGunHardpoint.AddComponent<RailGun>();

        // Create torpedo launcher (limited ammo)
        GameObject torpedoHardpoint = new GameObject("Torpedo_Hardpoint");
        torpedoHardpoint.transform.SetParent(playerShipObject.transform);
        torpedoHardpoint.transform.localPosition = new Vector3(0, 1, 0);
        TorpedoLauncher torpedoLauncher = torpedoHardpoint.AddComponent<TorpedoLauncher>();

        // Add WeaponManager
        playerWeaponManager = playerShipObject.AddComponent<WeaponManager>();
        yield return null;

        // Verify both weapons discovered
        Assert.AreEqual(2, playerWeaponManager.Weapons.Count, "Should have 2 weapons");

        // Verify initial ammo states
        Assert.AreEqual(0, railGun.AmmoCapacity, "RailGun should have infinite ammo (capacity=0)");
        Assert.AreEqual(6, torpedoLauncher.CurrentAmmo, "Torpedo should have 6 ammo");

        // Assign both to group 1 and set target
        playerWeaponManager.AssignWeaponToGroup(railGun, 1);
        playerWeaponManager.AssignWeaponToGroup(torpedoLauncher, 1);

        railGun.SetTarget(targetShip);
        torpedoLauncher.SetTarget(targetShip);

        // Fire group (simulating alpha strike)
        List<WeaponSystem> groupWeapons = playerWeaponManager.GetWeaponsInGroup(1);
        Assert.AreEqual(2, groupWeapons.Count, "Group 1 should have 2 weapons");

        foreach (WeaponSystem weapon in groupWeapons)
        {
            if (weapon.CanFire())
            {
                yield return weapon.FireWithSpinUp();
            }
        }

        // Verify torpedo ammo decremented
        Assert.AreEqual(5, torpedoLauncher.CurrentAmmo, "Torpedo ammo should decrement to 5");

        // Verify rail gun unaffected (no ammo system)
        Assert.AreEqual(0, railGun.AmmoCapacity, "RailGun capacity should still be 0 (infinite)");
        Assert.AreEqual(0, railGun.CurrentAmmo, "RailGun current ammo should still be 0 (infinite)");

        // Cleanup
        Object.DestroyImmediate(railGunHardpoint);
        Object.DestroyImmediate(torpedoHardpoint);
    }

    /// <summary>
    /// Test 5: UI reflects ammo changes.
    /// Verifies that weapon properties update correctly for UI consumption.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_UIReflectsAmmoChanges()
    {
        yield return null;

        // Create torpedo launcher
        GameObject torpedoHardpoint = new GameObject("Torpedo_Hardpoint");
        torpedoHardpoint.transform.SetParent(playerShipObject.transform);
        torpedoHardpoint.transform.localPosition = Vector3.zero;
        TorpedoLauncher torpedoLauncher = torpedoHardpoint.AddComponent<TorpedoLauncher>();

        // Add WeaponManager
        playerWeaponManager = playerShipObject.AddComponent<WeaponManager>();
        yield return null;

        // Create config panel
        GameObject configPanelObject = new GameObject("ConfigPanel");
        WeaponConfigPanel configPanel = configPanelObject.AddComponent<WeaponConfigPanel>();
        configPanel.Initialize(playerShip);

        // Create group panel
        GameObject groupPanelObject = new GameObject("GroupPanel");
        WeaponGroupPanel groupPanel = groupPanelObject.AddComponent<WeaponGroupPanel>();
        groupPanel.Initialize(playerShip, null);
        groupPanel.SetTarget(targetShip);

        // Verify initial UI data
        Assert.AreEqual(6, torpedoLauncher.CurrentAmmo, "Initial ammo should be 6");
        Assert.AreEqual(6, torpedoLauncher.AmmoCapacity, "Ammo capacity should be 6");

        // Calculate initial percentage (what UI would display)
        float initialPercent = (float)torpedoLauncher.CurrentAmmo / torpedoLauncher.AmmoCapacity;
        Assert.AreEqual(1.0f, initialPercent, 0.01f, "Initial ammo percent should be 100%");

        // Fire weapon
        torpedoLauncher.SetTarget(targetShip);
        yield return torpedoLauncher.FireWithSpinUp();

        // Verify UI data updated
        Assert.AreEqual(5, torpedoLauncher.CurrentAmmo, "Ammo should be 5 after firing");

        // Calculate new percentage
        float newPercent = (float)torpedoLauncher.CurrentAmmo / torpedoLauncher.AmmoCapacity;
        Assert.AreEqual(5f / 6f, newPercent, 0.01f, "Ammo percent should be ~83%");

        // Verify UI warning condition (AmmoCapacity > 0 && CurrentAmmo <= 0)
        bool shouldShowNoAmmoWarning = torpedoLauncher.AmmoCapacity > 0 && torpedoLauncher.CurrentAmmo <= 0;
        Assert.IsFalse(shouldShowNoAmmoWarning, "Should NOT show no ammo warning yet");

        // Deplete ammo
        var ammoField = typeof(WeaponSystem).GetField("currentAmmo",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        ammoField.SetValue(torpedoLauncher, 0);

        // Verify warning condition now true
        shouldShowNoAmmoWarning = torpedoLauncher.AmmoCapacity > 0 && torpedoLauncher.CurrentAmmo <= 0;
        Assert.IsTrue(shouldShowNoAmmoWarning, "Should show no ammo warning when depleted");

        // Cleanup
        Object.DestroyImmediate(torpedoHardpoint);
        Object.DestroyImmediate(configPanelObject);
        Object.DestroyImmediate(groupPanelObject);
    }
}
