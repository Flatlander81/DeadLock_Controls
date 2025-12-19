using UnityEngine;
using System;

/// <summary>
/// Mounted reactor core system. Critical system for ship survival.
/// When damaged: Heat capacity reduced OR generates passive heat.
/// When destroyed: CORE BREACH - Ship destruction!
/// </summary>
public class MountedReactor : MountedSystem
{
    [Header("Reactor Link")]
    [SerializeField] private Ship linkedShip;
    [SerializeField] private HeatManager linkedHeatManager;

    [Header("Degradation Effects")]
    [SerializeField] private bool heatCapacityPenalty = false; // True = capacity reduced, False = passive heat
    [SerializeField] private float damagedHeatCapacityMultiplier = 0.7f;
    [SerializeField] private float passiveHeatGeneration = 10f; // Heat per turn when damaged

    // Events
    /// <summary>Fired when reactor state changes.</summary>
    public event Action<MountedReactor> OnReactorStateChanged;

    /// <summary>Fired when core breach occurs (reactor destroyed).</summary>
    public event Action<MountedReactor> OnCoreBreach;

    // Properties
    public Ship LinkedShip => linkedShip;
    public HeatManager LinkedHeatManager => linkedHeatManager;
    public bool HasHeatCapacityPenalty => heatCapacityPenalty;

    /// <summary>
    /// Gets the current heat capacity multiplier based on system state.
    /// </summary>
    public float GetHeatCapacityMultiplier()
    {
        if (IsDestroyed) return 0f; // Doesn't matter - ship is dead
        if (IsDamaged && heatCapacityPenalty) return damagedHeatCapacityMultiplier;
        return 1f;
    }

    /// <summary>
    /// Gets the passive heat generation from damaged reactor.
    /// </summary>
    public float GetPassiveHeatGeneration()
    {
        if (IsDamaged && !heatCapacityPenalty) return passiveHeatGeneration;
        return 0f;
    }

    /// <summary>
    /// Returns true if the reactor is intact.
    /// </summary>
    public bool IsIntact()
    {
        return !IsDestroyed;
    }

    /// <summary>
    /// Links this mounted system to a Ship and HeatManager.
    /// </summary>
    public void SetLinkedReferences(Ship ship, HeatManager heatManager)
    {
        linkedShip = ship;
        linkedHeatManager = heatManager;
        Debug.Log($"[MountedReactor] Linked to ship {ship?.gameObject.name ?? "null"}");
    }

    /// <summary>
    /// Called when system is damaged. Randomly selects heat capacity or passive heat penalty.
    /// </summary>
    protected override void ApplyDegradation()
    {
        // Randomly choose between heat capacity penalty and passive heat generation
        heatCapacityPenalty = UnityEngine.Random.value > 0.5f;

        string penaltyType = heatCapacityPenalty
            ? $"Heat capacity x{damagedHeatCapacityMultiplier}"
            : $"+{passiveHeatGeneration} heat/turn";
        Debug.Log($"[MountedReactor] Reactor damaged - {penaltyType}");
        OnReactorStateChanged?.Invoke(this);
    }

    /// <summary>
    /// Called when system is destroyed. Triggers core breach!
    /// </summary>
    protected override void OnDestruction()
    {
        Debug.LogError($"[MountedReactor] CORE BREACH! Reactor destroyed - Ship lost!");
        OnCoreBreach?.Invoke(this);

        // Trigger ship death
        if (linkedShip != null)
        {
            linkedShip.Die();
        }
    }
}
