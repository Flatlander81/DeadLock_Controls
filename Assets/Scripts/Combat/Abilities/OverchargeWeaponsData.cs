using UnityEngine;

/// <summary>
/// Overcharge weapons ability - increases damage output at cost of heat.
/// Create via: Assets → Create → Abilities → Overcharge Weapons
/// </summary>
[CreateAssetMenu(fileName = "OverchargeWeapons", menuName = "Abilities/Overcharge Weapons")]
public class OverchargeWeaponsData : AbilityData
{
    [Header("Overcharge Settings")]
    [SerializeField] private float damageMultiplier = 1.5f;
    [SerializeField] private int durationTurns = 3;
    [SerializeField] private float heatPerTurn = 10f;

    public override void Execute(Ship ship)
    {
        Debug.Log($"{ship.gameObject.name} activated Overcharge Weapons! Damage increased by {damageMultiplier}x for {durationTurns} turns");
        Debug.Log($"Warning: Generates {heatPerTurn} heat per turn while active!");

        // Note: Damage multiplier and duration tracking would need to be implemented
        // This is a simplified implementation showing the concept
    }
}
