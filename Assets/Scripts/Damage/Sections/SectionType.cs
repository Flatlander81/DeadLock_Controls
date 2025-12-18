using System;

/// <summary>
/// Defines all ship section types.
/// Each section can be individually damaged and breached.
/// </summary>
[Serializable]
public enum SectionType
{
    /// <summary>Front section - contains forward weapons and sensors.</summary>
    Fore,

    /// <summary>Rear section - contains engines.</summary>
    Aft,

    /// <summary>Left side section - contains port weapons and thrusters.</summary>
    Port,

    /// <summary>Right side section - contains starboard weapons and thrusters.</summary>
    Starboard,

    /// <summary>Top section - contains bridge and main sensors.</summary>
    Dorsal,

    /// <summary>Bottom section - contains cargo and landing gear.</summary>
    Ventral,

    /// <summary>Protected center section - special access rules, no armor.</summary>
    Core
}
