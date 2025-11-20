using UnityEngine;

/// <summary>
/// Ballistic projectile that travels in a straight line.
/// No course correction, no homing. Used by cannons and guns.
/// </summary>
public class BallisticProjectile : Projectile
{
    [Header("Ballistic Properties")]
    [SerializeField] private Vector3 initialVelocity;
    [SerializeField] private Vector3 currentVelocity;

    private TrailRenderer trailRenderer;

    // Public properties
    public Vector3 InitialVelocity => initialVelocity;
    public Vector3 CurrentVelocity => currentVelocity;

    /// <summary>
    /// Initialize ballistic projectile.
    /// </summary>
    public override void Initialize(WeaponSystem.ProjectileSpawnInfo info)
    {
        base.Initialize(info);

        // Calculate initial velocity from forward direction and speed
        initialVelocity = transform.forward * speed;
        currentVelocity = initialVelocity;

        // Get trail renderer if present
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.Clear(); // Clear old trail from pooling
            trailRenderer.enabled = true;
        }

        Debug.Log($"BallisticProjectile velocity: {currentVelocity}, speed: {speed}");
    }

    /// <summary>
    /// Update movement: straight line, no course correction.
    /// </summary>
    protected override void UpdateMovement()
    {
        // Move in straight line at constant velocity
        transform.position += currentVelocity * Time.deltaTime;

        // Keep rotation aligned with velocity direction
        if (currentVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(currentVelocity);
        }
    }

    /// <summary>
    /// Reset to pool, disable trail renderer.
    /// </summary>
    public override void ResetToPool()
    {
        base.ResetToPool();

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
            trailRenderer.Clear();
        }

        currentVelocity = Vector3.zero;
        initialVelocity = Vector3.zero;
    }

    /// <summary>
    /// Draw debug gizmos showing trajectory.
    /// </summary>
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (isActive)
        {
            // Draw projected path
            Gizmos.color = Color.cyan;
            Vector3 futurePos = transform.position + currentVelocity * 1f; // 1 second ahead
            Gizmos.DrawLine(transform.position, futurePos);
        }
    }
}
