using UnityEngine;

/// <summary>
/// ScriptableObject that defines an ability's stats and behavior.
/// Create instances via: Assets → Create → Abilities → [Ability Type]
/// </summary>
public abstract class AbilityData : ScriptableObject
{
    [Header("Ability Configuration")]
    public string abilityName = "Unnamed Ability";
    public string description = "";
    public int heatCost = 0;
    public int maxCooldown = 1;
    public float spinUpTime = 0f;
    public Sprite icon;

    [Header("Visual")]
    public Color abilityColor = Color.cyan;

    /// <summary>
    /// Execute the ability effect on the target ship.
    /// </summary>
    /// <param name="ship">The ship using this ability</param>
    public abstract void Execute(Ship ship);

    /// <summary>
    /// Check if this ability can be used (override for custom conditions).
    /// </summary>
    /// <param name="ship">The ship trying to use this ability</param>
    /// <returns>True if ability can be used</returns>
    public virtual bool CanUse(Ship ship)
    {
        if (ship.HeatManager == null) return false;

        // Check heat affordability
        float totalHeat = ship.HeatManager.CurrentHeat + ship.HeatManager.PlannedHeat + heatCost;
        return totalHeat <= ship.HeatManager.MaxHeat * 2f;
    }

    /// <summary>
    /// Called when ability execution completes (for cleanup/reset).
    /// </summary>
    /// <param name="ship">The ship that used this ability</param>
    public virtual void OnExecuteComplete(Ship ship)
    {
        // Override in subclasses if needed
    }
}
