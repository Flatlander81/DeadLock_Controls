/// <summary>
/// Result struct returned when damage is applied to a section.
/// Contains detailed breakdown of how damage was distributed.
/// </summary>
public struct DamageResult
{
    /// <summary>Amount of damage absorbed by armor.</summary>
    public float DamageToArmor;

    /// <summary>Amount of damage that penetrated to structure.</summary>
    public float DamageToStructure;

    /// <summary>Damage that exceeded section capacity (for Core breach calculation).</summary>
    public float OverflowDamage;

    /// <summary>True if this hit depleted all remaining armor.</summary>
    public bool ArmorBroken;

    /// <summary>True if this hit caused the section to breach.</summary>
    public bool SectionBreached;

    /// <summary>True if the section was already breached before this hit.</summary>
    public bool WasAlreadyBreached;

    /// <summary>
    /// Total damage that was actually applied to the section.
    /// </summary>
    public float TotalDamageApplied => DamageToArmor + DamageToStructure;

    /// <summary>
    /// Creates a new damage result.
    /// </summary>
    public DamageResult(
        float damageToArmor,
        float damageToStructure,
        float overflowDamage = 0f,
        bool armorBroken = false,
        bool sectionBreached = false,
        bool wasAlreadyBreached = false)
    {
        DamageToArmor = damageToArmor;
        DamageToStructure = damageToStructure;
        OverflowDamage = overflowDamage;
        ArmorBroken = armorBroken;
        SectionBreached = sectionBreached;
        WasAlreadyBreached = wasAlreadyBreached;
    }

    /// <summary>
    /// Creates a result for when damage hits an already breached section.
    /// </summary>
    public static DamageResult AlreadyBreached(float incomingDamage)
    {
        return new DamageResult(
            damageToArmor: 0f,
            damageToStructure: 0f,
            overflowDamage: incomingDamage,
            armorBroken: false,
            sectionBreached: false,
            wasAlreadyBreached: true);
    }

    public override string ToString()
    {
        return $"DamageResult[Armor:{DamageToArmor:F1}, Structure:{DamageToStructure:F1}, " +
               $"Overflow:{OverflowDamage:F1}, ArmorBroken:{ArmorBroken}, " +
               $"Breached:{SectionBreached}, WasBreached:{WasAlreadyBreached}]";
    }
}
