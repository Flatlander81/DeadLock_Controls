/// <summary>
/// Result struct returned when a critical hit roll is made.
/// Contains information about the roll, what was hit, and the outcome.
/// </summary>
public struct CriticalHitResult
{
    /// <summary>The slot number that was rolled (1-based).</summary>
    public int RolledSlot;

    /// <summary>Total slots in the section (equals max structure).</summary>
    public int TotalSlots;

    /// <summary>The system that was hit, or null if empty slot.</summary>
    public MountedSystem SystemHit;

    /// <summary>True if the rolled slot was empty (no critical damage).</summary>
    public bool WasEmptySlot;

    /// <summary>True if a system was damaged by this hit.</summary>
    public bool SystemWasDamaged;

    /// <summary>True if a system was destroyed by this hit.</summary>
    public bool SystemWasDestroyed;

    /// <summary>The previous state of the system before this hit.</summary>
    public SystemState PreviousState;

    /// <summary>The new state of the system after this hit.</summary>
    public SystemState NewState;

    /// <summary>The section type where the critical occurred.</summary>
    public SectionType SectionType;

    /// <summary>
    /// Creates a result for an empty slot hit.
    /// </summary>
    public static CriticalHitResult EmptySlot(int rolledSlot, int totalSlots, SectionType sectionType)
    {
        return new CriticalHitResult
        {
            RolledSlot = rolledSlot,
            TotalSlots = totalSlots,
            SystemHit = null,
            WasEmptySlot = true,
            SystemWasDamaged = false,
            SystemWasDestroyed = false,
            PreviousState = SystemState.Operational,
            NewState = SystemState.Operational,
            SectionType = sectionType
        };
    }

    /// <summary>
    /// Creates a result for a system hit.
    /// </summary>
    public static CriticalHitResult SystemHitResult(
        int rolledSlot,
        int totalSlots,
        MountedSystem system,
        SystemState previousState,
        SystemState newState,
        SectionType sectionType)
    {
        return new CriticalHitResult
        {
            RolledSlot = rolledSlot,
            TotalSlots = totalSlots,
            SystemHit = system,
            WasEmptySlot = false,
            SystemWasDamaged = newState == SystemState.Damaged && previousState == SystemState.Operational,
            SystemWasDestroyed = newState == SystemState.Destroyed,
            PreviousState = previousState,
            NewState = newState,
            SectionType = sectionType
        };
    }

    /// <summary>
    /// Creates a result for hitting an already destroyed system.
    /// </summary>
    public static CriticalHitResult DestroyedSystemAbsorbed(
        int rolledSlot,
        int totalSlots,
        MountedSystem system,
        SectionType sectionType)
    {
        return new CriticalHitResult
        {
            RolledSlot = rolledSlot,
            TotalSlots = totalSlots,
            SystemHit = system,
            WasEmptySlot = false,
            SystemWasDamaged = false,
            SystemWasDestroyed = false,
            PreviousState = SystemState.Destroyed,
            NewState = SystemState.Destroyed,
            SectionType = sectionType
        };
    }

    /// <summary>
    /// Gets the system type that was hit, or None if empty.
    /// </summary>
    public ShipSystemType SystemTypeHit => SystemHit != null ? SystemHit.SystemType : ShipSystemType.None;

    public override string ToString()
    {
        if (WasEmptySlot)
        {
            return $"CriticalHit[{SectionType}] Slot {RolledSlot}/{TotalSlots}: MISS (empty slot)";
        }
        else if (SystemWasDestroyed)
        {
            return $"CriticalHit[{SectionType}] Slot {RolledSlot}/{TotalSlots}: {ShipSystemData.GetName(SystemTypeHit)} DESTROYED!";
        }
        else if (SystemWasDamaged)
        {
            return $"CriticalHit[{SectionType}] Slot {RolledSlot}/{TotalSlots}: {ShipSystemData.GetName(SystemTypeHit)} DAMAGED";
        }
        else
        {
            return $"CriticalHit[{SectionType}] Slot {RolledSlot}/{TotalSlots}: {ShipSystemData.GetName(SystemTypeHit)} (absorbed, already {NewState})";
        }
    }
}
