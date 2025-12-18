using UnityEngine;

/// <summary>
/// Abstract base class for all projectile types.
/// Handles movement, collision detection, damage application, and lifetime management.
/// </summary>
public abstract class Projectile : MonoBehaviour
{
    [Header("Projectile Properties")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float lifetime = 10f; // Max seconds before auto-destroy
    [SerializeField] protected float collisionRadius = 0.5f; // For sphere cast collision

    [Header("Gizmo Settings")]
    [SerializeField] protected float gizmoForwardLineLength = 2f; // Length of forward direction gizmo line

    [Header("Runtime State")]
    [SerializeField] protected Ship ownerShip;
    [SerializeField] protected Ship targetShip;
    [SerializeField] protected float currentAge = 0f;
    [SerializeField] protected bool isActive = false;
    private bool isBeingDestroyed = false; // Prevents double-destruction from lifetime + collision race

    // Pre-allocated buffer for SphereCastNonAlloc to avoid GC allocations every frame
    // Buffer size of 16 is sufficient for typical combat scenarios (rarely more than 16 overlapping colliders)
    private const int RaycastBufferSize = 16;
    private static readonly RaycastHit[] hitBuffer = new RaycastHit[RaycastBufferSize];

    // Public properties
    public float Damage => damage;
    public float Speed => speed;
    public float Lifetime => lifetime;
    public Ship OwnerShip => ownerShip;
    public Ship TargetShip => targetShip;
    public float CurrentAge => currentAge;
    public bool IsActive => isActive;

    /// <summary>
    /// Initialize the projectile with spawn info from weapon.
    /// Called when spawned from pool or instantiated.
    /// </summary>
    public virtual void Initialize(WeaponSystem.ProjectileSpawnInfo info)
    {
        damage = info.Damage;
        speed = info.Speed;
        ownerShip = info.OwnerShip;
        targetShip = info.TargetShip;

        transform.position = info.SpawnPosition;
        transform.rotation = info.SpawnRotation;

        currentAge = 0f;
        isActive = true;

        Debug.Log($"{GetType().Name} initialized: Damage={damage}, Speed={speed}, Owner={ownerShip?.gameObject.name}");
    }

    /// <summary>
    /// Update projectile each frame (movement, collision, lifetime).
    /// </summary>
    protected virtual void Update()
    {
        if (!isActive || isBeingDestroyed) return;

        // Update age
        currentAge += Time.deltaTime;

        // Check lifetime expiry
        if (currentAge >= lifetime)
        {
            OnLifetimeExpired();
            return;
        }

        // Update movement (implemented by subclasses)
        UpdateMovement();

        // Check for collisions (skip if already being destroyed)
        if (!isBeingDestroyed)
        {
            CheckCollisions();
        }
    }

    /// <summary>
    /// Update projectile movement (implemented by subclasses).
    /// Called each frame while projectile is active.
    /// </summary>
    protected abstract void UpdateMovement();

    /// <summary>
    /// Check for collisions with ships using sphere cast.
    /// Uses NonAlloc version to avoid GC allocations every frame.
    /// </summary>
    protected virtual void CheckCollisions()
    {
        // Use sphere cast with pre-allocated buffer to avoid GC allocations
        int hitCount = Physics.SphereCastNonAlloc(
            transform.position,
            collisionRadius,
            transform.forward,
            hitBuffer,
            speed * Time.deltaTime
        );

        for (int i = 0; i < hitCount; i++)
        {
            Ship hitShip = hitBuffer[i].collider.GetComponent<Ship>();
            if (hitShip == null)
            {
                hitShip = hitBuffer[i].collider.GetComponentInParent<Ship>();
            }

            // Check if we hit a ship (and not the owner)
            if (hitShip != null && hitShip != ownerShip)
            {
                OnHit(hitShip);
                return; // Projectile destroyed on first hit
            }
        }
    }

    /// <summary>
    /// Called when projectile hits a ship.
    /// Apply damage and destroy projectile.
    /// </summary>
    protected virtual void OnHit(Ship target)
    {
        // Prevent double-hit from race conditions
        if (isBeingDestroyed) return;
        isBeingDestroyed = true;

        // Safety check: ensure target is still valid
        if (target == null || target.IsDead)
        {
            Debug.LogWarning($"{GetType().Name} target became invalid before hit could apply");
            OnDestroyed();
            return;
        }

        Debug.Log($"{GetType().Name} hit {target.gameObject.name} for {damage} damage");

        // Apply damage to target
        target.TakeDamage(damage);

        // Destroy projectile
        OnDestroyed();
    }

    /// <summary>
    /// Called when projectile lifetime expires.
    /// </summary>
    protected virtual void OnLifetimeExpired()
    {
        // Prevent double-destruction from race conditions
        if (isBeingDestroyed) return;
        isBeingDestroyed = true;

        Debug.Log($"{GetType().Name} lifetime expired ({lifetime}s)");
        OnDestroyed();
    }

    /// <summary>
    /// Cleanup and destroy projectile.
    /// Override for VFX, particle effects, etc.
    /// </summary>
    public virtual void OnDestroyed()
    {
        isActive = false;

        // Return to pool instead of destroying
        ProjectileManager.ReturnToPool(this);
    }

    /// <summary>
    /// Reset projectile to pooled state (inactive, ready for reuse).
    /// </summary>
    public virtual void ResetToPool()
    {
        isActive = false;
        isBeingDestroyed = false;
        currentAge = 0f;
        ownerShip = null;
        targetShip = null;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Draw gizmos for debugging projectile collision radius.
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        if (!isActive) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);

        // Draw forward direction
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoForwardLineLength);
    }
}
