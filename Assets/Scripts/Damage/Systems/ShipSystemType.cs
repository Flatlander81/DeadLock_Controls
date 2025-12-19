/// <summary>
/// Enumeration of all mountable ship system types.
/// Based on GDD system definitions for the Hephaestus and other ships.
/// </summary>
public enum ShipSystemType
{
    /// <summary>No system (empty slot).</summary>
    None = 0,

    // === Power Systems ===
    /// <summary>Main reactor providing power. Size: 20 slots.</summary>
    ReactorCore,

    // === Propulsion Systems ===
    /// <summary>Main thrust engine. Size: 15 slots.</summary>
    MainEngine,

    // === Weapon Systems ===
    /// <summary>High-damage kinetic weapon. Size: 8 slots.</summary>
    NewtonianCannon,

    /// <summary>Guided explosive launcher. Size: 8 slots.</summary>
    TorpedoLauncher,

    /// <summary>Torpedo storage. Size: 8 slots.</summary>
    TorpedoMagazine,

    /// <summary>Multi-warhead missile system. Size: 6 slots.</summary>
    MissileBattery,

    /// <summary>Missile storage. Size: 6 slots.</summary>
    MissileMagazine,

    /// <summary>Electromagnetic rail weapon. Size: 5 slots.</summary>
    RailGun,

    /// <summary>Point defense turret. Size: 4 slots.</summary>
    PDTurret,

    // === Support Systems ===
    /// <summary>Detection and targeting. Size: 6 slots.</summary>
    Sensors,

    /// <summary>Heat dissipation system. Size: 5 slots.</summary>
    Radiator
}

/// <summary>
/// System state enumeration for MountedSystem health tracking.
/// </summary>
public enum SystemState
{
    /// <summary>System is fully functional.</summary>
    Operational,

    /// <summary>System has been hit once, may have degraded performance.</summary>
    Damaged,

    /// <summary>System is non-functional after being hit twice.</summary>
    Destroyed
}
