using UnityEngine;

/// <summary>
/// Point Defense Override ability - boosts defensive systems.
/// Create via: Assets → Create → Abilities → PD Override
/// </summary>
[CreateAssetMenu(fileName = "PDOverride", menuName = "Abilities/PD Override")]
public class PDOverrideData : AbilityData
{
    [Header("PD Override Settings")]
    [SerializeField] private float interceptChanceBonus = 0.5f; // 50% bonus
    [SerializeField] private int durationTurns = 2;

    public override void Execute(Ship ship)
    {
        Debug.Log($"{ship.gameObject.name} activated PD Override! Intercept chance increased by {interceptChanceBonus * 100}% for {durationTurns} turns");

        // Note: Point defense system would need to be implemented
        // This is a placeholder showing the intended behavior
    }
}
