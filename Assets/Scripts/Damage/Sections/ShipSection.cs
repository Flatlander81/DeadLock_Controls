using UnityEngine;
using System;

/// <summary>
/// Represents a single section of a ship that can receive damage.
/// Damage flows through armor first, then structure. Section breaches when structure hits 0.
/// </summary>
public class ShipSection : MonoBehaviour
{
    [Header("Section Configuration")]
    [SerializeField] private SectionType sectionType;
    [SerializeField] private float maxArmor = 100f;
    [SerializeField] private float maxStructure = 50f;

    [Header("Runtime State")]
    [SerializeField] private float currentArmor;
    [SerializeField] private float currentStructure;
    [SerializeField] private bool isBreached = false;

    [Header("References")]
    [SerializeField] private Ship parentShip;

    // Events
    /// <summary>Fired when armor takes damage. Parameters: section, damageAmount, remainingArmor.</summary>
    public event Action<ShipSection, float, float> OnArmorDamaged;

    /// <summary>Fired when structure takes damage. Parameters: section, damageAmount, remainingStructure.</summary>
    public event Action<ShipSection, float, float> OnStructureDamaged;

    /// <summary>Fired when section becomes breached. Parameters: section.</summary>
    public event Action<ShipSection> OnSectionBreached;

    // Public properties
    public SectionType SectionType => sectionType;
    public float MaxArmor => maxArmor;
    public float CurrentArmor => currentArmor;
    public float MaxStructure => maxStructure;
    public float CurrentStructure => currentStructure;
    public bool IsBreached => isBreached;
    public Ship ParentShip => parentShip;

    /// <summary>
    /// Initialize the section with configuration from SectionDefinitions.
    /// </summary>
    /// <param name="type">The section type.</param>
    /// <param name="ship">The parent ship.</param>
    public void Initialize(SectionType type, Ship ship)
    {
        sectionType = type;
        parentShip = ship;

        var config = SectionDefinitions.GetConfig(type);
        maxArmor = config.Armor;
        maxStructure = config.Structure;

        currentArmor = maxArmor;
        currentStructure = maxStructure;
        isBreached = false;

        Debug.Log($"[ShipSection] Initialized {sectionType} - Armor: {maxArmor}, Structure: {maxStructure}");
    }

    /// <summary>
    /// Initialize with custom values (for testing or special ships).
    /// </summary>
    /// <param name="type">The section type.</param>
    /// <param name="armor">Maximum armor value.</param>
    /// <param name="structure">Maximum structure value.</param>
    /// <param name="ship">The parent ship (optional).</param>
    public void Initialize(SectionType type, float armor, float structure, Ship ship = null)
    {
        sectionType = type;
        parentShip = ship;
        maxArmor = armor;
        maxStructure = structure;

        currentArmor = maxArmor;
        currentStructure = maxStructure;
        isBreached = false;

        Debug.Log($"[ShipSection] Initialized {sectionType} (custom) - Armor: {maxArmor}, Structure: {maxStructure}");
    }

    /// <summary>
    /// Apply damage to this section.
    /// Damage flows: Armor → Structure → Breach.
    /// </summary>
    /// <param name="incomingDamage">Amount of damage to apply.</param>
    /// <returns>Detailed result of how damage was distributed.</returns>
    public DamageResult ApplyDamage(float incomingDamage)
    {
        if (incomingDamage <= 0f)
        {
            return new DamageResult();
        }

        // Already breached - all damage overflows
        if (isBreached)
        {
            Debug.Log($"[ShipSection] {sectionType} already breached, {incomingDamage:F1} damage overflows");
            return DamageResult.AlreadyBreached(incomingDamage);
        }

        float remainingDamage = incomingDamage;
        float damageToArmor = 0f;
        float damageToStructure = 0f;
        float overflowDamage = 0f;
        bool armorBroken = false;
        bool sectionBreached = false;

        // Apply damage to armor first
        if (currentArmor > 0f)
        {
            damageToArmor = Mathf.Min(remainingDamage, currentArmor);
            currentArmor -= damageToArmor;
            remainingDamage -= damageToArmor;

            if (currentArmor <= 0f)
            {
                currentArmor = 0f;
                armorBroken = true;
                Debug.Log($"[ShipSection] {sectionType} armor broken!");
            }

            OnArmorDamaged?.Invoke(this, damageToArmor, currentArmor);
        }

        // Apply remaining damage to structure
        if (remainingDamage > 0f && currentStructure > 0f)
        {
            damageToStructure = Mathf.Min(remainingDamage, currentStructure);
            currentStructure -= damageToStructure;
            remainingDamage -= damageToStructure;

            OnStructureDamaged?.Invoke(this, damageToStructure, currentStructure);

            // Check for breach
            if (currentStructure <= 0f)
            {
                currentStructure = 0f;
                isBreached = true;
                sectionBreached = true;
                Debug.Log($"[ShipSection] {sectionType} BREACHED!");
                OnSectionBreached?.Invoke(this);
            }
        }

        // Any remaining damage overflows
        overflowDamage = remainingDamage;

        var result = new DamageResult(
            damageToArmor: damageToArmor,
            damageToStructure: damageToStructure,
            overflowDamage: overflowDamage,
            armorBroken: armorBroken,
            sectionBreached: sectionBreached,
            wasAlreadyBreached: false);

        Debug.Log($"[ShipSection] {sectionType} took {incomingDamage:F1} damage - {result}");

        return result;
    }

    /// <summary>
    /// Gets armor as a percentage (0-1).
    /// </summary>
    public float GetArmorPercentage()
    {
        if (maxArmor <= 0f) return 0f;
        return currentArmor / maxArmor;
    }

    /// <summary>
    /// Gets structure as a percentage (0-1).
    /// </summary>
    public float GetStructurePercentage()
    {
        if (maxStructure <= 0f) return 0f;
        return currentStructure / maxStructure;
    }

    /// <summary>
    /// Checks if section is operational (not breached).
    /// </summary>
    public bool IsOperational()
    {
        return !isBreached;
    }

    /// <summary>
    /// Resets section to full health.
    /// </summary>
    public void Reset()
    {
        currentArmor = maxArmor;
        currentStructure = maxStructure;
        isBreached = false;

        Debug.Log($"[ShipSection] {sectionType} reset to full health");
    }

    /// <summary>
    /// Sets the parent ship reference.
    /// </summary>
    /// <param name="ship">The parent ship.</param>
    public void SetParentShip(Ship ship)
    {
        parentShip = ship;
    }

    private void OnValidate()
    {
        // Ensure current values don't exceed max
        currentArmor = Mathf.Clamp(currentArmor, 0f, maxArmor);
        currentStructure = Mathf.Clamp(currentStructure, 0f, maxStructure);
    }
}
