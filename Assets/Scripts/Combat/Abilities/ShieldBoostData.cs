using UnityEngine;

/// <summary>
/// Shield boost ability - instantly restores shields.
/// Create via: Assets → Create → Abilities → Shield Boost
/// </summary>
[CreateAssetMenu(fileName = "ShieldBoost", menuName = "Abilities/Shield Boost")]
public class ShieldBoostData : AbilityData
{
    [Header("Shield Boost Settings")]
    [SerializeField] private float shieldRestoreAmount = 100f;

    public override void Execute(Ship ship)
    {
        ship.CurrentShields = Mathf.Min(ship.MaxShields, ship.CurrentShields + shieldRestoreAmount);
        Debug.Log($"{ship.gameObject.name} activated Shield Boost! Restored {shieldRestoreAmount} shields. Current: {ship.CurrentShields}/{ship.MaxShields}");
    }
}
