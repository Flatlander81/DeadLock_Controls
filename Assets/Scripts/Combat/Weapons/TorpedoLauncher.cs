using UnityEngine;

/// <summary>
/// Torpedo Launcher - Heavy homing projectile weapon with limited ammo.
/// Slow but powerful, narrow firing arc, cannot reload mid-combat.
/// </summary>
public class TorpedoLauncher : WeaponSystem
{
    [Header("Torpedo Settings")]
    [SerializeField] private float torpedoSpeed = 5f; // units per second
    [SerializeField] private float torpedoTurnRate = 45f; // degrees per second (for future use)

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
        firingArc = 30f; // Narrow forward arc
        maxRange = 50f;
        maxCooldown = 3; // 3 turn cooldown
        spinUpTime = 1.0f;
        ammoCapacity = 6; // Limited ammo, cannot reload mid-combat
        torpedoSpeed = 5f;
        torpedoTurnRate = 45f;
    }

    /// <summary>
    /// Fire the torpedo launcher - spawn homing projectile.
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

        Debug.Log($"{weaponName} fired! Torpedo tracking {assignedTarget.gameObject.name} " +
                  $"(Ammo remaining: {currentAmmo - 1})");
    }

    /// <summary>
    /// Get projectile spawn info for homing torpedo.
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
            Speed = torpedoSpeed,
            OwnerShip = ownerShip
        };
    }

    /// <summary>
    /// Draw gizmos showing narrow firing arc.
    /// </summary>
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw torpedo tracking line if we have a target
        if (assignedTarget != null && Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(hardpointTransform.position, assignedTarget.transform.position);
            Gizmos.DrawWireSphere(assignedTarget.transform.position, 1f);
        }
    }
}
