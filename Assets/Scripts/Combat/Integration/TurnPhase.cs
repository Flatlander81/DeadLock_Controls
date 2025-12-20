/// <summary>
/// Represents the current phase of a combat turn.
/// </summary>
public enum TurnPhase
{
    /// <summary>
    /// Command Phase: Players plan movements, assign targets, queue weapons and abilities.
    /// No time limit - waits for player to end turn.
    /// </summary>
    Command,

    /// <summary>
    /// Simulation Phase: All planned actions execute simultaneously.
    /// Ships move, weapons fire, projectiles travel, damage resolves.
    /// </summary>
    Simulation,

    /// <summary>
    /// Turn End Phase: Cleanup and maintenance.
    /// Cooldowns tick, heat dissipates, victory/defeat checks.
    /// </summary>
    TurnEnd
}
