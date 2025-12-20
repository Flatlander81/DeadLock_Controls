using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Coordinates ship movement execution during the Simulation phase.
/// Works with TurnManager and CombatCoordinator to execute planned movements.
/// Provides movement-aware position queries for weapon arc validation.
///
/// Part of Phase 3.5.4 - Movement and Weapon Arc Integration.
/// </summary>
public class MovementExecutor : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float simulationDuration = 3f;

    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private CombatCoordinator combatCoordinator;

    // Events for movement coordination
    public event Action OnMovementExecutionStarted;
    public event Action OnMovementExecutionComplete;
    public event Action<Ship> OnShipMovementStarted;
    public event Action<Ship> OnShipMovementComplete;

    // Movement state
    private List<Ship> movingShips = new List<Ship>();
    private bool isExecuting = false;
    private float executionStartTime;

    // Properties
    public bool IsExecuting => isExecuting;
    public float SimulationDuration => simulationDuration;
    public float ExecutionProgress => isExecuting ? Mathf.Clamp01((Time.time - executionStartTime) / simulationDuration) : 0f;

    private void Start()
    {
        // Auto-discover references
        if (turnManager == null)
        {
            turnManager = FindAnyObjectByType<TurnManager>();
        }

        if (combatCoordinator == null)
        {
            combatCoordinator = FindAnyObjectByType<CombatCoordinator>();
        }

        // Subscribe to turn events
        if (turnManager != null)
        {
            turnManager.OnSimulationPhaseStart += HandleSimulationStarted;
            turnManager.OnTurnEnd += HandleTurnEnded;
        }

        Debug.Log("MovementExecutor initialized");
    }

    private void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnSimulationPhaseStart -= HandleSimulationStarted;
            turnManager.OnTurnEnd -= HandleTurnEnded;
        }
    }

    /// <summary>
    /// Called when simulation phase starts - begin executing all planned movements.
    /// </summary>
    private void HandleSimulationStarted()
    {
        Debug.Log("[MovementExecutor] Simulation started - executing planned movements");
        ExecuteAllPlannedMovements();
    }

    /// <summary>
    /// Called when turn ends - reset movement state.
    /// </summary>
    private void HandleTurnEnded(int turnNumber)
    {
        isExecuting = false;
        movingShips.Clear();
    }

    /// <summary>
    /// Execute all planned movements for ships in the scene.
    /// </summary>
    public void ExecuteAllPlannedMovements()
    {
        movingShips.Clear();

        // Find all ships with planned movements
        Ship[] allShips = FindObjectsByType<Ship>(FindObjectsSortMode.None);

        foreach (var ship in allShips)
        {
            if (ship.HasPlannedMove && ship.CanMove())
            {
                // Sync duration with our simulation duration
                ship.SetMoveDuration(simulationDuration);
                movingShips.Add(ship);
            }
        }

        if (movingShips.Count > 0)
        {
            isExecuting = true;
            executionStartTime = Time.time;

            Debug.Log($"[MovementExecutor] Executing {movingShips.Count} ship movements");

            // Start all movements simultaneously
            foreach (var ship in movingShips)
            {
                ship.ExecuteMove();
                OnShipMovementStarted?.Invoke(ship);
            }

            OnMovementExecutionStarted?.Invoke();
        }
        else
        {
            Debug.Log("[MovementExecutor] No ships have planned movements");
        }
    }

    private void Update()
    {
        if (!isExecuting) return;

        // Check if all ships have completed their movements
        bool allComplete = true;
        foreach (var ship in movingShips)
        {
            if (ship.IsExecutingMove)
            {
                allComplete = false;
                break;
            }
        }

        // Also check if simulation duration has elapsed
        if (allComplete || (Time.time - executionStartTime) >= simulationDuration)
        {
            CompleteExecution();
        }
    }

    /// <summary>
    /// Complete the movement execution phase.
    /// </summary>
    private void CompleteExecution()
    {
        if (!isExecuting) return;

        isExecuting = false;

        foreach (var ship in movingShips)
        {
            OnShipMovementComplete?.Invoke(ship);
        }

        Debug.Log("[MovementExecutor] All ship movements complete");
        OnMovementExecutionComplete?.Invoke();
    }

    /// <summary>
    /// Gets a ship's position at a specific time during the current simulation.
    /// Time is normalized (0-1) representing progress through the simulation.
    /// </summary>
    public Vector3 GetShipPositionAtTime(Ship ship, float normalizedTime)
    {
        if (ship == null) return Vector3.zero;
        return ship.GetPositionAtTime(normalizedTime);
    }

    /// <summary>
    /// Gets a ship's rotation at a specific time during the current simulation.
    /// </summary>
    public Quaternion GetShipRotationAtTime(Ship ship, float normalizedTime)
    {
        if (ship == null) return Quaternion.identity;
        return ship.GetRotationAtTime(normalizedTime);
    }

    /// <summary>
    /// Calculates the optimal firing time for a weapon targeting a moving ship.
    /// Samples positions along both ship trajectories to find when target is in arc.
    /// </summary>
    /// <param name="firingShip">Ship with the weapon</param>
    /// <param name="targetShip">Ship being targeted</param>
    /// <param name="weapon">Weapon to check arc for</param>
    /// <param name="sampleCount">Number of samples along trajectory (higher = more accurate)</param>
    /// <returns>Optimal normalized firing time (0-1), or -1 if never in arc</returns>
    public float CalculateOptimalFiringTime(Ship firingShip, Ship targetShip, WeaponSystem weapon, int sampleCount = 10)
    {
        if (firingShip == null || targetShip == null || weapon == null)
        {
            return -1f;
        }

        float bestTime = -1f;
        float bestAngle = float.MaxValue;

        for (int i = 0; i <= sampleCount; i++)
        {
            float t = i / (float)sampleCount;

            // Get positions at this time
            Vector3 firingPos = firingShip.GetPositionAtTime(t);
            Quaternion firingRot = firingShip.GetRotationAtTime(t);
            Vector3 targetPos = targetShip.GetPositionAtTime(t);

            // Calculate weapon's forward direction at this time
            // Weapon hardpoint inherits ship's rotation
            Vector3 weaponForward = firingRot * Vector3.forward;

            // Calculate angle to target
            Vector3 toTarget = (targetPos - firingPos).normalized;
            float angle = Vector3.Angle(weaponForward, toTarget);

            // Check if within arc
            if (angle < (weapon.FiringArc / 2f))
            {
                // Within arc - prefer the time with best angle
                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    bestTime = t;
                }
            }
        }

        return bestTime;
    }

    /// <summary>
    /// Checks if a weapon will be in arc at any point during the simulation.
    /// </summary>
    public bool WillBeInArc(Ship firingShip, Ship targetShip, WeaponSystem weapon, int sampleCount = 10)
    {
        return CalculateOptimalFiringTime(firingShip, targetShip, weapon, sampleCount) >= 0f;
    }

    /// <summary>
    /// Finds all valid firing windows during the simulation.
    /// Returns list of (startTime, endTime) tuples where weapon is in arc.
    /// </summary>
    public List<(float startTime, float endTime)> FindFiringWindows(Ship firingShip, Ship targetShip, WeaponSystem weapon, int sampleCount = 20)
    {
        var windows = new List<(float startTime, float endTime)>();

        if (firingShip == null || targetShip == null || weapon == null)
        {
            return windows;
        }

        bool wasInArc = false;
        float windowStart = 0f;

        for (int i = 0; i <= sampleCount; i++)
        {
            float t = i / (float)sampleCount;

            // Get positions at this time
            Vector3 firingPos = firingShip.GetPositionAtTime(t);
            Quaternion firingRot = firingShip.GetRotationAtTime(t);
            Vector3 targetPos = targetShip.GetPositionAtTime(t);

            // Calculate weapon's forward direction at this time
            Vector3 weaponForward = firingRot * Vector3.forward;

            // Calculate angle to target
            Vector3 toTarget = (targetPos - firingPos).normalized;
            float angle = Vector3.Angle(weaponForward, toTarget);

            bool isInArc = angle < (weapon.FiringArc / 2f);

            if (isInArc && !wasInArc)
            {
                // Window opens
                windowStart = t;
                wasInArc = true;
            }
            else if (!isInArc && wasInArc)
            {
                // Window closes
                windows.Add((windowStart, t));
                wasInArc = false;
            }
        }

        // Close final window if still open
        if (wasInArc)
        {
            windows.Add((windowStart, 1f));
        }

        return windows;
    }

    /// <summary>
    /// Checks if a weapon will be in range at a specific time.
    /// </summary>
    public bool IsInRangeAtTime(Ship firingShip, Ship targetShip, WeaponSystem weapon, float normalizedTime)
    {
        if (firingShip == null || targetShip == null || weapon == null)
        {
            return false;
        }

        Vector3 firingPos = firingShip.GetPositionAtTime(normalizedTime);
        Vector3 targetPos = targetShip.GetPositionAtTime(normalizedTime);

        float distance = Vector3.Distance(firingPos, targetPos);
        return distance <= weapon.MaxRange;
    }

    /// <summary>
    /// Calculates distance between two ships at a specific time.
    /// </summary>
    public float GetDistanceAtTime(Ship ship1, Ship ship2, float normalizedTime)
    {
        if (ship1 == null || ship2 == null)
        {
            return float.MaxValue;
        }

        Vector3 pos1 = ship1.GetPositionAtTime(normalizedTime);
        Vector3 pos2 = ship2.GetPositionAtTime(normalizedTime);

        return Vector3.Distance(pos1, pos2);
    }

    /// <summary>
    /// Sets simulation duration. Should match TurnManager's simulation duration.
    /// </summary>
    public void SetSimulationDuration(float duration)
    {
        simulationDuration = duration;
    }
}
