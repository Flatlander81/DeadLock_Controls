using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to create Weapon Firing Integration test scenes.
/// Builds on Phase 3.5 Unified Test - adds weapon firing queue focus with WeaponFiringTestController.
/// Menu: Hephaestus/Testing/Phase 3.5 - Integration/Create Weapon Firing Test Scene
/// </summary>
public class WeaponFiringTestSetup : Editor
{
    [MenuItem("Hephaestus/Testing/Phase 3.5 - Integration/Create Weapon Firing Test Scene")]
    public static void CreateWeaponFiringTestScene()
    {
        Debug.Log("=== Creating Weapon Firing Test Scene ===");
        Debug.Log("Building on Phase 3.5 Unified Test with Weapon Firing focus...");

        // First, create the Phase 3.5 Unified Test Level as the base
        Phase35UnifiedTestSetup.CreateUnifiedTestLevel();

        // Find the created root
        GameObject testRoot = GameObject.Find("Phase35UnifiedTestLevel");
        if (testRoot == null)
        {
            Debug.LogError("Failed to find Phase35UnifiedTestLevel - cannot add WeaponFiringTestController");
            return;
        }

        // Rename to indicate this is the Weapon Firing variant
        testRoot.name = "WeaponFiringTestLevel";

        // Add the specialized WeaponFiringTestController
        GameObject testControllerObj = new GameObject("WeaponFiringTestController");
        testControllerObj.transform.SetParent(testRoot.transform);
        WeaponFiringTestController testController = testControllerObj.AddComponent<WeaponFiringTestController>();

        // Find and wire references
        Ship[] allShips = testRoot.GetComponentsInChildren<Ship>();
        Ship playerShip = null;
        Ship targetShip = null;

        foreach (var ship in allShips)
        {
            if (ship.gameObject.name.Contains("Player"))
            {
                playerShip = ship;
            }
            else if (targetShip == null)
            {
                targetShip = ship;
            }
        }

        WeaponManager weaponManager = playerShip?.GetComponent<WeaponManager>();

        // Wire up test controller references
        SerializedObject so = new SerializedObject(testController);

        SerializedProperty playerProp = so.FindProperty("playerShip");
        if (playerProp != null) playerProp.objectReferenceValue = playerShip;

        SerializedProperty enemyProp = so.FindProperty("targetShip");
        if (enemyProp != null) enemyProp.objectReferenceValue = targetShip;

        SerializedProperty wmProp = so.FindProperty("weaponManager");
        if (wmProp != null) wmProp.objectReferenceValue = weaponManager;

        so.ApplyModifiedProperties();

        // Select the test controller
        Selection.activeGameObject = testControllerObj;

        Debug.Log("=== Weapon Firing Test Scene Created ===");
        Debug.Log("");
        Debug.Log("This scene includes ALL Phase 3 + Phase 3.5 features:");
        Debug.Log("  - Full damage system (shields, sections, critical hits)");
        Debug.Log("  - System degradation (engines, radiators, weapons)");
        Debug.Log("  - Core protection and ship death conditions");
        Debug.Log("  - Turn system with phases");
        Debug.Log("  - Heat and cooldown systems");
        Debug.Log("");
        Debug.Log("WEAPON FIRING FEATURES (Focus of this test):");
        Debug.Log("  - WeaponFiringQueue for queued execution");
        Debug.Log("  - Weapon groups (1-4) with auto-assignment");
        Debug.Log("  - Firing arc and range validation");
        Debug.Log("  - Heat cost and cooldown management");
        Debug.Log("  - Projectile spawning and damage routing");
        Debug.Log("");
        Debug.Log("Press Play to test. Use J to toggle test panels.");
        Debug.Log("Keys 1-4 fire weapon groups, A for Alpha Strike.");
    }
}
