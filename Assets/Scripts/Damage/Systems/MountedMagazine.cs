using UnityEngine;
using System;

/// <summary>
/// Mounted magazine system for torpedoes and missiles.
/// When damaged: Reduced reload rate or ammo capacity.
/// When destroyed: INTERNAL EXPLOSION - Damage to section structure!
/// </summary>
public class MountedMagazine : MountedSystem
{
    public enum MagazineType
    {
        Torpedo,    // Internal explosion: 40 damage
        Missile     // Internal explosion: 25 damage
    }

    [Header("Magazine Configuration")]
    [SerializeField] private MagazineType magazineType = MagazineType.Torpedo;
    [SerializeField] private float torpedoExplosionDamage = 40f;
    [SerializeField] private float missileExplosionDamage = 25f;

    [Header("Degradation Effects")]
    [SerializeField] private float damagedReloadMultiplier = 1.5f; // 50% longer reload

    // Events
    /// <summary>Fired when magazine state changes.</summary>
    public event Action<MountedMagazine> OnMagazineStateChanged;

    /// <summary>Fired when magazine explodes (internal damage applied).</summary>
    public event Action<MountedMagazine, float> OnMagazineExplosion;

    // Properties
    public MagazineType Type => magazineType;

    /// <summary>
    /// Gets the internal explosion damage based on magazine type.
    /// </summary>
    public float GetExplosionDamage()
    {
        return magazineType == MagazineType.Torpedo ? torpedoExplosionDamage : missileExplosionDamage;
    }

    /// <summary>
    /// Gets the current reload multiplier based on system state.
    /// </summary>
    public float GetReloadMultiplier()
    {
        if (IsDestroyed) return float.MaxValue; // Can't reload
        if (IsDamaged) return damagedReloadMultiplier;
        return 1f;
    }

    /// <summary>
    /// Returns true if magazine can supply ammo.
    /// </summary>
    public bool CanSupplyAmmo()
    {
        return !IsDestroyed;
    }

    /// <summary>
    /// Sets the magazine type.
    /// </summary>
    public void SetMagazineType(MagazineType type)
    {
        magazineType = type;
    }

    /// <summary>
    /// Called when system is damaged.
    /// </summary>
    protected override void ApplyDegradation()
    {
        Debug.Log($"[MountedMagazine] {magazineType} Magazine damaged - Reload x{damagedReloadMultiplier}");
        OnMagazineStateChanged?.Invoke(this);
    }

    /// <summary>
    /// Called when system is destroyed. Triggers internal explosion!
    /// </summary>
    protected override void OnDestruction()
    {
        float explosionDamage = GetExplosionDamage();
        Debug.LogWarning($"[MountedMagazine] {magazineType} Magazine EXPLODED! Internal damage: {explosionDamage}");

        OnMagazineExplosion?.Invoke(this, explosionDamage);

        // Apply internal damage to parent section
        if (ParentSection != null)
        {
            // Apply damage directly to structure (bypasses armor)
            ApplyInternalDamage(explosionDamage);
        }
    }

    /// <summary>
    /// Applies internal explosion damage directly to section structure.
    /// </summary>
    private void ApplyInternalDamage(float damage)
    {
        if (ParentSection == null) return;

        // Internal explosions bypass armor and damage structure directly
        // We'll call ApplyDamage but the section should already have low/no armor
        // In a full implementation, we'd have a method for direct structure damage
        DamageResult result = ParentSection.ApplyDamage(damage);

        Debug.Log($"[MountedMagazine] Internal explosion dealt {result.DamageToStructure:F1} structure damage to {ParentSection.SectionType}");
    }
}
