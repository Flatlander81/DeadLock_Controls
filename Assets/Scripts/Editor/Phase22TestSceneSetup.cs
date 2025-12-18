using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor script to set up Phase 2.2 test scene from menu.
/// Creates complete test environment with all weapon types.
/// </summary>
public class Phase22TestSceneSetup : Editor
{
    [MenuItem("DeadLock/Setup Phase 2.2 Test Scene")]
    public static void SetupTestScene()
    {
        // Create new scene
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Debug.Log("=== Setting Up Phase 2.2 Test Scene ===");

        // Create ProjectileManager
        GameObject pmObj = new GameObject("ProjectileManager");
        pmObj.AddComponent<ProjectileManager>();
        Debug.Log("Created ProjectileManager");

        // Create Main Camera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0, 20, -25);
        camObj.transform.rotation = Quaternion.Euler(40, 0, 0);
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        Debug.Log("Created Main Camera");

        // Create Directional Light
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.95f, 0.85f);
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        Debug.Log("Created Directional Light");

        // Create Player Ship
        GameObject playerShip = CreatePlayerShip();
        Debug.Log("Created Player Ship with 6 weapons");

        // Create Enemy Ships - positioned within weapon ranges and arcs
        // Ranges: Cannon=20, RailGun=30, Missile=35, Torpedo=50
        // Arcs: Torpedo=30deg (narrow), Cannon=30deg, RailGun=90deg, Missile=360deg
        // Enemy must be directly ahead for narrow-arc weapons
        GameObject enemy1 = CreateEnemyShip("EnemyShip_Alpha", new Vector3(0, 0, 15));  // Directly ahead, close - all weapons work
        GameObject enemy2 = CreateEnemyShip("EnemyShip_Beta", new Vector3(10, 0, 20));  // Off to side - missiles/railguns only
        Debug.Log("Created 2 Enemy Ships");

        // Create Test Controller
        GameObject controllerObj = new GameObject("Phase22TestController");
        Phase22TestController controller = controllerObj.AddComponent<Phase22TestController>();
        controller.playerShip = playerShip.GetComponent<Ship>();
        controller.enemyShips = new Ship[] {
            enemy1.GetComponent<Ship>(),
            enemy2.GetComponent<Ship>()
        };
        Debug.Log("Created Test Controller");

        // Create ground plane for reference
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "GroundPlane";
        ground.transform.position = new Vector3(0, -2, 10);
        ground.transform.localScale = new Vector3(8, 1, 8);
        SetObjectColor(ground, new Color(0.2f, 0.2f, 0.25f));
        Debug.Log("Created Ground Plane");

        // Select the controller so user can see it
        Selection.activeGameObject = controllerObj;

        // Mark scene dirty so user is prompted to save
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("");
        Debug.Log("=== Phase 2.2 Test Scene Ready! ===");
        Debug.Log("Press PLAY to start testing.");
        Debug.Log("");
        Debug.Log("CONTROLS (during Play mode):");
        Debug.Log("  1,2,3,4  - Fire weapon groups");
        Debug.Log("  A        - Alpha Strike (all weapons)");
        Debug.Log("  Tab      - Cycle targets");
        Debug.Log("  Space    - Toggle config panel");
        Debug.Log("  R        - Reset cooldowns (cheat)");
        Debug.Log("  L        - Reload all ammo (cheat)");
        Debug.Log("");
    }

    /// <summary>
    /// Set color on a GameObject's renderer without causing material leaks.
    /// Creates a new material instance that is properly managed.
    /// </summary>
    static void SetObjectColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Create a new material based on the standard shader
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.sharedMaterial = mat;
        }
    }

    static GameObject CreatePlayerShip()
    {
        GameObject shipObj = new GameObject("PlayerShip");
        shipObj.transform.position = Vector3.zero;

        // Add components
        Ship ship = shipObj.AddComponent<Ship>();
        shipObj.AddComponent<HeatManager>();

        // Create hull visual
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.name = "Hull";
        hull.transform.SetParent(shipObj.transform);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localScale = new Vector3(3, 1.5f, 6);
        SetObjectColor(hull, new Color(0.2f, 0.4f, 0.8f));

        // Remove hull collider (ship component handles collision)
        DestroyImmediate(hull.GetComponent<Collider>());

        // Create bridge
        GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = "Bridge";
        bridge.transform.SetParent(shipObj.transform);
        bridge.transform.localPosition = new Vector3(0, 1f, -1f);
        bridge.transform.localScale = new Vector3(1.5f, 1f, 2f);
        SetObjectColor(bridge, new Color(0.3f, 0.5f, 0.9f));
        DestroyImmediate(bridge.GetComponent<Collider>());

        // Create weapon hardpoints container
        GameObject hardpoints = new GameObject("WeaponHardpoints");
        hardpoints.transform.SetParent(shipObj.transform);
        hardpoints.transform.localPosition = Vector3.zero;

        // Add weapons - 6 total
        CreateWeaponHardpoint<RailGun>(hardpoints, "RailGun_Port", new Vector3(-2f, 0.5f, 1.5f), Color.cyan);
        CreateWeaponHardpoint<RailGun>(hardpoints, "RailGun_Starboard", new Vector3(2f, 0.5f, 1.5f), Color.cyan);
        CreateWeaponHardpoint<NewtonianCannon>(hardpoints, "Cannon_Forward", new Vector3(0, 0, 3.5f), Color.magenta);
        CreateWeaponHardpoint<TorpedoLauncher>(hardpoints, "Torpedo_Forward", new Vector3(0, 1f, 2.5f), new Color(1f, 0.5f, 0f));
        CreateWeaponHardpoint<MissileBattery>(hardpoints, "Missile_Dorsal", new Vector3(0, 1.5f, 0), Color.yellow);
        CreateWeaponHardpoint<MissileBattery>(hardpoints, "Missile_Ventral", new Vector3(0, -0.5f, 0), Color.yellow);

        // Add WeaponManager AFTER creating hardpoints
        shipObj.AddComponent<WeaponManager>();

        return shipObj;
    }

    static void CreateWeaponHardpoint<T>(GameObject parent, string name, Vector3 localPos, Color color) where T : WeaponSystem
    {
        GameObject hardpoint = new GameObject(name);
        hardpoint.transform.SetParent(parent.transform);
        hardpoint.transform.localPosition = localPos;
        hardpoint.transform.localRotation = Quaternion.identity;

        // Add weapon
        hardpoint.AddComponent<T>();

        // Add visual
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "Visual";
        visual.transform.SetParent(hardpoint.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * 0.4f;
        SetObjectColor(visual, color);
        DestroyImmediate(visual.GetComponent<Collider>());
    }

    static GameObject CreateEnemyShip(string name, Vector3 position)
    {
        GameObject shipObj = new GameObject(name);
        shipObj.transform.position = position;

        // Add components
        shipObj.AddComponent<Ship>();
        shipObj.AddComponent<HeatManager>();

        // Create hull visual
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.name = "Hull";
        hull.transform.SetParent(shipObj.transform);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localScale = new Vector3(2.5f, 1.2f, 5);
        SetObjectColor(hull, new Color(0.8f, 0.2f, 0.2f));
        DestroyImmediate(hull.GetComponent<Collider>());

        // Add target indicator sphere above ship
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = "TargetIndicator";
        indicator.transform.SetParent(shipObj.transform);
        indicator.transform.localPosition = new Vector3(0, 3, 0);
        indicator.transform.localScale = Vector3.one * 0.8f;
        SetObjectColor(indicator, Color.red);
        DestroyImmediate(indicator.GetComponent<Collider>());

        return shipObj;
    }
}
