using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to automatically create a complete projectile test scene.
/// Sets up ships, weapons, ProjectileManager, and camera for testing Track B.
/// </summary>
public class ProjectileTestSceneSetup : EditorWindow
{
    [MenuItem("Hephaestus/Testing/Phase 2 - Weapons/Setup Projectile Test Scene")]
    public static void CreateProjectileTestScene()
    {
        // Ask user if they want to save current scene
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("Scene setup cancelled by user.");
            return;
        }

        Debug.Log("Creating Projectile Test Scene...");

        // Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 1. Create ProjectileManager first (singleton needs to exist)
        GameObject managerObj = new GameObject("ProjectileManager");
        ProjectileManager manager = managerObj.AddComponent<ProjectileManager>();
        Debug.Log("✓ Created ProjectileManager");

        // 2. Create projectile prefabs if they don't exist
        CreateProjectilePrefabsIfNeeded();

        // 3. Create Player Ship with weapons
        GameObject playerShip = CreateShip("PlayerShip", Vector3.zero, true);
        if (playerShip == null)
        {
            Debug.LogError("Failed to create player ship!");
            return;
        }

        // 4. Setup hardpoints and weapons on player ship
        SetupHardpointsAndWeapons(playerShip);

        // 5. Create Enemy Ship (close for testing)
        GameObject enemyShip = CreateShip("EnemyShip", new Vector3(0f, 0f, 15f), false);
        if (enemyShip == null)
        {
            Debug.LogError("Failed to create enemy ship!");
            return;
        }

        // 6. Create second enemy (for testing multiple targets)
        GameObject enemyShip2 = CreateShip("EnemyShip2", new Vector3(10f, 0f, 20f), false);

        // 7. Setup Camera for good viewing angle
        GameObject cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            cam.transform.position = new Vector3(15f, 10f, -10f);
            cam.transform.LookAt(new Vector3(0f, 0f, 10f));
        }

        // 8. Create Game Manager with TurnManager
        GameObject gameManager = new GameObject("GameManager");
        TurnManager turnManager = gameManager.AddComponent<TurnManager>();

        // Assign ships to TurnManager
        var shipsField = typeof(TurnManager).GetField("ships",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (shipsField != null)
        {
            Ship[] ships = new Ship[] {
                playerShip.GetComponent<Ship>(),
                enemyShip.GetComponent<Ship>(),
                enemyShip2.GetComponent<Ship>()
            };
            shipsField.SetValue(turnManager, ships);
        }

        // 9. Add ProjectileTester to GameManager
        ProjectileTester tester = gameManager.AddComponent<ProjectileTester>();

        // Assign ships to ProjectileTester
        var playerField = typeof(ProjectileTester).GetField("playerShip",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var enemy1Field = typeof(ProjectileTester).GetField("enemyShip1",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var enemy2Field = typeof(ProjectileTester).GetField("enemyShip2",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (playerField != null) playerField.SetValue(tester, playerShip.GetComponent<Ship>());
        if (enemy1Field != null) enemy1Field.SetValue(tester, enemyShip.GetComponent<Ship>());
        if (enemy2Field != null) enemy2Field.SetValue(tester, enemyShip2.GetComponent<Ship>());

        // 10. Add directional light
        if (GameObject.Find("Directional Light") == null)
        {
            GameObject light = new GameObject("Directional Light");
            Light lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // 11. Create a ground plane for reference
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -2f, 0f);
        ground.transform.localScale = new Vector3(10f, 1f, 10f);

        // 12. Save the scene
        string scenePath = "Assets/Scenes/ProjectileTestScene.unity";

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"✓ Projectile Test Scene created: {scenePath}");

        // Print instructions
        PrintInstructions();

        // Select GameManager
        Selection.activeGameObject = gameManager;
    }

    private static void CreateProjectilePrefabsIfNeeded()
    {
        string prefabPath = "Assets/Prefabs/Projectiles";

        // Check if prefabs exist
        bool needsCreation = false;

        if (!System.IO.File.Exists($"{prefabPath}/BallisticProjectile.prefab") ||
            !System.IO.File.Exists($"{prefabPath}/HomingProjectile.prefab") ||
            !System.IO.File.Exists($"{prefabPath}/InstantHitEffect.prefab"))
        {
            needsCreation = true;
        }

        if (needsCreation)
        {
            Debug.Log("Projectile prefabs not found, creating them...");
            ProjectilePrefabSetup.CreateProjectilePrefabs();
        }
        else
        {
            Debug.Log("✓ Projectile prefabs already exist");
        }
    }

    private static GameObject CreateShip(string name, Vector3 position, bool isPlayer)
    {
        // Try to find ship prefab
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
        }
        else
        {
            // Create cube placeholder
            ship = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ship.name = name;
            ship.transform.position = position;
            ship.transform.localScale = new Vector3(2f, 0.5f, 4f);
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

        // Ensure collider for projectile collision
        BoxCollider collider = ship.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = ship.AddComponent<BoxCollider>();
            collider.size = new Vector3(2f, 1f, 4f);
        }

        // Player ship needs extra components
        if (isPlayer)
        {
            WeaponManager weaponManager = ship.GetComponent<WeaponManager>();
            if (weaponManager == null)
            {
                weaponManager = ship.AddComponent<WeaponManager>();
            }

            MovementController controller = ship.GetComponent<MovementController>();
            if (controller == null)
            {
                controller = ship.AddComponent<MovementController>();
            }

            DebugUI debugUI = ship.GetComponent<DebugUI>();
            if (debugUI == null)
            {
                debugUI = ship.AddComponent<DebugUI>();
            }
        }

        Debug.Log($"✓ Created {name} at {position}");
        return ship;
    }

    private static void SetupHardpointsAndWeapons(GameObject ship)
    {
        // Remove existing hardpoints
        Transform existingHardpoints = ship.transform.Find("WeaponHardpoints");
        if (existingHardpoints != null)
        {
            DestroyImmediate(existingHardpoints.gameObject);
        }

        // Create hardpoint parent
        GameObject hardpointsParent = new GameObject("WeaponHardpoints");
        hardpointsParent.transform.SetParent(ship.transform);
        hardpointsParent.transform.localPosition = Vector3.zero;
        hardpointsParent.transform.localRotation = Quaternion.identity;

        // Create RailGun (instant hit - for testing instant hit effects)
        GameObject railGun1 = CreateHardpoint(hardpointsParent.transform, "RailGun_Port",
            new Vector3(-1f, 0f, 1f));
        railGun1.AddComponent<RailGun>();

        GameObject railGun2 = CreateHardpoint(hardpointsParent.transform, "RailGun_Starboard",
            new Vector3(1f, 0f, 1f));
        railGun2.AddComponent<RailGun>();

        // Create Cannon (ballistic - for testing ballistic projectiles)
        GameObject cannon = CreateHardpoint(hardpointsParent.transform, "Cannon_Forward",
            new Vector3(0f, 0f, 2f));
        cannon.AddComponent<NewtonianCannon>();

        Debug.Log("✓ Created 3 weapon hardpoints (2 RailGuns, 1 Cannon)");
    }

    private static GameObject CreateHardpoint(Transform parent, string name, Vector3 localPosition)
    {
        GameObject hardpoint = new GameObject(name);
        hardpoint.transform.SetParent(parent);
        hardpoint.transform.localPosition = localPosition;
        hardpoint.transform.localRotation = Quaternion.identity;

        hardpoint.AddComponent<HardpointGizmo>();

        return hardpoint;
    }

    private static void PrintInstructions()
    {
        Debug.Log("========================================");
        Debug.Log("PROJECTILE TEST SCENE READY!");
        Debug.Log("========================================");
        Debug.Log("Setup complete:");
        Debug.Log("  ✓ ProjectileManager (singleton)");
        Debug.Log("  ✓ Player ship with 3 weapons");
        Debug.Log("  ✓ 2 Enemy ships");
        Debug.Log("  ✓ Camera positioned for viewing");
        Debug.Log("  ✓ ProjectileTester with controls");
        Debug.Log("");
        Debug.Log("Next steps:");
        Debug.Log("1. Press PLAY to enter Play Mode");
        Debug.Log("2. Use keyboard controls:");
        Debug.Log("   SPACE - Fire RailGuns (instant hit beams)");
        Debug.Log("   F - Fire Cannon (ballistic projectile)");
        Debug.Log("   P - Spawn test projectile manually");
        Debug.Log("   H - Spawn homing missile");
        Debug.Log("   I - Get projectile pool info");
        Debug.Log("3. Watch projectiles fly in Game/Scene view!");
        Debug.Log("4. See PROJECTILE_TEST_GUIDE.md for details");
        Debug.Log("========================================");
    }
}
