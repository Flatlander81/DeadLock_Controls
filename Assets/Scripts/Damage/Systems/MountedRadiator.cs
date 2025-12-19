using UnityEngine;
using System;

/// <summary>
/// Mounted radiator system that modifies passive cooling.
/// When damaged: Passive cooling halved.
/// When destroyed: No passive cooling from this radiator.
/// </summary>
public class MountedRadiator : MountedSystem
{
    [Header("Radiator Link")]
    [SerializeField] private HeatManager linkedHeatManager;

    [Header("Degradation Effects")]
    [SerializeField] private float damagedCoolingMultiplier = 0.5f;
    [SerializeField] private float baseCoolingContribution = 5f; // How much this radiator contributes to total cooling

    // Events
    /// <summary>Fired when radiator state changes.</summary>
    public event Action<MountedRadiator> OnRadiatorStateChanged;

    // Properties
    public HeatManager LinkedHeatManager => linkedHeatManager;
    public float BaseCoolingContribution => baseCoolingContribution;

    /// <summary>
    /// Gets the current cooling multiplier based on system state.
    /// </summary>
    public float GetCoolingMultiplier()
    {
        if (IsDestroyed) return 0f;
        if (IsDamaged) return damagedCoolingMultiplier;
        return 1f;
    }

    /// <summary>
    /// Gets the effective cooling contribution from this radiator.
    /// </summary>
    public float GetEffectiveCooling()
    {
        return baseCoolingContribution * GetCoolingMultiplier();
    }

    /// <summary>
    /// Links this mounted system to a HeatManager.
    /// </summary>
    public void SetLinkedHeatManager(HeatManager heatManager)
    {
        linkedHeatManager = heatManager;
        Debug.Log($"[MountedRadiator] Linked to HeatManager");
    }

    /// <summary>
    /// Sets the base cooling contribution.
    /// </summary>
    public void SetBaseCoolingContribution(float cooling)
    {
        baseCoolingContribution = cooling;
    }

    /// <summary>
    /// Called when system is damaged.
    /// </summary>
    protected override void ApplyDegradation()
    {
        Debug.Log($"[MountedRadiator] Radiator damaged - Cooling x{damagedCoolingMultiplier}");
        OnRadiatorStateChanged?.Invoke(this);
    }

    /// <summary>
    /// Called when system is destroyed.
    /// </summary>
    protected override void OnDestruction()
    {
        Debug.Log($"[MountedRadiator] Radiator DESTROYED - No cooling from this unit");
        OnRadiatorStateChanged?.Invoke(this);
    }
}
