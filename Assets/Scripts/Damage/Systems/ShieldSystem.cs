using UnityEngine;
using System;

/// <summary>
/// Manages ship shields as a single bubble pool.
/// Shields absorb damage before it reaches sections.
/// No regeneration - Shield Boost only works when shields at 0.
/// </summary>
public class ShieldSystem : MonoBehaviour
{
    [Header("Shield Configuration")]
    [SerializeField] private float maxShields = 200f;

    [Header("Runtime State")]
    [SerializeField] private float currentShields;

    // Events
    /// <summary>Fired when shields take damage. Parameters: damageAmount, remainingShields.</summary>
    public event Action<float, float> OnShieldDamaged;

    /// <summary>Fired when shields are fully depleted.</summary>
    public event Action OnShieldDepleted;

    /// <summary>Fired when shields are restored from 0.</summary>
    public event Action<float> OnShieldRestored;

    // Properties
    public float MaxShields => maxShields;
    public float CurrentShields => currentShields;
    public bool IsShieldActive => currentShields > 0f;

    private void Awake()
    {
        currentShields = maxShields;
    }

    /// <summary>
    /// Initialize the shield system with custom values.
    /// </summary>
    /// <param name="max">Maximum shield capacity.</param>
    public void Initialize(float max)
    {
        maxShields = max;
        currentShields = maxShields;
        Debug.Log($"[ShieldSystem] Initialized with {maxShields} shields");
    }

    /// <summary>
    /// Absorb incoming damage with shields.
    /// </summary>
    /// <param name="incomingDamage">Amount of damage to absorb.</param>
    /// <returns>Overflow damage that was not absorbed.</returns>
    public float AbsorbDamage(float incomingDamage)
    {
        if (incomingDamage <= 0f)
        {
            return 0f;
        }

        // No shields left - all damage overflows
        if (currentShields <= 0f)
        {
            return incomingDamage;
        }

        float absorbed = Mathf.Min(incomingDamage, currentShields);
        float overflow = incomingDamage - absorbed;

        currentShields -= absorbed;

        Debug.Log($"[ShieldSystem] Absorbed {absorbed:F1} damage, {currentShields:F1} shields remaining");

        OnShieldDamaged?.Invoke(absorbed, currentShields);

        // Check for depletion
        if (currentShields <= 0f)
        {
            currentShields = 0f;
            Debug.Log("[ShieldSystem] Shields DEPLETED!");
            OnShieldDepleted?.Invoke();
        }

        return overflow;
    }

    /// <summary>
    /// Checks if shields can be restored (only when at 0).
    /// </summary>
    /// <returns>True if shields are at 0 and can be restored.</returns>
    public bool CanRestoreShields()
    {
        return currentShields <= 0f;
    }

    /// <summary>
    /// Restore shields (only works when shields are at 0).
    /// </summary>
    /// <param name="amount">Amount of shields to restore.</param>
    /// <returns>True if restoration was successful.</returns>
    public bool RestoreShields(float amount)
    {
        if (!CanRestoreShields())
        {
            Debug.LogWarning("[ShieldSystem] Cannot restore shields - shields are still active");
            return false;
        }

        if (amount <= 0f)
        {
            return false;
        }

        currentShields = Mathf.Min(amount, maxShields);
        Debug.Log($"[ShieldSystem] Shields restored to {currentShields:F1}");

        OnShieldRestored?.Invoke(currentShields);

        return true;
    }

    /// <summary>
    /// Gets shield level as a percentage (0-1).
    /// </summary>
    public float GetShieldPercentage()
    {
        if (maxShields <= 0f) return 0f;
        return currentShields / maxShields;
    }

    /// <summary>
    /// Resets shields to full capacity.
    /// </summary>
    public void Reset()
    {
        bool wasEmpty = currentShields <= 0f;
        currentShields = maxShields;
        Debug.Log($"[ShieldSystem] Reset to {currentShields:F1}");

        if (wasEmpty)
        {
            OnShieldRestored?.Invoke(currentShields);
        }
    }

    /// <summary>
    /// Sets shields to a specific value (for testing).
    /// </summary>
    /// <param name="amount">Shield amount to set.</param>
    public void SetShields(float amount)
    {
        bool wasEmpty = currentShields <= 0f;
        bool willBeEmpty = amount <= 0f;

        currentShields = Mathf.Clamp(amount, 0f, maxShields);

        if (!wasEmpty && willBeEmpty)
        {
            OnShieldDepleted?.Invoke();
        }
        else if (wasEmpty && !willBeEmpty)
        {
            OnShieldRestored?.Invoke(currentShields);
        }
    }
}
