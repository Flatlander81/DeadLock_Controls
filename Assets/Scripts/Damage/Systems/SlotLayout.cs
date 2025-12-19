using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages the slot layout for a ship section.
/// Tracks which systems occupy which slots and handles slot queries.
/// </summary>
[Serializable]
public class SlotLayout
{
    [Header("Configuration")]
    [SerializeField] private int totalSlots;
    [SerializeField] private SectionType sectionType;

    [Header("Mounted Systems")]
    [SerializeField] private List<MountedSystem> mountedSystems = new List<MountedSystem>();

    // Slot lookup cache (rebuilt when systems change)
    private MountedSystem[] slotToSystem;
    private bool cacheValid = false;

    /// <summary>
    /// Total number of slots in this layout.
    /// </summary>
    public int TotalSlots => totalSlots;

    /// <summary>
    /// Section type this layout belongs to.
    /// </summary>
    public SectionType SectionType => sectionType;

    /// <summary>
    /// List of all mounted systems.
    /// </summary>
    public IReadOnlyList<MountedSystem> MountedSystems => mountedSystems;

    /// <summary>
    /// Creates a new slot layout.
    /// </summary>
    /// <param name="slots">Total number of slots (usually equals max structure).</param>
    /// <param name="type">Section type.</param>
    public SlotLayout(int slots, SectionType type)
    {
        totalSlots = slots;
        sectionType = type;
        mountedSystems = new List<MountedSystem>();
        InvalidateCache();
    }

    /// <summary>
    /// Adds a system to the layout at the specified starting slot.
    /// </summary>
    /// <param name="system">System to add.</param>
    /// <returns>True if added successfully, false if slots are occupied.</returns>
    public bool AddSystem(MountedSystem system)
    {
        if (system == null) return false;

        // Check if slots are available
        int startSlot = system.SlotStart;
        int endSlot = system.SlotEnd;

        for (int slot = startSlot; slot <= endSlot; slot++)
        {
            if (slot < 1 || slot > totalSlots)
            {
                Debug.LogError($"[SlotLayout] System {system.SystemType} slot {slot} is out of range (1-{totalSlots})");
                return false;
            }

            MountedSystem existing = GetSystemAtSlot(slot);
            if (existing != null)
            {
                Debug.LogError($"[SlotLayout] Slot {slot} already occupied by {existing.SystemType}");
                return false;
            }
        }

        mountedSystems.Add(system);
        InvalidateCache();

        Debug.Log($"[SlotLayout] Added {system.SystemType} to {sectionType} at slots {startSlot}-{endSlot}");
        return true;
    }

    /// <summary>
    /// Adds a system to the layout with slot assignment.
    /// Use this when the system is already a MonoBehaviour on a GameObject.
    /// </summary>
    /// <param name="system">System to add (must be initialized).</param>
    /// <param name="startSlot">Starting slot (1-based). System must be initialized with matching slot.</param>
    /// <param name="size">System size in slots.</param>
    /// <returns>True if added successfully, false if slots are occupied.</returns>
    public bool AddSystem(MountedSystem system, int startSlot, int size)
    {
        if (system == null) return false;

        // Check if slots are available
        int endSlot = startSlot + size - 1;

        for (int slot = startSlot; slot <= endSlot; slot++)
        {
            if (slot < 1 || slot > totalSlots)
            {
                Debug.LogError($"[SlotLayout] System {system.SystemType} slot {slot} is out of range (1-{totalSlots})");
                return false;
            }

            MountedSystem existing = GetSystemAtSlot(slot);
            if (existing != null)
            {
                Debug.LogError($"[SlotLayout] Slot {slot} already occupied by {existing.SystemType}");
                return false;
            }
        }

        // Set slot info on system via reflection or directly if accessible
        // This is a workaround since MountedSystem fields are private with Initialize
        SetSystemSlots(system, startSlot, endSlot);

        mountedSystems.Add(system);
        InvalidateCache();

        Debug.Log($"[SlotLayout] Added {system.SystemType} to {sectionType} at slots {startSlot}-{endSlot}");
        return true;
    }

    /// <summary>
    /// Sets slot info on a MountedSystem using reflection.
    /// </summary>
    private void SetSystemSlots(MountedSystem system, int start, int end)
    {
        var slotStartField = typeof(MountedSystem).GetField("slotStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var slotEndField = typeof(MountedSystem).GetField("slotEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (slotStartField != null) slotStartField.SetValue(system, start);
        if (slotEndField != null) slotEndField.SetValue(system, end);
    }

    /// <summary>
    /// Removes a system from the layout.
    /// </summary>
    /// <param name="system">System to remove.</param>
    /// <returns>True if removed, false if not found.</returns>
    public bool RemoveSystem(MountedSystem system)
    {
        if (mountedSystems.Remove(system))
        {
            InvalidateCache();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the system at a specific slot.
    /// </summary>
    /// <param name="slot">Slot number (1-based).</param>
    /// <returns>The system at that slot, or null if empty.</returns>
    public MountedSystem GetSystemAtSlot(int slot)
    {
        if (slot < 1 || slot > totalSlots)
        {
            return null;
        }

        EnsureCacheValid();
        return slotToSystem[slot - 1]; // Convert to 0-based index
    }

    /// <summary>
    /// Checks if a slot is occupied by a system.
    /// </summary>
    public bool IsSlotOccupied(int slot)
    {
        return GetSystemAtSlot(slot) != null;
    }

    /// <summary>
    /// Checks if a slot is empty.
    /// </summary>
    public bool IsSlotEmpty(int slot)
    {
        return GetSystemAtSlot(slot) == null;
    }

    /// <summary>
    /// Gets the count of occupied slots.
    /// </summary>
    public int GetOccupiedSlotCount()
    {
        int count = 0;
        foreach (var system in mountedSystems)
        {
            count += system.Size;
        }
        return count;
    }

    /// <summary>
    /// Gets the count of empty slots.
    /// </summary>
    public int GetEmptySlotCount()
    {
        return totalSlots - GetOccupiedSlotCount();
    }

    /// <summary>
    /// Gets all systems of a specific type.
    /// </summary>
    public List<MountedSystem> GetSystemsOfType(ShipSystemType type)
    {
        List<MountedSystem> result = new List<MountedSystem>();
        foreach (var system in mountedSystems)
        {
            if (system.SystemType == type)
            {
                result.Add(system);
            }
        }
        return result;
    }

    /// <summary>
    /// Gets all operational systems.
    /// </summary>
    public List<MountedSystem> GetOperationalSystems()
    {
        List<MountedSystem> result = new List<MountedSystem>();
        foreach (var system in mountedSystems)
        {
            if (system.IsOperational)
            {
                result.Add(system);
            }
        }
        return result;
    }

    /// <summary>
    /// Gets all destroyed systems.
    /// </summary>
    public List<MountedSystem> GetDestroyedSystems()
    {
        List<MountedSystem> result = new List<MountedSystem>();
        foreach (var system in mountedSystems)
        {
            if (system.IsDestroyed)
            {
                result.Add(system);
            }
        }
        return result;
    }

    /// <summary>
    /// Clears all systems from the layout.
    /// </summary>
    public void Clear()
    {
        mountedSystems.Clear();
        InvalidateCache();
    }

    /// <summary>
    /// Invalidates the slot lookup cache.
    /// </summary>
    public void InvalidateCache()
    {
        cacheValid = false;
    }

    /// <summary>
    /// Ensures the slot lookup cache is valid.
    /// </summary>
    private void EnsureCacheValid()
    {
        if (cacheValid && slotToSystem != null && slotToSystem.Length == totalSlots)
        {
            return;
        }

        // Rebuild cache
        slotToSystem = new MountedSystem[totalSlots];

        foreach (var system in mountedSystems)
        {
            for (int slot = system.SlotStart; slot <= system.SlotEnd; slot++)
            {
                if (slot >= 1 && slot <= totalSlots)
                {
                    slotToSystem[slot - 1] = system;
                }
            }
        }

        cacheValid = true;
    }

    /// <summary>
    /// Gets a visual representation of the slot layout.
    /// </summary>
    public string GetLayoutVisualization()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== {sectionType} Slot Layout ({totalSlots} slots) ===");

        EnsureCacheValid();

        int currentSlot = 1;
        while (currentSlot <= totalSlots)
        {
            MountedSystem system = slotToSystem[currentSlot - 1];

            if (system != null)
            {
                // Show system range
                string state = system.CurrentState.ToString().Substring(0, 1);
                sb.AppendLine($"[{system.SlotStart,3}-{system.SlotEnd,3}] {ShipSystemData.GetName(system.SystemType)} ({state})");
                currentSlot = system.SlotEnd + 1;
            }
            else
            {
                // Count consecutive empty slots
                int emptyStart = currentSlot;
                while (currentSlot <= totalSlots && slotToSystem[currentSlot - 1] == null)
                {
                    currentSlot++;
                }
                int emptyEnd = currentSlot - 1;

                if (emptyStart == emptyEnd)
                {
                    sb.AppendLine($"[{emptyStart,3}    ] (empty)");
                }
                else
                {
                    sb.AppendLine($"[{emptyStart,3}-{emptyEnd,3}] (empty)");
                }
            }
        }

        sb.AppendLine($"Occupied: {GetOccupiedSlotCount()}/{totalSlots}, Empty: {GetEmptySlotCount()}");
        return sb.ToString();
    }
}
