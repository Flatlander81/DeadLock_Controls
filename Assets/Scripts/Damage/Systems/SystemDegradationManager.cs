using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages all mounted systems on a ship and calculates combined degradation effects.
/// Tracks system states and recalculates ship stats when systems change.
/// </summary>
public class SystemDegradationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ship ship;
    [SerializeField] private HeatManager heatManager;
    [SerializeField] private SectionManager sectionManager;

    [Header("Cached Systems")]
    [SerializeField] private List<MountedEngine> engines = new List<MountedEngine>();
    [SerializeField] private List<MountedRadiator> radiators = new List<MountedRadiator>();
    [SerializeField] private List<MountedSensors> sensors = new List<MountedSensors>();
    [SerializeField] private List<MountedReactor> reactors = new List<MountedReactor>();
    [SerializeField] private List<MountedWeapon> weapons = new List<MountedWeapon>();
    [SerializeField] private List<MountedPDTurret> pdTurrets = new List<MountedPDTurret>();
    [SerializeField] private List<MountedMagazine> magazines = new List<MountedMagazine>();

    [Header("Calculated Stats")]
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float turnRateMultiplier = 1f;
    [SerializeField] private float coolingMultiplier = 1f;
    [SerializeField] private float targetingRangeMultiplier = 1f;
    [SerializeField] private float heatCapacityMultiplier = 1f;
    [SerializeField] private float passiveHeatGeneration = 0f;

    // Events
    /// <summary>Fired when any degradation value changes.</summary>
    public event Action OnDegradationChanged;

    // Properties
    public float SpeedMultiplier => speedMultiplier;
    public float TurnRateMultiplier => turnRateMultiplier;
    public float CoolingMultiplier => coolingMultiplier;
    public float TargetingRangeMultiplier => targetingRangeMultiplier;
    public float HeatCapacityMultiplier => heatCapacityMultiplier;
    public float PassiveHeatGeneration => passiveHeatGeneration;

    // System counts
    public int EngineCount => engines.Count;
    public int RadiatorCount => radiators.Count;
    public int SensorCount => sensors.Count;
    public int ReactorCount => reactors.Count;
    public int WeaponCount => weapons.Count;
    public int PDTurretCount => pdTurrets.Count;
    public int MagazineCount => magazines.Count;

    private void Awake()
    {
        // Auto-find references
        if (ship == null) ship = GetComponent<Ship>();
        if (heatManager == null) heatManager = GetComponent<HeatManager>();
        if (sectionManager == null)
        {
            sectionManager = GetComponent<SectionManager>();
            if (sectionManager == null) sectionManager = GetComponentInChildren<SectionManager>();
        }
    }

    private void Start()
    {
        // Subscribe to section manager events for system changes
        if (sectionManager != null)
        {
            // Find and cache all mounted systems
            RefreshSystemCache();
        }
    }

    /// <summary>
    /// Sets references manually (for testing or setup).
    /// </summary>
    public void SetReferences(Ship s, HeatManager hm, SectionManager sm)
    {
        ship = s;
        heatManager = hm;
        sectionManager = sm;
    }

    /// <summary>
    /// Finds and caches all mounted systems from all sections.
    /// </summary>
    public void RefreshSystemCache()
    {
        engines.Clear();
        radiators.Clear();
        sensors.Clear();
        reactors.Clear();
        weapons.Clear();
        pdTurrets.Clear();
        magazines.Clear();

        // Find all MountedSystem components in children
        MountedSystem[] allSystems = GetComponentsInChildren<MountedSystem>(true);

        foreach (var system in allSystems)
        {
            RegisterSystem(system);
        }

        // Also check sections if we have a section manager
        if (sectionManager != null)
        {
            foreach (var section in sectionManager.GetAllSections())
            {
                if (section?.SlotLayout == null) continue;

                foreach (var mountedSystem in section.SlotLayout.MountedSystems)
                {
                    RegisterSystem(mountedSystem);
                }
            }
        }

        RecalculateAllMultipliers();
        Debug.Log($"[SystemDegradationManager] Cached {engines.Count} engines, {radiators.Count} radiators, {sensors.Count} sensors, {reactors.Count} reactors, {weapons.Count} weapons");
    }

    /// <summary>
    /// Registers a mounted system and subscribes to its events.
    /// </summary>
    public void RegisterSystem(MountedSystem system)
    {
        if (system == null) return;

        // Subscribe to damage events
        system.OnSystemDamaged += HandleSystemStateChanged;

        // Add to appropriate list based on type
        switch (system)
        {
            case MountedEngine engine:
                if (!engines.Contains(engine))
                {
                    engines.Add(engine);
                    engine.OnEngineStateChanged += (e) => RecalculateMovementStats();
                }
                break;

            case MountedRadiator radiator:
                if (!radiators.Contains(radiator))
                {
                    radiators.Add(radiator);
                    radiator.OnRadiatorStateChanged += (r) => RecalculateCoolingStats();
                }
                break;

            case MountedSensors sensor:
                if (!sensors.Contains(sensor))
                {
                    sensors.Add(sensor);
                    sensor.OnSensorStateChanged += (s) => RecalculateSensorStats();
                }
                break;

            case MountedReactor reactor:
                if (!reactors.Contains(reactor))
                {
                    reactors.Add(reactor);
                    reactor.OnReactorStateChanged += (r) => RecalculateReactorStats();
                    reactor.OnCoreBreach += HandleCoreBreach;
                }
                break;

            case MountedWeapon weapon:
                if (!weapons.Contains(weapon))
                {
                    weapons.Add(weapon);
                }
                break;

            case MountedPDTurret pd:
                if (!pdTurrets.Contains(pd))
                {
                    pdTurrets.Add(pd);
                }
                break;

            case MountedMagazine mag:
                if (!magazines.Contains(mag))
                {
                    magazines.Add(mag);
                    mag.OnMagazineExplosion += HandleMagazineExplosion;
                }
                break;
        }
    }

    /// <summary>
    /// Handles system state changes.
    /// </summary>
    private void HandleSystemStateChanged(MountedSystem system, SystemState previousState, SystemState newState)
    {
        Debug.Log($"[SystemDegradationManager] System {system.SystemType} changed: {previousState} -> {newState}");
        RecalculateAllMultipliers();
    }

    /// <summary>
    /// Handles core breach (reactor destroyed).
    /// </summary>
    private void HandleCoreBreach(MountedReactor reactor)
    {
        Debug.LogError($"[SystemDegradationManager] CORE BREACH! Ship destroyed!");
        // Ship.Die() is called by the reactor itself
    }

    /// <summary>
    /// Handles magazine explosion.
    /// </summary>
    private void HandleMagazineExplosion(MountedMagazine magazine, float damage)
    {
        Debug.LogWarning($"[SystemDegradationManager] Magazine explosion: {damage} internal damage");
    }

    /// <summary>
    /// Recalculates all stat multipliers.
    /// </summary>
    public void RecalculateAllMultipliers()
    {
        RecalculateMovementStats();
        RecalculateCoolingStats();
        RecalculateSensorStats();
        RecalculateReactorStats();
        OnDegradationChanged?.Invoke();
    }

    /// <summary>
    /// Recalculates movement stats from engines.
    /// Uses the BEST engine multiplier (having multiple engines provides redundancy).
    /// </summary>
    private void RecalculateMovementStats()
    {
        if (engines.Count == 0)
        {
            speedMultiplier = 1f;
            turnRateMultiplier = 1f;
            return;
        }

        // Find the best engine (highest multipliers)
        float bestSpeed = 0f;
        float bestTurn = 0f;

        foreach (var engine in engines)
        {
            bestSpeed = Mathf.Max(bestSpeed, engine.GetSpeedMultiplier());
            bestTurn = Mathf.Max(bestTurn, engine.GetTurnRateMultiplier());
        }

        speedMultiplier = bestSpeed;
        turnRateMultiplier = bestTurn;

        Debug.Log($"[SystemDegradationManager] Movement: Speed x{speedMultiplier:F2}, Turn x{turnRateMultiplier:F2}");
    }

    /// <summary>
    /// Recalculates cooling stats from radiators.
    /// Total cooling is sum of all radiator contributions.
    /// </summary>
    private void RecalculateCoolingStats()
    {
        if (radiators.Count == 0)
        {
            coolingMultiplier = 1f;
            return;
        }

        // Sum effective cooling from all radiators
        float totalEffectiveCooling = 0f;
        float totalBaseCooling = 0f;

        foreach (var radiator in radiators)
        {
            totalEffectiveCooling += radiator.GetEffectiveCooling();
            totalBaseCooling += radiator.BaseCoolingContribution;
        }

        // Calculate as percentage of maximum possible
        coolingMultiplier = totalBaseCooling > 0 ? totalEffectiveCooling / totalBaseCooling : 1f;

        Debug.Log($"[SystemDegradationManager] Cooling: x{coolingMultiplier:F2} ({totalEffectiveCooling:F1}/{totalBaseCooling:F1})");
    }

    /// <summary>
    /// Recalculates sensor stats.
    /// Uses the BEST sensor multiplier (having multiple sensors provides redundancy).
    /// </summary>
    private void RecalculateSensorStats()
    {
        if (sensors.Count == 0)
        {
            targetingRangeMultiplier = 1f;
            return;
        }

        // Find the best sensor (highest multipliers)
        float bestRange = 0f;

        foreach (var sensor in sensors)
        {
            bestRange = Mathf.Max(bestRange, sensor.GetTargetingRangeMultiplier());
        }

        targetingRangeMultiplier = bestRange;

        Debug.Log($"[SystemDegradationManager] Sensors: Range x{targetingRangeMultiplier:F2}");
    }

    /// <summary>
    /// Recalculates reactor stats.
    /// Uses the WORST reactor multiplier (any damaged reactor affects the whole ship).
    /// </summary>
    private void RecalculateReactorStats()
    {
        if (reactors.Count == 0)
        {
            heatCapacityMultiplier = 1f;
            passiveHeatGeneration = 0f;
            return;
        }

        // Use worst heat capacity and sum passive heat from all damaged reactors
        float worstCapacity = 1f;
        float totalPassiveHeat = 0f;

        foreach (var reactor in reactors)
        {
            worstCapacity = Mathf.Min(worstCapacity, reactor.GetHeatCapacityMultiplier());
            totalPassiveHeat += reactor.GetPassiveHeatGeneration();
        }

        heatCapacityMultiplier = worstCapacity;
        passiveHeatGeneration = totalPassiveHeat;

        Debug.Log($"[SystemDegradationManager] Reactor: Capacity x{heatCapacityMultiplier:F2}, Passive Heat +{passiveHeatGeneration:F1}");
    }

    /// <summary>
    /// Gets the weapon damage multiplier for a specific weapon.
    /// </summary>
    public float GetWeaponDamageMultiplier(WeaponSystem weapon)
    {
        foreach (var mountedWeapon in weapons)
        {
            if (mountedWeapon.LinkedWeapon == weapon)
            {
                return mountedWeapon.GetDamageMultiplier();
            }
        }
        return 1f; // No mounted system found, weapon is fine
    }

    /// <summary>
    /// Gets the weapon cooldown multiplier for a specific weapon.
    /// </summary>
    public float GetWeaponCooldownMultiplier(WeaponSystem weapon)
    {
        foreach (var mountedWeapon in weapons)
        {
            if (mountedWeapon.LinkedWeapon == weapon)
            {
                return mountedWeapon.GetCooldownMultiplier();
            }
        }
        return 1f; // No mounted system found, weapon is fine
    }

    /// <summary>
    /// Checks if a specific weapon can fire (not destroyed).
    /// </summary>
    public bool CanWeaponFire(WeaponSystem weapon)
    {
        foreach (var mountedWeapon in weapons)
        {
            if (mountedWeapon.LinkedWeapon == weapon)
            {
                return mountedWeapon.CanFire();
            }
        }
        return true; // No mounted system found, weapon is fine
    }

    /// <summary>
    /// Checks if the ship can move (at least one engine operational).
    /// </summary>
    public bool CanShipMove()
    {
        if (engines.Count == 0) return true; // No engines tracked = can move

        foreach (var engine in engines)
        {
            if (engine.CanMove()) return true;
        }
        return false; // All engines destroyed
    }

    /// <summary>
    /// Gets a summary of current degradation status.
    /// </summary>
    public string GetDegradationSummary()
    {
        return $"Speed: x{speedMultiplier:F2}, Turn: x{turnRateMultiplier:F2}, " +
               $"Cooling: x{coolingMultiplier:F2}, Range: x{targetingRangeMultiplier:F2}, " +
               $"Heat Cap: x{heatCapacityMultiplier:F2}, Passive Heat: +{passiveHeatGeneration:F1}";
    }

    /// <summary>
    /// Gets all engines.
    /// </summary>
    public IReadOnlyList<MountedEngine> GetEngines() => engines;

    /// <summary>
    /// Gets all radiators.
    /// </summary>
    public IReadOnlyList<MountedRadiator> GetRadiators() => radiators;

    /// <summary>
    /// Gets all sensors.
    /// </summary>
    public IReadOnlyList<MountedSensors> GetSensors() => sensors;

    /// <summary>
    /// Gets all reactors.
    /// </summary>
    public IReadOnlyList<MountedReactor> GetReactors() => reactors;

    /// <summary>
    /// Gets all weapons.
    /// </summary>
    public IReadOnlyList<MountedWeapon> GetWeapons() => weapons;

    /// <summary>
    /// Gets all PD turrets.
    /// </summary>
    public IReadOnlyList<MountedPDTurret> GetPDTurrets() => pdTurrets;

    /// <summary>
    /// Gets all magazines.
    /// </summary>
    public IReadOnlyList<MountedMagazine> GetMagazines() => magazines;
}
