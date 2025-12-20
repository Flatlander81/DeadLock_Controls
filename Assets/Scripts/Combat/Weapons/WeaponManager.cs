using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all weapons on a ship.
/// Discovers weapons, manages firing groups, and coordinates weapon firing.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private List<WeaponSystem> weapons = new List<WeaponSystem>();

    [Header("Firing Groups")]
    [SerializeField] private Dictionary<int, List<WeaponSystem>> weaponGroups = new Dictionary<int, List<WeaponSystem>>();

    // HashSet for O(1) weapon containment checks (mirrors weapons list)
    private HashSet<WeaponSystem> weaponSet = new HashSet<WeaponSystem>();

    private Ship ship;

    // Public properties
    public List<WeaponSystem> Weapons => weapons;
    public int WeaponCount => weapons.Count;

    /// <summary>
    /// Initialize weapon manager and discover all weapons.
    /// </summary>
    private void Start()
    {
        ship = GetComponent<Ship>();
        if (ship == null)
        {
            Debug.LogError($"WeaponManager on {gameObject.name}: No Ship component found!");
            return;
        }

        // Initialize weapon groups dictionary
        for (int i = 0; i <= 4; i++)
        {
            weaponGroups[i] = new List<WeaponSystem>();
        }

        // Find all WeaponSystem components on this ship and its children
        DiscoverWeapons();

        Debug.Log($"WeaponManager initialized on {gameObject.name}. Found {weapons.Count} weapons.");
    }

    /// <summary>
    /// Discover all WeaponSystem components on ship and children.
    /// </summary>
    private void DiscoverWeapons()
    {
        weapons.Clear();
        weaponSet.Clear();

        // Find all weapons in children (hardpoints)
        WeaponSystem[] foundWeapons = GetComponentsInChildren<WeaponSystem>();

        foreach (WeaponSystem weapon in foundWeapons)
        {
            weapons.Add(weapon);
            weaponSet.Add(weapon); // O(1) lookup support
            weapon.Initialize(ship);

            // Add to group 0 (unassigned) by default
            weaponGroups[weapon.AssignedGroup].Add(weapon);

            Debug.Log($"  Discovered weapon: {weapon.WeaponName} on hardpoint {weapon.transform.name}");
        }
    }

    /// <summary>
    /// Assign a weapon to a firing group (1-4, or 0 for unassigned).
    /// </summary>
    public void AssignWeaponToGroup(WeaponSystem weapon, int groupNumber)
    {
        // O(1) containment check using HashSet instead of O(n) List.Contains
        if (!weaponSet.Contains(weapon))
        {
            Debug.LogWarning($"Weapon {weapon.WeaponName} not found on this ship");
            return;
        }

        if (groupNumber < 0 || groupNumber > 4)
        {
            Debug.LogWarning($"Invalid group number {groupNumber} (must be 0-4)");
            return;
        }

        // Remove from old group
        int oldGroup = weapon.AssignedGroup;
        if (weaponGroups.ContainsKey(oldGroup))
        {
            weaponGroups[oldGroup].Remove(weapon);
        }

        // Assign to new group
        weapon.AssignToGroup(groupNumber);
        weaponGroups[groupNumber].Add(weapon);

        Debug.Log($"Assigned {weapon.WeaponName} to group {groupNumber}");
    }

    /// <summary>
    /// Get all weapons in a specific group.
    /// </summary>
    public List<WeaponSystem> GetWeaponsInGroup(int groupNumber)
    {
        if (groupNumber < 0 || groupNumber > 4)
        {
            Debug.LogWarning($"Invalid group number {groupNumber} (must be 0-4)");
            return new List<WeaponSystem>();
        }

        return weaponGroups[groupNumber];
    }

    /// <summary>
    /// Set target for all weapons in a group.
    /// </summary>
    public void SetGroupTarget(int groupNumber, Ship target)
    {
        List<WeaponSystem> groupWeapons = GetWeaponsInGroup(groupNumber);

        foreach (WeaponSystem weapon in groupWeapons)
        {
            weapon.SetTarget(target);
        }

        Debug.Log($"Group {groupNumber} targeting {(target != null ? target.gameObject.name : "none")} ({groupWeapons.Count} weapons)");
    }

    /// <summary>
    /// Fire all weapons in a specific group.
    /// Coroutine executed during Simulation phase.
    /// </summary>
    public IEnumerator FireGroup(int groupNumber)
    {
        List<WeaponSystem> groupWeapons = GetWeaponsInGroup(groupNumber);

        if (groupWeapons.Count == 0)
        {
            Debug.Log($"Group {groupNumber} has no weapons");
            yield break;
        }

        Debug.Log($"{gameObject.name} firing group {groupNumber} ({groupWeapons.Count} weapons)...");

        // Fire all weapons in parallel (they handle their own spin-up)
        foreach (WeaponSystem weapon in groupWeapons)
        {
            if (weapon.CanFire())
            {
                StartCoroutine(weapon.FireWithSpinUp());
            }
            else
            {
                Debug.Log($"  {weapon.WeaponName} cannot fire");
            }
        }

        // Wait for all weapons to finish (including spin-up)
        // Find longest spin-up time
        float maxSpinUpTime = 0f;
        foreach (WeaponSystem weapon in groupWeapons)
        {
            if (weapon.CanFire() && weapon.SpinUpTime > maxSpinUpTime)
            {
                maxSpinUpTime = weapon.SpinUpTime;
            }
        }

        if (maxSpinUpTime > 0f)
        {
            yield return new WaitForSeconds(maxSpinUpTime + 0.1f); // Extra time for firing
        }

        Debug.Log($"Group {groupNumber} finished firing");
    }

    /// <summary>
    /// Fire all assigned weapons at a target (Alpha Strike).
    /// </summary>
    public IEnumerator FireAlphaStrike(Ship target)
    {
        Debug.Log($"{gameObject.name} Alpha Strike on {target.gameObject.name}!");

        // Set target for all weapons
        foreach (WeaponSystem weapon in weapons)
        {
            weapon.SetTarget(target);
        }

        // Fire all weapons that can fire
        float maxSpinUpTime = 0f;

        foreach (WeaponSystem weapon in weapons)
        {
            if (weapon.CanFire())
            {
                StartCoroutine(weapon.FireWithSpinUp());

                if (weapon.SpinUpTime > maxSpinUpTime)
                {
                    maxSpinUpTime = weapon.SpinUpTime;
                }
            }
        }

        // Wait for all weapons to finish
        if (maxSpinUpTime > 0f)
        {
            yield return new WaitForSeconds(maxSpinUpTime + 0.1f);
        }

        Debug.Log($"Alpha Strike complete");
    }

    /// <summary>
    /// Tick down cooldowns for all weapons.
    /// Called at end of turn.
    /// </summary>
    public void TickAllCooldowns()
    {
        foreach (WeaponSystem weapon in weapons)
        {
            weapon.TickCooldown();
        }
    }

    /// <summary>
    /// Calculate total heat cost for firing a group.
    /// Used for heat budget planning.
    /// </summary>
    public int CalculateGroupHeatCost(int groupNumber)
    {
        List<WeaponSystem> groupWeapons = GetWeaponsInGroup(groupNumber);

        int totalHeat = 0;
        foreach (WeaponSystem weapon in groupWeapons)
        {
            if (weapon.CanFire())
            {
                totalHeat += weapon.HeatCost;
            }
        }

        return totalHeat;
    }

    /// <summary>
    /// Check if all weapons in a group are ready to fire.
    /// </summary>
    public bool IsGroupReady(int groupNumber)
    {
        List<WeaponSystem> groupWeapons = GetWeaponsInGroup(groupNumber);

        if (groupWeapons.Count == 0)
            return false;

        foreach (WeaponSystem weapon in groupWeapons)
        {
            if (!weapon.CanFire())
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get a weapon by hardpoint name.
    /// </summary>
    public WeaponSystem GetWeaponByHardpoint(string hardpointName)
    {
        return weapons.FirstOrDefault(w => w.transform.name == hardpointName);
    }

    /// <summary>
    /// Clear all weapon targets.
    /// </summary>
    public void ClearAllTargets()
    {
        foreach (WeaponSystem weapon in weapons)
        {
            weapon.SetTarget(null);
        }

        Debug.Log($"{gameObject.name} cleared all weapon targets");
    }

    #region Queue Integration

    /// <summary>
    /// Queue a weapon group to fire at a target.
    /// Uses WeaponFiringQueue for turn-based combat.
    /// </summary>
    public int QueueFireGroup(int groupNumber, Ship target)
    {
        if (WeaponFiringQueue.Instance == null)
        {
            Debug.LogWarning($"{gameObject.name}: WeaponFiringQueue not found, falling back to immediate fire");
            StartCoroutine(FireGroup(groupNumber));
            return 0;
        }

        // Set target for all weapons in group first
        SetGroupTarget(groupNumber, target);

        // Queue via WeaponFiringQueue
        int queued = WeaponFiringQueue.Instance.QueueGroupFire(this, groupNumber, target);

        // Update queued state on weapons
        foreach (var weapon in GetWeaponsInGroup(groupNumber))
        {
            if (WeaponFiringQueue.Instance.IsWeaponQueued(weapon))
            {
                weapon.IsQueuedToFire = true;
            }
        }

        return queued;
    }

    /// <summary>
    /// Queue an alpha strike at a target.
    /// Uses WeaponFiringQueue for turn-based combat.
    /// </summary>
    public int QueueAlphaStrike(Ship target)
    {
        if (WeaponFiringQueue.Instance == null)
        {
            Debug.LogWarning($"{gameObject.name}: WeaponFiringQueue not found, falling back to immediate fire");
            StartCoroutine(FireAlphaStrike(target));
            return 0;
        }

        // Queue via WeaponFiringQueue
        int queued = WeaponFiringQueue.Instance.QueueAlphaStrike(this, target);

        // Update queued state on weapons
        foreach (var weapon in weapons)
        {
            if (WeaponFiringQueue.Instance.IsWeaponQueued(weapon))
            {
                weapon.IsQueuedToFire = true;
            }
        }

        return queued;
    }

    /// <summary>
    /// Queue a single weapon to fire at a target.
    /// </summary>
    public bool QueueWeaponFire(WeaponSystem weapon, Ship target)
    {
        if (WeaponFiringQueue.Instance == null)
        {
            Debug.LogWarning($"{gameObject.name}: WeaponFiringQueue not found");
            return false;
        }

        if (!weaponSet.Contains(weapon))
        {
            Debug.LogWarning($"{gameObject.name}: Weapon {weapon.WeaponName} not found on this ship");
            return false;
        }

        bool success = WeaponFiringQueue.Instance.QueueFire(weapon, target);
        if (success)
        {
            weapon.IsQueuedToFire = true;
        }

        return success;
    }

    /// <summary>
    /// Cancel a queued weapon command.
    /// </summary>
    public bool CancelQueuedWeapon(WeaponSystem weapon)
    {
        if (WeaponFiringQueue.Instance == null)
        {
            return false;
        }

        bool success = WeaponFiringQueue.Instance.CancelQueuedCommand(weapon);
        if (success)
        {
            weapon.IsQueuedToFire = false;
        }

        return success;
    }

    /// <summary>
    /// Get all queued commands for this ship.
    /// </summary>
    public List<WeaponFireCommand> GetQueuedCommands()
    {
        if (WeaponFiringQueue.Instance == null)
        {
            return new List<WeaponFireCommand>();
        }

        return WeaponFiringQueue.Instance.GetCommandsForShip(ship);
    }

    /// <summary>
    /// Get the total heat cost of queued weapons for this ship.
    /// </summary>
    public int GetQueuedHeatCost()
    {
        if (WeaponFiringQueue.Instance == null)
        {
            return 0;
        }

        return WeaponFiringQueue.Instance.GetQueuedHeatCostForShip(ship);
    }

    /// <summary>
    /// Clear the queued state on all weapons.
    /// Called when queue is cleared or turn ends.
    /// </summary>
    public void ClearQueuedState()
    {
        foreach (var weapon in weapons)
        {
            weapon.IsQueuedToFire = false;
        }
    }

    #endregion
}
