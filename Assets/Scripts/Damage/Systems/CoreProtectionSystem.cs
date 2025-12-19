using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages Core section protection rules.
/// Core cannot be hit directly unless adjacent section is breached.
/// 5% "lucky shot" chance on structure damage can punch through to Core.
/// </summary>
public class CoreProtectionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SectionManager sectionManager;
    [SerializeField] private Ship parentShip;

    [Header("Configuration")]
    [SerializeField] private float luckyShotChance = 0.05f; // 5% chance

    [Header("Debug")]
    [SerializeField] private bool logCoreAccessChecks = false;

    // Adjacent section mapping: which sections protect Core from which angles
    private static readonly Dictionary<SectionType, Vector3> SectionDirections = new Dictionary<SectionType, Vector3>
    {
        { SectionType.Fore, Vector3.forward },
        { SectionType.Aft, Vector3.back },
        { SectionType.Port, Vector3.left },
        { SectionType.Starboard, Vector3.right },
        { SectionType.Dorsal, Vector3.up },
        { SectionType.Ventral, Vector3.down }
    };

    // Events
    /// <summary>Fired when a lucky shot hits Core.</summary>
    public event Action<Vector3> OnLuckyShotHitCore;

    /// <summary>Fired when Core access is granted due to breach.</summary>
    public event Action<SectionType, Vector3> OnCoreExposedFromBreach;

    // Properties
    public float LuckyShotChance => luckyShotChance;

    private void Awake()
    {
        if (sectionManager == null)
        {
            sectionManager = GetComponent<SectionManager>();
            if (sectionManager == null)
            {
                sectionManager = GetComponentInParent<SectionManager>();
            }
        }

        if (parentShip == null)
        {
            parentShip = GetComponent<Ship>();
            if (parentShip == null)
            {
                parentShip = GetComponentInParent<Ship>();
            }
        }
    }

    /// <summary>
    /// Sets references manually (for testing or setup).
    /// </summary>
    public void SetReferences(SectionManager sm, Ship ship)
    {
        sectionManager = sm;
        parentShip = ship;
    }

    /// <summary>
    /// Determines if an attack from the given direction can hit the Core.
    /// Core is protected unless the adjacent section is breached.
    /// </summary>
    /// <param name="attackDirection">World-space direction of incoming attack (towards the ship).</param>
    /// <returns>True if Core can be hit from this angle.</returns>
    public bool CanHitCore(Vector3 attackDirection)
    {
        if (sectionManager == null)
        {
            Debug.LogWarning("[CoreProtectionSystem] No SectionManager - Core protected by default");
            return false;
        }

        // Convert attack direction to local space
        Vector3 localDirection = transform.InverseTransformDirection(attackDirection.normalized);

        // Get the adjacent section for this attack angle
        SectionType adjacentSection = GetAdjacentSection(localDirection);

        // Check if that section is breached
        bool isBreached = IsAdjacentSectionBreached(adjacentSection);

        if (logCoreAccessChecks)
        {
            Debug.Log($"[CoreProtectionSystem] Attack from {localDirection:F2}, " +
                     $"Adjacent: {adjacentSection}, Breached: {isBreached}");
        }

        if (isBreached)
        {
            OnCoreExposedFromBreach?.Invoke(adjacentSection, attackDirection);
        }

        return isBreached;
    }

    /// <summary>
    /// Determines which section is "adjacent" to Core from the given attack direction.
    /// Uses dot product to find the most aligned section.
    /// The attack direction represents the direction the projectile is traveling.
    /// An attack traveling forward (+Z) hits the Fore section.
    /// </summary>
    /// <param name="localDirection">Attack direction in local space (direction attack is traveling).</param>
    /// <returns>The section type that would be hit first from this angle.</returns>
    public SectionType GetAdjacentSection(Vector3 localDirection)
    {
        SectionType bestMatch = SectionType.Fore;
        float bestDot = float.MinValue;

        foreach (var kvp in SectionDirections)
        {
            // Match attack direction to section direction
            // Attack traveling +Z (forward) hits Fore section (which is in +Z direction)
            float dot = Vector3.Dot(localDirection, kvp.Value);

            if (dot > bestDot)
            {
                bestDot = dot;
                bestMatch = kvp.Key;
            }
        }

        return bestMatch;
    }

    /// <summary>
    /// Checks if the specified section is breached.
    /// </summary>
    /// <param name="section">The section type to check.</param>
    /// <returns>True if the section is breached.</returns>
    public bool IsAdjacentSectionBreached(SectionType section)
    {
        if (sectionManager == null) return false;

        ShipSection shipSection = sectionManager.GetSection(section);
        return shipSection != null && shipSection.IsBreached;
    }

    /// <summary>
    /// Checks if the specified section is breached (by direction).
    /// </summary>
    /// <param name="attackDirection">World-space attack direction.</param>
    /// <returns>True if the adjacent section is breached.</returns>
    public bool IsAdjacentSectionBreached(Vector3 attackDirection)
    {
        Vector3 localDirection = transform.InverseTransformDirection(attackDirection.normalized);
        SectionType adjacentSection = GetAdjacentSection(localDirection);
        return IsAdjacentSectionBreached(adjacentSection);
    }

    /// <summary>
    /// Rolls for a lucky shot that bypasses Core protection.
    /// Called when structure damage occurs on any section.
    /// </summary>
    /// <returns>True if lucky shot hits (5% chance by default).</returns>
    public bool RollLuckyShot()
    {
        float roll = UnityEngine.Random.value;
        bool isLuckyShot = roll < luckyShotChance;

        if (isLuckyShot)
        {
            Debug.Log($"[CoreProtectionSystem] LUCKY SHOT! Roll: {roll:F3} < {luckyShotChance:F3}");
            OnLuckyShotHitCore?.Invoke(Vector3.zero);
        }

        return isLuckyShot;
    }

    /// <summary>
    /// Forces a lucky shot (for testing).
    /// </summary>
    public bool ForceLuckyShot()
    {
        Debug.Log("[CoreProtectionSystem] Forced lucky shot hit!");
        OnLuckyShotHitCore?.Invoke(Vector3.zero);
        return true;
    }

    /// <summary>
    /// Checks if Core is currently exposed from any angle.
    /// </summary>
    /// <returns>True if any adjacent section is breached.</returns>
    public bool IsCoreExposed()
    {
        if (sectionManager == null) return false;

        foreach (SectionType section in SectionDirections.Keys)
        {
            if (IsAdjacentSectionBreached(section))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all sections that are breached and exposing Core.
    /// </summary>
    /// <returns>List of breached section types.</returns>
    public List<SectionType> GetExposedAngles()
    {
        List<SectionType> exposed = new List<SectionType>();

        if (sectionManager == null) return exposed;

        foreach (SectionType section in SectionDirections.Keys)
        {
            if (IsAdjacentSectionBreached(section))
            {
                exposed.Add(section);
            }
        }

        return exposed;
    }

    /// <summary>
    /// Gets the world direction from which Core is exposed due to a breached section.
    /// </summary>
    /// <param name="breachedSection">The breached section type.</param>
    /// <returns>World direction from which Core is exposed.</returns>
    public Vector3 GetExposedDirection(SectionType breachedSection)
    {
        if (SectionDirections.TryGetValue(breachedSection, out Vector3 localDir))
        {
            return transform.TransformDirection(-localDir);
        }
        return Vector3.zero;
    }
}
