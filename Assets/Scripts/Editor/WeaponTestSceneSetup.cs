using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to automatically create a complete weapon test scene.
/// Run this to set up everything needed to test the weapon system.
/// </summary>
public class WeaponTestSceneSetup : EditorWindow
{
    [MenuItem("Hephaestus/Testing/Setup Weapon Test Scene")]
    public static void CreateWeaponTestScene()
    {
        // Ask user if they want to save current scene
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("Scene setup cancelled by user.");
            return;
        }

        Debug.Log("Creating Weapon Test Scene...");

        // Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 1. Create Player Ship
        GameObject playerShip = CreateShip("PlayerShip", Vector3.zero, true);
        if (playerShip == null)
        {
            Debug.LogError("Failed to create player ship!");
            return;
        }

        // 2. Setup hardpoints and weapons on player ship
        SetupHardpointsAndWeapons(playerShip);

        // 3. Create Enemy Ship
        GameObject enemyShip = CreateShip("EnemyShip", new Vector3(0f, 0f, 30f), false);
        if (enemyShip == null)
        {
            Debug.LogError("Failed to create enemy ship!");
            return;
        }

        // 4. Setup Camera
        GameObject cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 15f, -25f);
            cam.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("No Main Camera found in scene.");
        }

        // 5. Create Game Manager with TurnManager
        GameObject gameManager = new GameObject("GameManager");
        TurnManager turnManager = gameManager.AddComponent<TurnManager>();

        // Assign ships to TurnManager using reflection since ships array might be private
        var shipsField = typeof(TurnManager).GetField("ships",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (shipsField != null)
        {
            Ship[] ships = new Ship[] { playerShip.GetComponent<Ship>(), enemyShip.GetComponent<Ship>() };
            shipsField.SetValue(turnManager, ships);
            Debug.Log("Assigned ships to TurnManager via reflection");
        }
        else
        {
            Debug.LogWarning("Could not find ships field in TurnManager. You may need to assign ships manually.");
        }

        // 6. Add WeaponTester to GameManager
        WeaponTester tester = gameManager.AddComponent<WeaponTester>();

        // Assign ships to WeaponTester using reflection
        var playerShipField = typeof(WeaponTester).GetField("playerShip",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var enemyShipField = typeof(WeaponTester).GetField("enemyShip",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (playerShipField != null && enemyShipField != null)
        {
            playerShipField.SetValue(tester, playerShip.GetComponent<Ship>());
            enemyShipField.SetValue(tester, enemyShip.GetComponent<Ship>());
            Debug.Log("Assigned ships to WeaponTester");
        }

        // 7. Add directional light if not present
        if (GameObject.Find("Directional Light") == null)
        {
            GameObject light = new GameObject("Directional Light");
            Light lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // 8. Save the scene
        string scenePath = "Assets/Scenes/WeaponTestScene.unity";

        // Create Scenes folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"✓ Weapon Test Scene created and saved to {scenePath}");

        // Print instructions
        Debug.Log("========================================");
        Debug.Log("WEAPON TEST SCENE READY!");
        Debug.Log("========================================");
        Debug.Log("1. Press PLAY to enter Play Mode");
        Debug.Log("2. Use keyboard controls:");
        Debug.Log("   SPACE - Fire Group 1 (RailGuns)");
        Debug.Log("   F     - Fire Group 2 (Cannon)");
        Debug.Log("   A     - Alpha Strike (all weapons)");
        Debug.Log("   ARROWS - Move enemy ship");
        Debug.Log("   R     - Reset enemy position");
        Debug.Log("   K     - Kill enemy");
        Debug.Log("3. Watch Console for weapon firing logs");
        Debug.Log("4. Check DebugUI for heat/hull/shields");
        Debug.Log("========================================");

        // Select GameManager so user can see the setup
        Selection.activeGameObject = gameManager;
    }

    private static GameObject CreateShip(string name, Vector3 position, bool isPlayer)
    {
        // Try to find the ship prefab
        string[] prefabGuids = AssetDatabase.FindAssets("Scifi_Ship_Cruiser-FBX");
        GameObject shipPrefab = null;

        if (prefabGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
            shipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        GameObject ship;
        if (shipPrefab != null)
        {
            ship = PrefabUtility.InstantiatePrefab(shipPrefab) as GameObject;
            ship.name = name;
            ship.transform.position = position;
            ship.transform.rotation = Quaternion.identity;
            Debug.Log($"Created {name} from prefab");
        }
        else
        {
            // Create simple cube as placeholder if prefab not found
            ship = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ship.name = name;
            ship.transform.position = position;
            ship.transform.localScale = new Vector3(2f, 0.5f, 4f);
            Debug.LogWarning($"Ship prefab not found. Created {name} as cube placeholder.");
        }

        // Add required components
        Ship shipComponent = ship.GetComponent<Ship>();
        if (shipComponent == null)
        {
            shipComponent = ship.AddComponent<Ship>();
        }

        HeatManager heatManager = ship.GetComponent<HeatManager>();
        if (heatManager == null)
        {
            heatManager = ship.AddComponent<HeatManager>();
        }

        // Player ship needs WeaponManager and MovementController
        if (isPlayer)
        {
            WeaponManager weaponManager = ship.GetComponent<WeaponManager>();
            if (weaponManager == null)
            {
                weaponManager = ship.AddComponent<WeaponManager>();
            }

            MovementController movementController = ship.GetComponent<MovementController>();
            if (movementController == null)
            {
                movementController = ship.AddComponent<MovementController>();
            }

            DebugUI debugUI = ship.GetComponent<DebugUI>();
            if (debugUI == null)
            {
                debugUI = ship.AddComponent<DebugUI>();
            }
        }

        return ship;
    }

    private static void SetupHardpointsAndWeapons(GameObject ship)
    {
        // Check if hardpoints already exist
        Transform existingHardpoints = ship.transform.Find("WeaponHardpoints");
        if (existingHardpoints != null)
        {
            Debug.Log("Hardpoints already exist, removing old setup...");
            DestroyImmediate(existingHardpoints.gameObject);
        }

        // Create hardpoint parent
        GameObject hardpointsParent = new GameObject("WeaponHardpoints");
        hardpointsParent.transform.SetParent(ship.transform);
        hardpointsParent.transform.localPosition = Vector3.zero;
        hardpointsParent.transform.localRotation = Quaternion.identity;

        // Create RailGun hardpoints (port and starboard)
        GameObject portRailGun = CreateHardpoint(hardpointsParent.transform, "RailGun_Port_Hardpoint",
            new Vector3(-1f, 0f, 1f));
        portRailGun.AddComponent<RailGun>();

        GameObject starboardRailGun = CreateHardpoint(hardpointsParent.transform, "RailGun_Starboard_Hardpoint",
            new Vector3(1f, 0f, 1f));
        starboardRailGun.AddComponent<RailGun>();

        // Create Cannon hardpoint (forward center)
        GameObject cannon = CreateHardpoint(hardpointsParent.transform, "Cannon_Forward_Hardpoint",
            new Vector3(0f, 0f, 2f));
        cannon.AddComponent<NewtonianCannon>();

        Debug.Log($"✓ Created 3 weapon hardpoints on {ship.name}");
        Debug.Log("  - 2x RailGun (360° turrets, 30 range, 20 damage, 15 heat)");
        Debug.Log("  - 1x Newtonian Cannon (180° forward, 20 range, 40 damage, 30 heat)");
    }

    private static GameObject CreateHardpoint(Transform parent, string name, Vector3 localPosition)
    {
        GameObject hardpoint = new GameObject(name);
        hardpoint.transform.SetParent(parent);
        hardpoint.transform.localPosition = localPosition;
        hardpoint.transform.localRotation = Quaternion.identity;

        // Add gizmo visualization
        hardpoint.AddComponent<HardpointGizmo>();

        return hardpoint;
    }
}
