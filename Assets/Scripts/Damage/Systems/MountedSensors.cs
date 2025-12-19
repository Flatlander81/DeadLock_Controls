using UnityEngine;
using System;

/// <summary>
/// Mounted sensors system that modifies targeting range.
/// When damaged: Targeting range halved.
/// When destroyed: Blind (minimal targeting range).
/// </summary>
public class MountedSensors : MountedSystem
{
    [Header("Sensor Effects")]
    [SerializeField] private float damagedRangeMultiplier = 0.5f;
    [SerializeField] private float destroyedRangeMultiplier = 0.1f; // Not completely blind, minimal range

    // Events
    /// <summary>Fired when sensor state changes.</summary>
    public event Action<MountedSensors> OnSensorStateChanged;

    /// <summary>
    /// Gets the current targeting range multiplier based on system state.
    /// </summary>
    public float GetTargetingRangeMultiplier()
    {
        if (IsDestroyed) return destroyedRangeMultiplier;
        if (IsDamaged) return damagedRangeMultiplier;
        return 1f;
    }

    /// <summary>
    /// Gets the current accuracy multiplier based on system state.
    /// Damaged sensors also affect accuracy.
    /// </summary>
    public float GetAccuracyMultiplier()
    {
        if (IsDestroyed) return 0.25f;
        if (IsDamaged) return 0.75f;
        return 1f;
    }

    /// <summary>
    /// Returns true if sensors are functional enough to target.
    /// </summary>
    public bool CanTarget()
    {
        return !IsDestroyed; // Can still target even when damaged
    }

    /// <summary>
    /// Called when system is damaged.
    /// </summary>
    protected override void ApplyDegradation()
    {
        Debug.Log($"[MountedSensors] Sensors damaged - Range x{damagedRangeMultiplier}");
        OnSensorStateChanged?.Invoke(this);
    }

    /// <summary>
    /// Called when system is destroyed.
    /// </summary>
    protected override void OnDestruction()
    {
        Debug.Log($"[MountedSensors] Sensors DESTROYED - Targeting severely impaired!");
        OnSensorStateChanged?.Invoke(this);
    }
}
