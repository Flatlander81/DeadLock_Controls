using UnityEngine;
using System;

/// <summary>
/// Handles critical hit roll logic for ship sections.
/// When structure takes damage, rolls against the section's slot layout
/// to determine if a mounted system is hit.
/// </summary>
public class CriticalHitSystem : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool logCriticalRolls = true;

    [Header("Statistics")]
    [SerializeField] private int totalRolls = 0;
    [SerializeField] private int systemHits = 0;
    [SerializeField] private int emptyHits = 0;

    // Events
    /// <summary>Fired when a critical roll is made. Parameters: result.</summary>
    public event Action<CriticalHitResult> OnCriticalRoll;

    /// <summary>Fired when a system is damaged by critical. Parameters: result.</summary>
    public event Action<CriticalHitResult> OnSystemCriticalDamage;

    /// <summary>Fired when a system is destroyed by critical. Parameters: result.</summary>
    public event Action<CriticalHitResult> OnSystemCriticalDestroy;

    // Properties
    public int TotalRolls => totalRolls;
    public int SystemHits => systemHits;
    public int EmptyHits => emptyHits;
    public float SystemHitRate => totalRolls > 0 ? (float)systemHits / totalRolls : 0f;

    /// <summary>
    /// Rolls a critical hit against a section's slot layout.
    /// Called when structure takes damage.
    /// </summary>
    /// <param name="section">The section being hit.</param>
    /// <returns>Result of the critical roll.</returns>
    public CriticalHitResult RollCritical(ShipSection section)
    {
        if (section == null)
        {
            Debug.LogError("[CriticalHitSystem] Cannot roll critical on null section");
            return new CriticalHitResult();
        }

        SlotLayout layout = section.SlotLayout;
        if (layout == null)
        {
            Debug.LogWarning($"[CriticalHitSystem] Section {section.SectionType} has no slot layout");
            return CriticalHitResult.EmptySlot(0, 0, section.SectionType);
        }

        return RollCritical(layout, section.SectionType);
    }

    /// <summary>
    /// Rolls a critical hit against a slot layout.
    /// </summary>
    /// <param name="layout">The slot layout to roll against.</param>
    /// <param name="sectionType">The section type for logging.</param>
    /// <returns>Result of the critical roll.</returns>
    public CriticalHitResult RollCritical(SlotLayout layout, SectionType sectionType)
    {
        if (layout == null || layout.TotalSlots <= 0)
        {
            Debug.LogWarning("[CriticalHitSystem] Invalid slot layout");
            return CriticalHitResult.EmptySlot(0, 0, sectionType);
        }

        // Roll random slot (1 to TotalSlots inclusive)
        int rolledSlot = UnityEngine.Random.Range(1, layout.TotalSlots + 1);

        return ProcessCriticalRoll(layout, rolledSlot, sectionType);
    }

    /// <summary>
    /// Forces a critical hit on a specific slot (for testing).
    /// </summary>
    /// <param name="layout">The slot layout.</param>
    /// <param name="slot">The slot to hit.</param>
    /// <param name="sectionType">The section type.</param>
    /// <returns>Result of the critical hit.</returns>
    public CriticalHitResult ForceCritical(SlotLayout layout, int slot, SectionType sectionType)
    {
        if (layout == null || layout.TotalSlots <= 0)
        {
            Debug.LogWarning("[CriticalHitSystem] Invalid slot layout");
            return CriticalHitResult.EmptySlot(0, 0, sectionType);
        }

        // Clamp slot to valid range
        int clampedSlot = Mathf.Clamp(slot, 1, layout.TotalSlots);

        return ProcessCriticalRoll(layout, clampedSlot, sectionType);
    }

    /// <summary>
    /// Processes a critical roll at a specific slot.
    /// </summary>
    private CriticalHitResult ProcessCriticalRoll(SlotLayout layout, int slot, SectionType sectionType)
    {
        totalRolls++;

        MountedSystem system = layout.GetSystemAtSlot(slot);
        CriticalHitResult result;

        if (system == null)
        {
            // Empty slot - no critical damage
            emptyHits++;
            result = CriticalHitResult.EmptySlot(slot, layout.TotalSlots, sectionType);

            if (logCriticalRolls)
            {
                Debug.Log($"[CriticalHitSystem] {result}");
            }
        }
        else
        {
            // System hit - apply damage
            systemHits++;
            SystemState previousState = system.CurrentState;

            bool stateChanged = system.TakeCriticalHit();

            if (stateChanged)
            {
                result = CriticalHitResult.SystemHitResult(
                    slot,
                    layout.TotalSlots,
                    system,
                    previousState,
                    system.CurrentState,
                    sectionType
                );
            }
            else
            {
                // System already destroyed, absorbed the hit
                result = CriticalHitResult.DestroyedSystemAbsorbed(slot, layout.TotalSlots, system, sectionType);
            }

            if (logCriticalRolls)
            {
                Debug.Log($"[CriticalHitSystem] {result}");
            }

            // Fire events based on result
            if (result.SystemWasDamaged)
            {
                OnSystemCriticalDamage?.Invoke(result);
            }
            else if (result.SystemWasDestroyed)
            {
                OnSystemCriticalDestroy?.Invoke(result);
            }
        }

        OnCriticalRoll?.Invoke(result);
        return result;
    }

    /// <summary>
    /// Calculates the probability of hitting a system in the layout.
    /// </summary>
    /// <param name="layout">The slot layout.</param>
    /// <returns>Probability (0-1) of hitting a system.</returns>
    public static float CalculateSystemHitProbability(SlotLayout layout)
    {
        if (layout == null || layout.TotalSlots <= 0)
        {
            return 0f;
        }

        return (float)layout.GetOccupiedSlotCount() / layout.TotalSlots;
    }

    /// <summary>
    /// Calculates the probability of hitting a specific system.
    /// </summary>
    /// <param name="layout">The slot layout.</param>
    /// <param name="system">The system to calculate for.</param>
    /// <returns>Probability (0-1) of hitting that system.</returns>
    public static float CalculateSystemHitProbability(SlotLayout layout, MountedSystem system)
    {
        if (layout == null || layout.TotalSlots <= 0 || system == null)
        {
            return 0f;
        }

        return (float)system.Size / layout.TotalSlots;
    }

    /// <summary>
    /// Resets statistics.
    /// </summary>
    public void ResetStatistics()
    {
        totalRolls = 0;
        systemHits = 0;
        emptyHits = 0;
    }
}
