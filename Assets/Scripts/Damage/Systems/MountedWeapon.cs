using UnityEngine;
using System;

/// <summary>
/// Mounted weapon system that links to a WeaponSystem and applies damage/cooldown penalties.
/// When damaged: Cooldown doubled OR damage halved (randomly chosen).
/// When destroyed: Weapon non-functional.
/// </summary>
public class MountedWeapon : MountedSystem
{
    [Header("Weapon Link")]
    [SerializeField] private WeaponSystem linkedWeapon;

    [Header("Degradation Effects")]
    [SerializeField] private bool cooldownPenalty = false; // True = cooldown doubled, False = damage halved
    [SerializeField] private float damagePenaltyMultiplier = 0.5f;
    [SerializeField] private float cooldownPenaltyMultiplier = 2f;

    // Events
    /// <summary>Fired when weapon link is established.</summary>
    public event Action<MountedWeapon, WeaponSystem> OnWeaponLinked;

    // Properties
    public WeaponSystem LinkedWeapon => linkedWeapon;
    public bool HasCooldownPenalty => cooldownPenalty;

    /// <summary>
    /// Gets the current damage multiplier based on system state.
    /// </summary>
    public float GetDamageMultiplier()
    {
        if (IsDestroyed) return 0f;
        if (IsDamaged && !cooldownPenalty) return damagePenaltyMultiplier;
        return 1f;
    }

    /// <summary>
    /// Gets the current cooldown multiplier based on system state.
    /// </summary>
    public float GetCooldownMultiplier()
    {
        if (IsDestroyed) return float.MaxValue; // Effectively disabled
        if (IsDamaged && cooldownPenalty) return cooldownPenaltyMultiplier;
        return 1f;
    }

    /// <summary>
    /// Returns true if this weapon can fire (not destroyed).
    /// </summary>
    public bool CanFire()
    {
        return !IsDestroyed;
    }

    /// <summary>
    /// Links this mounted system to a WeaponSystem.
    /// </summary>
    public void SetLinkedWeapon(WeaponSystem weapon)
    {
        linkedWeapon = weapon;
        OnWeaponLinked?.Invoke(this, weapon);
        Debug.Log($"[MountedWeapon] Linked {ShipSystemData.GetName(SystemType)} to {weapon?.WeaponName ?? "null"}");
    }

    /// <summary>
    /// Called when system is damaged. Randomly selects cooldown or damage penalty.
    /// </summary>
    protected override void ApplyDegradation()
    {
        // Randomly choose between cooldown penalty and damage penalty
        cooldownPenalty = UnityEngine.Random.value > 0.5f;

        string penaltyType = cooldownPenalty ? "Cooldown x2" : "Damage x0.5";
        Debug.Log($"[MountedWeapon] {ShipSystemData.GetName(SystemType)} damaged - {penaltyType}");
    }

    /// <summary>
    /// Called when system is destroyed.
    /// </summary>
    protected override void OnDestruction()
    {
        Debug.Log($"[MountedWeapon] {ShipSystemData.GetName(SystemType)} DESTROYED - Weapon disabled");
    }
}
