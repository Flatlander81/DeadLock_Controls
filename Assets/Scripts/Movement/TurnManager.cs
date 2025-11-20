using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the turn-based game flow with Command and Simulation phases.
/// Controls when players can plan movements and when movements are executed.
/// </summary>
public class TurnManager : MonoBehaviour
{
    /// <summary>
    /// Game phase enumeration.
    /// </summary>
    public enum Phase
    {
        Command,    // Players plan movements
        Simulation  // Movements are executed
    }

    [Header("Phase Settings")]
    [SerializeField] private float simulationDuration = 3f;

    [Header("Collision Detection")]
    [SerializeField] private float collisionThreshold = 2f;

    // Singleton-style instance
    public static TurnManager Instance { get; private set; }

    // Current phase
    public Phase CurrentPhase { get; private set; }

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
    /// Cache all ship components in the scene.
    /// </summary>
    private void Start()
    {
        allShips = FindObjectsOfType<Ship>();
        CurrentPhase = Phase.Command;
        Debug.Log($"TurnManager initialized. Found {allShips.Length} ships. Starting in Command Phase.");
    }

    /// <summary>
    /// Update loop handles simulation timer and auto-advance to Command phase.
    /// </summary>
    private void Update()
    {
        if (CurrentPhase == Phase.Simulation)
        {
            simulationTimer += Time.deltaTime;

            if (simulationTimer >= simulationDuration)
            {
                EndSimulation();
            }
        }
    }

    /// <summary>
    /// Ends the Command Phase, checks for collisions, and begins Simulation.
    /// </summary>
    public void EndCommandPhase()
    {
        if (CurrentPhase != Phase.Command)
        {
            Debug.LogWarning("Cannot end Command Phase - not currently in Command Phase");
            return;
        }

        Debug.Log("Ending Command Phase...");

        // Check for collisions before executing
        CheckCollisions();

        // Switch to Simulation phase
        CurrentPhase = Phase.Simulation;
        simulationTimer = 0f;

        // Execute all planned moves
        foreach (Ship ship in allShips)
        {
            ship.ExecuteMove();
        }

        // Execute all queued abilities
        StartCoroutine(ExecuteAllAbilities());

        Debug.Log($"Simulation Phase started. Duration: {simulationDuration}s");
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

        // TODO: Weapon firing will be triggered by Track C (Targeting System)
        // Track C will call WeaponManager.FireGroup() or FireAlphaStrike() based on player input
        // For now, weapon cooldowns will tick at end of turn like abilities
    }

    /// <summary>
    /// Checks for collisions between all ships' planned positions.
    /// Updates projection colors to indicate collisions.
    /// </summary>
    private void CheckCollisions()
    {
        Debug.Log("Checking for collisions...");

        // Reset all collision markings first
        foreach (Ship ship in allShips)
        {
            ship.MarkCollision(false);
        }

        // Nested loop to check all pairs
        for (int i = 0; i < allShips.Length; i++)
        {
            for (int j = i + 1; j < allShips.Length; j++)
            {
                Ship shipA = allShips[i];
                Ship shipB = allShips[j];

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
        Debug.Log("Ending Simulation Phase...");

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

        CurrentPhase = Phase.Command;

        // Reset all ships for new turn
        foreach (Ship ship in allShips)
        {
            if (ship != null && ship.gameObject.activeSelf)
            {
                ship.ResetPlannedMove();
            }
        }

        Debug.Log("Returned to Command Phase. Ready for new turn.");
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
        allShips = FindObjectsOfType<Ship>();
        Debug.Log($"Ship list refreshed. Found {allShips.Length} ships.");
    }
}
