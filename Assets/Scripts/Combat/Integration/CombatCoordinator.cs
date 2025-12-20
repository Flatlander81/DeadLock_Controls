using UnityEngine;
using System.Collections;

/// <summary>
/// Central coordinator for combat flow during simulation phase.
/// Orchestrates the execution order of movement, weapons, projectiles, and damage.
/// </summary>
public class CombatCoordinator : TurnEventSubscriber
{
    [Header("Simulation Timing")]
    [Tooltip("Portion of simulation duration dedicated to movement (0-1)")]
    [SerializeField] private float movementPortion = 0.3f;

    [Tooltip("Portion of simulation duration dedicated to weapon firing (0-1)")]
    [SerializeField] private float weaponsPortion = 0.2f;

    [Tooltip("Delay after movement before weapons fire")]
    [SerializeField] private float postMovementDelay = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool logEvents = true;

    // Singleton instance
    public static CombatCoordinator Instance { get; private set; }

    // Timing calculations
    private float movementDuration;
    private float weaponsDuration;
    private float projectileDuration;

    // State tracking
    private bool isSimulating;
    private Coroutine simulationCoroutine;

    /// <summary>
    /// Whether combat simulation is currently running.
    /// </summary>
    public bool IsSimulating => isSimulating;

    /// <summary>
    /// Current stage of the simulation.
    /// </summary>
    public SimulationStage CurrentStage { get; private set; } = SimulationStage.Idle;

    /// <summary>
    /// Stages within the simulation phase.
    /// </summary>
    public enum SimulationStage
    {
        Idle,           // Not simulating
        Movement,       // Ships moving to planned positions
        WeaponFiring,   // Weapons firing at targets
        ProjectileTravel, // Projectiles traveling
        DamageResolution, // Damage being applied
        Complete        // Simulation finished
    }

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
    /// Calculate timing portions when simulation starts.
    /// </summary>
    private void CalculateTiming()
    {
        if (TurnManager.Instance == null) return;

        float totalDuration = TurnManager.Instance.SimulationDuration;
        movementDuration = totalDuration * movementPortion;
        weaponsDuration = totalDuration * weaponsPortion;
        projectileDuration = totalDuration * (1f - movementPortion - weaponsPortion);
    }

    #region Event Handlers

    protected override void HandleCommandPhaseStart()
    {
        if (logEvents)
        {
            Debug.Log("[CombatCoordinator] Command Phase started - awaiting orders");
        }

        CurrentStage = SimulationStage.Idle;
        isSimulating = false;
    }

    protected override void HandleSimulationPhaseStart()
    {
        if (logEvents)
        {
            Debug.Log("[CombatCoordinator] Simulation Phase started - beginning combat sequence");
        }

        CalculateTiming();
        isSimulating = true;
        CurrentStage = SimulationStage.Movement;

        // Start the simulation sequence
        if (simulationCoroutine != null)
        {
            StopCoroutine(simulationCoroutine);
        }
        simulationCoroutine = StartCoroutine(ExecuteSimulation());
    }

    protected override void HandleSimulationPhaseEnd()
    {
        if (logEvents)
        {
            Debug.Log("[CombatCoordinator] Simulation Phase ended - combat complete");
        }

        CurrentStage = SimulationStage.Complete;
        isSimulating = false;

        if (simulationCoroutine != null)
        {
            StopCoroutine(simulationCoroutine);
            simulationCoroutine = null;
        }
    }

    protected override void HandleTurnEnd(int completedTurnNumber)
    {
        if (logEvents)
        {
            Debug.Log($"[CombatCoordinator] Turn {completedTurnNumber} ended");
        }

        CurrentStage = SimulationStage.Idle;
    }

    protected override void HandleSimulationProgress(float progress)
    {
        // Update stage based on progress
        if (!isSimulating) return;

        if (progress < movementPortion)
        {
            CurrentStage = SimulationStage.Movement;
        }
        else if (progress < movementPortion + weaponsPortion)
        {
            CurrentStage = SimulationStage.WeaponFiring;
        }
        else
        {
            CurrentStage = SimulationStage.ProjectileTravel;
        }
    }

    #endregion

    /// <summary>
    /// Main simulation execution coroutine.
    /// Orchestrates the order of combat operations.
    /// </summary>
    private IEnumerator ExecuteSimulation()
    {
        if (logEvents)
        {
            Debug.Log("[CombatCoordinator] === SIMULATION SEQUENCE BEGIN ===");
        }

        // Stage 1: Movement (handled by TurnManager calling Ship.ExecuteMove)
        CurrentStage = SimulationStage.Movement;
        if (logEvents)
        {
            Debug.Log($"[CombatCoordinator] Stage 1: Movement ({movementDuration:F1}s)");
        }

        yield return new WaitForSeconds(movementDuration);

        // Brief delay after movement settles
        yield return new WaitForSeconds(postMovementDelay);

        // Stage 2: Weapon Firing
        CurrentStage = SimulationStage.WeaponFiring;
        if (logEvents)
        {
            Debug.Log($"[CombatCoordinator] Stage 2: Weapon Firing ({weaponsDuration:F1}s)");
        }

        // Execute weapon firing queue here (will be implemented in Step 3.5.2)
        // For now, weapons are fired immediately via TargetingController
        yield return new WaitForSeconds(weaponsDuration);

        // Stage 3: Projectile Travel
        CurrentStage = SimulationStage.ProjectileTravel;
        if (logEvents)
        {
            Debug.Log($"[CombatCoordinator] Stage 3: Projectile Travel ({projectileDuration:F1}s)");
        }

        // Projectiles update automatically via ProjectileManager
        yield return new WaitForSeconds(projectileDuration);

        // Stage 4: Final Damage Resolution
        CurrentStage = SimulationStage.DamageResolution;
        if (logEvents)
        {
            Debug.Log("[CombatCoordinator] Stage 4: Damage Resolution");
        }

        // Damage is resolved as projectiles hit (handled by collision system)
        // This is mostly a marker for any final cleanup

        CurrentStage = SimulationStage.Complete;
        if (logEvents)
        {
            Debug.Log("[CombatCoordinator] === SIMULATION SEQUENCE COMPLETE ===");
        }

        simulationCoroutine = null;
    }

    /// <summary>
    /// Get the current simulation stage as a readable string.
    /// </summary>
    public string GetStageDescription()
    {
        return CurrentStage switch
        {
            SimulationStage.Idle => "Awaiting Orders",
            SimulationStage.Movement => "Ships Moving",
            SimulationStage.WeaponFiring => "Weapons Firing",
            SimulationStage.ProjectileTravel => "Projectiles In Flight",
            SimulationStage.DamageResolution => "Resolving Damage",
            SimulationStage.Complete => "Simulation Complete",
            _ => "Unknown"
        };
    }
}
