using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Tests that simulate the Phase 2.2 test scene setup and verify all weapons can fire.
/// This ensures the manual test scene will work correctly.
/// </summary>
public class TestSceneSetupTests
{
    private GameObject projectileManagerObj;
    private GameObject playerShipObj;
    private Ship playerShip;
    private WeaponManager weaponManager;
    private GameObject enemyShipObj;
    private Ship enemyShip;

    [SetUp]
    public void Setup()
    {
        // Reset ProjectileManager
        ProjectileManager.ResetInstance();

        // Create ProjectileManager
        projectileManagerObj = new GameObject("ProjectileManager");
        projectileManagerObj.AddComponent<ProjectileManager>();

        // Create player ship at origin facing +Z
        playerShipObj = new GameObject("PlayerShip");
        playerShipObj.transform.position = Vector3.zero;
        playerShipObj.transform.rotation = Quaternion.identity; // Facing +Z
        playerShip = playerShipObj.AddComponent<Ship>();
        playerShipObj.AddComponent<HeatManager>();

        // Create enemy ship directly in front
        enemyShipObj = new GameObject("EnemyShip");
        enemyShipObj.transform.position = new Vector3(0, 0, 15); // 15 units ahead
        enemyShip = enemyShipObj.AddComponent<Ship>();
        enemyShipObj.AddComponent<HeatManager>();
    }

    [TearDown]
    public void Teardown()
    {
        ProjectileManager.ResetInstance();

        if (projectileManagerObj != null) Object.DestroyImmediate(projectileManagerObj);
        if (playerShipObj != null) Object.DestroyImmediate(playerShipObj);
        if (enemyShipObj != null) Object.DestroyImmediate(enemyShipObj);
    }

    /// <summary>
    /// Helper to create a weapon hardpoint like the test scene does.
    /// </summary>
    private T CreateWeaponHardpoint<T>(string name, Vector3 localPos) where T : WeaponSystem
    {
        GameObject hardpoint = new GameObject(name);
        hardpoint.transform.SetParent(playerShipObj.transform);
        hardpoint.transform.localPosition = localPos;
        hardpoint.transform.localRotation = Quaternion.identity;
        return hardpoint.AddComponent<T>();
    }

    /// <summary>
    /// Test 1: RailGun at side position can fire at target directly ahead.
    /// RailGun has 90° arc, so target at 0° should be in arc.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_RailGun_CanFireAtTargetAhead()
    {
        // Create RailGun at port side (like test scene)
        RailGun railGun = CreateWeaponHardpoint<RailGun>("RailGun_Port", new Vector3(-2f, 0.5f, 1.5f));

        // Add WeaponManager
        weaponManager = playerShipObj.AddComponent<WeaponManager>();
        yield return null;

        // Set target
        railGun.SetTarget(enemyShip);

        // Debug info
        Vector3 toTarget = (enemyShip.transform.position - railGun.transform.position).normalized;
        float angle = Vector3.Angle(railGun.transform.forward, toTarget);
        float distance = Vector3.Distance(railGun.transform.position, enemyShip.transform.position);

        Debug.Log($"RailGun test: angle={angle}, distance={distance}, arc={railGun.FiringArc}, range={railGun.MaxRange}");
        Debug.Log($"  Hardpoint pos={railGun.transform.position}, fwd={railGun.transform.forward}");
        Debug.Log($"  Target pos={enemyShip.transform.position}");

        // Verify
        Assert.IsTrue(railGun.IsInArc(enemyShip.transform.position), $"Target should be in arc (angle={angle}, halfArc={railGun.FiringArc/2})");
        Assert.IsTrue(railGun.IsInRange(enemyShip.transform.position), $"Target should be in range (dist={distance}, range={railGun.MaxRange})");
        Assert.IsTrue(railGun.CanFire(), "RailGun should be able to fire");
    }

    /// <summary>
    /// Test 2: NewtonianCannon can fire at target ahead.
    /// Cannon has 60° arc (spinal mount) and 20 range.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_NewtonianCannon_CanFireAtTargetAhead()
    {
        // Create Cannon at forward position
        NewtonianCannon cannon = CreateWeaponHardpoint<NewtonianCannon>("Cannon_Forward", new Vector3(0, 0, 3.5f));

        weaponManager = playerShipObj.AddComponent<WeaponManager>();
        yield return null;

        cannon.SetTarget(enemyShip);

        float angle = Vector3.Angle(cannon.transform.forward, (enemyShip.transform.position - cannon.transform.position).normalized);
        float distance = Vector3.Distance(cannon.transform.position, enemyShip.transform.position);

        Debug.Log($"Cannon test: angle={angle}, distance={distance}, arc={cannon.FiringArc}, range={cannon.MaxRange}");

        Assert.IsTrue(cannon.IsInArc(enemyShip.transform.position), $"Target should be in arc (angle={angle})");
        Assert.IsTrue(cannon.IsInRange(enemyShip.transform.position), $"Target should be in range (dist={distance}, range={cannon.MaxRange})");
        Assert.IsTrue(cannon.CanFire(), "Cannon should be able to fire");
    }

    /// <summary>
    /// Test 3: TorpedoLauncher can fire at target directly ahead.
    /// Torpedo has narrow 30° arc but target is directly ahead.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TorpedoLauncher_CanFireAtTargetAhead()
    {
        // Create Torpedo at forward position
        TorpedoLauncher torpedo = CreateWeaponHardpoint<TorpedoLauncher>("Torpedo_Forward", new Vector3(0, 1f, 2.5f));

        weaponManager = playerShipObj.AddComponent<WeaponManager>();
        yield return null;

        torpedo.SetTarget(enemyShip);

        float angle = Vector3.Angle(torpedo.transform.forward, (enemyShip.transform.position - torpedo.transform.position).normalized);
        float distance = Vector3.Distance(torpedo.transform.position, enemyShip.transform.position);

        Debug.Log($"Torpedo test: angle={angle}, distance={distance}, arc={torpedo.FiringArc}, range={torpedo.MaxRange}");
        Debug.Log($"  Hardpoint pos={torpedo.transform.position}, fwd={torpedo.transform.forward}");

        Assert.IsTrue(torpedo.IsInArc(enemyShip.transform.position), $"Target should be in arc (angle={angle}, halfArc={torpedo.FiringArc/2})");
        Assert.IsTrue(torpedo.IsInRange(enemyShip.transform.position), $"Target should be in range (dist={distance})");
        Assert.IsTrue(torpedo.CanFire(), "Torpedo should be able to fire");
    }

    /// <summary>
    /// Test 4: MissileBattery can fire at any target (360° arc).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MissileBattery_CanFireAtTargetAhead()
    {
        // Create Missile at dorsal position
        MissileBattery missile = CreateWeaponHardpoint<MissileBattery>("Missile_Dorsal", new Vector3(0, 1.5f, 0));

        weaponManager = playerShipObj.AddComponent<WeaponManager>();
        yield return null;

        missile.SetTarget(enemyShip);

        float distance = Vector3.Distance(missile.transform.position, enemyShip.transform.position);

        Debug.Log($"Missile test: distance={distance}, arc={missile.FiringArc}, range={missile.MaxRange}");

        Assert.IsTrue(missile.IsInArc(enemyShip.transform.position), "360° arc should always be in arc");
        Assert.IsTrue(missile.IsInRange(enemyShip.transform.position), $"Target should be in range (dist={distance}, range={missile.MaxRange})");
        Assert.IsTrue(missile.CanFire(), "Missile should be able to fire");
    }

    /// <summary>
    /// Test 5: Full test scene setup - all 6 weapons can fire at target directly ahead.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_FullTestSceneSetup_AllWeaponsCanFire()
    {
        // Create all weapons like test scene
        CreateWeaponHardpoint<RailGun>("RailGun_Port", new Vector3(-2f, 0.5f, 1.5f));
        CreateWeaponHardpoint<RailGun>("RailGun_Starboard", new Vector3(2f, 0.5f, 1.5f));
        CreateWeaponHardpoint<NewtonianCannon>("Cannon_Forward", new Vector3(0, 0, 3.5f));
        CreateWeaponHardpoint<TorpedoLauncher>("Torpedo_Forward", new Vector3(0, 1f, 2.5f));
        CreateWeaponHardpoint<MissileBattery>("Missile_Dorsal", new Vector3(0, 1.5f, 0));
        CreateWeaponHardpoint<MissileBattery>("Missile_Ventral", new Vector3(0, -0.5f, 0));

        weaponManager = playerShipObj.AddComponent<WeaponManager>();
        yield return null;

        Assert.AreEqual(6, weaponManager.Weapons.Count, "Should have 6 weapons");

        // Set target for all weapons
        foreach (WeaponSystem weapon in weaponManager.Weapons)
        {
            weapon.SetTarget(enemyShip);
        }

        // Verify each weapon can fire
        int canFireCount = 0;
        foreach (WeaponSystem weapon in weaponManager.Weapons)
        {
            float angle = Vector3.Angle(weapon.transform.forward,
                (enemyShip.transform.position - weapon.transform.position).normalized);
            float distance = Vector3.Distance(weapon.transform.position, enemyShip.transform.position);

            bool inArc = weapon.IsInArc(enemyShip.transform.position);
            bool inRange = weapon.IsInRange(enemyShip.transform.position);
            bool canFire = weapon.CanFireSilent();

            Debug.Log($"{weapon.WeaponName}: angle={angle:F1}, dist={distance:F1}, arc={weapon.FiringArc}, range={weapon.MaxRange}");
            Debug.Log($"  inArc={inArc}, inRange={inRange}, canFire={canFire}");

            if (canFire) canFireCount++;
        }

        Assert.AreEqual(6, canFireCount, $"All 6 weapons should be able to fire, but only {canFireCount} can");
    }

    /// <summary>
    /// Test 6: Weapons can actually fire and produce effects.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponsFire_ProduceProjectiles()
    {
        // Create one of each type
        RailGun railGun = CreateWeaponHardpoint<RailGun>("RailGun", new Vector3(0, 0, 1));
        TorpedoLauncher torpedo = CreateWeaponHardpoint<TorpedoLauncher>("Torpedo", new Vector3(0, 0, 2));
        MissileBattery missile = CreateWeaponHardpoint<MissileBattery>("Missile", new Vector3(0, 1, 0));

        weaponManager = playerShipObj.AddComponent<WeaponManager>();
        yield return null;

        // Set targets
        railGun.SetTarget(enemyShip);
        torpedo.SetTarget(enemyShip);
        missile.SetTarget(enemyShip);

        // Fire RailGun (fast ballistic projectile)
        Assert.IsTrue(railGun.CanFire(), "RailGun should be able to fire");
        yield return railGun.FireWithSpinUp();

        // Fire Torpedo and verify projectile spawned
        Assert.IsTrue(torpedo.CanFire(), "Torpedo should be able to fire");
        int torpedoAmmoBefore = torpedo.CurrentAmmo;
        yield return torpedo.FireWithSpinUp();
        Assert.AreEqual(torpedoAmmoBefore - 1, torpedo.CurrentAmmo, "Torpedo ammo should decrease");

        // Check torpedo projectile immediately after spawn
        HomingProjectile[] projectilesAfterTorpedo = Object.FindObjectsByType<HomingProjectile>(FindObjectsSortMode.None);
        Assert.GreaterOrEqual(projectilesAfterTorpedo.Length, 1, "Should have at least 1 homing projectile after torpedo");

        // Fire Missile and verify projectile spawned
        Assert.IsTrue(missile.CanFire(), "Missile should be able to fire");
        int missileAmmoBefore = missile.CurrentAmmo;
        yield return missile.FireWithSpinUp();
        Assert.AreEqual(missileAmmoBefore - 1, missile.CurrentAmmo, "Missile ammo should decrease");

        // Verify ammo was consumed (the key test - projectiles may be destroyed on hit)
        Assert.AreEqual(5, torpedo.CurrentAmmo, "Torpedo should have 5 ammo remaining");
        Assert.AreEqual(19, missile.CurrentAmmo, "Missile should have 19 ammo remaining");
    }

    /// <summary>
    /// Test 7: Heat is applied when weapons fire.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponsFire_ApplyHeat()
    {
        MissileBattery missile = CreateWeaponHardpoint<MissileBattery>("Missile", new Vector3(0, 1, 0));

        weaponManager = playerShipObj.AddComponent<WeaponManager>();
        yield return null;

        missile.SetTarget(enemyShip);

        HeatManager heatManager = playerShip.HeatManager;
        Assert.IsNotNull(heatManager, "Ship should have HeatManager");

        float heatBefore = heatManager.CurrentHeat;
        int expectedHeatCost = missile.HeatCost;

        yield return missile.FireWithSpinUp();

        // Heat should have increased (may be committed or planned)
        float totalHeat = heatManager.CurrentHeat + heatManager.PlannedHeat;
        Assert.GreaterOrEqual(totalHeat, heatBefore + expectedHeatCost - 1,
            $"Heat should increase by {expectedHeatCost}. Before={heatBefore}, After current={heatManager.CurrentHeat}, planned={heatManager.PlannedHeat}");
    }

    /// <summary>
    /// Test 8: Cooldown prevents re-firing.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_Cooldown_PreventsFiring()
    {
        TorpedoLauncher torpedo = CreateWeaponHardpoint<TorpedoLauncher>("Torpedo", new Vector3(0, 0, 2));

        weaponManager = playerShipObj.AddComponent<WeaponManager>();
        yield return null;

        torpedo.SetTarget(enemyShip);

        // Fire once
        Assert.IsTrue(torpedo.CanFire(), "Should be able to fire initially");
        yield return torpedo.FireWithSpinUp();

        // Should be on cooldown now
        Assert.Greater(torpedo.CurrentCooldown, 0, "Should be on cooldown after firing");
        Assert.IsFalse(torpedo.CanFireSilent(), "Should NOT be able to fire while on cooldown");
    }

    /// <summary>
    /// Test 9: Ammo depletion prevents firing.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AmmoDepletion_PreventsFiring()
    {
        TorpedoLauncher torpedo = CreateWeaponHardpoint<TorpedoLauncher>("Torpedo", new Vector3(0, 0, 2));

        weaponManager = playerShipObj.AddComponent<WeaponManager>();
        yield return null;

        torpedo.SetTarget(enemyShip);

        // Set ammo to 1
        var ammoField = typeof(WeaponSystem).GetField("currentAmmo",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        ammoField.SetValue(torpedo, 1);

        // Fire
        Assert.IsTrue(torpedo.CanFire(), "Should be able to fire with 1 ammo");
        yield return torpedo.FireWithSpinUp();

        Assert.AreEqual(0, torpedo.CurrentAmmo, "Ammo should be 0");

        // Reset cooldown to isolate ammo check
        var cdField = typeof(WeaponSystem).GetField("currentCooldown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        cdField.SetValue(torpedo, 0);

        Assert.IsFalse(torpedo.CanFireSilent(), "Should NOT be able to fire with 0 ammo");
    }

    /// <summary>
    /// Test 10: RailGun (infinite ammo) never runs out.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_RailGun_InfiniteAmmo()
    {
        RailGun railGun = CreateWeaponHardpoint<RailGun>("RailGun", new Vector3(0, 0, 1));

        weaponManager = playerShipObj.AddComponent<WeaponManager>();
        yield return null;

        railGun.SetTarget(enemyShip);

        Assert.AreEqual(0, railGun.AmmoCapacity, "RailGun should have 0 capacity (infinite)");

        // Fire multiple times
        for (int i = 0; i < 3; i++)
        {
            Assert.IsTrue(railGun.CanFire(), $"RailGun should always be able to fire (iteration {i})");
            yield return railGun.FireWithSpinUp();

            // Reset cooldown for next shot
            var cdField = typeof(WeaponSystem).GetField("currentCooldown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cdField.SetValue(railGun, 0);
        }

        Assert.AreEqual(0, railGun.CurrentAmmo, "RailGun current ammo should stay at 0 (infinite)");
    }
}
