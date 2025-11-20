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
        // Check cooldown
        if (currentCooldown > 0)
        {
            Debug.Log($"{weaponName}: On cooldown ({currentCooldown} turns remaining)");
            return false;
        }

        // Check ammo (0 = infinite)
        if (ammoCapacity > 0 && currentAmmo <= 0)
        {
            Debug.Log($"{weaponName}: Out of ammo");
            return false;
        }

        // Check target
        if (assignedTarget == null)
        {
            Debug.Log($"{weaponName}: No target assigned");
            return false;
        }

        // Check target alive
        if (assignedTarget.IsDead)
        {
            Debug.Log($"{weaponName}: Target is dead");
            return false;
        }

        // Check arc
        if (!IsInArc(assignedTarget.transform.position))
        {
            Debug.Log($"{weaponName}: Target not in firing arc");
            return false;
        }

        // Check range
        if (!IsInRange(assignedTarget.transform.position))
        {
            Debug.Log($"{weaponName}: Target out of range");
            return false;
        }

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
