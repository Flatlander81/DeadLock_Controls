using UnityEngine;
using System;

/// <summary>
/// Mounted Point Defense turret system.
/// When damaged: Engagement rate halved.
/// When destroyed: PD turret non-functional.
/// Placeholder for Phase 4 - PD interception system.
/// </summary>
public class MountedPDTurret : MountedSystem
{
    [Header("PD Turret Effects")]
    [SerializeField] private float damagedEngagementMultiplier = 0.5f;
    [SerializeField] private float baseInterceptChance = 0.5f; // Base chance to intercept incoming projectile

    // Events
    /// <summary>Fired when PD turret state changes.</summary>
    public event Action<MountedPDTurret> OnPDStateChanged;

    // Properties
    public float BaseInterceptChance => baseInterceptChance;

    /// <summary>
    /// Gets the current engagement rate multiplier based on system state.
    /// </summary>
    public float GetEngagementMultiplier()
    {
        if (IsDestroyed) return 0f;
        if (IsDamaged) return damagedEngagementMultiplier;
        return 1f;
    }

    /// <summary>
    /// Gets the effective intercept chance.
    /// </summary>
    public float GetEffectiveInterceptChance()
    {
        return baseInterceptChance * GetEngagementMultiplier();
    }

    /// <summary>
    /// Returns true if the PD turret can engage targets.
    /// </summary>
    public bool CanEngage()
    {
        return !IsDestroyed;
    }

    /// <summary>
    /// Attempts to intercept an incoming projectile.
    /// Placeholder for Phase 4 implementation.
    /// </summary>
    /// <returns>True if projectile intercepted.</returns>
    public bool TryIntercept()
    {
        if (!CanEngage()) return false;

        float roll = UnityEngine.Random.value;
        return roll <= GetEffectiveInterceptChance();
    }

    /// <summary>
    /// Called when system is damaged.
    /// </summary>
    protected override void ApplyDegradation()
    {
        Debug.Log($"[MountedPDTurret] PD Turret damaged - Engagement x{damagedEngagementMultiplier}");
        OnPDStateChanged?.Invoke(this);
    }

    /// <summary>
    /// Called when system is destroyed.
    /// </summary>
    protected override void OnDestruction()
    {
        Debug.Log($"[MountedPDTurret] PD Turret DESTROYED - Point defense offline!");
        OnPDStateChanged?.Invoke(this);
    }
}
