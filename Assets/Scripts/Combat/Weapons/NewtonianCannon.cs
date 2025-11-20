using UnityEngine;

/// <summary>
/// Newtonian Cannon - Ballistic projectile weapon with travel time.
/// Requires lead calculation for moving targets, forward firing arc only.
/// </summary>
public class NewtonianCannon : WeaponSystem
{
    [Header("Cannon Settings")]
    [SerializeField] private float projectileSpeed = 2f; // units per second

    /// <summary>
    /// Initialize cannon with default stats.
    /// </summary>
    private void Awake()
    {
        weaponName = "Newtonian Cannon";
        damage = 40f;
        heatCost = 30;
        firingArc = 180f; // Forward hemisphere only
        maxRange = 20f;
        maxCooldown = 0; // No cooldown
        spinUpTime = 0.5f;
        ammoCapacity = 0; // Infinite ammo
        projectileSpeed = 2f;
    }

    /// <summary>
    /// Fire the cannon - spawn ballistic projectile.
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

        // Spawn ballistic projectile via ProjectileManager (Track B)
        ProjectileManager.SpawnBallisticProjectile(info);

        Debug.Log($"{weaponName} fired! Ballistic projectile toward {assignedTarget.gameObject.name} " +
                  $"(lead target at {info.TargetPosition})");
    }

    /// <summary>
    /// Get projectile spawn info with lead calculation for moving targets.
    /// </summary>
    public override ProjectileSpawnInfo GetProjectileInfo()
    {
        if (assignedTarget == null)
        {
            return default;
        }

        // Calculate lead position for moving target
        Vector3 targetPosition = CalculateLeadPosition();

        // Apply damage multiplier from ship (Overcharge ability)
        float actualDamage = damage;
        if (ownerShip != null)
        {
            actualDamage *= ownerShip.WeaponDamageMultiplier;
        }

        // Calculate rotation to face target
        Vector3 directionToTarget = (targetPosition - hardpointTransform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        return new ProjectileSpawnInfo
        {
            Type = ProjectileSpawnInfo.ProjectileType.Ballistic,
            SpawnPosition = hardpointTransform.position,
            SpawnRotation = targetRotation,
            TargetPosition = targetPosition,
            TargetShip = assignedTarget,
            Damage = actualDamage,
            Speed = projectileSpeed,
            OwnerShip = ownerShip
        };
    }

    /// <summary>
    /// Calculate lead position for moving target.
    /// Estimates where target will be when projectile arrives.
    /// </summary>
    private Vector3 CalculateLeadPosition()
    {
        Vector3 targetCurrentPos = assignedTarget.transform.position;

        // Get target velocity (if Ship component exists)
        Vector3 targetVelocity = Vector3.zero;
        Ship targetShip = assignedTarget;
        if (targetShip != null)
        {
            // Estimate velocity based on ship's forward direction and speed
            // This is approximate - Track C may provide better velocity data
            targetVelocity = targetShip.transform.forward * targetShip.CurrentSpeed;
        }

        // Calculate time to impact (simple approximation)
        float distanceToTarget = Vector3.Distance(hardpointTransform.position, targetCurrentPos);
        float timeToImpact = distanceToTarget / projectileSpeed;

        // Calculate lead position
        Vector3 leadPosition = targetCurrentPos + (targetVelocity * timeToImpact);

        return leadPosition;
    }

    /// <summary>
    /// Draw gizmos showing lead calculation.
    /// </summary>
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw lead position if we have a target
        if (assignedTarget != null && Application.isPlaying)
        {
            Vector3 leadPos = CalculateLeadPosition();
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(leadPos, 0.5f);
            Gizmos.DrawLine(hardpointTransform.position, leadPos);
        }
    }
}
