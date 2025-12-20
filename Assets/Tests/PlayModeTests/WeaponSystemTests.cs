using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for Phase 2 - Weapon System (Track A).
/// Tests weapon base classes, firing logic, and integration.
/// </summary>
public class WeaponSystemTests
{
    private GameObject shipObject;
    private Ship ship;
    private WeaponManager weaponManager;

    // Test weapon hardpoints
    private GameObject railGunHardpoint;
    private GameObject cannonHardpoint;
    private RailGun railGun;
    private NewtonianCannon cannon;

    // Target ship for testing
    private GameObject targetObject;
    private Ship targetShip;

    [SetUp]
    public void Setup()
    {
        // Reset ProjectileManager to clear any leftover projectiles from previous tests
        ProjectileManager.ResetInstance();

        // Create ProjectileManager for this test
        var pmObj = new GameObject("ProjectileManager");
        pmObj.AddComponent<ProjectileManager>();

        // Create test ship
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();

        // Add HeatManager (required for weapons)
        var heatManager = shipObject.AddComponent<HeatManager>();

        // Create weapon hardpoints
        railGunHardpoint = new GameObject("RailGun_Hardpoint");
        railGunHardpoint.transform.SetParent(shipObject.transform);
        railGunHardpoint.transform.localPosition = new Vector3(1f, 0f, 0f);
        railGun = railGunHardpoint.AddComponent<RailGun>();

        cannonHardpoint = new GameObject("Cannon_Hardpoint");
        cannonHardpoint.transform.SetParent(shipObject.transform);
        cannonHardpoint.transform.localPosition = new Vector3(-1f, 0f, 0f);
        cannon = cannonHardpoint.AddComponent<NewtonianCannon>();

        // Add WeaponManager
        weaponManager = shipObject.AddComponent<WeaponManager>();

        // Create target ship (with collider for projectile detection)
        targetObject = new GameObject("TargetShip");
        targetObject.transform.position = new Vector3(0f, 0f, 10f); // 10 units ahead
        targetShip = targetObject.AddComponent<Ship>();
        targetObject.AddComponent<HeatManager>();
        targetObject.AddComponent<BoxCollider>(); // Required for projectile collision detection
    }

    [TearDown]
    public void Teardown()
    {
        // Clear all projectiles before destroying objects
        ProjectileManager.ResetInstance();

        if (shipObject != null) Object.DestroyImmediate(shipObject);
        if (targetObject != null) Object.DestroyImmediate(targetObject);
    }

    /// <summary>
    /// Test 1: Create weapon, verify properties set.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponInitialization()
    {
        yield return null; // Wait for Start/Awake

        // RailGun properties
        Assert.AreEqual("Rail Gun", railGun.WeaponName, "RailGun name incorrect");
        Assert.AreEqual(20f, railGun.Damage, "RailGun damage incorrect");
        Assert.AreEqual(15, railGun.HeatCost, "RailGun heat cost incorrect");
        Assert.AreEqual(360f, railGun.FiringArc, "RailGun arc incorrect");
        Assert.AreEqual(30f, railGun.MaxRange, "RailGun range incorrect");
        Assert.AreEqual(0, railGun.MaxCooldown, "RailGun cooldown incorrect");
        Assert.AreEqual(0.2f, railGun.SpinUpTime, 0.01f, "RailGun spin-up time incorrect");

        // NewtonianCannon properties
        Assert.AreEqual("Newtonian Cannon", cannon.WeaponName, "Cannon name incorrect");
        Assert.AreEqual(40f, cannon.Damage, "Cannon damage incorrect");
        Assert.AreEqual(30, cannon.HeatCost, "Cannon heat cost incorrect");
        Assert.AreEqual(60f, cannon.FiringArc, "Cannon arc incorrect");
        Assert.AreEqual(20f, cannon.MaxRange, "Cannon range incorrect");
        Assert.AreEqual(0, cannon.MaxCooldown, "Cannon cooldown incorrect");
        Assert.AreEqual(0.5f, cannon.SpinUpTime, 0.01f, "Cannon spin-up time incorrect");
    }

    /// <summary>
    /// Test 2: Test IsInArc() with various positions.
    /// RailGun has 360° arc, Cannon has 60° arc (spinal mount).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ArcCheck()
    {
        yield return null;

        // Position target directly ahead (0, 0, 10)
        targetObject.transform.position = new Vector3(0f, 0f, 10f);
        Assert.IsTrue(railGun.IsInArc(targetObject.transform.position), "RailGun: Target ahead should be in 360° arc");
        Assert.IsTrue(cannon.IsInArc(targetObject.transform.position), "Cannon: Target ahead should be in 60° forward arc");

        // Position target behind (0, 0, -10)
        targetObject.transform.position = new Vector3(0f, 0f, -10f);
        Assert.IsTrue(railGun.IsInArc(targetObject.transform.position), "RailGun: Target behind should be in 360° arc");
        Assert.IsFalse(cannon.IsInArc(targetObject.transform.position), "Cannon: Target behind should NOT be in 60° forward arc");

        // Position target to the side (10, 0, 0)
        targetObject.transform.position = new Vector3(10f, 0f, 0f);
        Assert.IsTrue(railGun.IsInArc(targetObject.transform.position), "RailGun: Target to side should be in 360° arc");
        Assert.IsFalse(cannon.IsInArc(targetObject.transform.position), "Cannon: Target to side should NOT be in 60° forward arc");

        // Position target at 45° forward-right (7, 0, 7) - outside 60° arc (30° each side)
        targetObject.transform.position = new Vector3(7f, 0f, 7f);
        Assert.IsTrue(railGun.IsInArc(targetObject.transform.position), "RailGun: Target at 45° should be in 360° arc");
        Assert.IsFalse(cannon.IsInArc(targetObject.transform.position), "Cannon: Target at 45° should NOT be in 60° arc");

        // Position target at 20° forward-right (~3.4, 0, 10) - inside 60° arc
        targetObject.transform.position = new Vector3(3.4f, 0f, 10f);
        Assert.IsTrue(cannon.IsInArc(targetObject.transform.position), "Cannon: Target at ~20° should be in 60° arc");
    }

    /// <summary>
    /// Test 3: Test IsInRange() with various distances.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_RangeCheck()
    {
        yield return null;

        // Target at 10 units - within range of both
        targetObject.transform.position = new Vector3(0f, 0f, 10f);
        Assert.IsTrue(railGun.IsInRange(targetObject.transform.position), "RailGun: 10 units should be within 30 unit range");
        Assert.IsTrue(cannon.IsInRange(targetObject.transform.position), "Cannon: 10 units should be within 20 unit range");

        // Target at 25 units - within RailGun range, outside Cannon range
        targetObject.transform.position = new Vector3(0f, 0f, 25f);
        Assert.IsTrue(railGun.IsInRange(targetObject.transform.position), "RailGun: 25 units should be within 30 unit range");
        Assert.IsFalse(cannon.IsInRange(targetObject.transform.position), "Cannon: 25 units should be outside 20 unit range");

        // Target at 35 units - outside both ranges
        targetObject.transform.position = new Vector3(0f, 0f, 35f);
        Assert.IsFalse(railGun.IsInRange(targetObject.transform.position), "RailGun: 35 units should be outside 30 unit range");
        Assert.IsFalse(cannon.IsInRange(targetObject.transform.position), "Cannon: 35 units should be outside 20 unit range");

        // Target at exact range boundary (30 for RailGun)
        // Note: RailGun hardpoint is at (1, 0, 0), so we position target to be exactly 30 units away
        // Target at (1, 0, 30) is exactly 30 units from hardpoint at (1, 0, 0)
        targetObject.transform.position = new Vector3(1f, 0f, 30f);
        Assert.IsTrue(railGun.IsInRange(targetObject.transform.position), "RailGun: 30 units should be at max range");
    }

    /// <summary>
    /// Test 4: Verify CanFire() conditions checked correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_CanFire()
    {
        yield return null;

        // No target assigned - cannot fire
        Assert.IsFalse(railGun.CanFire(), "Cannot fire without target");

        // Assign target - can fire
        railGun.SetTarget(targetShip);
        Assert.IsTrue(railGun.CanFire(), "Should be able to fire with valid target in arc and range");

        // Move target out of range
        targetObject.transform.position = new Vector3(0f, 0f, 50f);
        Assert.IsFalse(railGun.CanFire(), "Cannot fire when target out of range");

        // Move target back in range but out of arc (for cannon)
        targetObject.transform.position = new Vector3(0f, 0f, -10f);
        cannon.SetTarget(targetShip);
        Assert.IsFalse(cannon.CanFire(), "Cannon cannot fire when target behind (out of 60° arc)");

        // Kill target - cannot fire
        targetObject.transform.position = new Vector3(0f, 0f, 10f);
        // Deal enough damage to deplete shields (200) and hull (500) = 700 total
        // Expect the death warning log
        UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, "TargetShip has been destroyed!");
        targetShip.TakeDamage(targetShip.CurrentShields + targetShip.CurrentHull);
        Assert.IsTrue(targetShip.IsDead, "Target should be dead");
        Assert.IsFalse(railGun.CanFire(), "Cannot fire at dead target");
    }

    /// <summary>
    /// Test 5: Assign weapon to group, verify stored.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_GroupAssignment()
    {
        yield return null;

        // Verify initial state (group 0 = unassigned)
        Assert.AreEqual(0, railGun.AssignedGroup, "RailGun should start in group 0");
        Assert.AreEqual(0, cannon.AssignedGroup, "Cannon should start in group 0");

        // Assign RailGun to group 1
        weaponManager.AssignWeaponToGroup(railGun, 1);
        Assert.AreEqual(1, railGun.AssignedGroup, "RailGun should be in group 1");

        // Verify group 1 contains RailGun
        var group1Weapons = weaponManager.GetWeaponsInGroup(1);
        Assert.AreEqual(1, group1Weapons.Count, "Group 1 should have 1 weapon");
        Assert.Contains(railGun, group1Weapons, "Group 1 should contain RailGun");

        // Assign Cannon to group 2
        weaponManager.AssignWeaponToGroup(cannon, 2);
        Assert.AreEqual(2, cannon.AssignedGroup, "Cannon should be in group 2");

        // Verify group 2 contains Cannon
        var group2Weapons = weaponManager.GetWeaponsInGroup(2);
        Assert.AreEqual(1, group2Weapons.Count, "Group 2 should have 1 weapon");
        Assert.Contains(cannon, group2Weapons, "Group 2 should contain Cannon");

        // Reassign RailGun to group 2 (should move from group 1)
        weaponManager.AssignWeaponToGroup(railGun, 2);
        Assert.AreEqual(2, railGun.AssignedGroup, "RailGun should now be in group 2");
        Assert.AreEqual(0, weaponManager.GetWeaponsInGroup(1).Count, "Group 1 should be empty");
        Assert.AreEqual(2, weaponManager.GetWeaponsInGroup(2).Count, "Group 2 should have 2 weapons");
    }

    /// <summary>
    /// Test 6: Fire rail gun, verify fast projectile hits target.
    /// RailGun fires a fast ballistic projectile (40 units/sec).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_RailGunFastProjectile()
    {
        yield return null;

        railGun.SetTarget(targetShip);
        float initialShields = targetShip.CurrentShields;

        // Fire weapon
        yield return railGun.FireWithSpinUp();

        // Wait for projectile to travel and hit (40 units/sec at 10 unit distance = 0.25 sec)
        yield return new WaitForSeconds(0.5f);

        // Verify damage was dealt - this confirms projectile spawned and hit
        Assert.Less(targetShip.CurrentShields, initialShields, "Target shields should decrease from RailGun projectile hit");
        Assert.AreEqual(initialShields - 20f, targetShip.CurrentShields, 0.01f, "Should deal exactly 20 damage");
    }

    /// <summary>
    /// Test 7: Fire cannon, verify projectile spawn called.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_CannonBallisticSpawn()
    {
        yield return null;

        cannon.SetTarget(targetShip);

        // Fire weapon (will call ProjectileManager.SpawnBallisticProjectile stub)
        // Note: Since ProjectileManager is a stub, we just verify it doesn't error
        yield return cannon.FireWithSpinUp();

        // Verify GetProjectileInfo returns correct type
        var info = cannon.GetProjectileInfo();
        Assert.AreEqual(WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic, info.Type, "Cannon should spawn ballistic projectile");
        Assert.AreEqual(40f, info.Damage, "Projectile should have 40 damage");
        Assert.AreEqual(15f, info.Speed, "Projectile should have 15 units/sec speed");
        Assert.AreEqual(targetShip, info.TargetShip, "Projectile should target correct ship");
    }

    /// <summary>
    /// Test 8: Fire weapon, tick cooldown, verify decreases.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponCooldown()
    {
        yield return null;

        // Note: RailGun and Cannon have 0 max cooldown
        // We'll manually test the cooldown system by setting it

        // Manually start cooldown for testing
        railGun.StartCooldown();
        Assert.AreEqual(0, railGun.CurrentCooldown, "RailGun has 0 max cooldown, so current should be 0");

        // Create a test scenario: modify MaxCooldown via reflection for testing
        var maxCooldownField = typeof(WeaponSystem).GetField("maxCooldown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (maxCooldownField != null)
        {
            maxCooldownField.SetValue(railGun, 3); // Set to 3 turns for testing

            railGun.StartCooldown();
            Assert.AreEqual(3, railGun.CurrentCooldown, "Current cooldown should be 3");

            // Tick once
            railGun.TickCooldown();
            Assert.AreEqual(2, railGun.CurrentCooldown, "Cooldown should decrease to 2");

            // Tick again
            railGun.TickCooldown();
            Assert.AreEqual(1, railGun.CurrentCooldown, "Cooldown should decrease to 1");

            // Tick to zero
            railGun.TickCooldown();
            Assert.AreEqual(0, railGun.CurrentCooldown, "Cooldown should reach 0");

            // Tick when already zero (should stay zero)
            railGun.TickCooldown();
            Assert.AreEqual(0, railGun.CurrentCooldown, "Cooldown should stay at 0");

            // Reset for other tests
            maxCooldownField.SetValue(railGun, 0);
        }
    }

    /// <summary>
    /// Test 9: Fire weapon, verify heat added to ship.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponHeatCost()
    {
        yield return null;

        railGun.SetTarget(targetShip);
        float initialHeat = ship.HeatManager.CurrentHeat;

        // Fire RailGun (15 heat cost)
        yield return railGun.FireWithSpinUp();

        Assert.AreEqual(initialHeat + 15f, ship.HeatManager.CurrentHeat, 0.01f, "RailGun should generate 15 heat");

        // Fire Cannon (30 heat cost)
        cannon.SetTarget(targetShip);
        yield return cannon.FireWithSpinUp();

        Assert.AreEqual(initialHeat + 15f + 30f, ship.HeatManager.CurrentHeat, 0.01f, "Cannon should add 30 heat (total 45)");
    }

    /// <summary>
    /// Test 10: Multiple weapons in group, verify total heat.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponGroupHeatCalculation()
    {
        yield return null;

        // Assign both weapons to group 1
        weaponManager.AssignWeaponToGroup(railGun, 1);
        weaponManager.AssignWeaponToGroup(cannon, 1);

        // Set targets so CanFire returns true
        railGun.SetTarget(targetShip);
        cannon.SetTarget(targetShip);

        // Calculate group heat cost
        int groupHeat = weaponManager.CalculateGroupHeatCost(1);

        // RailGun (15) + Cannon (30) = 45
        Assert.AreEqual(45, groupHeat, "Group 1 should cost 45 heat total");
    }

    /// <summary>
    /// Test 11: Fire weapon, verify Fire() called after SpinUpTime.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SpinUpDelay()
    {
        yield return null;

        // Deplete shields first so damage goes to hull
        targetShip.TakeDamage(targetShip.CurrentShields);
        Assert.AreEqual(0f, targetShip.CurrentShields, "Shields should be depleted");

        railGun.SetTarget(targetShip);
        float initialHull = targetShip.CurrentHull;

        // Start firing (spin-up = 0.2s for RailGun)
        float startTime = Time.time;
        var fireCoroutine = railGun.FireWithSpinUp();

        // Check damage hasn't been applied yet
        yield return null;
        Assert.AreEqual(initialHull, targetShip.CurrentHull, "Damage should not be applied immediately");

        // Wait for spin-up to complete
        yield return fireCoroutine;

        // Verify spin-up time elapsed
        float elapsed = Time.time - startTime;
        Assert.GreaterOrEqual(elapsed, 0.2f, "Spin-up should take at least 0.2 seconds");

        // Wait for projectile to hit (RailGun now fires projectile at 40 units/sec)
        yield return new WaitForSeconds(0.5f);

        // Verify damage applied after spin-up + projectile travel
        Assert.Less(targetShip.CurrentHull, initialHull, "Damage should be applied after spin-up");
    }

    /// <summary>
    /// Test 12: Set multipliers, fire weapon, verify modified damage/heat.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_OverchargeMultiplier()
    {
        yield return null;

        // Deplete shields first so damage goes to hull
        targetShip.TakeDamage(targetShip.CurrentShields);
        Assert.AreEqual(0f, targetShip.CurrentShields, "Shields should be depleted");

        railGun.SetTarget(targetShip);
        float initialHull = targetShip.CurrentHull;
        float initialHeat = ship.HeatManager.CurrentHeat;

        // Set Overcharge multipliers (2x damage, 2x heat)
        ship.WeaponDamageMultiplier = 2.0f;
        ship.WeaponHeatMultiplier = 2.0f;

        // Fire weapon
        yield return railGun.FireWithSpinUp();

        // Wait for projectile to hit (RailGun fires 40 units/sec projectile)
        yield return new WaitForSeconds(0.5f);

        // Verify 2x damage (20 * 2 = 40)
        Assert.AreEqual(initialHull - 40f, targetShip.CurrentHull, 0.01f, "Should deal 40 damage with 2x multiplier");

        // Verify 2x heat (15 * 2 = 30)
        Assert.AreEqual(initialHeat + 30f, ship.HeatManager.CurrentHeat, 0.01f, "Should generate 30 heat with 2x multiplier");

        // Reset multipliers
        ship.WeaponDamageMultiplier = 1.0f;
        ship.WeaponHeatMultiplier = 1.0f;

        // Fire again with normal multipliers
        yield return new WaitForSeconds(0.3f);
        yield return railGun.FireWithSpinUp();

        // Wait for projectile to hit
        yield return new WaitForSeconds(0.5f);

        // Verify normal damage (40 more damage)
        Assert.AreEqual(initialHull - 40f - 20f, targetShip.CurrentHull, 0.01f, "Should deal 20 damage with 1x multiplier");
    }
}
