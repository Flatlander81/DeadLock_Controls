using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static data class containing ship section configurations.
/// Defines armor, structure, and collider settings for each section type.
/// </summary>
public static class SectionDefinitions
{
    /// <summary>
    /// Configuration data for a single ship section.
    /// </summary>
    public struct SectionConfig
    {
        public SectionType Type;
        public float Armor;
        public float Structure;
        public Vector3 ColliderPosition;
        public Vector3 ColliderSize;
        public bool UseSphereCollider;
        public float SphereRadius;

        public SectionConfig(
            SectionType type,
            float armor,
            float structure,
            Vector3 position,
            Vector3 size,
            bool useSphere = false,
            float sphereRadius = 0f)
        {
            Type = type;
            Armor = armor;
            Structure = structure;
            ColliderPosition = position;
            ColliderSize = size;
            UseSphereCollider = useSphere;
            SphereRadius = sphereRadius;
        }
    }

    /// <summary>
    /// Hephaestus ship section configurations from GDD.
    /// </summary>
    private static readonly Dictionary<SectionType, SectionConfig> HephaestusConfigs = new Dictionary<SectionType, SectionConfig>
    {
        {
            SectionType.Fore,
            new SectionConfig(
                SectionType.Fore,
                armor: 100f,
                structure: 50f,
                position: new Vector3(0f, 0f, 3f),
                size: new Vector3(2f, 1.5f, 2f))
        },
        {
            SectionType.Aft,
            new SectionConfig(
                SectionType.Aft,
                armor: 60f,
                structure: 40f,
                position: new Vector3(0f, 0f, -3f),
                size: new Vector3(2f, 1.5f, 2f))
        },
        {
            SectionType.Port,
            new SectionConfig(
                SectionType.Port,
                armor: 80f,
                structure: 50f,
                position: new Vector3(-2f, 0f, 0f),
                size: new Vector3(1.5f, 1.5f, 4f))
        },
        {
            SectionType.Starboard,
            new SectionConfig(
                SectionType.Starboard,
                armor: 80f,
                structure: 50f,
                position: new Vector3(2f, 0f, 0f),
                size: new Vector3(1.5f, 1.5f, 4f))
        },
        {
            SectionType.Dorsal,
            new SectionConfig(
                SectionType.Dorsal,
                armor: 70f,
                structure: 40f,
                position: new Vector3(0f, 1.5f, 0f),
                size: new Vector3(3f, 1f, 5f))
        },
        {
            SectionType.Ventral,
            new SectionConfig(
                SectionType.Ventral,
                armor: 70f,
                structure: 40f,
                position: new Vector3(0f, -1.5f, 0f),
                size: new Vector3(3f, 1f, 5f))
        },
        {
            SectionType.Core,
            new SectionConfig(
                SectionType.Core,
                armor: 0f,
                structure: 30f,
                position: Vector3.zero,
                size: Vector3.zero,
                useSphere: true,
                sphereRadius: 1f)
        }
    };

    /// <summary>
    /// Gets the configuration for a specific section type.
    /// </summary>
    /// <param name="type">The section type to get configuration for.</param>
    /// <returns>Section configuration data.</returns>
    public static SectionConfig GetConfig(SectionType type)
    {
        if (HephaestusConfigs.TryGetValue(type, out SectionConfig config))
        {
            return config;
        }

        Debug.LogError($"SectionDefinitions: No configuration found for section type {type}");
        return default;
    }

    /// <summary>
    /// Gets all section configurations.
    /// </summary>
    /// <returns>Dictionary of all section configurations.</returns>
    public static Dictionary<SectionType, SectionConfig> GetAllConfigs()
    {
        return new Dictionary<SectionType, SectionConfig>(HephaestusConfigs);
    }

    /// <summary>
    /// Gets all section types as an array.
    /// </summary>
    /// <returns>Array of all section types.</returns>
    public static SectionType[] GetAllSectionTypes()
    {
        return new SectionType[]
        {
            SectionType.Fore,
            SectionType.Aft,
            SectionType.Port,
            SectionType.Starboard,
            SectionType.Dorsal,
            SectionType.Ventral,
            SectionType.Core
        };
    }

    /// <summary>
    /// Gets the total armor across all sections.
    /// </summary>
    /// <returns>Sum of all section armor values.</returns>
    public static float GetTotalArmor()
    {
        float total = 0f;
        foreach (var config in HephaestusConfigs.Values)
        {
            total += config.Armor;
        }
        return total;
    }

    /// <summary>
    /// Gets the total structure across all sections.
    /// </summary>
    /// <returns>Sum of all section structure values.</returns>
    public static float GetTotalStructure()
    {
        float total = 0f;
        foreach (var config in HephaestusConfigs.Values)
        {
            total += config.Structure;
        }
        return total;
    }
}
