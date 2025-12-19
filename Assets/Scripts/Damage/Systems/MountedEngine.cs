using UnityEngine;
using System;

/// <summary>
/// Mounted engine system that modifies ship speed and turn rate.
/// When damaged: Max speed halved, turn rate halved.
/// When destroyed: Ship immobile.
/// </summary>
public class MountedEngine : MountedSystem
{
    [Header("Engine Link")]
    [SerializeField] private Ship linkedShip;

    [Header("Degradation Effects")]
    [SerializeField] private float damagedSpeedMultiplier = 0.5f;
    [SerializeField] private float damagedTurnRateMultiplier = 0.5f;

    // Events
    /// <summary>Fired when engine state changes.</summary>
    public event Action<MountedEngine> OnEngineStateChanged;

    // Properties
    public Ship LinkedShip => linkedShip;

    /// <summary>
    /// Gets the current speed multiplier based on system state.
    /// </summary>
    public float GetSpeedMultiplier()
    {
        if (IsDestroyed) return 0f;
        if (IsDamaged) return damagedSpeedMultiplier;
        return 1f;
    }

    /// <summary>
    /// Gets the current turn rate multiplier based on system state.
    /// </summary>
    public float GetTurnRateMultiplier()
    {
        if (IsDestroyed) return 0f;
        if (IsDamaged) return damagedTurnRateMultiplier;
        return 1f;
    }

    /// <summary>
    /// Returns true if the ship can move (engine not destroyed).
    /// </summary>
    public bool CanMove()
    {
        return !IsDestroyed;
    }

    /// <summary>
    /// Links this mounted system to a Ship.
    /// </summary>
    public void SetLinkedShip(Ship ship)
    {
        linkedShip = ship;
        Debug.Log($"[MountedEngine] Linked to ship {ship?.gameObject.name ?? "null"}");
    }

    /// <summary>
    /// Called when system is damaged.
    /// </summary>
    protected override void ApplyDegradation()
    {
        Debug.Log($"[MountedEngine] Engine damaged - Speed x{damagedSpeedMultiplier}, Turn x{damagedTurnRateMultiplier}");
        OnEngineStateChanged?.Invoke(this);
    }

    /// <summary>
    /// Called when system is destroyed.
    /// </summary>
    protected override void OnDestruction()
    {
        Debug.Log($"[MountedEngine] Engine DESTROYED - Ship immobile!");
        OnEngineStateChanged?.Invoke(this);
    }
}
