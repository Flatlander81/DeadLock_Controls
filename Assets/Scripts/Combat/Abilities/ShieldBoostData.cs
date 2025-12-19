using UnityEngine;

/// <summary>
/// Shield boost ability - instantly restores shields.
/// Can only be activated when shields are at 0 (depleted).
/// Create via: Assets → Create → Abilities → Shield Boost
/// </summary>
[CreateAssetMenu(fileName = "ShieldBoost", menuName = "Abilities/Shield Boost")]
public class ShieldBoostData : AbilityData
{
    [Header("Shield Boost Settings")]
    [SerializeField] private float shieldRestoreAmount = 100f;

    /// <summary>
    /// Can only activate Shield Boost when shields are depleted.
    /// </summary>
    public override bool CanActivate(Ship ship)
    {
        // Check if ship has ShieldSystem
        if (ship.ShieldSystem != null)
        {
            return ship.ShieldSystem.CanRestoreShields();
        }

        // Fallback to legacy shield check
        return ship.CurrentShields <= 0f;
    }

    /// <summary>
    /// Explains why Shield Boost cannot be activated.
    /// </summary>
    public override string GetActivationBlockedReason(Ship ship)
    {
        if (ship.ShieldSystem != null)
        {
            if (!ship.ShieldSystem.CanRestoreShields())
            {
                return $"Shields active ({ship.ShieldSystem.CurrentShields:F0}/{ship.ShieldSystem.MaxShields:F0})";
            }
        }
        else if (ship.CurrentShields > 0f)
        {
            return $"Shields active ({ship.CurrentShields:F0}/{ship.MaxShields:F0})";
        }

        return string.Empty;
    }

    public override void Execute(Ship ship)
    {
        // Use ShieldSystem if available
        if (ship.ShieldSystem != null)
        {
            if (ship.ShieldSystem.RestoreShields(shieldRestoreAmount))
            {
                Debug.Log($"{ship.gameObject.name} activated Shield Boost! Restored {shieldRestoreAmount} shields via ShieldSystem.");
            }
            else
            {
                Debug.LogWarning($"{ship.gameObject.name} Shield Boost failed - shields not depleted");
            }
        }
        else
        {
            // Fallback to legacy behavior
            if (ship.CurrentShields <= 0f)
            {
                ship.CurrentShields = Mathf.Min(ship.MaxShields, shieldRestoreAmount);
                Debug.Log($"{ship.gameObject.name} activated Shield Boost! Restored {shieldRestoreAmount} shields (legacy). Current: {ship.CurrentShields}/{ship.MaxShields}");
            }
            else
            {
                Debug.LogWarning($"{ship.gameObject.name} Shield Boost failed - shields not depleted (legacy)");
            }
        }
    }
}
