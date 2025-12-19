using UnityEngine;

/// <summary>
/// Torpedo Launcher - Heavy homing projectile weapon with limited ammo.
/// Slow but powerful, 360° arc with broadside firing - launches from the side closest to target.
/// Cannot reload mid-combat.
/// </summary>
public class TorpedoLauncher : WeaponSystem
{
    [Header("Torpedo Settings")]
    [SerializeField] private float torpedoSpeed = 5f; // units per second
    [SerializeField] private float torpedoTurnRate = 45f; // degrees per second

    [Header("Broadside Hardpoints")]
    [SerializeField] private Transform portHardpoint;      // Left side launch point
    [SerializeField] private Transform starboardHardpoint; // Right side launch point

    // Public properties for projectile spawning
    public float TorpedoSpeed => torpedoSpeed;
    public float TorpedoTurnRate => torpedoTurnRate;

    /// <summary>
    /// Initialize torpedo launcher with default stats.
    /// </summary>
    private void Awake()
    {
        weaponName = "Torpedo Launcher";
        damage = 80f;
        heatCost = 25;
        firingArc = 360f; // Full arc - broadside weapon can fire at any target
        maxRange = 50f;
        maxCooldown = 3; // 3 turn cooldown
        spinUpTime = 1.0f;
        ammoCapacity = 6; // Limited ammo, cannot reload mid-combat
        torpedoSpeed = 5f;
        torpedoTurnRate = 45f;
    }

    /// <summary>
    /// Fire the torpedo launcher - spawn homing projectile from broadside.
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

        // Spawn homing projectile via ProjectileManager with slow turn rate
        ProjectileManager.SpawnHomingProjectile(info, torpedoTurnRate);

        string side = GetFiringSide() >= 0 ? "starboard" : "port";
        Debug.Log($"{weaponName} fired from {side}! Torpedo tracking {assignedTarget.gameObject.name} " +
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
    /// Get projectile spawn info for homing torpedo.
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
            Speed = torpedoSpeed,
            OwnerShip = ownerShip
        };
    }

    /// <summary>
    /// Draw gizmos showing broadside hardpoints and firing arc.
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

        // Draw torpedo tracking line if we have a target
        if (assignedTarget != null && Application.isPlaying)
        {
            Transform spawnPoint = GetBroadsideHardpoint();
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
            Gizmos.DrawLine(spawnPoint.position, assignedTarget.transform.position);
            Gizmos.DrawWireSphere(assignedTarget.transform.position, 1f);
        }
    }
}
