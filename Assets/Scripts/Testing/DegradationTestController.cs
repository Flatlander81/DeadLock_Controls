using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime controller for testing system degradation effects.
/// Allows manual testing of degraded/destroyed systems during play mode.
/// </summary>
public class DegradationTestController : MonoBehaviour
{
    [Header("References")]
    public Ship testShip;
    public Ship targetShip;

    [Header("Degradation Manager")]
    public SystemDegradationManager degradationManager;

    [Header("Test Systems")]
    public List<MountedSystem> testSystems = new List<MountedSystem>();

    private int selectedSystemIndex = 0;
    private bool showUI = true;

    void Start()
    {
        if (testShip == null)
        {
            testShip = GetComponent<Ship>();
        }

        if (degradationManager == null && testShip != null)
        {
            degradationManager = testShip.GetComponent<SystemDegradationManager>();
        }

        if (degradationManager != null)
        {
            RefreshSystemList();
        }

        Debug.Log("=== Degradation Test Controller ===");
        Debug.Log("CONTROLS:");
        Debug.Log("  F1-F7: Select system type to damage");
        Debug.Log("  D: Damage selected system");
        Debug.Log("  K: Kill (destroy) selected system");
        Debug.Log("  R: Repair selected system");
        Debug.Log("  Tab: Cycle through systems");
        Debug.Log("  H: Toggle UI");
    }

    void RefreshSystemList()
    {
        testSystems.Clear();

        // Add all engines
        foreach (var engine in degradationManager.GetEngines())
        {
            testSystems.Add(engine);
        }

        // Add all radiators
        foreach (var radiator in degradationManager.GetRadiators())
        {
            testSystems.Add(radiator);
        }

        // Add all sensors
        foreach (var sensor in degradationManager.GetSensors())
        {
            testSystems.Add(sensor);
        }

        // Add all reactors
        foreach (var reactor in degradationManager.GetReactors())
        {
            testSystems.Add(reactor);
        }

        // Add all weapons
        foreach (var weapon in degradationManager.GetWeapons())
        {
            testSystems.Add(weapon);
        }

        // Add all PD turrets
        foreach (var pd in degradationManager.GetPDTurrets())
        {
            testSystems.Add(pd);
        }

        // Add all magazines
        foreach (var mag in degradationManager.GetMagazines())
        {
            testSystems.Add(mag);
        }

        Debug.Log($"Found {testSystems.Count} systems to test");
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

        // Cycle systems
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (testSystems.Count > 0)
            {
                selectedSystemIndex = (selectedSystemIndex + 1) % testSystems.Count;
                Debug.Log($"Selected: {GetSelectedSystemName()}");
            }
        }

        // Damage selected system
        if (Input.GetKeyDown(KeyCode.D))
        {
            DamageSelectedSystem();
        }

        // Kill selected system
        if (Input.GetKeyDown(KeyCode.K))
        {
            KillSelectedSystem();
        }

        // Repair selected system
        if (Input.GetKeyDown(KeyCode.R))
        {
            RepairSelectedSystem();
        }

        // Quick select by system type
        if (Input.GetKeyDown(KeyCode.F1)) SelectSystemByType<MountedEngine>();
        if (Input.GetKeyDown(KeyCode.F2)) SelectSystemByType<MountedRadiator>();
        if (Input.GetKeyDown(KeyCode.F3)) SelectSystemByType<MountedSensors>();
        if (Input.GetKeyDown(KeyCode.F4)) SelectSystemByType<MountedReactor>();
        if (Input.GetKeyDown(KeyCode.F5)) SelectSystemByType<MountedWeapon>();
        if (Input.GetKeyDown(KeyCode.F6)) SelectSystemByType<MountedPDTurret>();
        if (Input.GetKeyDown(KeyCode.F7)) SelectSystemByType<MountedMagazine>();
    }

    void SelectSystemByType<T>() where T : MountedSystem
    {
        for (int i = 0; i < testSystems.Count; i++)
        {
            if (testSystems[i] is T)
            {
                selectedSystemIndex = i;
                Debug.Log($"Selected: {GetSelectedSystemName()}");
                return;
            }
        }
        Debug.Log($"No {typeof(T).Name} found");
    }

    string GetSelectedSystemName()
    {
        if (testSystems.Count == 0 || selectedSystemIndex >= testSystems.Count)
            return "None";

        var system = testSystems[selectedSystemIndex];
        return $"{system.GetType().Name} ({system.CurrentState})";
    }

    void DamageSelectedSystem()
    {
        if (testSystems.Count == 0 || selectedSystemIndex >= testSystems.Count)
        {
            Debug.LogWarning("No system selected");
            return;
        }

        var system = testSystems[selectedSystemIndex];
        if (system.CurrentState == SystemState.Operational)
        {
            system.TakeCriticalHit();
            Debug.Log($"Damaged {system.GetType().Name} - Now {system.CurrentState}");
            LogDegradationStats();
        }
        else
        {
            Debug.Log($"{system.GetType().Name} is already {system.CurrentState}");
        }
    }

    void KillSelectedSystem()
    {
        if (testSystems.Count == 0 || selectedSystemIndex >= testSystems.Count)
        {
            Debug.LogWarning("No system selected");
            return;
        }

        var system = testSystems[selectedSystemIndex];
        while (system.CurrentState != SystemState.Destroyed)
        {
            system.TakeCriticalHit();
        }
        Debug.Log($"Destroyed {system.GetType().Name}");
        LogDegradationStats();
    }

    void RepairSelectedSystem()
    {
        if (testSystems.Count == 0 || selectedSystemIndex >= testSystems.Count)
        {
            Debug.LogWarning("No system selected");
            return;
        }

        var system = testSystems[selectedSystemIndex];
        system.Repair();
        Debug.Log($"Repaired {system.GetType().Name} - Now {system.CurrentState}");
        LogDegradationStats();
    }

    void LogDegradationStats()
    {
        if (degradationManager != null)
        {
            Debug.Log($"[Degradation] {degradationManager.GetDegradationSummary()}");
        }
    }

    void OnGUI()
    {
        if (!showUI) return;

        GUILayout.BeginArea(new Rect(10, 10, 350, 500));
        GUI.Box(new Rect(0, 0, 350, 500), "");

        GUILayout.Label("<b>DEGRADATION TEST CONTROLLER</b>");
        GUILayout.Space(5);

        // Current selection
        GUILayout.Label($"<b>Selected:</b> {GetSelectedSystemName()}");
        GUILayout.Label($"Index: {selectedSystemIndex + 1}/{testSystems.Count}");
        GUILayout.Space(10);

        // Degradation stats
        if (degradationManager != null)
        {
            GUILayout.Label("<b>CURRENT MULTIPLIERS:</b>");
            GUILayout.Label($"  Speed: x{degradationManager.SpeedMultiplier:F2}");
            GUILayout.Label($"  Turn Rate: x{degradationManager.TurnRateMultiplier:F2}");
            GUILayout.Label($"  Cooling: x{degradationManager.CoolingMultiplier:F2}");
            GUILayout.Label($"  Targeting Range: x{degradationManager.TargetingRangeMultiplier:F2}");
            GUILayout.Label($"  Heat Capacity: x{degradationManager.HeatCapacityMultiplier:F2}");
            GUILayout.Label($"  Passive Heat: +{degradationManager.PassiveHeatGeneration:F1}/turn");

            GUILayout.Space(10);
            GUILayout.Label($"<b>CAN MOVE:</b> {degradationManager.CanShipMove()}");
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>SYSTEM LIST:</b>");

        int i = 0;
        foreach (var system in testSystems)
        {
            string prefix = (i == selectedSystemIndex) ? ">" : " ";
            Color stateColor = system.CurrentState switch
            {
                SystemState.Operational => Color.green,
                SystemState.Damaged => Color.yellow,
                SystemState.Destroyed => Color.red,
                _ => Color.white
            };

            GUI.color = stateColor;
            GUILayout.Label($"{prefix} {system.GetType().Name.Replace("Mounted", "")}: {system.CurrentState}");
            GUI.color = Color.white;
            i++;
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>CONTROLS:</b>");
        GUILayout.Label("Tab: Next system | D: Damage | K: Kill | R: Repair");
        GUILayout.Label("F1-F7: Select by type | H: Toggle UI");

        GUILayout.EndArea();
    }
}
