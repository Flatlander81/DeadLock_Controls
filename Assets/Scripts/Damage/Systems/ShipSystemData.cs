using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static data definitions for ship systems.
/// Contains size, default stats, and configuration per system type.
/// Based on GDD system specifications.
/// </summary>
public static class ShipSystemData
{
    /// <summary>
    /// Configuration data for a ship system type.
    /// </summary>
    public struct SystemConfig
    {
        /// <summary>Display name of the system.</summary>
        public string Name;

        /// <summary>Number of slots this system occupies.</summary>
        public int Size;

        /// <summary>Color for debug visualization.</summary>
        public Color DebugColor;

        /// <summary>Brief description of the system.</summary>
        public string Description;

        public SystemConfig(string name, int size, Color color, string description)
        {
            Name = name;
            Size = size;
            DebugColor = color;
            Description = description;
        }
    }

    // System configurations from GDD
    private static readonly Dictionary<ShipSystemType, SystemConfig> systemConfigs = new Dictionary<ShipSystemType, SystemConfig>
    {
        { ShipSystemType.None, new SystemConfig("Empty", 0, Color.gray, "Empty slot") },

        // Power Systems
        { ShipSystemType.ReactorCore, new SystemConfig("Reactor Core", 20, new Color(1f, 0.5f, 0f), "Main power reactor - destruction causes catastrophic failure") },

        // Propulsion Systems
        { ShipSystemType.MainEngine, new SystemConfig("Main Engine", 15, new Color(0.5f, 0.8f, 1f), "Primary thrust system - damage reduces speed") },

        // Weapon Systems
        { ShipSystemType.NewtonianCannon, new SystemConfig("Newtonian Cannon", 8, new Color(1f, 0.2f, 0.2f), "High-damage kinetic weapon") },
        { ShipSystemType.TorpedoLauncher, new SystemConfig("Torpedo Launcher", 8, new Color(1f, 0.4f, 0.1f), "Guided explosive launcher") },
        { ShipSystemType.TorpedoMagazine, new SystemConfig("Torpedo Magazine", 8, new Color(1f, 0.6f, 0.2f), "Torpedo storage - destruction causes explosion") },
        { ShipSystemType.MissileBattery, new SystemConfig("Missile Battery", 6, new Color(0.9f, 0.3f, 0.3f), "Multi-warhead missile system") },
        { ShipSystemType.MissileMagazine, new SystemConfig("Missile Magazine", 6, new Color(0.9f, 0.5f, 0.3f), "Missile storage - destruction causes explosion") },
        { ShipSystemType.RailGun, new SystemConfig("Rail Gun", 5, new Color(0.6f, 0.2f, 0.8f), "Electromagnetic rail weapon") },
        { ShipSystemType.PDTurret, new SystemConfig("PD Turret", 4, new Color(0.4f, 0.8f, 0.4f), "Point defense turret") },

        // Support Systems
        { ShipSystemType.Sensors, new SystemConfig("Sensors", 6, new Color(0.2f, 0.8f, 0.8f), "Detection and targeting - damage reduces accuracy") },
        { ShipSystemType.Radiator, new SystemConfig("Radiator", 5, new Color(0.8f, 0.8f, 0.2f), "Heat dissipation - damage increases heat buildup") }
    };

    /// <summary>
    /// Gets the configuration for a system type.
    /// </summary>
    public static SystemConfig GetConfig(ShipSystemType type)
    {
        if (systemConfigs.TryGetValue(type, out SystemConfig config))
        {
            return config;
        }
        return systemConfigs[ShipSystemType.None];
    }

    /// <summary>
    /// Gets the size (slot count) for a system type.
    /// </summary>
    public static int GetSize(ShipSystemType type)
    {
        return GetConfig(type).Size;
    }

    /// <summary>
    /// Gets the display name for a system type.
    /// </summary>
    public static string GetName(ShipSystemType type)
    {
        return GetConfig(type).Name;
    }

    /// <summary>
    /// Gets the debug visualization color for a system type.
    /// </summary>
    public static Color GetColor(ShipSystemType type)
    {
        return GetConfig(type).DebugColor;
    }

    /// <summary>
    /// Gets all system types (excluding None).
    /// </summary>
    public static IEnumerable<ShipSystemType> GetAllSystemTypes()
    {
        foreach (ShipSystemType type in System.Enum.GetValues(typeof(ShipSystemType)))
        {
            if (type != ShipSystemType.None)
            {
                yield return type;
            }
        }
    }

    /// <summary>
    /// Checks if a system type is a weapon.
    /// </summary>
    public static bool IsWeapon(ShipSystemType type)
    {
        return type == ShipSystemType.NewtonianCannon ||
               type == ShipSystemType.TorpedoLauncher ||
               type == ShipSystemType.MissileBattery ||
               type == ShipSystemType.RailGun ||
               type == ShipSystemType.PDTurret;
    }

    /// <summary>
    /// Checks if a system type is explosive (causes damage on destruction).
    /// </summary>
    public static bool IsExplosive(ShipSystemType type)
    {
        return type == ShipSystemType.TorpedoMagazine ||
               type == ShipSystemType.MissileMagazine ||
               type == ShipSystemType.ReactorCore;
    }
}
