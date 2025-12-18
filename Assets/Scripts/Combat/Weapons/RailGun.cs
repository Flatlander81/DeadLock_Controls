using UnityEngine;

/// <summary>
/// Rail Gun weapon - Ultra-fast ballistic projectile weapon.
/// High precision, visible projectile but nearly impossible to dodge.
/// 90-degree firing arc, infinite ammo.
/// </summary>
public class RailGun : WeaponSystem
{
    [Header("RailGun Settings")]
    [SerializeField] private float projectileSpeed = 40f; // Fast but visible

    /// <summary>
    /// Initialize rail gun with default stats.
    /// </summary>
    private void Awake()
    {
        weaponName = "Rail Gun";
        damage = 20f;
        heatCost = 15;
        firingArc = 360f; // Turret - can fire in any direction
        maxRange = 30f;
        maxCooldown = 0; // No cooldown
        spinUpTime = 0.2f;
        ammoCapacity = 0; // Infinite ammo
        projectileSpeed = 40f; // 40 units/sec - fast but visible (~0.5s to reach 20 unit target)
    }

    /// <summary>
    /// Fire the rail gun - spawns ultra-fast ballistic projectile.
    /// </summary>
    protected override void Fire()
    {
        if (assignedTarget == null)
        {
            Debug.LogWarning($"{weaponName}: No target to fire at");
            return;
        }

        // Get projectile info with lead calculation
        ProjectileSpawnInfo info = GetProjectileInfo();

        // Spawn fast ballistic projectile via ProjectileManager
        ProjectileManager.SpawnBallisticProjectile(info);

        Debug.Log($"{weaponName} fired! Fast projectile toward {assignedTarget.gameObject.name} at {projectileSpeed} units/sec");
    }

    /// <summary>
    /// Get projectile spawn info for Track B.
    /// Uses base class helper for common ballistic projectile calculations.
    /// </summary>
    public override ProjectileSpawnInfo GetProjectileInfo()
    {
        return CreateBallisticProjectileInfo(projectileSpeed);
    }

}
