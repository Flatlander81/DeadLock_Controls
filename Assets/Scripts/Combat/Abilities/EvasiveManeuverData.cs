using UnityEngine;

/// <summary>
/// Evasive maneuver ability - temporarily boosts turn rate.
/// Create via: Assets → Create → Abilities → Evasive Maneuver
/// </summary>
[CreateAssetMenu(fileName = "EvasiveManeuver", menuName = "Abilities/Evasive Maneuver")]
public class EvasiveManeuverData : AbilityData
{
    [Header("Evasive Maneuver Settings")]
    [SerializeField] private float turnRateMultiplier = 2f;
    [SerializeField] private int durationTurns = 2;

    public override void Execute(Ship ship)
    {
        // Apply turn rate boost
        ship.MaxTurnAngle *= turnRateMultiplier;
        Debug.Log($"{ship.gameObject.name} activated Evasive Maneuver! Turn rate increased by {turnRateMultiplier}x for {durationTurns} turns");

        // Note: Duration tracking would need to be implemented in Ship or AbilitySystem
        // For now, this is a simplified implementation
    }

    public override void OnExecuteComplete(Ship ship)
    {
        // Reset turn rate after duration expires
        ship.MaxTurnAngle /= turnRateMultiplier;
        Debug.Log($"{ship.gameObject.name} Evasive Maneuver ended. Turn rate restored to normal.");
    }
}
