using UnityEngine;

/// <summary>
/// Rail Gun weapon - Instant-hit energy weapon with no travel time.
/// High precision, no cooldown, 360-degree turret.
/// </summary>
public class RailGun : WeaponSystem
{
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
    }

    /// <summary>
    /// Fire the rail gun - instant hit with no projectile travel.
    /// </summary>
    protected override void Fire()
    {
        if (assignedTarget == null)
        {
            Debug.LogWarning($"{weaponName}: No target to fire at");
            return;
        }

        // Apply damage multiplier from ship (Overcharge ability)
        float actualDamage = damage;
        if (ownerShip != null)
        {
            actualDamage *= ownerShip.WeaponDamageMultiplier;
        }

        // Instant hit - apply damage immediately
        assignedTarget.TakeDamage(actualDamage);

        Debug.Log($"{weaponName} fired! Hit {assignedTarget.gameObject.name} for {actualDamage} damage (instant hit)");

        // Spawn visual tracer effect via ProjectileManager (Track B)
        ProjectileManager.SpawnInstantHitEffect(
            hardpointTransform.position,
            assignedTarget.transform.position,
            actualDamage
        );
    }

    /// <summary>
    /// Get projectile spawn info for Track B.
    /// </summary>
    public override ProjectileSpawnInfo GetProjectileInfo()
    {
        float actualDamage = damage;
        if (ownerShip != null)
        {
            actualDamage *= ownerShip.WeaponDamageMultiplier;
        }

        return new ProjectileSpawnInfo
        {
            Type = ProjectileSpawnInfo.ProjectileType.InstantHit,
            SpawnPosition = hardpointTransform.position,
            SpawnRotation = hardpointTransform.rotation,
            TargetPosition = assignedTarget != null ? assignedTarget.transform.position : Vector3.zero,
            TargetShip = assignedTarget,
            Damage = actualDamage,
            Speed = 0f, // Instant hit - no travel time
            OwnerShip = ownerShip
        };
    }
}
