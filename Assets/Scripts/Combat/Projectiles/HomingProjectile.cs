using UnityEngine;

/// <summary>
/// Homing projectile that seeks a target.
/// Adjusts course to track target. Used by missiles and torpedoes.
/// If target is destroyed, continues on ballistic trajectory.
/// </summary>
public class HomingProjectile : Projectile
{
    [Header("Homing Properties")]
    [SerializeField] private float turnRate = 90f; // Degrees per second
    [SerializeField] private bool isHoming = true; // False if target destroyed

    private TrailRenderer trailRenderer;
    private ParticleSystem thrusterEffect;
    private Vector3 currentVelocity;

    // Public properties
    public float TurnRate => turnRate;
    public bool IsHoming => isHoming;
    public Vector3 CurrentVelocity => currentVelocity;

    /// <summary>
    /// Initialize homing projectile with target.
    /// </summary>
    public override void Initialize(WeaponSystem.ProjectileSpawnInfo info)
    {
        base.Initialize(info);

        // Start homing if we have a target
        isHoming = (targetShip != null && !targetShip.IsDead);

        // Initial velocity is forward direction
        currentVelocity = transform.forward * speed;

        // Get components if present
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = true;
        }

        thrusterEffect = GetComponentInChildren<ParticleSystem>();
        if (thrusterEffect != null)
        {
            thrusterEffect.Play();
        }

        Debug.Log($"HomingProjectile initialized: Target={targetShip?.gameObject.name}, Homing={isHoming}");
    }

    /// <summary>
    /// Update movement: rotate toward target, then move forward.
    /// </summary>
    protected override void UpdateMovement()
    {
        // Check if target still valid
        if (isHoming && (targetShip == null || targetShip.IsDead))
        {
            Debug.Log($"HomingProjectile target lost, switching to ballistic mode");
            isHoming = false;

            // Stop thruster effect
            if (thrusterEffect != null && thrusterEffect.isPlaying)
            {
                thrusterEffect.Stop();
            }
        }

        if (isHoming && targetShip != null)
        {
            // Calculate direction to target
            Vector3 directionToTarget = (targetShip.transform.position - transform.position).normalized;

            // Rotate toward target at turnRate
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnRate * Time.deltaTime
            );

            // Update velocity to current forward direction
            currentVelocity = transform.forward * speed;
        }

        // Move forward at speed (whether homing or ballistic)
        transform.position += currentVelocity * Time.deltaTime;
    }

    /// <summary>
    /// Override collision check to handle target destruction.
    /// </summary>
    protected override void CheckCollisions()
    {
        base.CheckCollisions();

        // Additional check: if we're very close to target, count as hit
        if (isHoming && targetShip != null && !targetShip.IsDead)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetShip.transform.position);
            if (distanceToTarget <= collisionRadius)
            {
                OnHit(targetShip);
            }
        }
    }

    /// <summary>
    /// Reset to pool, disable effects.
    /// </summary>
    public override void ResetToPool()
    {
        base.ResetToPool();

        isHoming = false;
        currentVelocity = Vector3.zero;

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
            trailRenderer.Clear();
        }

        if (thrusterEffect != null && thrusterEffect.isPlaying)
        {
            thrusterEffect.Stop();
        }
    }

    /// <summary>
    /// Draw debug gizmos showing target tracking.
    /// </summary>
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (isActive && isHoming && targetShip != null)
        {
            // Draw line to target
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetShip.transform.position);

            // Draw target position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetShip.transform.position, 0.5f);
        }

        if (isActive)
        {
            // Draw velocity vector
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + currentVelocity.normalized * 2f);
        }
    }
}
