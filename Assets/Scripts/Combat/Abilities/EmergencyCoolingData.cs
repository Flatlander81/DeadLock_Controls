using UnityEngine;

/// <summary>
/// Emergency cooling ability - instantly reduces heat.
/// Create via: Assets → Create → Abilities → Emergency Cooling
/// </summary>
[CreateAssetMenu(fileName = "EmergencyCooling", menuName = "Abilities/Emergency Cooling")]
public class EmergencyCoolingData : AbilityData
{
    [Header("Emergency Cooling Settings")]
    [SerializeField] private float coolingAmount = 50f;

    public override void Execute(Ship ship)
    {
        if (ship.HeatManager != null)
        {
            ship.HeatManager.InstantCooling(coolingAmount);
            Debug.Log($"{ship.gameObject.name} activated Emergency Cooling! Reduced heat by {coolingAmount}");
        }
    }
}
