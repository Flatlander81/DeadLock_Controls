using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to automatically create a complete targeting UI test scene.
/// Sets up player ship, enemy targets, TargetingController, UIManager, and all necessary components.
/// Track C - Targeting UI System testing.
/// </summary>
public class TargetingTestSceneSetup : EditorWindow
{
    [MenuItem("Tools/Setup Targeting Test Scene")]
    public static void CreateTargetingTestScene()
    {
        // Ask user if they want to save current scene
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("Scene setup cancelled by user.");
            return;
        }

        Debug.Log("Creating Targeting Test Scene...");

        // Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 1. Create Player Ship (Hephaestus)
        GameObject playerShip = CreateShip("Hephaestus", Vector3.zero, true);
        if (playerShip == null)
        {
            Debug.LogError("Failed to create player ship!");
            return;
        }

        // 2. Setup hardpoints and weapons on player ship
        SetupHardpointsAndWeapons(playerShip);

        // 3. Create Enemy Ships at strategic positions
        GameObject enemy1 = CreateShip("Enemy1", new Vector3(15f, 0f, 0f), false); // Right
        GameObject enemy2 = CreateShip("Enemy2", new Vector3(0f, 0f, 15f), false);  // Front
        GameObject enemy3 = CreateShip("Enemy3", new Vector3(0f, 0f, -15f), false); // Behind (for arc testing)

        // 4. Setup Camera for good viewing angle
        GameObject cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            cam.transform.position = new Vector3(10f, 15f, -15f);
            cam.transform.LookAt(new Vector3(0f, 0f, 0f));

            // Add OrbitCamera for better control
            OrbitCamera orbitCam = cam.GetComponent<OrbitCamera>();
            if (orbitCam == null)
            {
                orbitCam = cam.AddComponent<OrbitCamera>();
            }
        }

        // 5. Create Game Manager with TurnManager
        GameObject gameManager = new GameObject("GameManager");
        TurnManager turnManager = gameManager.AddComponent<TurnManager>();

        // Assign ships to TurnManager using reflection
        var shipsField = typeof(TurnManager).GetField("ships",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (shipsField != null && enemy1 != null && enemy2 != null && enemy3 != null)
        {
            Ship[] ships = new Ship[] {
                playerShip.GetComponent<Ship>(),
                enemy1.GetComponent<Ship>(),
                enemy2.GetComponent<Ship>(),
                enemy3.GetComponent<Ship>()
            };
            shipsField.SetValue(turnManager, ships);
            Debug.Log("✓ Assigned 4 ships to TurnManager");
        }

        // 6. Create TargetingController
        GameObject targetingObj = new GameObject("TargetingController");
        TargetingController targetingController = targetingObj.AddComponent<TargetingController>();

        // Assign player ship using reflection
        var playerShipField = typeof(TargetingController).GetField("playerShip",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (playerShipField != null)
        {
            playerShipField.SetValue(targetingController, playerShip.GetComponent<Ship>());
            Debug.Log("✓ Assigned player ship to TargetingController");
        }

        // Assign camera reference
        var cameraField = typeof(TargetingController).GetField("mainCamera",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (cameraField != null && cam != null)
        {
            cameraField.SetValue(targetingController, cam.GetComponent<Camera>());
        }

        // Create and assign selection indicator prefab
        GameObject selectionIndicatorPrefab = CreateSelectionIndicatorPrefab();
        var indicatorField = typeof(TargetingController).GetField("selectionIndicatorPrefab",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (indicatorField != null && selectionIndicatorPrefab != null)
        {
            indicatorField.SetValue(targetingController, selectionIndicatorPrefab);
            Debug.Log("✓ Created and assigned SelectionIndicator prefab");
        }

        // 7. Create UIManager
        GameObject uiManagerObj = new GameObject("UIManager");
        UIManager uiManager = uiManagerObj.AddComponent<UIManager>();

        // Assign references to UIManager using reflection
        var uiPlayerField = typeof(UIManager).GetField("playerShip",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var uiTargetingField = typeof(UIManager).GetField("targetingController",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (uiPlayerField != null)
        {
            uiPlayerField.SetValue(uiManager, playerShip.GetComponent<Ship>());
        }

        if (uiTargetingField != null)
        {
            uiTargetingField.SetValue(uiManager, targetingController);
        }

        Debug.Log("✓ Created UIManager with references");

        // 8. Link MovementController to TargetingController and set player ship
        MovementController movementController = playerShip.GetComponent<MovementController>();
        if (movementController != null)
        {
            var moveTargetingField = typeof(MovementController).GetField("targetingController",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (moveTargetingField != null)
            {
                moveTargetingField.SetValue(movementController, targetingController);
                Debug.Log("✓ Linked MovementController to TargetingController");
            }

            // Set player ship reference so only it can be moved
            var movePlayerShipField = typeof(MovementController).GetField("playerShip",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (movePlayerShipField != null)
            {
                movePlayerShipField.SetValue(movementController, playerShip.GetComponent<Ship>());
                Debug.Log("✓ Assigned player ship to MovementController (restricts movement to player only)");
            }
        }

        // 9. Add directional light if doesn't exist
        if (GameObject.Find("Directional Light") == null)
        {
            GameObject light = new GameObject("Directional Light");
            Light lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Debug.Log("✓ Created Directional Light");
        }

        // 10. Create a ground plane for reference
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -2f, 0f);
        ground.transform.localScale = new Vector3(10f, 1f, 10f);

        // Set ground material to darker color
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.2f, 0.2f, 0.2f);
            groundRenderer.material = groundMat;
        }

        Debug.Log("✓ Created ground plane");

        // 11. Add visual markers to enemy ships
        AddVisualMarkers(enemy1, Color.red);
        AddVisualMarkers(enemy2, Color.red);
        AddVisualMarkers(enemy3, Color.red);

        // 12. Save the scene
        string scenePath = "Assets/Scenes/TargetingTestScene.unity";

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"✓ Targeting Test Scene created: {scenePath}");

        // Print instructions
        PrintInstructions();

        // Select TargetingController for easy inspection
        Selection.activeGameObject = targetingObj;

        // Force repaint
        EditorApplication.RepaintHierarchyWindow();
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
        if (shipPrefab != null && isPlayer)
        {
            // Use prefab for player ship
            ship = PrefabUtility.InstantiatePrefab(shipPrefab) as GameObject;
            ship.name = name;
            ship.transform.position = position;
            ship.transform.rotation = Quaternion.identity;
        }
        else
        {
            // Create cube placeholder (enemies)
            ship = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ship.name = name;
            ship.transform.position = position;
            ship.transform.localScale = new Vector3(2f, 0.5f, 4f);

            // Color enemy ships red
            if (!isPlayer)
            {
                Renderer renderer = ship.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(0.8f, 0.2f, 0.2f);
                    renderer.material = mat;
                }
            }
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

        // Ensure collider for raycasting
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

            // Add visual wireframe for player ship
            GameObject wireframe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wireframe.name = "Wireframe";
            wireframe.transform.SetParent(ship.transform);
            wireframe.transform.localPosition = Vector3.zero;
            wireframe.transform.localRotation = Quaternion.identity;
            wireframe.transform.localScale = new Vector3(2.1f, 0.6f, 4.1f);

            // Remove collider from wireframe
            DestroyImmediate(wireframe.GetComponent<Collider>());

            // Make wireframe green and transparent
            Renderer wireRenderer = wireframe.GetComponent<Renderer>();
            if (wireRenderer != null)
            {
                Material wireMat = new Material(Shader.Find("Standard"));
                wireMat.SetFloat("_Mode", 3); // Transparent mode
                wireMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                wireMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                wireMat.SetInt("_ZWrite", 0);
                wireMat.DisableKeyword("_ALPHATEST_ON");
                wireMat.EnableKeyword("_ALPHABLEND_ON");
                wireMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                wireMat.renderQueue = 3000;

                wireMat.color = new Color(0f, 1f, 0f, 0.3f);
                wireRenderer.material = wireMat;
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

        // Create RailGun (360° turret - can fire in any direction)
        GameObject railGun1 = CreateHardpoint(hardpointsParent.transform, "RailGun_Port",
            new Vector3(-1f, 0f, 1f));
        railGun1.AddComponent<RailGun>();

        GameObject railGun2 = CreateHardpoint(hardpointsParent.transform, "RailGun_Starboard",
            new Vector3(1f, 0f, 1f));
        railGun2.AddComponent<RailGun>();

        // Create Cannon (180° forward arc - for testing arc warnings)
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

    private static GameObject CreateSelectionIndicatorPrefab()
    {
        // Create prefab folder if needed
        string prefabPath = "Assets/Prefabs/UI";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        // Check if prefab already exists
        string prefabFilePath = $"{prefabPath}/SelectionIndicator.prefab";
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFilePath);

        if (existingPrefab != null)
        {
            Debug.Log("✓ SelectionIndicator prefab already exists");
            return existingPrefab;
        }

        // Create new selection indicator
        GameObject indicator = new GameObject("SelectionIndicator");
        SelectionIndicator indicatorScript = indicator.AddComponent<SelectionIndicator>();

        // Save as prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(indicator, prefabFilePath);

        // Clean up temporary object
        DestroyImmediate(indicator);

        Debug.Log($"✓ Created SelectionIndicator prefab: {prefabFilePath}");
        return prefab;
    }

    private static void AddVisualMarkers(GameObject ship, Color color)
    {
        if (ship == null) return;

        // Add a small sphere above ship as marker
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "Marker";
        marker.transform.SetParent(ship.transform);
        marker.transform.localPosition = new Vector3(0f, 2f, 0f);
        marker.transform.localScale = Vector3.one * 0.5f;

        // Remove collider
        DestroyImmediate(marker.GetComponent<Collider>());

        // Set color
        Renderer renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.5f);
            renderer.material = mat;
        }
    }

    private static void PrintInstructions()
    {
        Debug.Log("========================================");
        Debug.Log("TARGETING TEST SCENE READY!");
        Debug.Log("========================================");
        Debug.Log("Setup complete:");
        Debug.Log("  ✓ Player ship 'Hephaestus' with 3 weapons");
        Debug.Log("  ✓ 3 Enemy ships (right, front, behind)");
        Debug.Log("  ✓ TargetingController with selection");
        Debug.Log("  ✓ UIManager for panels");
        Debug.Log("  ✓ Camera with orbit controls");
        Debug.Log("  ✓ SelectionIndicator prefab created");
        Debug.Log("");
        Debug.Log("Next steps:");
        Debug.Log("1. Press PLAY to enter Play Mode");
        Debug.Log("2. Left-click ships to select them:");
        Debug.Log("   - Click Enemy → Weapon Group Panel (right)");
        Debug.Log("   - Click Hephaestus → Weapon Config Panel (left)");
        Debug.Log("3. Configure weapons:");
        Debug.Log("   - Assign weapons to groups 1-4");
        Debug.Log("   - Click group buttons to cycle");
        Debug.Log("4. Fire at targets:");
        Debug.Log("   - Select enemy ship");
        Debug.Log("   - Press 1-4 to fire groups");
        Debug.Log("   - Press A for Alpha Strike");
        Debug.Log("5. Test features:");
        Debug.Log("   - Enemy3 (behind) for arc warnings");
        Debug.Log("   - Multiple targets for multi-group");
        Debug.Log("   - Heat warnings with all weapons");
        Debug.Log("");
        Debug.Log("See TARGETING_QUICK_START.md for keyboard controls");
        Debug.Log("See TARGETING_TEST_GUIDE.md for detailed testing");
        Debug.Log("========================================");
    }
}
