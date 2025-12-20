using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages weapon firing queue for turn-based combat.
/// Weapons are queued during Command Phase and executed during Simulation Phase.
/// </summary>
public class WeaponFiringQueue : TurnEventSubscriber
{
    [Header("Queue State")]
    [SerializeField] private List<WeaponFireCommand> queuedCommands = new List<WeaponFireCommand>();
    [SerializeField] private bool isExecuting = false;

    [Header("Debug")]
    [SerializeField] private bool logEvents = true;

    // Singleton instance
    public static WeaponFiringQueue Instance { get; private set; }

    // Events
    public event Action<WeaponFireCommand> OnCommandQueued;
    public event Action<WeaponFireCommand> OnCommandCancelled;
    public event Action OnQueueCleared;
    public event Action OnQueueExecutionStarted;
    public event Action OnQueueExecutionCompleted;
    public event Action<WeaponFireCommand, bool> OnCommandExecuted; // bool = success

    // Public properties
    public IReadOnlyList<WeaponFireCommand> QueuedCommands => queuedCommands.AsReadOnly();
    public bool IsExecuting => isExecuting;
    public int QueuedCount => queuedCommands.Count;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #region Queue Operations

    /// <summary>
    /// Queue a single weapon to fire at a target.
    /// </summary>
    public bool QueueFire(WeaponSystem weapon, Ship target)
    {
        if (weapon == null || target == null)
        {
            if (logEvents) Debug.LogWarning("[WeaponFiringQueue] Cannot queue: null weapon or target");
            return false;
        }

        // Check if weapon can be queued (cooldown, ammo, etc.)
        if (!CanQueueFire(weapon))
        {
            return false;
        }

        // Check if already queued
        if (IsWeaponQueued(weapon))
        {
            if (logEvents) Debug.LogWarning($"[WeaponFiringQueue] {weapon.WeaponName} is already queued");
            return false;
        }

        var command = new WeaponFireCommand(weapon, target);
        queuedCommands.Add(command);

        if (logEvents) Debug.Log($"[WeaponFiringQueue] Queued: {command}");
        OnCommandQueued?.Invoke(command);

        return true;
    }

    /// <summary>
    /// Queue all weapons in a group to fire at a target.
    /// </summary>
    public int QueueGroupFire(WeaponManager weaponManager, int groupNumber, Ship target)
    {
        if (weaponManager == null || target == null)
        {
            if (logEvents) Debug.LogWarning("[WeaponFiringQueue] Cannot queue group: null weapon manager or target");
            return 0;
        }

        var groupWeapons = weaponManager.GetWeaponsInGroup(groupNumber);
        int queuedCount = 0;

        foreach (var weapon in groupWeapons)
        {
            if (CanQueueFire(weapon) && !IsWeaponQueued(weapon))
            {
                var command = new WeaponFireCommand(weapon, target, groupNumber);
                queuedCommands.Add(command);
                queuedCount++;

                if (logEvents) Debug.Log($"[WeaponFiringQueue] Queued (Group {groupNumber}): {command}");
                OnCommandQueued?.Invoke(command);
            }
        }

        if (logEvents) Debug.Log($"[WeaponFiringQueue] Group {groupNumber}: {queuedCount} weapons queued");
        return queuedCount;
    }

    /// <summary>
    /// Queue all weapons on a ship to fire at a target (Alpha Strike).
    /// </summary>
    public int QueueAlphaStrike(WeaponManager weaponManager, Ship target)
    {
        if (weaponManager == null || target == null)
        {
            if (logEvents) Debug.LogWarning("[WeaponFiringQueue] Cannot queue alpha strike: null weapon manager or target");
            return 0;
        }

        int queuedCount = 0;

        foreach (var weapon in weaponManager.Weapons)
        {
            if (CanQueueFire(weapon) && !IsWeaponQueued(weapon))
            {
                var command = new WeaponFireCommand(weapon, target, 0, true);
                queuedCommands.Add(command);
                queuedCount++;

                if (logEvents) Debug.Log($"[WeaponFiringQueue] Queued (Alpha): {command}");
                OnCommandQueued?.Invoke(command);
            }
        }

        if (logEvents) Debug.Log($"[WeaponFiringQueue] Alpha Strike: {queuedCount} weapons queued");
        return queuedCount;
    }

    /// <summary>
    /// Check if a weapon can be queued to fire.
    /// Checks cooldown and ammo, but NOT arc (arc is checked at execution time).
    /// </summary>
    public bool CanQueueFire(WeaponSystem weapon)
    {
        if (weapon == null) return false;

        // Check cooldown
        if (weapon.CurrentCooldown > 0)
        {
            if (logEvents) Debug.Log($"[WeaponFiringQueue] {weapon.WeaponName} on cooldown ({weapon.CurrentCooldown} turns)");
            return false;
        }

        // Check ammo (0 = infinite)
        if (weapon.AmmoCapacity > 0 && weapon.CurrentAmmo <= 0)
        {
            if (logEvents) Debug.Log($"[WeaponFiringQueue] {weapon.WeaponName} out of ammo");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if a weapon is already queued.
    /// </summary>
    public bool IsWeaponQueued(WeaponSystem weapon)
    {
        foreach (var cmd in queuedCommands)
        {
            if (cmd.Weapon == weapon) return true;
        }
        return false;
    }

    /// <summary>
    /// Cancel a queued weapon command.
    /// </summary>
    public bool CancelQueuedCommand(WeaponSystem weapon)
    {
        for (int i = queuedCommands.Count - 1; i >= 0; i--)
        {
            if (queuedCommands[i].Weapon == weapon)
            {
                var cancelled = queuedCommands[i];
                queuedCommands.RemoveAt(i);

                if (logEvents) Debug.Log($"[WeaponFiringQueue] Cancelled: {cancelled}");
                OnCommandCancelled?.Invoke(cancelled);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Clear all queued commands.
    /// </summary>
    public void ClearQueue()
    {
        int count = queuedCommands.Count;
        queuedCommands.Clear();

        if (logEvents && count > 0) Debug.Log($"[WeaponFiringQueue] Queue cleared ({count} commands)");
        OnQueueCleared?.Invoke();
    }

    /// <summary>
    /// Get the total heat cost of all queued commands.
    /// </summary>
    public int GetQueuedHeatCost()
    {
        int totalHeat = 0;
        foreach (var cmd in queuedCommands)
        {
            if (cmd.Weapon != null)
            {
                totalHeat += cmd.HeatCost;
            }
        }
        return totalHeat;
    }

    /// <summary>
    /// Get queued heat cost for a specific ship.
    /// </summary>
    public int GetQueuedHeatCostForShip(Ship ship)
    {
        int totalHeat = 0;
        foreach (var cmd in queuedCommands)
        {
            if (cmd.Weapon != null && cmd.Weapon.OwnerShip == ship)
            {
                totalHeat += cmd.HeatCost;
            }
        }
        return totalHeat;
    }

    /// <summary>
    /// Get all queued commands for a specific ship.
    /// </summary>
    public List<WeaponFireCommand> GetCommandsForShip(Ship ship)
    {
        var result = new List<WeaponFireCommand>();
        foreach (var cmd in queuedCommands)
        {
            if (cmd.Weapon != null && cmd.Weapon.OwnerShip == ship)
            {
                result.Add(cmd);
            }
        }
        return result;
    }

    #endregion

    #region Execution

    /// <summary>
    /// Execute the queued weapon commands.
    /// Called by CombatCoordinator during Simulation Phase.
    /// </summary>
    public IEnumerator ExecuteQueue()
    {
        if (queuedCommands.Count == 0)
        {
            if (logEvents) Debug.Log("[WeaponFiringQueue] No commands to execute");
            yield break;
        }

        isExecuting = true;
        OnQueueExecutionStarted?.Invoke();

        if (logEvents) Debug.Log($"[WeaponFiringQueue] Executing {queuedCommands.Count} commands...");

        // Sort by spin-up time (lower spin-up fires first)
        queuedCommands.Sort((a, b) =>
            (a.Weapon?.SpinUpTime ?? 0f).CompareTo(b.Weapon?.SpinUpTime ?? 0f));

        // Execute commands grouped by spin-up time
        var commandsBySpinUp = GroupBySpinUpTime();

        foreach (var group in commandsBySpinUp)
        {
            float spinUpTime = group.Key;
            var commands = group.Value;

            if (logEvents) Debug.Log($"[WeaponFiringQueue] Firing weapons with spin-up {spinUpTime:F2}s ({commands.Count} weapons)");

            // Start all weapons in this spin-up group
            var activeCoroutines = new List<Coroutine>();
            foreach (var cmd in commands)
            {
                if (ValidateCommandAtExecution(cmd))
                {
                    // Set target on weapon
                    cmd.Weapon.SetTarget(cmd.Target);

                    // Start firing coroutine
                    var coroutine = StartCoroutine(ExecuteCommand(cmd));
                    activeCoroutines.Add(coroutine);
                }
                else
                {
                    if (logEvents) Debug.LogWarning($"[WeaponFiringQueue] Skipped (invalid): {cmd}");
                    OnCommandExecuted?.Invoke(cmd, false);
                }
            }

            // Wait for all weapons in this group to complete
            if (spinUpTime > 0f)
            {
                yield return new WaitForSeconds(spinUpTime + 0.1f);
            }
            else
            {
                yield return null; // Wait one frame for instant weapons
            }
        }

        isExecuting = false;
        OnQueueExecutionCompleted?.Invoke();

        if (logEvents) Debug.Log("[WeaponFiringQueue] Execution complete");
    }

    /// <summary>
    /// Group commands by spin-up time for efficient execution.
    /// </summary>
    private Dictionary<float, List<WeaponFireCommand>> GroupBySpinUpTime()
    {
        var groups = new Dictionary<float, List<WeaponFireCommand>>();

        foreach (var cmd in queuedCommands)
        {
            float spinUp = cmd.Weapon?.SpinUpTime ?? 0f;

            if (!groups.ContainsKey(spinUp))
            {
                groups[spinUp] = new List<WeaponFireCommand>();
            }
            groups[spinUp].Add(cmd);
        }

        return groups;
    }

    /// <summary>
    /// Validate a command right before execution.
    /// Checks arc and range (ship may have moved since queuing).
    /// </summary>
    private bool ValidateCommandAtExecution(WeaponFireCommand cmd)
    {
        if (!cmd.IsValid)
        {
            if (logEvents) Debug.LogWarning($"[WeaponFiringQueue] {cmd.Weapon?.WeaponName ?? "null"}: Command no longer valid");
            return false;
        }

        // Check arc (ship may have moved)
        if (!cmd.Weapon.IsInArc(cmd.Target.transform.position))
        {
            if (logEvents) Debug.LogWarning($"[WeaponFiringQueue] {cmd.Weapon.WeaponName}: Target out of arc after movement");
            return false;
        }

        // Check range
        if (!cmd.Weapon.IsInRange(cmd.Target.transform.position))
        {
            if (logEvents) Debug.LogWarning($"[WeaponFiringQueue] {cmd.Weapon.WeaponName}: Target out of range after movement");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Execute a single weapon command.
    /// </summary>
    private IEnumerator ExecuteCommand(WeaponFireCommand cmd)
    {
        if (logEvents) Debug.Log($"[WeaponFiringQueue] Executing: {cmd}");

        yield return StartCoroutine(cmd.Weapon.FireWithSpinUp());

        OnCommandExecuted?.Invoke(cmd, true);
    }

    #endregion

    #region Turn Event Handlers

    protected override void HandleCommandPhaseStart()
    {
        // Queue is ready to accept commands
        if (logEvents) Debug.Log("[WeaponFiringQueue] Command Phase - ready for weapon orders");
    }

    protected override void HandleSimulationPhaseStart()
    {
        // Execution is handled by CombatCoordinator calling ExecuteQueue()
        if (logEvents) Debug.Log($"[WeaponFiringQueue] Simulation Phase - {queuedCommands.Count} weapons queued");
    }

    protected override void HandleTurnEnd(int completedTurnNumber)
    {
        // Clear queue at end of turn
        ClearQueue();
        if (logEvents) Debug.Log($"[WeaponFiringQueue] Turn {completedTurnNumber} ended - queue cleared");
    }

    #endregion
}
