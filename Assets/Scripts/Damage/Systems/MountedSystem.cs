using UnityEngine;
using System;

/// <summary>
/// Base class for systems mounted in ship sections.
/// Tracks health state and slot assignment. Subclasses implement specific degradation effects.
/// </summary>
public class MountedSystem : MonoBehaviour
{
    [Header("System Configuration")]
    [SerializeField] private ShipSystemType systemType = ShipSystemType.None;
    [SerializeField] private int slotStart = 0;
    [SerializeField] private int slotEnd = 0;

    [Header("Runtime State")]
    [SerializeField] private SystemState currentState = SystemState.Operational;
    [SerializeField] private int hitsTaken = 0;

    [Header("References")]
    [SerializeField] private ShipSection parentSection;
    [SerializeField] private Ship parentShip;

    // Events
    /// <summary>Fired when system takes a critical hit. Parameters: system, previous state, new state.</summary>
    public event Action<MountedSystem, SystemState, SystemState> OnSystemDamaged;

    /// <summary>Fired when system is destroyed. Parameters: system.</summary>
    public event Action<MountedSystem> OnSystemDestroyed;

    // Properties
    public ShipSystemType SystemType => systemType;
    public int SlotStart => slotStart;
    public int SlotEnd => slotEnd;
    public int Size => ShipSystemData.GetSize(systemType);
    public SystemState CurrentState => currentState;
    public int HitsTaken => hitsTaken;
    public ShipSection ParentSection => parentSection;
    public Ship ParentShip => parentShip;
    public bool IsOperational => currentState == SystemState.Operational;
    public bool IsDamaged => currentState == SystemState.Damaged;
    public bool IsDestroyed => currentState == SystemState.Destroyed;

    /// <summary>
    /// Initialize the system with type and slot assignment.
    /// </summary>
    /// <param name="type">The system type.</param>
    /// <param name="startSlot">Starting slot index (1-based).</param>
    /// <param name="section">Parent section containing this system.</param>
    /// <param name="ship">Parent ship (optional).</param>
    public void Initialize(ShipSystemType type, int startSlot, ShipSection section, Ship ship = null)
    {
        systemType = type;
        slotStart = startSlot;
        slotEnd = startSlot + ShipSystemData.GetSize(type) - 1;
        parentSection = section;
        parentShip = ship ?? section?.ParentShip;
        currentState = SystemState.Operational;
        hitsTaken = 0;

        Debug.Log($"[MountedSystem] Initialized {ShipSystemData.GetName(type)} at slots {slotStart}-{slotEnd}");
    }

    /// <summary>
    /// Checks if a slot number falls within this system's slot range.
    /// </summary>
    /// <param name="slot">Slot number to check (1-based).</param>
    /// <returns>True if slot is within this system's range.</returns>
    public bool OccupiesSlot(int slot)
    {
        return slot >= slotStart && slot <= slotEnd;
    }

    /// <summary>
    /// Apply a critical hit to this system.
    /// First hit: Operational → Damaged. Second hit: Damaged → Destroyed.
    /// </summary>
    /// <returns>True if state changed, false if already destroyed.</returns>
    public bool TakeCriticalHit()
    {
        if (currentState == SystemState.Destroyed)
        {
            Debug.Log($"[MountedSystem] {ShipSystemData.GetName(systemType)} already destroyed, absorbing hit");
            return false;
        }

        hitsTaken++;
        SystemState previousState = currentState;

        if (currentState == SystemState.Operational)
        {
            currentState = SystemState.Damaged;
            Debug.Log($"[MountedSystem] {ShipSystemData.GetName(systemType)} DAMAGED (hit #{hitsTaken})");

            // Apply degradation effects
            ApplyDegradation();

            OnSystemDamaged?.Invoke(this, previousState, currentState);
        }
        else if (currentState == SystemState.Damaged)
        {
            currentState = SystemState.Destroyed;
            Debug.Log($"[MountedSystem] {ShipSystemData.GetName(systemType)} DESTROYED (hit #{hitsTaken})");

            // Apply destruction effects
            OnDestruction();

            OnSystemDamaged?.Invoke(this, previousState, currentState);
            OnSystemDestroyed?.Invoke(this);
        }

        return true;
    }

    /// <summary>
    /// Virtual method for applying degradation effects when damaged.
    /// Override in subclasses for specific system behaviors.
    /// </summary>
    protected virtual void ApplyDegradation()
    {
        // Base implementation does nothing
        // Subclasses override to implement specific degradation effects
        // e.g., Engine reduces speed, Sensors reduce accuracy, Radiator increases heat
    }

    /// <summary>
    /// Virtual method for destruction effects.
    /// Override in subclasses for specific system behaviors.
    /// </summary>
    protected virtual void OnDestruction()
    {
        // Base implementation does nothing
        // Subclasses override to implement specific destruction effects
        // e.g., Magazine explosion, Reactor meltdown
    }

    /// <summary>
    /// Repairs the system back to operational state.
    /// </summary>
    public void Repair()
    {
        SystemState previousState = currentState;
        currentState = SystemState.Operational;
        hitsTaken = 0;

        Debug.Log($"[MountedSystem] {ShipSystemData.GetName(systemType)} repaired to Operational");

        if (previousState != SystemState.Operational)
        {
            OnSystemDamaged?.Invoke(this, previousState, currentState);
        }
    }

    /// <summary>
    /// Gets a status string for this system.
    /// </summary>
    public string GetStatusString()
    {
        string stateName = currentState.ToString();
        return $"{ShipSystemData.GetName(systemType)} [{slotStart}-{slotEnd}]: {stateName}";
    }

    /// <summary>
    /// Sets the parent references.
    /// </summary>
    public void SetParentReferences(ShipSection section, Ship ship = null)
    {
        parentSection = section;
        parentShip = ship ?? section?.ParentShip;
    }

    /// <summary>
    /// Gets the debug color for this system based on state.
    /// </summary>
    public Color GetDebugColor()
    {
        Color baseColor = ShipSystemData.GetColor(systemType);

        switch (currentState)
        {
            case SystemState.Damaged:
                return Color.Lerp(baseColor, Color.yellow, 0.5f);
            case SystemState.Destroyed:
                return Color.Lerp(baseColor, Color.black, 0.7f);
            default:
                return baseColor;
        }
    }
}
