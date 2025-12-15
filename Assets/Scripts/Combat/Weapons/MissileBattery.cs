using UnityEngine;

/// <summary>
/// Missile Battery - Fast homing projectile weapon with turret mount.
/// High rate of fire, 360-degree coverage, moderate damage per missile.
/// </summary>
public class MissileBattery : WeaponSystem
{
    [Header("Missile Settings")]
    [SerializeField] private float missileSpeed = 15f; // units per second
    [SerializeField] private float missileTurnRate = 90f; // degrees per second

    // Public properties for projectile spawning
    public float MissileSpeed => missileSpeed;
    public float MissileTurnRate => missileTurnRate;

    /// <summary>
    /// Initialize missile battery with default stats.
    /// </summary>
    private void Awake()
    {
        weaponName = "Missile Battery";
        damage = 30f;
        heatCost = 20;
        firingArc = 360f; // Turret mount - full coverage
        maxRange = 35f;
        maxCooldown = 1; // 1 turn cooldown
        spinUpTime = 0.4f;
        ammoCapacity = 20; // Limited ammo
        missileSpeed = 15f;
        missileTurnRate = 90f;
    }

    /// <summary>
    /// Fire the missile battery - spawn homing projectile.
    /// Note: Ammo consumption and cooldown handled by base class FireWithSpinUp().
    /// </summary>
    protected override void Fire()
    {
        if (assignedTarget == null)
        {
            Debug.LogWarning($"{weaponName}: No target to fire at");
            return;
        }

        // Get projectile info
        ProjectileSpawnInfo info = GetProjectileInfo();

        // Spawn homing projectile via ProjectileManager
        ProjectileManager.SpawnHomingProjectile(info);

        Debug.Log($"{weaponName} fired! Missile tracking {assignedTarget.gameObject.name} " +
                  $"(Ammo remaining: {currentAmmo - 1})");
    }

    /// <summary>
    /// Get projectile spawn info for homing missile.
    /// </summary>
    public override ProjectileSpawnInfo GetProjectileInfo()
    {
        if (assignedTarget == null)
        {
            return default;
        }

        // Apply damage multiplier from ship (Overcharge ability)
        float actualDamage = damage;
        if (ownerShip != null)
        {
            actualDamage *= ownerShip.WeaponDamageMultiplier;
        }

        // Calculate rotation to face target initially
        Vector3 directionToTarget = (assignedTarget.transform.position - hardpointTransform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        return new ProjectileSpawnInfo
        {
            Type = ProjectileSpawnInfo.ProjectileType.Homing,
            SpawnPosition = hardpointTransform.position,
            SpawnRotation = targetRotation,
            TargetPosition = assignedTarget.transform.position,
            TargetShip = assignedTarget,
            Damage = actualDamage,
            Speed = missileSpeed,
            OwnerShip = ownerShip
        };
    }

    /// <summary>
    /// Draw gizmos showing turret coverage.
    /// </summary>
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw missile tracking line if we have a target
        if (assignedTarget != null && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hardpointTransform.position, assignedTarget.transform.position);
            Gizmos.DrawWireSphere(assignedTarget.transform.position, 0.5f);
        }
    }
}
