using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages all sections on a ship.
/// Provides centralized access to section data and damage routing.
/// </summary>
public class SectionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ship parentShip;

    [Header("Runtime State")]
    [SerializeField] private List<ShipSection> sections = new List<ShipSection>();

    // Lookup dictionary for O(1) section access
    private Dictionary<SectionType, ShipSection> sectionLookup = new Dictionary<SectionType, ShipSection>();

    // Events
    /// <summary>Fired when any section takes armor damage.</summary>
    public event Action<ShipSection, float, float> OnSectionArmorDamaged;

    /// <summary>Fired when any section takes structure damage.</summary>
    public event Action<ShipSection, float, float> OnSectionStructureDamaged;

    /// <summary>Fired when any section becomes breached.</summary>
    public event Action<ShipSection> OnSectionBreached;

    // Properties
    public Ship ParentShip => parentShip;
    public int SectionCount => sections.Count;

    private void Awake()
    {
        if (parentShip == null)
        {
            parentShip = GetComponent<Ship>();
        }
    }

    /// <summary>
    /// Registers a section with the manager.
    /// </summary>
    /// <param name="section">The section to register.</param>
    public void RegisterSection(ShipSection section)
    {
        if (section == null)
        {
            Debug.LogWarning("[SectionManager] Cannot register null section");
            return;
        }

        if (sectionLookup.ContainsKey(section.SectionType))
        {
            Debug.LogWarning($"[SectionManager] Section {section.SectionType} already registered, replacing");
            UnregisterSection(sectionLookup[section.SectionType]);
        }

        sections.Add(section);
        sectionLookup[section.SectionType] = section;

        // Subscribe to section events
        section.OnArmorDamaged += HandleSectionArmorDamaged;
        section.OnStructureDamaged += HandleSectionStructureDamaged;
        section.OnSectionBreached += HandleSectionBreached;

        Debug.Log($"[SectionManager] Registered section: {section.SectionType}");
    }

    /// <summary>
    /// Unregisters a section from the manager.
    /// </summary>
    /// <param name="section">The section to unregister.</param>
    public void UnregisterSection(ShipSection section)
    {
        if (section == null) return;

        // Unsubscribe from events
        section.OnArmorDamaged -= HandleSectionArmorDamaged;
        section.OnStructureDamaged -= HandleSectionStructureDamaged;
        section.OnSectionBreached -= HandleSectionBreached;

        sections.Remove(section);
        sectionLookup.Remove(section.SectionType);

        Debug.Log($"[SectionManager] Unregistered section: {section.SectionType}");
    }

    /// <summary>
    /// Auto-registers all ShipSection components in children.
    /// </summary>
    public void AutoRegisterChildSections()
    {
        ShipSection[] childSections = GetComponentsInChildren<ShipSection>();

        foreach (ShipSection section in childSections)
        {
            if (!sectionLookup.ContainsKey(section.SectionType))
            {
                section.SetParentShip(parentShip);
                RegisterSection(section);
            }
        }

        Debug.Log($"[SectionManager] Auto-registered {childSections.Length} sections");
    }

    /// <summary>
    /// Gets a section by type.
    /// </summary>
    /// <param name="type">The section type to find.</param>
    /// <returns>The section, or null if not found.</returns>
    public ShipSection GetSection(SectionType type)
    {
        if (sectionLookup.TryGetValue(type, out ShipSection section))
        {
            return section;
        }

        Debug.LogWarning($"[SectionManager] Section {type} not found");
        return null;
    }

    /// <summary>
    /// Gets all registered sections.
    /// </summary>
    /// <returns>List of all sections.</returns>
    public List<ShipSection> GetAllSections()
    {
        return new List<ShipSection>(sections);
    }

    /// <summary>
    /// Gets all breached sections.
    /// </summary>
    /// <returns>List of breached sections.</returns>
    public List<ShipSection> GetBreachedSections()
    {
        List<ShipSection> breached = new List<ShipSection>();

        foreach (ShipSection section in sections)
        {
            if (section.IsBreached)
            {
                breached.Add(section);
            }
        }

        return breached;
    }

    /// <summary>
    /// Gets all operational (non-breached) sections.
    /// </summary>
    /// <returns>List of operational sections.</returns>
    public List<ShipSection> GetOperationalSections()
    {
        List<ShipSection> operational = new List<ShipSection>();

        foreach (ShipSection section in sections)
        {
            if (section.IsOperational())
            {
                operational.Add(section);
            }
        }

        return operational;
    }

    /// <summary>
    /// Gets total remaining armor across all sections.
    /// </summary>
    /// <returns>Sum of current armor values.</returns>
    public float GetTotalArmorRemaining()
    {
        float total = 0f;

        foreach (ShipSection section in sections)
        {
            total += section.CurrentArmor;
        }

        return total;
    }

    /// <summary>
    /// Gets total remaining structure across all sections.
    /// </summary>
    /// <returns>Sum of current structure values.</returns>
    public float GetTotalStructureRemaining()
    {
        float total = 0f;

        foreach (ShipSection section in sections)
        {
            total += section.CurrentStructure;
        }

        return total;
    }

    /// <summary>
    /// Gets total maximum armor across all sections.
    /// </summary>
    /// <returns>Sum of max armor values.</returns>
    public float GetTotalMaxArmor()
    {
        float total = 0f;

        foreach (ShipSection section in sections)
        {
            total += section.MaxArmor;
        }

        return total;
    }

    /// <summary>
    /// Gets total maximum structure across all sections.
    /// </summary>
    /// <returns>Sum of max structure values.</returns>
    public float GetTotalMaxStructure()
    {
        float total = 0f;

        foreach (ShipSection section in sections)
        {
            total += section.MaxStructure;
        }

        return total;
    }

    /// <summary>
    /// Resets all sections to full health.
    /// </summary>
    public void ResetAllSections()
    {
        foreach (ShipSection section in sections)
        {
            section.Reset();
        }

        Debug.Log("[SectionManager] All sections reset");
    }

    /// <summary>
    /// Checks if all sections are breached (ship destroyed).
    /// </summary>
    /// <returns>True if all sections breached.</returns>
    public bool AreAllSectionsBreached()
    {
        if (sections.Count == 0) return false;

        foreach (ShipSection section in sections)
        {
            if (!section.IsBreached)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the Core section is breached.
    /// </summary>
    /// <returns>True if Core is breached.</returns>
    public bool IsCoreBreached()
    {
        ShipSection core = GetSection(SectionType.Core);
        return core != null && core.IsBreached;
    }

    /// <summary>
    /// Sets the parent ship reference.
    /// </summary>
    /// <param name="ship">The parent ship.</param>
    public void SetParentShip(Ship ship)
    {
        parentShip = ship;
    }

    // Event handlers
    private void HandleSectionArmorDamaged(ShipSection section, float damage, float remaining)
    {
        OnSectionArmorDamaged?.Invoke(section, damage, remaining);
    }

    private void HandleSectionStructureDamaged(ShipSection section, float damage, float remaining)
    {
        OnSectionStructureDamaged?.Invoke(section, damage, remaining);
    }

    private void HandleSectionBreached(ShipSection section)
    {
        OnSectionBreached?.Invoke(section);

        // Check for ship destruction via Core breach
        if (section.SectionType == SectionType.Core)
        {
            Debug.Log("[SectionManager] CORE BREACHED - Ship destroyed!");
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        foreach (ShipSection section in sections)
        {
            if (section != null)
            {
                section.OnArmorDamaged -= HandleSectionArmorDamaged;
                section.OnStructureDamaged -= HandleSectionStructureDamaged;
                section.OnSectionBreached -= HandleSectionBreached;
            }
        }
    }
}
