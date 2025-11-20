/// <summary>
/// Interface contract for Track C (Targeting UI System).
/// Defines the methods that the targeting system must implement.
/// Track A (Weapons) will call these methods to interface with targeting.
/// </summary>
public interface ITargetingSystem
{
    /// <summary>
    /// Select a target ship for the player's weapons.
    /// Track C will implement target selection UI and validation.
    /// </summary>
    /// <param name="target">The ship to target</param>
    void SelectTarget(Ship target);

    /// <summary>
    /// Assign a weapon group to target a specific ship.
    /// Track C will implement group management UI.
    /// </summary>
    /// <param name="groupNumber">Weapon group (1-4)</param>
    /// <param name="target">The ship to target</param>
    void AssignGroupToTarget(int groupNumber, Ship target);

    /// <summary>
    /// Execute an Alpha Strike (fire all weapons) at a target.
    /// Track C will implement the UI command for this.
    /// </summary>
    /// <param name="target">The ship to alpha strike</param>
    void AlphaStrike(Ship target);

    /// <summary>
    /// Get the currently selected target.
    /// Track C will maintain the current target state.
    /// </summary>
    /// <returns>Currently targeted ship, or null if no target</returns>
    Ship GetCurrentTarget();

    /// <summary>
    /// Clear the current target selection.
    /// Track C will implement target deselection.
    /// </summary>
    void ClearTarget();

    /// <summary>
    /// Get all valid targets in range/arc of any weapon.
    /// Track C will implement target filtering and validation.
    /// </summary>
    /// <returns>List of valid target ships</returns>
    System.Collections.Generic.List<Ship> GetValidTargets();

    /// <summary>
    /// Check if a specific ship is a valid target.
    /// Track C will implement validation logic (range, arc, alive, etc.).
    /// </summary>
    /// <param name="ship">Ship to check</param>
    /// <returns>True if ship is valid target</returns>
    bool IsValidTarget(Ship ship);
}
