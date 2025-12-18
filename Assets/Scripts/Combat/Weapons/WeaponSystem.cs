using UnityEngine;
using System.Collections;

/// <summary>
/// Abstract base class for all weapon systems.
/// Attached to weapon hardpoint GameObjects on ships.
/// </summary>
public abstract class WeaponSystem : MonoBehaviour
{
    /// <summary>
    /// Information for spawning projectiles.
    /// Used to coordinate with Track B (Projectile System).
    /// </summary>
    public struct ProjectileSpawnInfo
    {
        public enum ProjectileType { InstantHit, Ballistic, Homing }

        public ProjectileType Type;
        public Vector3 SpawnPosition;
        public Quaternion SpawnRotation;
        public Vector3 TargetPosition;
        public Ship TargetShip;
        public float Damage;
        public float Speed;
        public Ship OwnerShip;
    }

    [Header("Weapon Stats")]
    [SerializeField] protected string weaponName = "Weapon";
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected int heatCost = 10;
    [SerializeField] protected float firingArc = 180f; // degrees: 360 = turret, 180 = forward, 30 = narrow
    [SerializeField] protected float maxRange = 20f;
    [SerializeField] protected int maxCooldown = 0; // turns
    [SerializeField] protected float spinUpTime = 0f; // seconds before firing
    [SerializeField] protected int ammoCapacity = 0; // 0 = infinite

    [Header("Runtime State")]
    [SerializeField] protected int currentCooldown = 0;
    [SerializeField] protected int currentAmmo = 0;
    [SerializeField] protected int assignedGroup = 0; // 0-4, where 0 = unassigned
    [SerializeField] protected Ship assignedTarget = null;

    // Protected references
    protected Ship ownerShip;
    protected Transform hardpointTransform;

    // Public properties
    public string WeaponName => weaponName;
    public float Damage => damage;
    public int HeatCost => heatCost;
    public float FiringArc => firingArc;
    public float MaxRange => maxRange;
    public int MaxCooldown => maxCooldown;
    public float SpinUpTime => spinUpTime;
    public int AmmoCapacity => ammoCapacity;
    public int CurrentAmmo => currentAmmo;
    public int CurrentCooldown => currentCooldown;
    public int AssignedGroup => assignedGroup;
    public Ship AssignedTarget => assignedTarget;
    public Ship OwnerShip => ownerShip;

    /// <summary>
    /// Initialize the weapon system.
    /// Called by WeaponManager when ship is created.
    /// </summary>
    public virtual void Initialize(Ship owner)
    {
        ownerShip = owner;
        hardpointTransform = transform;
        currentCooldown = 0;
        currentAmmo = ammoCapacity;

        Debug.Log($"{weaponName} initialized on {owner.gameObject.name} at hardpoint {hardpointTransform.name}");
    }

    /// <summary>
    /// Check if target is within the weapon's firing arc.
    /// </summary>
    public bool IsInArc(Vector3 targetPosition)
    {
        Vector3 toTarget = (targetPosition - hardpointTransform.position).normalized;
        float angle = Vector3.Angle(hardpointTransform.forward, toTarget);

        // 360 degree arc = turret (can fire in any direction)
        if (firingArc >= 360f) return true;

        // Check if within half-arc on either side of forward
        // Use strict less-than to exclude boundary cases (e.g., 90° is NOT in 180° forward arc)
        return angle < (firingArc / 2f);
    }

    /// <summary>
    /// Check if target is within the weapon's maximum range.
    /// </summary>
    public bool IsInRange(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(hardpointTransform.position, targetPosition);
        return distance <= maxRange;
    }

    /// <summary>
    /// Check if weapon can fire.
    /// Validates: cooldown ready, has ammo, has target, target in arc + range.
    /// </summary>
    public bool CanFire()
    {
        return CanFireInternal(true);
    }

    /// <summary>
    /// Check if weapon can fire without logging (for UI polling).
    /// </summary>
    public bool CanFireSilent()
    {
        return CanFireInternal(false);
    }

    /// <summary>
    /// Helper method for conditional logging to reduce code duplication.
    /// </summary>
    private void LogIfEnabled(bool shouldLog, string message)
    {
        if (shouldLog)
        {
            Debug.Log($"{weaponName}: {message}");
        }
    }

    /// <summary>
    /// Helper method to check a condition and log if it fails.
    /// Returns true if the condition FAILS (weapon cannot fire).
    /// </summary>
    private bool CheckFailCondition(bool failCondition, bool logReasons, string failMessage)
    {
        if (failCondition)
        {
            LogIfEnabled(logReasons, failMessage);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Internal fire check with optional logging.
    /// </summary>
    private bool CanFireInternal(bool logReasons)
    {
        // Check cooldown
        if (CheckFailCondition(currentCooldown > 0, logReasons,
            $"On cooldown ({currentCooldown} turns remaining)")) return false;

        // Check ammo (0 = infinite)
        if (CheckFailCondition(ammoCapacity > 0 && currentAmmo <= 0, logReasons,
            "Out of ammo")) return false;

        // Check target
        if (CheckFailCondition(assignedTarget == null, logReasons,
            "No target assigned")) return false;

        // Check target alive
        if (CheckFailCondition(assignedTarget.IsDead, logReasons,
            "Target is dead")) return false;

        // Check arc
        if (CheckFailCondition(!IsInArc(assignedTarget.transform.position), logReasons,
            "Target not in firing arc")) return false;

        // Check range
        if (CheckFailCondition(!IsInRange(assignedTarget.transform.position), logReasons,
            "Target out of range")) return false;

        return true;
    }

    /// <summary>
    /// Assign weapon to a firing group (1-4, or 0 for unassigned).
    /// </summary>
    public void AssignToGroup(int groupNumber)
    {
        if (groupNumber < 0 || groupNumber > 4)
        {
            Debug.LogWarning($"{weaponName}: Invalid group number {groupNumber} (must be 0-4)");
            return;
        }

        assignedGroup = groupNumber;
        Debug.Log($"{weaponName} assigned to group {groupNumber}");
    }

    /// <summary>
    /// Set the weapon's target.
    /// </summary>
    public void SetTarget(Ship target)
    {
        assignedTarget = target;
        if (target != null)
        {
            Debug.Log($"{weaponName} targeting {target.gameObject.name}");
        }
        else
        {
            Debug.Log($"{weaponName} target cleared");
        }
    }

    /// <summary>
    /// Fire the weapon with spin-up delay.
    /// Coroutine called during Simulation phase.
    /// </summary>
    public IEnumerator FireWithSpinUp()
    {
        // Wait for spin-up
        if (spinUpTime > 0f)
        {
            Debug.Log($"{weaponName} spinning up for {spinUpTime}s...");
            yield return new WaitForSeconds(spinUpTime);
        }

        // Check if we can still fire (target might have moved out of arc/range)
        if (!CanFire())
        {
            Debug.LogWarning($"{weaponName} cannot fire after spin-up");
            yield break;
        }

        // Execute firing
        Fire();

        // Consume ammo
        if (ammoCapacity > 0)
        {
            currentAmmo--;
        }

        // Apply heat to owner ship
        if (ownerShip != null && ownerShip.HeatManager != null)
        {
            float actualHeatCost = heatCost * ownerShip.WeaponHeatMultiplier;
            ownerShip.HeatManager.AddPlannedHeat(actualHeatCost);
            ownerShip.HeatManager.CommitPlannedHeat();
        }

        // Start cooldown
        StartCooldown();
    }

    /// <summary>
    /// Execute the weapon's firing behavior.
    /// Implemented by derived classes (instant hit, ballistic, homing, etc.).
    /// </summary>
    protected abstract void Fire();

    /// <summary>
    /// Get projectile spawn information for the Projectile System (Track B).
    /// </summary>
    public abstract ProjectileSpawnInfo GetProjectileInfo();

    /// <summary>
    /// Start weapon cooldown.
    /// </summary>
    public void StartCooldown()
    {
        currentCooldown = maxCooldown;
        if (maxCooldown > 0)
        {
            Debug.Log($"{weaponName} on cooldown for {maxCooldown} turns");
        }
    }

    /// <summary>
    /// Tick down weapon cooldown.
    /// Called at end of turn by WeaponManager.
    /// </summary>
    public void TickCooldown()
    {
        if (currentCooldown > 0)
        {
            currentCooldown--;
            if (currentCooldown == 0)
            {
                Debug.Log($"{weaponName} cooldown finished");
            }
        }
    }

    /// <summary>
    /// Reload weapon ammo (for future use).
    /// </summary>
    public void Reload()
    {
        currentAmmo = ammoCapacity;
        Debug.Log($"{weaponName} reloaded to {ammoCapacity} rounds");
    }

    /// <summary>
    /// Calculate lead position for moving target.
    /// Estimates where target will be when projectile arrives.
    /// Used by ballistic weapons (RailGun, NewtonianCannon).
    /// </summary>
    /// <param name="projectileSpeed">Speed of the projectile in units per second</param>
    /// <returns>Lead position to aim at</returns>
    protected Vector3 CalculateLeadPosition(float projectileSpeed)
    {
        if (assignedTarget == null) return Vector3.zero;

        Vector3 targetCurrentPos = assignedTarget.transform.position;

        // Get target velocity based on ship's forward direction and speed
        Vector3 targetVelocity = assignedTarget.transform.forward * assignedTarget.CurrentSpeed;

        // Calculate time to impact
        float distanceToTarget = Vector3.Distance(hardpointTransform.position, targetCurrentPos);
        float timeToImpact = distanceToTarget / projectileSpeed;

        // Calculate lead position
        return targetCurrentPos + (targetVelocity * timeToImpact);
    }

    /// <summary>
    /// Create base projectile spawn info with common calculations.
    /// Handles lead position, damage multiplier, and rotation calculation.
    /// Used by ballistic weapons to reduce code duplication.
    /// </summary>
    /// <param name="projectileSpeed">Speed of the projectile</param>
    /// <param name="projectileType">Type of projectile to spawn</param>
    /// <returns>Populated ProjectileSpawnInfo struct</returns>
    protected ProjectileSpawnInfo CreateBallisticProjectileInfo(float projectileSpeed, ProjectileSpawnInfo.ProjectileType projectileType = ProjectileSpawnInfo.ProjectileType.Ballistic)
    {
        if (assignedTarget == null) return default;

        // Calculate lead position for moving target
        Vector3 targetPosition = CalculateLeadPosition(projectileSpeed);

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
            Type = projectileType,
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
    /// Draw debug gizmos for weapon arc and range.
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        if (hardpointTransform == null)
            hardpointTransform = transform;

        // Draw range sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(hardpointTransform.position, maxRange);

        // Draw firing arc
        if (firingArc < 360f)
        {
            Gizmos.color = Color.cyan;
            Vector3 forward = hardpointTransform.forward;
            float halfArc = firingArc / 2f;

            // Draw arc lines
            Vector3 rightEdge = Quaternion.Euler(0, halfArc, 0) * forward * maxRange;
            Vector3 leftEdge = Quaternion.Euler(0, -halfArc, 0) * forward * maxRange;

            Gizmos.DrawLine(hardpointTransform.position, hardpointTransform.position + rightEdge);
            Gizmos.DrawLine(hardpointTransform.position, hardpointTransform.position + leftEdge);
        }
    }
}
