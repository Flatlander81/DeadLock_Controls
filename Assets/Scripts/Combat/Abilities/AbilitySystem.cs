using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all abilities on a ship.
/// Handles ability activation, queuing, execution, and cooldown tracking.
/// Now uses ScriptableObject-based AbilityData instead of MonoBehaviour components.
/// </summary>
public class AbilitySystem : MonoBehaviour
{
    /// <summary>
    /// Wrapper class to track ability state (cooldown, queued status).
    /// </summary>
    [System.Serializable]
    public class AbilitySlot
    {
        public AbilityData abilityData;
        public int currentCooldown;
        public bool isQueued;
        public bool isExecuting;

        public bool IsOnCooldown => currentCooldown > 0;
        public bool CanActivate => !IsOnCooldown && !isQueued && !isExecuting;

        public AbilitySlot(AbilityData data)
        {
            abilityData = data;
            currentCooldown = 0;
            isQueued = false;
            isExecuting = false;
        }
    }

    [Header("Ability Configuration")]
    [SerializeField] private List<AbilityData> abilityDataList = new List<AbilityData>();

    [Header("Debug Info")]
    [SerializeField] private List<AbilitySlot> abilitySlots = new List<AbilitySlot>();

    private Ship ship;

    // Public properties
    public List<AbilitySlot> AbilitySlots => abilitySlots;
    public int AbilityCount => abilitySlots.Count;

    /// <summary>
    /// Initialize the ability system and create slots for all abilities.
    /// </summary>
    private void Start()
    {
        ship = GetComponent<Ship>();
        if (ship == null)
        {
            Debug.LogError($"AbilitySystem on {gameObject.name}: No Ship component found!");
            return;
        }

        // Create ability slots from configured ability data
        abilitySlots.Clear();
        foreach (AbilityData abilityData in abilityDataList)
        {
            if (abilityData != null)
            {
                abilitySlots.Add(new AbilitySlot(abilityData));
            }
        }

        Debug.Log($"AbilitySystem initialized on {gameObject.name}. Loaded {abilitySlots.Count} abilities.");
    }

    /// <summary>
    /// Attempt to activate an ability by name.
    /// </summary>
    /// <param name="abilityName">Name of the ability to activate</param>
    /// <returns>True if ability was successfully queued</returns>
    public bool TryActivateAbility(string abilityName)
    {
        AbilitySlot slot = GetAbilitySlot(abilityName);
        if (slot == null)
        {
            Debug.LogWarning($"Ability '{abilityName}' not found on {gameObject.name}");
            return false;
        }

        return TryActivateSlot(slot);
    }

    /// <summary>
    /// Attempt to activate an ability by index (for hotkeys).
    /// </summary>
    /// <param name="index">Index of the ability (0-5)</param>
    /// <returns>True if ability was successfully queued</returns>
    public bool TryActivateAbilityByIndex(int index)
    {
        if (index < 0 || index >= abilitySlots.Count)
        {
            Debug.LogWarning($"Ability index {index} out of range (0-{abilitySlots.Count - 1})");
            return false;
        }

        return TryActivateSlot(abilitySlots[index]);
    }

    /// <summary>
    /// Internal method to activate an ability slot.
    /// </summary>
    private bool TryActivateSlot(AbilitySlot slot)
    {
        if (slot == null || slot.abilityData == null)
        {
            Debug.LogWarning("Cannot activate null ability slot");
            return false;
        }

        // Check if ability can be activated
        if (!slot.CanActivate)
        {
            if (slot.IsOnCooldown)
            {
                Debug.LogWarning($"{slot.abilityData.abilityName} is on cooldown ({slot.currentCooldown} turns remaining)");
            }
            else if (slot.isQueued)
            {
                Debug.LogWarning($"{slot.abilityData.abilityName} is already queued");
            }
            else if (slot.isExecuting)
            {
                Debug.LogWarning($"{slot.abilityData.abilityName} is already executing");
            }
            return false;
        }

        // Check ship-specific conditions (heat cost, etc.)
        if (!slot.abilityData.CanUse(ship))
        {
            // Get specific blocked reason if available
            string blockedReason = slot.abilityData.GetActivationBlockedReason(ship);
            if (!string.IsNullOrEmpty(blockedReason))
            {
                Debug.LogWarning($"{slot.abilityData.abilityName} cannot be used: {blockedReason}");
            }
            else
            {
                Debug.LogWarning($"{slot.abilityData.abilityName} cannot be used (insufficient resources or conditions not met)");
            }
            return false;
        }

        // Add planned heat cost
        if (ship.HeatManager != null && slot.abilityData.heatCost > 0)
        {
            ship.HeatManager.AddPlannedHeat(slot.abilityData.heatCost);
        }

        // Mark as queued
        slot.isQueued = true;
        Debug.Log($"Queued ability: {slot.abilityData.abilityName} (Heat cost: {slot.abilityData.heatCost})");

        return true;
    }

    /// <summary>
    /// Execute all queued abilities during the Simulation phase.
    /// Called by TurnManager when Simulation begins.
    /// </summary>
    public IEnumerator ExecuteQueuedAbilities()
    {
        // Get all queued slots
        List<AbilitySlot> queuedSlots = abilitySlots.Where(s => s.isQueued).ToList();

        if (queuedSlots.Count == 0)
        {
            yield break;
        }

        Debug.Log($"{gameObject.name} executing {queuedSlots.Count} queued abilities...");

        // Execute all queued abilities
        foreach (AbilitySlot slot in queuedSlots)
        {
            slot.isExecuting = true;
            slot.isQueued = false;

            // Handle spin-up time
            if (slot.abilityData.spinUpTime > 0)
            {
                Debug.Log($"{slot.abilityData.abilityName} spinning up for {slot.abilityData.spinUpTime}s...");
                yield return new WaitForSeconds(slot.abilityData.spinUpTime);
            }

            // Execute the ability
            Debug.Log($"Executing {slot.abilityData.abilityName}...");
            slot.abilityData.Execute(ship);

            // Commit this ability's heat cost (not all planned heat)
            if (ship.HeatManager != null && slot.abilityData.heatCost > 0)
            {
                ship.HeatManager.CommitSpecificHeat(slot.abilityData.heatCost);
            }

            // Set cooldown
            slot.currentCooldown = slot.abilityData.maxCooldown;

            // Call completion callback
            slot.abilityData.OnExecuteComplete(ship);

            slot.isExecuting = false;
        }

        Debug.Log($"{gameObject.name} finished executing abilities");
    }

    /// <summary>
    /// Tick down cooldowns for all abilities.
    /// Called at the end of each turn.
    /// </summary>
    public void TickAllCooldowns()
    {
        foreach (AbilitySlot slot in abilitySlots)
        {
            if (slot.currentCooldown > 0)
            {
                slot.currentCooldown--;
                if (slot.currentCooldown == 0)
                {
                    Debug.Log($"{slot.abilityData.abilityName} cooldown finished");
                }
            }
        }
    }

    /// <summary>
    /// Clear the ability queue (cancel all queued abilities).
    /// </summary>
    public void ClearQueue()
    {
        foreach (AbilitySlot slot in abilitySlots)
        {
            if (slot.isQueued)
            {
                slot.isQueued = false;

                // Refund planned heat for this specific ability
                if (ship.HeatManager != null && slot.abilityData.heatCost > 0)
                {
                    ship.HeatManager.RemovePlannedHeat(slot.abilityData.heatCost);
                }
            }
        }
    }

    /// <summary>
    /// Get an ability slot by ability name.
    /// </summary>
    /// <param name="name">Name of the ability</param>
    /// <returns>The ability slot, or null if not found</returns>
    public AbilitySlot GetAbilitySlot(string name)
    {
        return abilitySlots.FirstOrDefault(s => s.abilityData != null && s.abilityData.abilityName == name);
    }

    /// <summary>
    /// Get an ability slot by index.
    /// </summary>
    /// <param name="index">Index of the ability</param>
    /// <returns>The ability slot, or null if out of range</returns>
    public AbilitySlot GetAbilitySlotByIndex(int index)
    {
        if (index >= 0 && index < abilitySlots.Count)
        {
            return abilitySlots[index];
        }
        return null;
    }

    /// <summary>
    /// Cancel all queued abilities (e.g., if ship is destroyed).
    /// </summary>
    public void CancelAllQueuedAbilities()
    {
        ClearQueue();
        Debug.Log($"{gameObject.name} cancelled all queued abilities");
    }
}
