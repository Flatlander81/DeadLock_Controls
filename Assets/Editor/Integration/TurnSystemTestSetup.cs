using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to create Turn System test scenes.
/// Builds on Phase 3.5 Unified Test - adds turn system focus with TurnSystemTestController.
/// Menu: Hephaestus/Testing/Phase 3.5 - Integration/Create Turn System Test Scene
/// </summary>
public class TurnSystemTestSetup : Editor
{
    [MenuItem("Hephaestus/Testing/Phase 3.5 - Integration/Create Turn System Test Scene")]
    public static void CreateTurnSystemTestScene()
    {
        Debug.Log("=== Creating Turn System Test Scene ===");
        Debug.Log("Building on Phase 3.5 Unified Test with Turn System focus...");

        // First, create the Phase 3.5 Unified Test Level as the base
        Phase35UnifiedTestSetup.CreateUnifiedTestLevel();

        // Find the created root
        GameObject testRoot = GameObject.Find("Phase35UnifiedTestLevel");
        if (testRoot == null)
        {
            Debug.LogError("Failed to find Phase35UnifiedTestLevel - cannot add TurnSystemTestController");
            return;
        }

        // Rename to indicate this is the Turn System variant
        testRoot.name = "TurnSystemTestLevel";

        // Find the existing Phase35UnifiedTestController and disable it
        // We want to use the specialized TurnSystemTestController instead
        Phase35UnifiedTestController unifiedController = testRoot.GetComponentInChildren<Phase35UnifiedTestController>();
        if (unifiedController != null)
        {
            // Keep it but add our specialized controller too
            // The unified controller provides all Phase 3 features
            // The turn system controller provides focused turn testing
        }

        // Add the specialized TurnSystemTestController
        GameObject testControllerObj = new GameObject("TurnSystemTestController");
        testControllerObj.transform.SetParent(testRoot.transform);
        TurnSystemTestController testController = testControllerObj.AddComponent<TurnSystemTestController>();

        // Find and wire references
        TurnManager turnManager = testRoot.GetComponentInChildren<TurnManager>();
        CombatCoordinator coordinator = testRoot.GetComponentInChildren<CombatCoordinator>();
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
        // TurnSystemTestController will auto-discover references in Start() if not set

        so.ApplyModifiedProperties();

        // Select the test controller
        Selection.activeGameObject = testControllerObj;

        Debug.Log("=== Turn System Test Scene Created ===");
        Debug.Log("");
        Debug.Log("This scene includes ALL Phase 3 + Phase 3.5 features:");
        Debug.Log("  - Full damage system (shields, sections, critical hits)");
        Debug.Log("  - System degradation (engines, radiators, weapons)");
        Debug.Log("  - Core protection and ship death conditions");
        Debug.Log("  - Weapon firing with projectiles");
        Debug.Log("  - Heat and cooldown systems");
        Debug.Log("");
        Debug.Log("TURN SYSTEM FEATURES (Focus of this test):");
        Debug.Log("  - TurnManager phase control");
        Debug.Log("  - Command Phase -> Simulation Phase -> Turn End flow");
        Debug.Log("  - Event subscription and firing");
        Debug.Log("");
        Debug.Log("Press Play to test. Use J to toggle test panels.");
        Debug.Log("Use the Turn System panel to control phases manually.");
    }
}
