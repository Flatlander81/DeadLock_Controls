using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for Phase 2 - Projectile System (Track B).
/// Tests projectile spawning, movement, collision, and pooling.
/// </summary>
public class ProjectileSystemTests
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
        // Reset any existing ProjectileManager singleton to ensure clean state
        ProjectileManager.ResetInstance();

        // Create ProjectileManager
        managerObject = new GameObject("ProjectileManager");
        manager = managerObject.AddComponent<ProjectileManager>();

        // Create owner ship
        shipObject = new GameObject("OwnerShip");
        ship = shipObject.AddComponent<Ship>();
        shipObject.AddComponent<HeatManager>();
        shipObject.transform.position = Vector3.zero;

        // Add collider to ship for projectile collision
        BoxCollider shipCollider = shipObject.AddComponent<BoxCollider>();
        shipCollider.size = new Vector3(2f, 1f, 4f);

        // Create target ship
        targetObject = new GameObject("TargetShip");
        targetShip = targetObject.AddComponent<Ship>();
        targetObject.AddComponent<HeatManager>();
        targetObject.transform.position = new Vector3(0f, 0f, 10f);

        // Add collider to target
        BoxCollider targetCollider = targetObject.AddComponent<BoxCollider>();
        targetCollider.size = new Vector3(2f, 1f, 4f);
    }

    [TearDown]
    public void Teardown()
    {
        // Reset the ProjectileManager singleton to clean up all projectiles
        ProjectileManager.ResetInstance();

        if (managerObject != null) Object.DestroyImmediate(managerObject);
        if (shipObject != null) Object.DestroyImmediate(shipObject);
        if (targetObject != null) Object.DestroyImmediate(targetObject);

        // Clean up any remaining projectiles that might have been orphaned
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in projectiles)
        {
            if (obj.GetComponent<Projectile>() != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    /// <summary>
    /// Test 1: Spawn ballistic, verify created and moving.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_BallisticSpawn()
    {
        yield return null; // Wait for initialization

        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
            SpawnPosition = Vector3.zero,
            SpawnRotation = Quaternion.identity,
            TargetPosition = new Vector3(0f, 0f, 10f),
            TargetShip = targetShip,
            Damage = 40f,
            Speed = 5f,
            OwnerShip = ship
        };

        ProjectileManager.SpawnBallisticProjectile(info);

        yield return null; // Wait one frame

        // Verify projectile spawned
        int activeCount = ProjectileManager.GetActiveProjectileCount();
        Assert.AreEqual(1, activeCount, "Should have 1 active projectile");
    }

    /// <summary>
    /// Test 2: Spawn ballistic, verify travels straight.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_BallisticTrajectory()
    {
        yield return null;

        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
            SpawnPosition = Vector3.zero,
            SpawnRotation = Quaternion.LookRotation(Vector3.forward),
            TargetPosition = new Vector3(0f, 0f, 10f),
            TargetShip = targetShip,
            Damage = 40f,
            Speed = 5f,
            OwnerShip = ship
        };

        ProjectileManager.SpawnBallisticProjectile(info);
        yield return null;

        // Find the spawned projectile
        BallisticProjectile projectile = Object.FindFirstObjectByType<BallisticProjectile>();
        Assert.IsNotNull(projectile, "Ballistic projectile should exist");

        Vector3 startPos = projectile.transform.position;

        // Wait a bit for movement
        yield return new WaitForSeconds(0.5f);

        Vector3 endPos = projectile.transform.position;

        // Verify projectile moved forward
        Assert.Greater(endPos.z, startPos.z, "Projectile should move forward (z+)");

        // Verify straight trajectory (no lateral movement)
        Assert.AreEqual(startPos.x, endPos.x, 0.1f, "Projectile should not move laterally (x)");
        Assert.AreEqual(startPos.y, endPos.y, 0.1f, "Projectile should not move vertically (y)");
    }

    /// <summary>
    /// Test 3: Spawn at target, verify collision detected.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_BallisticCollision()
    {
        yield return null;

        // Deplete target shields so damage goes to hull
        targetShip.TakeDamage(targetShip.CurrentShields);

        float initialHull = targetShip.CurrentHull;

        // Spawn projectile very close to target
        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
            SpawnPosition = new Vector3(0f, 0f, 9f), // 1 unit from target
            SpawnRotation = Quaternion.LookRotation(Vector3.forward),
            TargetPosition = targetObject.transform.position,
            TargetShip = targetShip,
            Damage = 40f,
            Speed = 5f,
            OwnerShip = ship
        };

        ProjectileManager.SpawnBallisticProjectile(info);

        // Wait for collision
        yield return new WaitForSeconds(0.3f);

        // Verify damage applied
        Assert.Less(targetShip.CurrentHull, initialHull, "Target should take damage from collision");
    }

    /// <summary>
    /// Test 4: Projectile hits ship, verify damage applied.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_BallisticDamageApplication()
    {
        yield return null;

        // Deplete shields
        targetShip.TakeDamage(targetShip.CurrentShields);
        float initialHull = targetShip.CurrentHull;

        // Spawn close to target
        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
            SpawnPosition = new Vector3(0f, 0f, 8f),
            SpawnRotation = Quaternion.LookRotation(Vector3.forward),
            TargetPosition = targetObject.transform.position,
            TargetShip = targetShip,
            Damage = 50f, // Specific damage amount
            Speed = 10f,
            OwnerShip = ship
        };

        ProjectileManager.SpawnBallisticProjectile(info);

        // Wait for hit
        yield return new WaitForSeconds(0.3f);

        // Verify correct damage amount
        float expectedHull = initialHull - 50f;
        Assert.AreEqual(expectedHull, targetShip.CurrentHull, 1f, "Should deal exactly 50 damage");
    }

    /// <summary>
    /// Test 5: Wait lifetime, verify auto-destroyed.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_BallisticLifetimeExpiry()
    {
        yield return null;

        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
            SpawnPosition = Vector3.zero,
            SpawnRotation = Quaternion.LookRotation(Vector3.up), // Shoot upward to miss target
            TargetPosition = new Vector3(0f, 100f, 0f),
            TargetShip = null,
            Damage = 40f,
            Speed = 1f,
            OwnerShip = ship
        };

        ProjectileManager.SpawnBallisticProjectile(info);
        yield return null;

        // Verify projectile exists
        BallisticProjectile projectile = Object.FindFirstObjectByType<BallisticProjectile>();
        Assert.IsNotNull(projectile, "Projectile should exist after spawn");

        // Manually set short lifetime for testing
        var lifetimeField = typeof(Projectile).GetField("lifetime",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (lifetimeField != null)
        {
            lifetimeField.SetValue(projectile, 0.5f); // 0.5 second lifetime
        }

        // Wait for lifetime to expire
        yield return new WaitForSeconds(0.6f);

        // Verify projectile returned to pool (inactive)
        Assert.IsFalse(projectile.IsActive, "Projectile should be inactive after lifetime expires");
        Assert.IsFalse(projectile.gameObject.activeSelf, "Projectile GameObject should be deactivated");
    }

    /// <summary>
    /// Test 6: Spawn homing with target, verify created.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HomingSpawn()
    {
        yield return null;

        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Homing,
            SpawnPosition = Vector3.zero,
            SpawnRotation = Quaternion.identity,
            TargetPosition = targetObject.transform.position,
            TargetShip = targetShip,
            Damage = 60f,
            Speed = 3f,
            OwnerShip = ship
        };

        ProjectileManager.SpawnHomingProjectile(info);
        yield return null;

        // Verify homing projectile spawned
        HomingProjectile projectile = Object.FindFirstObjectByType<HomingProjectile>();
        Assert.IsNotNull(projectile, "Homing projectile should exist");
        Assert.AreEqual(targetShip, projectile.TargetShip, "Target should be assigned");
        Assert.IsTrue(projectile.IsHoming, "Should be in homing mode");
    }

    /// <summary>
    /// Test 7: Spawn homing, move target, verify projectile turns.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HomingSeeks()
    {
        yield return null;

        // Spawn homing projectile
        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Homing,
            SpawnPosition = Vector3.zero,
            SpawnRotation = Quaternion.LookRotation(Vector3.forward),
            TargetPosition = targetObject.transform.position,
            TargetShip = targetShip,
            Damage = 60f,
            Speed = 2f,
            OwnerShip = ship
        };

        ProjectileManager.SpawnHomingProjectile(info);
        yield return null;

        HomingProjectile projectile = Object.FindFirstObjectByType<HomingProjectile>();
        Assert.IsNotNull(projectile, "Homing projectile should exist");

        // Move target to the side
        targetObject.transform.position = new Vector3(10f, 0f, 10f);

        // Wait for homing to adjust
        yield return new WaitForSeconds(0.5f);

        // Verify projectile rotated toward new target position
        Vector3 directionToTarget = (targetObject.transform.position - projectile.transform.position).normalized;
        float angle = Vector3.Angle(projectile.transform.forward, directionToTarget);

        Assert.Less(angle, 45f, "Homing projectile should rotate toward target (angle should be small)");
    }

    /// <summary>
    /// Test 8: Spawn homing at moving target, verify hit.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HomingHitMovingTarget()
    {
        yield return null;

        // Deplete shields
        targetShip.TakeDamage(targetShip.CurrentShields);
        float initialHull = targetShip.CurrentHull;

        // Spawn homing close to target
        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Homing,
            SpawnPosition = new Vector3(0f, 0f, 5f),
            SpawnRotation = Quaternion.LookRotation(Vector3.forward),
            TargetPosition = targetObject.transform.position,
            TargetShip = targetShip,
            Damage = 60f,
            Speed = 10f, // Fast homing
            OwnerShip = ship
        };

        ProjectileManager.SpawnHomingProjectile(info);
        yield return null;

        // Wait for impact
        yield return new WaitForSeconds(1f);

        // Verify damage applied
        Assert.Less(targetShip.CurrentHull, initialHull, "Homing projectile should hit target");
    }

    /// <summary>
    /// Test 9: Destroy target mid-flight, verify continues ballistic.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HomingTargetDestroyed()
    {
        yield return null;

        // Spawn homing projectile
        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Homing,
            SpawnPosition = Vector3.zero,
            SpawnRotation = Quaternion.LookRotation(Vector3.forward),
            TargetPosition = targetObject.transform.position,
            TargetShip = targetShip,
            Damage = 60f,
            Speed = 2f,
            OwnerShip = ship
        };

        ProjectileManager.SpawnHomingProjectile(info);
        yield return new WaitForSeconds(0.1f);

        HomingProjectile projectile = Object.FindFirstObjectByType<HomingProjectile>();
        Assert.IsNotNull(projectile, "Homing projectile should exist");
        Assert.IsTrue(projectile.IsHoming, "Should start in homing mode");

        // Destroy target
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "TargetShip has been destroyed!");
        targetShip.TakeDamage(targetShip.CurrentShields + targetShip.CurrentHull);

        // Wait for homing to detect target loss
        yield return new WaitForSeconds(0.2f);

        // Verify switched to ballistic mode
        Assert.IsFalse(projectile.IsHoming, "Should switch to ballistic mode when target destroyed");
    }

    /// <summary>
    /// Test 10: Spawn instant hit, verify tracer created and fades.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_InstantHitEffect()
    {
        yield return null;

        Vector3 start = Vector3.zero;
        Vector3 end = new Vector3(0f, 0f, 10f);

        ProjectileManager.SpawnInstantHitEffect(start, end, 20f);
        yield return null;

        // Verify effect created
        InstantHitEffect effect = Object.FindFirstObjectByType<InstantHitEffect>();
        Assert.IsNotNull(effect, "Instant hit effect should be created");
        Assert.IsTrue(effect.IsActive, "Effect should be active");

        // Wait for fade
        yield return new WaitForSeconds(effect.FadeOutDuration + 0.1f);

        // Verify effect inactive (returned to pool)
        Assert.IsFalse(effect.IsActive, "Effect should be inactive after fade");
        Assert.IsFalse(effect.gameObject.activeSelf, "Effect GameObject should be deactivated");
    }

    /// <summary>
    /// Test 11: Spawn 10, destroy 10, spawn 10 more, verify reused.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ProjectilePooling()
    {
        yield return null;

        // Spawn 10 ballistic projectiles
        for (int i = 0; i < 10; i++)
        {
            var info = new WeaponSystem.ProjectileSpawnInfo
            {
                Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
                SpawnPosition = new Vector3(i, 0f, 0f),
                SpawnRotation = Quaternion.identity,
                TargetPosition = Vector3.forward * 10f,
                TargetShip = null,
                Damage = 10f,
                Speed = 1f,
                OwnerShip = ship
            };
            ProjectileManager.SpawnBallisticProjectile(info);
        }

        yield return null;

        // Verify 10 active
        Assert.AreEqual(10, ProjectileManager.GetActiveProjectileCount(), "Should have 10 active projectiles");

        // Clear all projectiles (return to pool)
        ProjectileManager.ClearAllProjectiles();
        yield return null;

        // Verify all returned to pool
        Assert.AreEqual(0, ProjectileManager.GetActiveProjectileCount(), "Should have 0 active projectiles");

        int pooledCount = ProjectileManager.GetPooledProjectileCount();
        Assert.GreaterOrEqual(pooledCount, 10, "Pool should have at least 10 projectiles");

        // Spawn 10 more (should reuse pooled)
        for (int i = 0; i < 10; i++)
        {
            var info = new WeaponSystem.ProjectileSpawnInfo
            {
                Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
                SpawnPosition = new Vector3(i, 0f, 0f),
                SpawnRotation = Quaternion.identity,
                TargetPosition = Vector3.forward * 10f,
                TargetShip = null,
                Damage = 10f,
                Speed = 1f,
                OwnerShip = ship
            };
            ProjectileManager.SpawnBallisticProjectile(info);
        }

        yield return null;

        // Verify 10 active again (reused from pool)
        Assert.AreEqual(10, ProjectileManager.GetActiveProjectileCount(), "Should have 10 active projectiles (reused)");
    }

    /// <summary>
    /// Test 12: Spawn projectile, verify doesn't hit owner ship.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_NoFriendlyFire()
    {
        yield return null;

        float initialHull = ship.CurrentHull;

        // Spawn projectile pointing backward (toward owner)
        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
            SpawnPosition = new Vector3(0f, 0f, 1f), // In front of owner
            SpawnRotation = Quaternion.LookRotation(Vector3.back), // Pointing backward
            TargetPosition = Vector3.zero,
            TargetShip = ship, // Target is owner (should not hit)
            Damage = 50f,
            Speed = 5f,
            OwnerShip = ship
        };

        ProjectileManager.SpawnBallisticProjectile(info);

        // Wait for potential collision
        yield return new WaitForSeconds(0.5f);

        // Verify owner took no damage
        Assert.AreEqual(initialHull, ship.CurrentHull, 0.1f, "Owner ship should not take damage from own projectile");
    }
}
