using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the turn-based game flow with Command and Simulation phases.
/// Controls when players can plan movements and when movements are executed.
/// Central coordinator for all phase transitions - systems subscribe to events.
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Header("Phase Settings")]
    [SerializeField] private float simulationDuration = 3f;

    [Header("Collision Detection")]
    [SerializeField] private float collisionThreshold = 2f;

    // Singleton-style instance
    public static TurnManager Instance { get; private set; }

    #region Events

    /// <summary>
    /// Fired at the start of each new turn, before Command Phase begins.
    /// Parameter: turn number (1-indexed)
    /// </summary>
    public event Action<int> OnTurnStart;

    /// <summary>
    /// Fired when Command Phase begins. UI should enable planning controls.
    /// </summary>
    public event Action OnCommandPhaseStart;

    /// <summary>
    /// Fired when Simulation Phase begins. Systems should execute queued actions.
    /// </summary>
    public event Action OnSimulationPhaseStart;

    /// <summary>
    /// Fired when Simulation Phase ends, before cleanup.
    /// </summary>
    public event Action OnSimulationPhaseEnd;

    /// <summary>
    /// Fired at the end of a turn, after cleanup.
    /// Parameter: completed turn number
    /// </summary>
    public event Action<int> OnTurnEnd;

    /// <summary>
    /// Fired when simulation progress updates.
    /// Parameter: progress 0-1
    /// </summary>
    public event Action<float> OnSimulationProgress;

    #endregion

    #region Properties

    /// <summary>
    /// Current turn number (1-indexed, increments after each complete turn).
    /// </summary>
    public int CurrentTurn { get; private set; } = 1;

    /// <summary>
    /// Current phase of the turn (Command, Simulation, or TurnEnd).
    /// </summary>
    public TurnPhase CurrentPhase { get; private set; }

    /// <summary>
    /// Progress through simulation phase (0 = just started, 1 = complete).
    /// Only valid during Simulation phase.
    /// </summary>
    public float SimulationProgress { get; private set; }

    /// <summary>
    /// Duration of the simulation phase in seconds.
    /// </summary>
    public float SimulationDuration => simulationDuration;

    /// <summary>
    /// Whether the game is currently in Command Phase.
    /// </summary>
    public bool IsCommandPhase => CurrentPhase == TurnPhase.Command;

    /// <summary>
    /// Whether the game is currently in Simulation Phase.
    /// </summary>
    public bool IsSimulationPhase => CurrentPhase == TurnPhase.Simulation;

    #endregion

    // Legacy property for backwards compatibility
    public Phase LegacyCurrentPhase => CurrentPhase == TurnPhase.Command ? Phase.Command : Phase.Simulation;

    /// <summary>
    /// Legacy game phase enumeration for backwards compatibility.
    /// </summary>
    public enum Phase
    {
        Command,
        Simulation
    }

    // Cached ship references
    private Ship[] allShips;

    // Simulation timer
    private float simulationTimer = 0f;

    /// <summary>
    /// Initialize singleton and find all ships in the scene.
    /// </summary>
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

    /// <summary>
    /// Cache all ship components in the scene and start first turn.
    /// </summary>
    private void Start()
    {
        allShips = FindObjectsByType<Ship>(FindObjectsSortMode.None);
        CurrentPhase = TurnPhase.Command;
        CurrentTurn = 1;
        SimulationProgress = 0f;

        Debug.Log($"TurnManager initialized. Found {allShips.Length} ships. Starting Turn {CurrentTurn} in Command Phase.");

        // Fire initial events
        OnTurnStart?.Invoke(CurrentTurn);
        OnCommandPhaseStart?.Invoke();
    }

    /// <summary>
    /// Update loop handles simulation timer and progress tracking.
    /// </summary>
    private void Update()
    {
        if (CurrentPhase == TurnPhase.Simulation)
        {
            simulationTimer += Time.deltaTime;
            SimulationProgress = Mathf.Clamp01(simulationTimer / simulationDuration);
            OnSimulationProgress?.Invoke(SimulationProgress);

            if (simulationTimer >= simulationDuration)
            {
                EndSimulation();
            }
        }
    }

    /// <summary>
    /// Ends the Command Phase, checks for collisions, and begins Simulation.
    /// Called when player clicks "End Turn" button.
    /// </summary>
    public void EndCommandPhase()
    {
        if (CurrentPhase != TurnPhase.Command)
        {
            Debug.LogWarning("Cannot end Command Phase - not currently in Command Phase");
            return;
        }

        Debug.Log($"Turn {CurrentTurn}: Ending Command Phase...");

        // Check for collisions before executing
        CheckCollisions();

        // Switch to Simulation phase
        CurrentPhase = TurnPhase.Simulation;
        simulationTimer = 0f;
        SimulationProgress = 0f;

        // Fire simulation start event BEFORE executing moves
        // This allows CombatCoordinator and other systems to prepare
        OnSimulationPhaseStart?.Invoke();

        // Execute all planned moves (with null safety)
        if (allShips != null)
        {
            foreach (Ship ship in allShips)
            {
                if (ship != null && ship.gameObject.activeSelf)
                {
                    ship.ExecuteMove();
                }
            }
        }

        // Execute all queued abilities
        StartCoroutine(ExecuteAllAbilities());

        Debug.Log($"Turn {CurrentTurn}: Simulation Phase started. Duration: {simulationDuration}s");
    }

    /// <summary>
    /// Alias for EndCommandPhase for clearer API.
    /// </summary>
    public void StartSimulation()
    {
        EndCommandPhase();
    }

    /// <summary>
    /// Execute all queued abilities on all ships.
    /// </summary>
    private IEnumerator ExecuteAllAbilities()
    {
        // Execute abilities on all ships in parallel
        List<Coroutine> abilityCoroutines = new List<Coroutine>();

        foreach (Ship ship in allShips)
        {
            if (ship != null && ship.gameObject.activeSelf && ship.AbilitySystem != null)
            {
                Coroutine coroutine = StartCoroutine(ship.AbilitySystem.ExecuteQueuedAbilities());
                abilityCoroutines.Add(coroutine);
            }
        }

        // Wait for all abilities to complete
        foreach (Coroutine coroutine in abilityCoroutines)
        {
            yield return coroutine;
        }

        Debug.Log("All abilities executed");
    }

    /// <summary>
    /// Checks for collisions between all ships' planned positions.
    /// Updates projection colors to indicate collisions.
    /// </summary>
    private void CheckCollisions()
    {
        Debug.Log("Checking for collisions...");

        // Safety check for uninitialized state
        if (allShips == null || allShips.Length == 0)
        {
            Debug.LogWarning("CheckCollisions called but no ships registered");
            return;
        }

        // Reset all collision markings first
        foreach (Ship ship in allShips)
        {
            if (ship != null)
            {
                ship.MarkCollision(false);
            }
        }

        // Nested loop to check all pairs
        for (int i = 0; i < allShips.Length; i++)
        {
            for (int j = i + 1; j < allShips.Length; j++)
            {
                Ship shipA = allShips[i];
                Ship shipB = allShips[j];

                if (shipA == null || shipB == null) continue;

                // Only check if both ships have planned moves
                if (shipA.HasPlannedMove && shipB.HasPlannedMove)
                {
                    // Calculate distance between planned positions
                    float distance = Vector3.Distance(shipA.PlannedPosition, shipB.PlannedPosition);

                    // Check for collision
                    if (distance < collisionThreshold)
                    {
                        // Mark both ships as colliding
                        shipA.MarkCollision(true);
                        shipB.MarkCollision(true);

                        Debug.LogWarning($"Collision detected between {shipA.gameObject.name} and {shipB.gameObject.name}! Distance: {distance:F2}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ends the Simulation Phase and returns to Command Phase.
    /// Resets all ships for new turn planning.
    /// Applies heat cooling, shield regeneration, and heat damage.
    /// </summary>
    private void EndSimulation()
    {
        Debug.Log($"Turn {CurrentTurn}: Ending Simulation Phase...");

        // Fire simulation end event
        OnSimulationPhaseEnd?.Invoke();

        // Transition to TurnEnd phase for cleanup
        CurrentPhase = TurnPhase.TurnEnd;

        // Apply end-of-turn effects for all ships
        foreach (Ship ship in allShips)
        {
            if (ship != null && ship.gameObject.activeSelf)
            {
                // Tick ability cooldowns
                if (ship.AbilitySystem != null)
                {
                    ship.AbilitySystem.TickAllCooldowns();
                }

                // Tick weapon cooldowns
                if (ship.WeaponManager != null)
                {
                    ship.WeaponManager.TickAllCooldowns();
                }

                // Apply passive cooling
                if (ship.HeatManager != null)
                {
                    ship.HeatManager.ApplyPassiveCooling();
                }

                // Regenerate shields
                ship.RegenerateShields();

                // Apply heat damage (must be after cooling)
                ship.ApplyHeatDamage();
            }
        }

        // Fire turn end event
        int completedTurn = CurrentTurn;
        OnTurnEnd?.Invoke(completedTurn);

        // Increment turn counter
        CurrentTurn++;

        // Switch to Command phase
        CurrentPhase = TurnPhase.Command;
        SimulationProgress = 0f;

        // Reset all ships for new turn
        foreach (Ship ship in allShips)
        {
            if (ship != null && ship.gameObject.activeSelf)
            {
                ship.ResetPlannedMove();
            }
        }

        Debug.Log($"Turn {completedTurn} complete. Starting Turn {CurrentTurn} in Command Phase.");

        // Fire new turn events
        OnTurnStart?.Invoke(CurrentTurn);
        OnCommandPhaseStart?.Invoke();
    }

    /// <summary>
    /// Force-ends the current turn immediately (for testing/debugging).
    /// </summary>
    public void ForceEndTurn()
    {
        if (CurrentPhase == TurnPhase.Command)
        {
            EndCommandPhase();
        }
        else if (CurrentPhase == TurnPhase.Simulation)
        {
            simulationTimer = simulationDuration; // Will trigger EndSimulation on next Update
        }
    }

    /// <summary>
    /// Gets all ships in the scene.
    /// </summary>
    public Ship[] GetAllShips()
    {
        return allShips;
    }

    /// <summary>
    /// Refreshes the cached ship list. Call this if ships are added/removed dynamically.
    /// </summary>
    public void RefreshShipList()
    {
        allShips = FindObjectsByType<Ship>(FindObjectsSortMode.None);
        Debug.Log($"Ship list refreshed. Found {allShips.Length} ships.");
    }

    /// <summary>
    /// Registers a new ship with the turn manager.
    /// </summary>
    public void RegisterShip(Ship ship)
    {
        if (ship == null) return;

        var shipList = new List<Ship>(allShips ?? new Ship[0]);
        if (!shipList.Contains(ship))
        {
            shipList.Add(ship);
            allShips = shipList.ToArray();
            Debug.Log($"Ship {ship.gameObject.name} registered with TurnManager.");
        }
    }

    /// <summary>
    /// Unregisters a ship from the turn manager.
    /// </summary>
    public void UnregisterShip(Ship ship)
    {
        if (ship == null || allShips == null) return;

        var shipList = new List<Ship>(allShips);
        if (shipList.Remove(ship))
        {
            allShips = shipList.ToArray();
            Debug.Log($"Ship {ship.gameObject.name} unregistered from TurnManager.");
        }
    }
}
