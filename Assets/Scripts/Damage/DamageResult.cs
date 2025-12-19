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

    /// <summary>Critical hit result if structure was damaged (null if no critical).</summary>
    public CriticalHitResult? CriticalResult;

    /// <summary>
    /// Total damage that was actually applied to the section.
    /// </summary>
    public float TotalDamageApplied => DamageToArmor + DamageToStructure;

    /// <summary>True if a critical hit occurred.</summary>
    public bool HadCritical => CriticalResult.HasValue;

    /// <summary>True if a system was damaged by the critical hit.</summary>
    public bool SystemDamaged => CriticalResult.HasValue && CriticalResult.Value.SystemWasDamaged;

    /// <summary>True if a system was destroyed by the critical hit.</summary>
    public bool SystemDestroyed => CriticalResult.HasValue && CriticalResult.Value.SystemWasDestroyed;

    /// <summary>
    /// Creates a new damage result.
    /// </summary>
    public DamageResult(
        float damageToArmor,
        float damageToStructure,
        float overflowDamage = 0f,
        bool armorBroken = false,
        bool sectionBreached = false,
        bool wasAlreadyBreached = false,
        CriticalHitResult? criticalResult = null)
    {
        DamageToArmor = damageToArmor;
        DamageToStructure = damageToStructure;
        OverflowDamage = overflowDamage;
        ArmorBroken = armorBroken;
        SectionBreached = sectionBreached;
        WasAlreadyBreached = wasAlreadyBreached;
        CriticalResult = criticalResult;
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
            wasAlreadyBreached: true,
            criticalResult: null);
    }

    public override string ToString()
    {
        string critStr = "";
        if (CriticalResult.HasValue)
        {
            var crit = CriticalResult.Value;
            if (crit.WasEmptySlot)
            {
                critStr = $", Critical: Miss (slot {crit.RolledSlot})";
            }
            else if (crit.SystemWasDestroyed)
            {
                critStr = $", Critical: {ShipSystemData.GetName(crit.SystemTypeHit)} DESTROYED!";
            }
            else if (crit.SystemWasDamaged)
            {
                critStr = $", Critical: {ShipSystemData.GetName(crit.SystemTypeHit)} damaged";
            }
            else
            {
                critStr = $", Critical: {ShipSystemData.GetName(crit.SystemTypeHit)} (absorbed)";
            }
        }

        return $"DamageResult[Armor:{DamageToArmor:F1}, Structure:{DamageToStructure:F1}, " +
               $"Overflow:{OverflowDamage:F1}, ArmorBroken:{ArmorBroken}, " +
               $"Breached:{SectionBreached}, WasBreached:{WasAlreadyBreached}{critStr}]";
    }
}
