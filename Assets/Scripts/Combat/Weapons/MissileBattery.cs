using UnityEngine;

/// <summary>
/// Missile Battery - Fast homing projectile weapon with broadside firing.
/// 360° coverage, fires volleys from the side of the ship closest to the target.
/// Moderate damage per missile, limited ammo.
/// </summary>
public class MissileBattery : WeaponSystem
{
    [Header("Missile Settings")]
    [SerializeField] private float missileSpeed = 15f; // units per second
    [SerializeField] private float missileTurnRate = 90f; // degrees per second

    [Header("Broadside Hardpoints")]
    [SerializeField] private Transform portHardpoint;      // Left side launch point
    [SerializeField] private Transform starboardHardpoint; // Right side launch point

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
        firingArc = 360f; // Full arc - broadside weapon can fire at any target
        maxRange = 35f;
        maxCooldown = 1; // 1 turn cooldown
        spinUpTime = 0.4f;
        ammoCapacity = 20; // Limited ammo
        missileSpeed = 15f;
        missileTurnRate = 90f;
    }

    /// <summary>
    /// Fire the missile battery - spawn homing projectile volley from broadside.
    /// Note: Ammo consumption and cooldown handled by base class FireWithSpinUp().
    /// </summary>
    protected override void Fire()
    {
        if (assignedTarget == null)
        {
            Debug.LogWarning($"{weaponName}: No target to fire at");
            return;
        }

        // Get projectile info (includes broadside spawn position)
        ProjectileSpawnInfo info = GetProjectileInfo();

        // Spawn homing projectile via ProjectileManager with fast turn rate
        ProjectileManager.SpawnHomingProjectile(info, missileTurnRate);

        string side = GetFiringSide() >= 0 ? "starboard" : "port";
        Debug.Log($"{weaponName} fired volley from {side}! Missile tracking {assignedTarget.gameObject.name} " +
                  $"(Ammo remaining: {currentAmmo - 1})");
    }

    /// <summary>
    /// Determines which side to fire from based on target position.
    /// Returns positive for starboard (right), negative for port (left).
    /// </summary>
    private float GetFiringSide()
    {
        if (assignedTarget == null || ownerShip == null)
            return 0f;

        Vector3 toTarget = (assignedTarget.transform.position - ownerShip.transform.position).normalized;
        return Vector3.Dot(toTarget, ownerShip.transform.right);
    }

    /// <summary>
    /// Gets the appropriate hardpoint transform based on target position.
    /// Falls back to weapon transform if hardpoints not set.
    /// </summary>
    private Transform GetBroadsideHardpoint()
    {
        float side = GetFiringSide();

        // Choose hardpoint based on which side target is on
        if (side >= 0 && starboardHardpoint != null)
            return starboardHardpoint;
        else if (side < 0 && portHardpoint != null)
            return portHardpoint;

        // Fallback to default hardpoint
        return hardpointTransform;
    }

    /// <summary>
    /// Set the broadside hardpoint transforms.
    /// </summary>
    public void SetBroadsideHardpoints(Transform port, Transform starboard)
    {
        portHardpoint = port;
        starboardHardpoint = starboard;
    }

    /// <summary>
    /// Get projectile spawn info for homing missile.
    /// Spawns from the broadside closest to the target.
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

        // Get spawn position from appropriate broadside
        Transform spawnPoint = GetBroadsideHardpoint();
        Vector3 spawnPosition = spawnPoint.position;

        // Calculate rotation to face target initially
        Vector3 directionToTarget = (assignedTarget.transform.position - spawnPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        return new ProjectileSpawnInfo
        {
            Type = ProjectileSpawnInfo.ProjectileType.Homing,
            SpawnPosition = spawnPosition,
            SpawnRotation = targetRotation,
            TargetPosition = assignedTarget.transform.position,
            TargetShip = assignedTarget,
            Damage = actualDamage,
            Speed = missileSpeed,
            OwnerShip = ownerShip
        };
    }

    /// <summary>
    /// Draw gizmos showing broadside hardpoints.
    /// </summary>
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw broadside hardpoints
        if (portHardpoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(portHardpoint.position, 0.3f);
        }
        if (starboardHardpoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(starboardHardpoint.position, 0.3f);
        }

        // Draw missile tracking line if we have a target
        if (assignedTarget != null && Application.isPlaying)
        {
            Transform spawnPoint = GetBroadsideHardpoint();
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnPoint.position, assignedTarget.transform.position);
            Gizmos.DrawWireSphere(assignedTarget.transform.position, 0.5f);
        }
    }
}
