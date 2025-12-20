using UnityEngine;

/// <summary>
/// Represents a queued weapon firing command.
/// Created during Command Phase, executed during Simulation Phase.
/// </summary>
public struct WeaponFireCommand
{
    /// <summary>
    /// The weapon to fire.
    /// </summary>
    public WeaponSystem Weapon;

    /// <summary>
    /// The target ship.
    /// </summary>
    public Ship Target;

    /// <summary>
    /// Weapon group number (1-4, or 0 if not part of group fire).
    /// </summary>
    public int GroupNumber;

    /// <summary>
    /// Time when the command was queued.
    /// </summary>
    public float QueueTime;

    /// <summary>
    /// Whether this command is part of an alpha strike.
    /// </summary>
    public bool IsAlphaStrike;

    /// <summary>
    /// Create a new weapon fire command.
    /// </summary>
    public WeaponFireCommand(WeaponSystem weapon, Ship target, int groupNumber = 0, bool isAlphaStrike = false)
    {
        Weapon = weapon;
        Target = target;
        GroupNumber = groupNumber;
        QueueTime = Time.time;
        IsAlphaStrike = isAlphaStrike;
    }

    /// <summary>
    /// Check if this command is still valid (weapon and target exist).
    /// </summary>
    public bool IsValid => Weapon != null && Target != null && !Target.IsDead;

    /// <summary>
    /// Get the heat cost for this command.
    /// </summary>
    public int HeatCost => Weapon != null ? Weapon.HeatCost : 0;

    public override string ToString()
    {
        string groupStr = GroupNumber > 0 ? $"[G{GroupNumber}]" : "";
        string alphaStr = IsAlphaStrike ? "[ALPHA]" : "";
        return $"{alphaStr}{groupStr} {Weapon?.WeaponName ?? "null"} -> {Target?.gameObject.name ?? "null"}";
    }
}
