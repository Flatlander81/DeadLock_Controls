using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime controller for testing Ship Death mechanics.
/// Allows testing of death conditions: Core breach and combat ineffective.
/// </summary>
public class ShipDeathTestController : MonoBehaviour
{
    [Header("References")]
    public Ship testShip;
    public ShipDeathController deathController;
    public SystemDegradationManager degradationManager;
    public SectionManager sectionManager;

    [Header("UI Settings")]
    [SerializeField] private bool showUI = true;

    // Track event counts
    private int shipDestroyedCount = 0;
    private int shipDisabledCount = 0;
    private ShipDeathController.DeathCause lastDeathCause = ShipDeathController.DeathCause.None;

    void Start()
    {
        if (testShip == null)
        {
            testShip = FindObjectOfType<Ship>();
        }

        if (testShip != null)
        {
            if (deathController == null)
            {
                deathController = testShip.GetComponent<ShipDeathController>();
            }

            if (degradationManager == null)
            {
                degradationManager = testShip.GetComponent<SystemDegradationManager>();
            }

            if (sectionManager == null)
            {
                sectionManager = testShip.GetComponent<SectionManager>();
            }
        }

        // Subscribe to death events
        if (deathController != null)
        {
            deathController.OnShipDestroyed += HandleShipDestroyed;
            deathController.OnShipDisabled += HandleShipDisabled;
        }

        Debug.Log("=== Ship Death Test Controller ===");
        Debug.Log("CONTROLS:");
        Debug.Log("  W: Destroy all weapons");
        Debug.Log("  E: Destroy all engines");
        Debug.Log("  X: Destroy reactor (instant death)");
        Debug.Log("  B: Breach Core section");
        Debug.Log("  A: Breach ALL sections");
        Debug.Log("  R: Reset ship state");
        Debug.Log("  H: Toggle UI");
    }

    void OnDestroy()
    {
        if (deathController != null)
        {
            deathController.OnShipDestroyed -= HandleShipDestroyed;
            deathController.OnShipDisabled -= HandleShipDisabled;
        }
    }

    void HandleShipDestroyed(Ship ship, ShipDeathController.DeathCause cause)
    {
        shipDestroyedCount++;
        lastDeathCause = cause;
        Debug.LogError($"SHIP DESTROYED! Cause: {cause}");
    }

    void HandleShipDisabled(Ship ship)
    {
        shipDisabledCount++;
        Debug.LogWarning("SHIP DISABLED - Combat Ineffective!");
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Toggle UI
        if (Input.GetKeyDown(KeyCode.H))
        {
            showUI = !showUI;
        }

        // Destroy all weapons
        if (Input.GetKeyDown(KeyCode.W))
        {
            DestroyAllWeapons();
        }

        // Destroy all engines
        if (Input.GetKeyDown(KeyCode.E))
        {
            DestroyAllEngines();
        }

        // Destroy reactor (instant death)
        if (Input.GetKeyDown(KeyCode.X))
        {
            DestroyReactor();
        }

        // Breach Core section
        if (Input.GetKeyDown(KeyCode.B))
        {
            BreachCore();
        }

        // Breach ALL sections
        if (Input.GetKeyDown(KeyCode.A))
        {
            BreachAllSections();
        }

        // Reset ship state
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetShipState();
        }
    }

    void DestroyAllWeapons()
    {
        if (degradationManager == null)
        {
            Debug.LogError("No SystemDegradationManager found!");
            return;
        }

        int destroyed = 0;
        foreach (var weapon in degradationManager.GetWeapons())
        {
            while (weapon.CurrentState != SystemState.Destroyed)
            {
                weapon.TakeCriticalHit();
            }
            destroyed++;
        }

        foreach (var pd in degradationManager.GetPDTurrets())
        {
            while (pd.CurrentState != SystemState.Destroyed)
            {
                pd.TakeCriticalHit();
            }
            destroyed++;
        }

        Debug.Log($"Destroyed {destroyed} weapons/turrets.");

        // Check death conditions
        deathController?.CheckDeathConditions();
        LogCombatStatus();
    }

    void DestroyAllEngines()
    {
        if (degradationManager == null)
        {
            Debug.LogError("No SystemDegradationManager found!");
            return;
        }

        int destroyed = 0;
        foreach (var engine in degradationManager.GetEngines())
        {
            while (engine.CurrentState != SystemState.Destroyed)
            {
                engine.TakeCriticalHit();
            }
            destroyed++;
        }

        Debug.Log($"Destroyed {destroyed} engines.");

        // Check death conditions
        deathController?.CheckDeathConditions();
        LogCombatStatus();
    }

    void DestroyReactor()
    {
        if (degradationManager == null)
        {
            Debug.LogError("No SystemDegradationManager found!");
            return;
        }

        var reactors = degradationManager.GetReactors();
        if (reactors.Count == 0)
        {
            Debug.LogError("No reactors found!");
            return;
        }

        foreach (var reactor in reactors)
        {
            while (reactor.CurrentState != SystemState.Destroyed)
            {
                reactor.TakeCriticalHit();
            }
            Debug.Log($"Reactor DESTROYED - should trigger Core Breach!");
        }

        // Check death conditions (should trigger automatically via event)
        deathController?.CheckDeathConditions();
    }

    void BreachCore()
    {
        if (sectionManager == null)
        {
            Debug.LogError("No SectionManager found!");
            return;
        }

        ShipSection core = sectionManager.GetSection(SectionType.Core);
        if (core == null)
        {
            Debug.LogError("Core section not found!");
            return;
        }

        if (!core.IsBreached)
        {
            float damageNeeded = core.CurrentArmor + core.CurrentStructure + 100f;
            core.ApplyDamage(damageNeeded);
            Debug.Log("Core section BREACHED!");
        }
        else
        {
            Debug.Log("Core section is already breached.");
        }

        // Check death conditions
        deathController?.CheckDeathConditions();
    }

    void BreachAllSections()
    {
        if (sectionManager == null)
        {
            Debug.LogError("No SectionManager found!");
            return;
        }

        int breached = 0;
        foreach (var section in sectionManager.GetAllSections())
        {
            if (section != null && !section.IsBreached)
            {
                float damageNeeded = section.CurrentArmor + section.CurrentStructure + 100f;
                section.ApplyDamage(damageNeeded);
                breached++;
            }
        }

        Debug.Log($"Breached {breached} sections.");

        // Check death conditions
        deathController?.CheckDeathConditions();
    }

    void ResetShipState()
    {
        // Reset death controller
        if (deathController != null)
        {
            deathController.Reset();
        }

        // Reset all sections
        if (sectionManager != null)
        {
            sectionManager.ResetAllSections();
        }

        // Reset all systems
        if (degradationManager != null)
        {
            foreach (var engine in degradationManager.GetEngines())
            {
                engine.Repair();
            }
            foreach (var weapon in degradationManager.GetWeapons())
            {
                weapon.Repair();
            }
            foreach (var pd in degradationManager.GetPDTurrets())
            {
                pd.Repair();
            }
            foreach (var reactor in degradationManager.GetReactors())
            {
                reactor.Repair();
            }
            foreach (var radiator in degradationManager.GetRadiators())
            {
                radiator.Repair();
            }
            foreach (var sensor in degradationManager.GetSensors())
            {
                sensor.Repair();
            }
        }

        // Re-enable ship if disabled
        if (testShip != null && !testShip.gameObject.activeSelf)
        {
            testShip.gameObject.SetActive(true);
        }

        // Reset counters
        shipDestroyedCount = 0;
        shipDisabledCount = 0;
        lastDeathCause = ShipDeathController.DeathCause.None;

        Debug.Log("Ship state reset to operational.");
        LogCombatStatus();
    }

    void LogCombatStatus()
    {
        if (deathController == null) return;

        Debug.Log($"Combat Status: {deathController.GetStatusSummary()}");
        Debug.Log($"  All Weapons Destroyed: {deathController.AreAllWeaponsDestroyed()}");
        Debug.Log($"  All Engines Destroyed: {deathController.AreAllEnginesDestroyed()}");
        Debug.Log($"  Combat Ineffective: {deathController.IsCombatIneffective()}");
    }

    void OnGUI()
    {
        if (!showUI) return;

        GUILayout.BeginArea(new Rect(420, 10, 350, 500));
        GUI.Box(new Rect(0, 0, 350, 500), "");

        GUILayout.Label("<b>SHIP DEATH TEST CONTROLLER</b>");
        GUILayout.Space(5);

        // Ship state
        if (deathController != null)
        {
            GUILayout.Label("<b>SHIP STATE:</b>");

            GUI.color = deathController.IsDestroyed ? Color.red : Color.green;
            GUILayout.Label($"  Destroyed: {deathController.IsDestroyed}");
            GUI.color = Color.white;

            GUI.color = deathController.IsDisabled ? Color.yellow : Color.green;
            GUILayout.Label($"  Disabled: {deathController.IsDisabled}");
            GUI.color = Color.white;

            if (deathController.IsDestroyed)
            {
                GUILayout.Label($"  Death Cause: {deathController.Cause}");
            }

            GUILayout.Label($"  Status: {deathController.GetStatusSummary()}");
        }

        GUILayout.Space(10);

        // Combat effectiveness
        if (deathController != null)
        {
            GUILayout.Label("<b>COMBAT EFFECTIVENESS:</b>");

            bool allWeaponsDestroyed = deathController.AreAllWeaponsDestroyed();
            bool allEnginesDestroyed = deathController.AreAllEnginesDestroyed();
            bool combatIneffective = deathController.IsCombatIneffective();

            GUI.color = allWeaponsDestroyed ? Color.red : Color.green;
            GUILayout.Label($"  All Weapons Destroyed: {allWeaponsDestroyed}");
            GUI.color = Color.white;

            GUI.color = allEnginesDestroyed ? Color.red : Color.green;
            GUILayout.Label($"  All Engines Destroyed: {allEnginesDestroyed}");
            GUI.color = Color.white;

            GUI.color = combatIneffective ? Color.red : Color.green;
            GUILayout.Label($"  Combat Ineffective: {combatIneffective}");
            GUI.color = Color.white;
        }

        GUILayout.Space(10);

        // System counts
        if (degradationManager != null)
        {
            GUILayout.Label("<b>SYSTEM STATUS:</b>");

            var weapons = degradationManager.GetWeapons();
            var engines = degradationManager.GetEngines();
            var reactors = degradationManager.GetReactors();
            var pds = degradationManager.GetPDTurrets();

            int destroyedWeapons = 0;
            foreach (var w in weapons) if (w.IsDestroyed) destroyedWeapons++;

            int destroyedEngines = 0;
            foreach (var e in engines) if (e.IsDestroyed) destroyedEngines++;

            int destroyedReactors = 0;
            foreach (var r in reactors) if (r.IsDestroyed) destroyedReactors++;

            int destroyedPDs = 0;
            foreach (var p in pds) if (p.IsDestroyed) destroyedPDs++;

            GUILayout.Label($"  Weapons: {destroyedWeapons}/{weapons.Count} destroyed");
            GUILayout.Label($"  Engines: {destroyedEngines}/{engines.Count} destroyed");
            GUILayout.Label($"  Reactors: {destroyedReactors}/{reactors.Count} destroyed");
            GUILayout.Label($"  PD Turrets: {destroyedPDs}/{pds.Count} destroyed");
        }

        GUILayout.Space(10);

        // Section status (breached count)
        if (sectionManager != null)
        {
            GUILayout.Label("<b>SECTION STATUS:</b>");

            int breachedCount = 0;
            int totalCount = 0;
            bool coreBreached = false;

            foreach (var section in sectionManager.GetAllSections())
            {
                if (section != null)
                {
                    totalCount++;
                    if (section.IsBreached)
                    {
                        breachedCount++;
                        if (section.SectionType == SectionType.Core)
                        {
                            coreBreached = true;
                        }
                    }
                }
            }

            GUILayout.Label($"  Sections Breached: {breachedCount}/{totalCount}");

            GUI.color = coreBreached ? Color.red : Color.green;
            GUILayout.Label($"  Core Breached: {coreBreached}");
            GUI.color = Color.white;
        }

        GUILayout.Space(10);

        // Event counters
        GUILayout.Label("<b>EVENT COUNTERS:</b>");
        GUILayout.Label($"  Ship Destroyed Events: {shipDestroyedCount}");
        GUILayout.Label($"  Ship Disabled Events: {shipDisabledCount}");
        if (lastDeathCause != ShipDeathController.DeathCause.None)
        {
            GUILayout.Label($"  Last Death Cause: {lastDeathCause}");
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>CONTROLS:</b>");
        GUILayout.Label("W: Destroy weapons | E: Destroy engines");
        GUILayout.Label("X: Destroy reactor | B: Breach Core");
        GUILayout.Label("A: Breach ALL sections | R: Reset | H: Toggle UI");

        GUILayout.EndArea();
    }
}
