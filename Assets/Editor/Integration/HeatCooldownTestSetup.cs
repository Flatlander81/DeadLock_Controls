using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to create Heat and Cooldown Integration test scenes.
/// Builds on Phase 3.5 Unified Test - adds heat/cooldown focus with HeatCooldownTestController.
/// Menu: Hephaestus/Testing/Phase 3.5 - Integration/Create Heat Cooldown Test Scene
/// </summary>
public class HeatCooldownTestSetup : Editor
{
    [MenuItem("Hephaestus/Testing/Phase 3.5 - Integration/Create Heat Cooldown Test Scene")]
    public static void CreateHeatCooldownTestScene()
    {
        Debug.Log("=== Creating Heat Cooldown Test Scene ===");
        Debug.Log("Building on Phase 3.5 Unified Test with Heat/Cooldown focus...");

        // First, create the Phase 3.5 Unified Test Level as the base
        Phase35UnifiedTestSetup.CreateUnifiedTestLevel();

        // Find the created root
        GameObject testRoot = GameObject.Find("Phase35UnifiedTestLevel");
        if (testRoot == null)
        {
            Debug.LogError("Failed to find Phase35UnifiedTestLevel - cannot add HeatCooldownTestController");
            return;
        }

        // Rename to indicate this is the Heat/Cooldown variant
        testRoot.name = "HeatCooldownTestLevel";

        // Add the specialized HeatCooldownTestController
        GameObject testControllerObj = new GameObject("HeatCooldownTestController");
        testControllerObj.transform.SetParent(testRoot.transform);
        HeatCooldownTestController testController = testControllerObj.AddComponent<HeatCooldownTestController>();

        // Find and wire references
        TurnEndProcessor turnEndProcessor = testRoot.GetComponentInChildren<TurnEndProcessor>();
        Ship[] allShips = testRoot.GetComponentsInChildren<Ship>();
        Ship playerShip = null;

        foreach (var ship in allShips)
        {
            if (ship.gameObject.name.Contains("Player"))
            {
                playerShip = ship;
                break;
            }
        }

        // Wire up test controller references
        SerializedObject so = new SerializedObject(testController);

        SerializedProperty processorProp = so.FindProperty("turnEndProcessor");
        if (processorProp != null) processorProp.objectReferenceValue = turnEndProcessor;

        SerializedProperty shipProp = so.FindProperty("testShip");
        if (shipProp != null) shipProp.objectReferenceValue = playerShip;

        so.ApplyModifiedProperties();

        // Select the test controller
        Selection.activeGameObject = testControllerObj;

        Debug.Log("=== Heat Cooldown Test Scene Created ===");
        Debug.Log("");
        Debug.Log("This scene includes ALL Phase 3 + Phase 3.5 features:");
        Debug.Log("  - Full damage system (shields, sections, critical hits)");
        Debug.Log("  - System degradation (engines, radiators, weapons)");
        Debug.Log("  - Core protection and ship death conditions");
        Debug.Log("  - Turn system with phases");
        Debug.Log("  - Weapon firing queue");
        Debug.Log("");
        Debug.Log("HEAT/COOLDOWN FEATURES (Focus of this test):");
        Debug.Log("  - TurnEndProcessor for heat dissipation");
        Debug.Log("  - Base dissipation: 10 heat/turn");
        Debug.Log("  - Radiator bonus: +5/operational, +2.5/damaged, +0/destroyed");
        Debug.Log("  - Weapon cooldown ticking each turn");
        Debug.Log("  - Ability cooldown ticking each turn");
        Debug.Log("  - OnHeatDissipated, OnWeaponReady events");
        Debug.Log("");
        Debug.Log("RADIATOR STATUS:");
        Debug.Log("  - 3 radiators on player ship (Radiator1, Radiator2, Radiator3)");
        Debug.Log("  - Use Heat/CD tab to damage radiators and see dissipation change");
        Debug.Log("");
        Debug.Log("Press Play to test. Use J to toggle test panels.");
        Debug.Log("Use T to advance turns and see heat dissipate.");
    }
}
