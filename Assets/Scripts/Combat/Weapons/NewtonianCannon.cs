using UnityEngine;

/// <summary>
/// Newtonian Cannon - Heavy spinal-mounted ballistic weapon.
/// Massive damage but narrow firing arc - ship must face target directly.
/// Requires lead calculation for moving targets.
/// </summary>
public class NewtonianCannon : WeaponSystem
{
    [Header("Cannon Settings")]
    [SerializeField] private float projectileSpeed = 15f; // units per second

    [Header("Gizmo Settings")]
    [SerializeField] private float leadPositionGizmoRadius = 0.5f; // Size of lead position indicator

    /// <summary>
    /// Initialize cannon with default stats.
    /// </summary>
    private void Awake()
    {
        weaponName = "Newtonian Cannon";
        damage = 40f;
        heatCost = 30;
        firingArc = 60f; // Spinal mount - must face target (30° left/right)
        maxRange = 20f;
        maxCooldown = 0; // No cooldown
        spinUpTime = 0.5f;
        ammoCapacity = 0; // Infinite ammo
        projectileSpeed = 15f; // Fast but not instant - requires some lead
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
    /// Uses base class helper for common ballistic projectile calculations.
    /// </summary>
    public override ProjectileSpawnInfo GetProjectileInfo()
    {
        return CreateBallisticProjectileInfo(projectileSpeed);
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
            Vector3 leadPos = CalculateLeadPosition(projectileSpeed);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(leadPos, leadPositionGizmoRadius);
            Gizmos.DrawLine(hardpointTransform.position, leadPos);
        }
    }
}
