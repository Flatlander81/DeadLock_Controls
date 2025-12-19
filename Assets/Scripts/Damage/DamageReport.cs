/// <summary>
/// Detailed report of how damage was distributed through shields and sections.
/// Returned by DamageRouter after processing damage.
/// </summary>
public struct DamageReport
{
    /// <summary>Total incoming damage before any absorption.</summary>
    public float TotalIncomingDamage;

    /// <summary>Damage absorbed by shields.</summary>
    public float ShieldDamage;

    /// <summary>Damage absorbed by section armor.</summary>
    public float ArmorDamage;

    /// <summary>Damage that reached section structure.</summary>
    public float StructureDamage;

    /// <summary>Damage that overflowed past the section.</summary>
    public float OverflowDamage;

    /// <summary>True if shields were depleted by this hit.</summary>
    public bool ShieldsDepleted;

    /// <summary>True if section armor was broken by this hit.</summary>
    public bool ArmorBroken;

    /// <summary>True if section was breached by this hit.</summary>
    public bool SectionBreached;

    /// <summary>The type of section that was hit.</summary>
    public SectionType SectionHit;

    /// <summary>Reference to the section that was hit (may be null).</summary>
    public ShipSection Section;

    /// <summary>Critical hit result if structure was damaged (null if no critical).</summary>
    public CriticalHitResult? CriticalResult;

    /// <summary>True if Core was protected and damage was redirected.</summary>
    public bool CoreWasProtected;

    /// <summary>True if a lucky shot punched through to Core.</summary>
    public bool WasLuckyShot;

    /// <summary>True if a critical hit occurred.</summary>
    public bool HadCritical => CriticalResult.HasValue;

    /// <summary>True if a system was damaged by the critical hit.</summary>
    public bool SystemDamaged => CriticalResult.HasValue && CriticalResult.Value.SystemWasDamaged;

    /// <summary>True if a system was destroyed by the critical hit.</summary>
    public bool SystemDestroyed => CriticalResult.HasValue && CriticalResult.Value.SystemWasDestroyed;

    /// <summary>
    /// Total damage that was actually applied (shields + armor + structure).
    /// </summary>
    public float TotalDamageApplied => ShieldDamage + ArmorDamage + StructureDamage;

    /// <summary>
    /// Creates a new damage report.
    /// </summary>
    public DamageReport(
        float totalIncoming,
        float shieldDamage,
        float armorDamage,
        float structureDamage,
        float overflowDamage,
        bool shieldsDepleted,
        bool armorBroken,
        bool sectionBreached,
        SectionType sectionHit,
        ShipSection section,
        CriticalHitResult? criticalResult = null,
        bool coreWasProtected = false,
        bool wasLuckyShot = false)
    {
        TotalIncomingDamage = totalIncoming;
        ShieldDamage = shieldDamage;
        ArmorDamage = armorDamage;
        StructureDamage = structureDamage;
        OverflowDamage = overflowDamage;
        ShieldsDepleted = shieldsDepleted;
        ArmorBroken = armorBroken;
        SectionBreached = sectionBreached;
        SectionHit = sectionHit;
        Section = section;
        CriticalResult = criticalResult;
        CoreWasProtected = coreWasProtected;
        WasLuckyShot = wasLuckyShot;
    }

    /// <summary>
    /// Creates a report for damage that was fully absorbed by shields.
    /// </summary>
    public static DamageReport ShieldsAbsorbed(float totalIncoming, float absorbed, bool depleted)
    {
        return new DamageReport(
            totalIncoming: totalIncoming,
            shieldDamage: absorbed,
            armorDamage: 0f,
            structureDamage: 0f,
            overflowDamage: 0f,
            shieldsDepleted: depleted,
            armorBroken: false,
            sectionBreached: false,
            sectionHit: SectionType.Core, // Default
            section: null,
            criticalResult: null
        );
    }

    /// <summary>
    /// Creates a report for damage that bypassed shields (no shields active).
    /// </summary>
    public static DamageReport NoShields(float totalIncoming, DamageResult sectionResult, SectionType type, ShipSection section)
    {
        return new DamageReport(
            totalIncoming: totalIncoming,
            shieldDamage: 0f,
            armorDamage: sectionResult.DamageToArmor,
            structureDamage: sectionResult.DamageToStructure,
            overflowDamage: sectionResult.OverflowDamage,
            shieldsDepleted: false,
            armorBroken: sectionResult.ArmorBroken,
            sectionBreached: sectionResult.SectionBreached,
            sectionHit: type,
            section: section,
            criticalResult: sectionResult.CriticalResult
        );
    }

    public override string ToString()
    {
        string critStr = "";
        if (CriticalResult.HasValue)
        {
            var crit = CriticalResult.Value;
            if (crit.WasEmptySlot)
            {
                critStr = $", Critical:Miss";
            }
            else if (crit.SystemWasDestroyed)
            {
                critStr = $", Critical:{ShipSystemData.GetName(crit.SystemTypeHit)} DESTROYED";
            }
            else if (crit.SystemWasDamaged)
            {
                critStr = $", Critical:{ShipSystemData.GetName(crit.SystemTypeHit)} damaged";
            }
        }

        string coreStr = "";
        if (CoreWasProtected)
        {
            coreStr = ", CoreProtected";
        }
        if (WasLuckyShot)
        {
            coreStr += ", LUCKY_SHOT";
        }

        return $"DamageReport[Total:{TotalIncomingDamage:F1}, Shield:{ShieldDamage:F1}, " +
               $"Armor:{ArmorDamage:F1}, Structure:{StructureDamage:F1}, " +
               $"Overflow:{OverflowDamage:F1}, Section:{SectionHit}, " +
               $"ShieldsDepleted:{ShieldsDepleted}, ArmorBroken:{ArmorBroken}, Breached:{SectionBreached}{critStr}{coreStr}]";
    }
}
