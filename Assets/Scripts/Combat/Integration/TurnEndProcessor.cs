using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Handles all turn-end cleanup operations.
/// Processes heat dissipation, cooldown ticking, and notifies systems of turn completion.
/// Subscribes to TurnManager.OnTurnEnd event.
/// </summary>
public class TurnEndProcessor : TurnEventSubscriber
{
    [Header("Dissipation Settings")]
    [SerializeField] private float baseDissipationRate = 10f;
    [SerializeField] private float radiatorBonusPerUnit = 5f;

    [Header("Ship References")]
    [SerializeField] private List<Ship> trackedShips = new List<Ship>();

    [Header("Debug Info")]
    [SerializeField] private int lastProcessedTurn = 0;
    [SerializeField] private float lastTotalDissipation = 0f;
    [SerializeField] private int lastCooldownsProcessed = 0;

    // Events
    /// <summary>Fired when heat is dissipated, with the amount dissipated.</summary>
    public event Action<Ship, float> OnHeatDissipated;

    /// <summary>Fired when a weapon comes off cooldown.</summary>
    public event Action<WeaponSystem> OnWeaponReady;

    /// <summary>Fired when an ability comes off cooldown.</summary>
    public event Action<string> OnAbilityReady;

    // Properties
    public float BaseDissipationRate => baseDissipationRate;
    public float RadiatorBonusPerUnit => radiatorBonusPerUnit;
    public int LastProcessedTurn => lastProcessedTurn;

    /// <summary>
    /// Auto-discover ships on enable if none are assigned.
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();

        if (trackedShips.Count == 0)
        {
            RefreshShipList();
        }
    }

    /// <summary>
    /// Refreshes the list of tracked ships by finding all Ship components in scene.
    /// </summary>
    public void RefreshShipList()
    {
        trackedShips.Clear();
        Ship[] allShips = FindObjectsByType<Ship>(FindObjectsSortMode.None);
        trackedShips.AddRange(allShips);
        Debug.Log($"[TurnEndProcessor] Tracking {trackedShips.Count} ships");
    }

    /// <summary>
    /// Manually register a ship to be processed at turn end.
    /// </summary>
    public void RegisterShip(Ship ship)
    {
        if (ship != null && !trackedShips.Contains(ship))
        {
            trackedShips.Add(ship);
        }
    }

    /// <summary>
    /// Unregister a ship (e.g., when destroyed).
    /// </summary>
    public void UnregisterShip(Ship ship)
    {
        trackedShips.Remove(ship);
    }

    /// <summary>
    /// Handle turn end - process all ships.
    /// </summary>
    protected override void HandleTurnEnd(int turnNumber)
    {
        ProcessTurnEnd(turnNumber);
    }

    /// <summary>
    /// Main turn-end processing method.
    /// Called when the turn ends.
    /// </summary>
    public void ProcessTurnEnd(int turnNumber)
    {
        lastProcessedTurn = turnNumber;
        lastTotalDissipation = 0f;
        lastCooldownsProcessed = 0;

        Debug.Log($"[TurnEndProcessor] Processing turn end for turn {turnNumber}...");

        ProcessAllShips();

        Debug.Log($"[TurnEndProcessor] Turn {turnNumber} complete. " +
                  $"Total dissipation: {lastTotalDissipation:F1}, Cooldowns processed: {lastCooldownsProcessed}");
    }

    /// <summary>
    /// Process all tracked ships.
    /// </summary>
    public void ProcessAllShips()
    {
        // Remove any null/destroyed ships
        trackedShips.RemoveAll(s => s == null || s.IsDead);

        foreach (Ship ship in trackedShips)
        {
            ProcessShip(ship);
        }
    }

    /// <summary>
    /// Process a single ship's turn-end operations.
    /// </summary>
    private void ProcessShip(Ship ship)
    {
        if (ship == null || ship.IsDead) return;

        ProcessHeatDissipation(ship);
        ProcessCooldowns(ship);
        ProcessAbilityCooldowns(ship);
    }

    /// <summary>
    /// Process heat dissipation for a ship.
    /// Dissipation = baseDissipation + radiatorBonus
    /// </summary>
    public void ProcessHeatDissipation(Ship ship)
    {
        if (ship == null) return;

        // Use GetComponent to handle cases where Ship.Start() hasn't run yet
        HeatManager heatManager = ship.HeatManager ?? ship.GetComponent<HeatManager>();
        if (heatManager == null) return;

        float previousHeat = heatManager.CurrentHeat;

        // Calculate total dissipation
        float dissipation = CalculateDissipation(ship);

        // Apply dissipation via HeatManager
        if (dissipation > 0)
        {
            heatManager.DissipateHeat(dissipation);
        }

        // Apply passive heat generation from damaged reactors
        float passiveHeat = heatManager.PassiveHeatGeneration;
        if (passiveHeat > 0)
        {
            heatManager.AddPlannedHeat(passiveHeat);
            heatManager.CommitPlannedHeat();
        }

        float actualDissipation = previousHeat - heatManager.CurrentHeat;
        lastTotalDissipation += Mathf.Max(0, actualDissipation);

        if (Mathf.Abs(actualDissipation) > 0.01f)
        {
            Debug.Log($"[TurnEndProcessor] {ship.gameObject.name}: Heat {previousHeat:F1} -> {heatManager.CurrentHeat:F1} " +
                      $"(Dissipated: {dissipation:F1}, Passive: +{passiveHeat:F1})");
            OnHeatDissipated?.Invoke(ship, dissipation);
        }
    }

    /// <summary>
    /// Calculate total heat dissipation for a ship.
    /// </summary>
    public float CalculateDissipation(Ship ship)
    {
        float dissipation = baseDissipationRate;

        // Add radiator bonus
        dissipation += GetRadiatorBonus(ship);

        return dissipation;
    }

    /// <summary>
    /// Get radiator bonus for a ship based on operational radiators.
    /// </summary>
    public float GetRadiatorBonus(Ship ship)
    {
        if (ship == null) return 0f;

        SystemDegradationManager degradation = ship.GetComponent<SystemDegradationManager>();
        if (degradation == null) return 0f;

        float totalBonus = 0f;
        var radiators = degradation.GetRadiators();

        foreach (var radiator in radiators)
        {
            if (radiator == null) continue;

            // Destroyed radiators contribute nothing
            if (radiator.IsDestroyed) continue;

            // Damaged radiators contribute half
            float contribution = radiatorBonusPerUnit;
            if (radiator.IsDamaged)
            {
                contribution *= 0.5f;
            }

            totalBonus += contribution;
        }

        return totalBonus;
    }

    /// <summary>
    /// Process weapon cooldowns for a ship.
    /// </summary>
    public void ProcessCooldowns(Ship ship)
    {
        if (ship == null) return;

        WeaponManager weaponManager = ship.GetComponent<WeaponManager>();
        if (weaponManager == null) return;

        var weapons = weaponManager.Weapons;
        foreach (WeaponSystem weapon in weapons)
        {
            if (weapon == null) continue;

            int previousCooldown = weapon.CurrentCooldown;
            weapon.TickCooldown();
            lastCooldownsProcessed++;

            // Fire event if weapon just became ready
            if (previousCooldown > 0 && weapon.CurrentCooldown == 0)
            {
                Debug.Log($"[TurnEndProcessor] {weapon.WeaponName} is now ready!");
                OnWeaponReady?.Invoke(weapon);
            }
        }
    }

    /// <summary>
    /// Process ability cooldowns for a ship.
    /// </summary>
    public void ProcessAbilityCooldowns(Ship ship)
    {
        if (ship == null) return;

        AbilitySystem abilitySystem = ship.GetComponent<AbilitySystem>();
        if (abilitySystem == null) return;

        // Track which abilities are about to come off cooldown
        List<string> readyAbilities = new List<string>();
        foreach (var slot in abilitySystem.AbilitySlots)
        {
            if (slot.currentCooldown == 1) // Will become 0 after tick
            {
                readyAbilities.Add(slot.abilityData.abilityName);
            }
        }

        // Tick all cooldowns
        abilitySystem.TickAllCooldowns();

        // Fire events for abilities that just became ready
        foreach (string abilityName in readyAbilities)
        {
            Debug.Log($"[TurnEndProcessor] Ability '{abilityName}' is now ready!");
            OnAbilityReady?.Invoke(abilityName);
        }
    }

    /// <summary>
    /// Get a summary of the last turn's processing.
    /// </summary>
    public string GetLastTurnSummary()
    {
        return $"Turn {lastProcessedTurn}: Dissipated {lastTotalDissipation:F1} heat, " +
               $"Processed {lastCooldownsProcessed} cooldowns";
    }
}
