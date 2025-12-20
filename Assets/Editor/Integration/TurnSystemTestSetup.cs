using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor script to create Turn System test scenes.
/// Menu: Hephaestus/Testing/Phase 3.5 - Integration/Create Turn System Test Scene
/// </summary>
public class TurnSystemTestSetup : Editor
{
    [MenuItem("Hephaestus/Testing/Phase 3.5 - Integration/Create Turn System Test Scene")]
    public static void CreateTurnSystemTestScene()
    {
        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create TurnManager
        GameObject turnManagerObj = new GameObject("TurnManager");
        TurnManager turnManager = turnManagerObj.AddComponent<TurnManager>();
        Debug.Log("Created TurnManager");

        // Create CombatCoordinator
        GameObject coordinatorObj = new GameObject("CombatCoordinator");
        CombatCoordinator coordinator = coordinatorObj.AddComponent<CombatCoordinator>();
        Debug.Log("Created CombatCoordinator");

        // Create Test Controller
        GameObject testControllerObj = new GameObject("TurnSystemTestController");
        TurnSystemTestController testController = testControllerObj.AddComponent<TurnSystemTestController>();
        Debug.Log("Created TurnSystemTestController");

        // Create a simple player ship
        GameObject playerShip = CreateTestShip("Hephaestus", Vector3.zero, Color.cyan);
        Debug.Log("Created player ship: Hephaestus");

        // Create enemy ships
        GameObject enemy1 = CreateTestShip("Enemy_Alpha", new Vector3(15, 0, 10), Color.red);
        GameObject enemy2 = CreateTestShip("Enemy_Beta", new Vector3(-15, 0, 10), Color.red);
        Debug.Log("Created enemy ships");

        // Create ProjectileManager if not exists
        if (Object.FindFirstObjectByType<ProjectileManager>() == null)
        {
            GameObject projectileManagerObj = new GameObject("ProjectileManager");
            projectileManagerObj.AddComponent<ProjectileManager>();
            Debug.Log("Created ProjectileManager");
        }

        // Setup camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0, 30, -20);
            mainCamera.transform.rotation = Quaternion.Euler(50, 0, 0);
            mainCamera.orthographic = false;
            mainCamera.fieldOfView = 60;
        }

        // Add directional light if not present
        if (Object.FindFirstObjectByType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            light.intensity = 1f;
        }

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log("=== Turn System Test Scene Created ===");
        Debug.Log("Press Play to test turn system events.");
        Debug.Log("Use the OnGUI buttons to control phases manually.");
        Debug.Log("Watch the event log for event firing sequence.");

        // Select the test controller
        Selection.activeGameObject = testControllerObj;
    }

    private static GameObject CreateTestShip(string name, Vector3 position, Color color)
    {
        // Create ship GameObject
        GameObject shipObj = new GameObject(name);
        shipObj.transform.position = position;

        // Add Ship component
        Ship ship = shipObj.AddComponent<Ship>();

        // Create visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Hull";
        visual.transform.SetParent(shipObj.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(2, 0.5f, 4);

        // Set color
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }

        // Add collider for projectile detection
        BoxCollider collider = shipObj.AddComponent<BoxCollider>();
        collider.size = new Vector3(2, 1, 4);
        collider.isTrigger = true;

        // Create bow indicator
        GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bow.name = "Bow";
        bow.transform.SetParent(shipObj.transform);
        bow.transform.localPosition = new Vector3(0, 0, 2.5f);
        bow.transform.localScale = Vector3.one * 0.5f;

        Renderer bowRenderer = bow.GetComponent<Renderer>();
        if (bowRenderer != null)
        {
            Material bowMat = new Material(Shader.Find("Standard"));
            bowMat.color = Color.white;
            bowRenderer.material = bowMat;
        }

        // Remove collider from bow
        Object.DestroyImmediate(bow.GetComponent<Collider>());

        return shipObj;
    }

    [MenuItem("Hephaestus/Testing/Phase 3.5 - Integration/Open Turn System Test Scene")]
    public static void OpenTurnSystemTestScene()
    {
        string[] guids = AssetDatabase.FindAssets("TurnSystemTest t:Scene");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            EditorSceneManager.OpenScene(path);
        }
        else
        {
            Debug.LogWarning("Turn System Test Scene not found. Creating new one...");
            CreateTurnSystemTestScene();
        }
    }
}
