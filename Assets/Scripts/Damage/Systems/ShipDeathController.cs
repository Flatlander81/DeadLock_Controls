using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Monitors ship state for death conditions and handles death/disabled states.
/// Death Conditions:
/// - Core Breach: Reactor destroyed = instant death
/// - Combat Ineffective: All weapons AND engines destroyed = disabled
/// </summary>
public class ShipDeathController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ship ship;
    [SerializeField] private SectionManager sectionManager;
    [SerializeField] private SystemDegradationManager degradationManager;

    [Header("State")]
    [SerializeField] private bool isDestroyed = false;
    [SerializeField] private bool isDisabled = false;
    [SerializeField] private DeathCause deathCause = DeathCause.None;

    // Events
    /// <summary>Fired when ship is destroyed (cannot continue).</summary>
    public event Action<Ship, DeathCause> OnShipDestroyed;

    /// <summary>Fired when ship is disabled (combat ineffective but not dead).</summary>
    public event Action<Ship> OnShipDisabled;

    /// <summary>Fired when ship state changes (for UI updates).</summary>
    public event Action<Ship, bool, bool> OnShipStateChanged;

    // Properties
    public bool IsDestroyed => isDestroyed;
    public bool IsDisabled => isDisabled;
    public DeathCause Cause => deathCause;

    /// <summary>
    /// Causes of ship destruction.
    /// </summary>
    public enum DeathCause
    {
        None,
        CoreBreach,      // Reactor destroyed
        CoreBreached,    // Core section breached
        AllSectionsBreached,
        HullDepleted
    }

    private void Awake()
    {
        if (ship == null)
        {
            ship = GetComponent<Ship>();
        }

        if (sectionManager == null)
        {
            sectionManager = GetComponent<SectionManager>();
            if (sectionManager == null)
            {
                sectionManager = GetComponentInParent<SectionManager>();
            }
        }

        if (degradationManager == null)
        {
            degradationManager = GetComponent<SystemDegradationManager>();
            if (degradationManager == null)
            {
                degradationManager = GetComponentInParent<SystemDegradationManager>();
            }
        }
    }

    private void Start()
    {
        // Subscribe to events for death condition monitoring
        if (sectionManager != null)
        {
            sectionManager.OnSectionBreached += HandleSectionBreached;
        }

        if (degradationManager != null)
        {
            degradationManager.OnDegradationChanged += CheckCombatIneffective;

            // Subscribe to reactor core breach events
            foreach (var reactor in degradationManager.GetReactors())
            {
                reactor.OnCoreBreach += HandleCoreBreach;
            }
        }
    }

    /// <summary>
    /// Sets references manually (for testing or setup).
    /// </summary>
    public void SetReferences(Ship s, SectionManager sm, SystemDegradationManager dm)
    {
        ship = s;
        sectionManager = sm;
        degradationManager = dm;
    }

    /// <summary>
    /// Subscribe to a reactor's OnCoreBreach event (for testing or dynamic reactor registration).
    /// </summary>
    public void SubscribeToReactor(MountedReactor reactor)
    {
        if (reactor != null)
        {
            reactor.OnCoreBreach += HandleCoreBreach;
        }
    }

    /// <summary>
    /// Subscribe to section manager events (call after setting references if not using Start).
    /// </summary>
    public void SubscribeToEvents()
    {
        if (sectionManager != null)
        {
            sectionManager.OnSectionBreached += HandleSectionBreached;
        }

        if (degradationManager != null)
        {
            degradationManager.OnDegradationChanged += CheckCombatIneffective;

            // Subscribe to reactor core breach events
            foreach (var reactor in degradationManager.GetReactors())
            {
                reactor.OnCoreBreach += HandleCoreBreach;
            }
        }
    }

    /// <summary>
    /// Checks all death conditions. Called after damage is applied.
    /// </summary>
    public void CheckDeathConditions()
    {
        if (isDestroyed) return; // Already dead

        // Check Core Breach (reactor destroyed)
        if (IsCoreBreached())
        {
            TriggerDeath(DeathCause.CoreBreach);
            return;
        }

        // Check Core section breached
        if (sectionManager != null && sectionManager.IsCoreBreached())
        {
            TriggerDeath(DeathCause.CoreBreached);
            return;
        }

        // Check all sections breached
        if (sectionManager != null && sectionManager.AreAllSectionsBreached())
        {
            TriggerDeath(DeathCause.AllSectionsBreached);
            return;
        }

        // Check combat ineffective (all weapons AND engines destroyed)
        CheckCombatIneffective();
    }

    /// <summary>
    /// Checks if the reactor has been destroyed (Core Breach).
    /// </summary>
    public bool IsCoreBreached()
    {
        if (degradationManager == null) return false;

        foreach (var reactor in degradationManager.GetReactors())
        {
            if (reactor.IsDestroyed)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the ship is combat ineffective (all weapons AND engines destroyed).
    /// </summary>
    public bool IsCombatIneffective()
    {
        if (degradationManager == null) return false;

        bool allWeaponsDestroyed = AreAllWeaponsDestroyed();
        bool allEnginesDestroyed = AreAllEnginesDestroyed();

        return allWeaponsDestroyed && allEnginesDestroyed;
    }

    /// <summary>
    /// Checks if all weapons are destroyed.
    /// </summary>
    public bool AreAllWeaponsDestroyed()
    {
        if (degradationManager == null) return false;

        var weapons = degradationManager.GetWeapons();
        if (weapons.Count == 0) return false; // No weapons = not destroyed

        foreach (var weapon in weapons)
        {
            if (!weapon.IsDestroyed)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if all engines are destroyed.
    /// </summary>
    public bool AreAllEnginesDestroyed()
    {
        if (degradationManager == null) return false;

        var engines = degradationManager.GetEngines();
        if (engines.Count == 0) return false; // No engines = not destroyed

        foreach (var engine in engines)
        {
            if (!engine.IsDestroyed)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Triggers ship death with the specified cause.
    /// </summary>
    private void TriggerDeath(DeathCause cause)
    {
        if (isDestroyed) return;

        isDestroyed = true;
        deathCause = cause;

        Debug.LogError($"[ShipDeathController] SHIP DESTROYED! Cause: {cause}");

        OnShipDestroyed?.Invoke(ship, cause);
        OnShipStateChanged?.Invoke(ship, isDestroyed, isDisabled);

        // Call Ship.Die()
        if (ship != null)
        {
            ship.Die();
        }
    }

    /// <summary>
    /// Triggers ship disabled state.
    /// </summary>
    private void TriggerDisabled()
    {
        if (isDisabled || isDestroyed) return;

        isDisabled = true;

        Debug.LogWarning($"[ShipDeathController] Ship DISABLED - Combat Ineffective!");

        OnShipDisabled?.Invoke(ship);
        OnShipStateChanged?.Invoke(ship, isDestroyed, isDisabled);
    }

    /// <summary>
    /// Checks combat ineffective state and triggers disabled if needed.
    /// </summary>
    private void CheckCombatIneffective()
    {
        if (isDestroyed || isDisabled) return;

        if (IsCombatIneffective())
        {
            TriggerDisabled();
        }
    }

    /// <summary>
    /// Handles section breached event.
    /// </summary>
    private void HandleSectionBreached(ShipSection section)
    {
        CheckDeathConditions();
    }

    /// <summary>
    /// Handles reactor core breach event (reactor destroyed).
    /// </summary>
    private void HandleCoreBreach(MountedReactor reactor)
    {
        TriggerDeath(DeathCause.CoreBreach);
    }

    /// <summary>
    /// Resets the death controller state (for testing or respawning).
    /// </summary>
    public void Reset()
    {
        isDestroyed = false;
        isDisabled = false;
        deathCause = DeathCause.None;

        OnShipStateChanged?.Invoke(ship, isDestroyed, isDisabled);
    }

    /// <summary>
    /// Gets a summary of the ship's survival state.
    /// </summary>
    public string GetStatusSummary()
    {
        if (isDestroyed)
        {
            return $"DESTROYED ({deathCause})";
        }
        else if (isDisabled)
        {
            return "DISABLED (Combat Ineffective)";
        }
        else
        {
            string status = "OPERATIONAL";

            if (AreAllWeaponsDestroyed())
            {
                status += " [No Weapons]";
            }
            if (AreAllEnginesDestroyed())
            {
                status += " [No Engines]";
            }

            return status;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (sectionManager != null)
        {
            sectionManager.OnSectionBreached -= HandleSectionBreached;
        }

        if (degradationManager != null)
        {
            degradationManager.OnDegradationChanged -= CheckCombatIneffective;

            foreach (var reactor in degradationManager.GetReactors())
            {
                reactor.OnCoreBreach -= HandleCoreBreach;
            }
        }
    }
}
