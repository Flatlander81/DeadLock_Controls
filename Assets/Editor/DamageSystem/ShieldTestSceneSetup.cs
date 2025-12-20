using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor automation for creating shield test scenes.
/// Provides menu item for quick test environment setup.
/// </summary>
public static class ShieldTestSceneSetup
{
    /// <summary>
    /// Creates a new test scene with a ship configured with shields and sections.
    /// </summary>
    [MenuItem("Hephaestus/Testing/Phase 3 - Damage/Create Shield Test Scene")]
    public static void CreateShieldTestScene()
    {
        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create test ship
        GameObject shipObj = CreateTestShip();

        // Create test controller
        GameObject controllerObj = new GameObject("ShieldTestController");
        ShieldTestController controller = controllerObj.AddComponent<ShieldTestController>();

        // Use SerializedObject to properly wire references (can't use SetTargetShip at edit time)
        Ship ship = shipObj.GetComponent<Ship>();
        DamageRouter damageRouter = shipObj.GetComponent<DamageRouter>();
        ShieldSystem shieldSystem = shipObj.GetComponent<ShieldSystem>();
        SectionManager sectionManager = shipObj.GetComponent<SectionManager>();

        SerializedObject so = new SerializedObject(controller);

        SerializedProperty shipProp = so.FindProperty("targetShip");
        if (shipProp != null) shipProp.objectReferenceValue = ship;

        SerializedProperty routerProp = so.FindProperty("targetDamageRouter");
        if (routerProp != null) routerProp.objectReferenceValue = damageRouter;

        SerializedProperty shieldProp = so.FindProperty("targetShieldSystem");
        if (shieldProp != null) shieldProp.objectReferenceValue = shieldSystem;

        SerializedProperty sectionProp = so.FindProperty("targetSectionManager");
        if (sectionProp != null) sectionProp.objectReferenceValue = sectionManager;

        so.ApplyModifiedProperties();

        // Position camera to view ship
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0f, 10f, -15f);
            mainCamera.transform.LookAt(shipObj.transform);
        }

        // Save scene
        string scenePath = "Assets/Scenes/Testing/ShieldTestScene.unity";
        EnsureDirectoryExists("Assets/Scenes/Testing");

        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log($"[ShieldTestSceneSetup] Created test scene at {scenePath}");
        EditorUtility.DisplayDialog("Test Scene Created",
            $"Shield test scene created at:\n{scenePath}\n\n" +
            "Use the ShieldTestController to:\n" +
            "- Apply damage through DamageRouter\n" +
            "- Test shield absorption\n" +
            "- Test Shield Boost activation conditions",
            "OK");
    }

    /// <summary>
    /// Creates a test ship with all damage system components.
    /// </summary>
    private static GameObject CreateTestShip()
    {
        // Create ship GameObject with basic components
        GameObject shipObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shipObj.name = "TestShip_Hephaestus";
        shipObj.transform.position = Vector3.zero;
        shipObj.transform.localScale = new Vector3(2f, 1f, 4f);

        // Remove default collider (sections will have their own)
        Object.DestroyImmediate(shipObj.GetComponent<Collider>());

        // Add Ship component
        Ship ship = shipObj.AddComponent<Ship>();

        // Add HeatManager (required by Ship)
        shipObj.AddComponent<HeatManager>();

        // Add ShieldSystem with GDD values
        ShieldSystem shieldSystem = shipObj.AddComponent<ShieldSystem>();
        shieldSystem.Initialize(200f); // GDD: 200 shields

        // Add SectionManager
        SectionManager sectionManager = shipObj.AddComponent<SectionManager>();

        // Add DamageRouter
        DamageRouter damageRouter = shipObj.AddComponent<DamageRouter>();
        damageRouter.SetReferences(shieldSystem, sectionManager, ship);

        // Create sections container
        GameObject sectionsContainer = new GameObject("Sections");
        sectionsContainer.transform.SetParent(shipObj.transform);
        sectionsContainer.transform.localPosition = Vector3.zero;
        sectionsContainer.transform.localRotation = Quaternion.identity;

        // Create each section
        foreach (SectionType sectionType in SectionDefinitions.GetAllSectionTypes())
        {
            CreateTestSection(sectionsContainer.transform, sectionType, ship);
        }

        // Add debug visualizer
        shipObj.AddComponent<SectionDebugVisualizer>();

        Debug.Log($"[ShieldTestSceneSetup] Created test ship with ShieldSystem (200), 7 sections, and DamageRouter");

        return shipObj;
    }

    /// <summary>
    /// Creates a single test section.
    /// </summary>
    private static void CreateTestSection(Transform parent, SectionType sectionType, Ship ship)
    {
        var config = SectionDefinitions.GetConfig(sectionType);

        // Create section GameObject
        GameObject sectionObj = new GameObject($"Section_{sectionType}");
        sectionObj.transform.SetParent(parent);
        sectionObj.transform.localPosition = config.ColliderPosition;
        sectionObj.transform.localRotation = Quaternion.identity;

        // Add ShipSection component
        ShipSection section = sectionObj.AddComponent<ShipSection>();
        section.Initialize(sectionType, ship);

        // Add collider based on config
        if (config.UseSphereCollider)
        {
            SphereCollider sphereCol = sectionObj.AddComponent<SphereCollider>();
            sphereCol.radius = config.SphereRadius;
            sphereCol.isTrigger = true;
        }
        else
        {
            BoxCollider boxCol = sectionObj.AddComponent<BoxCollider>();
            boxCol.size = config.ColliderSize;
            boxCol.isTrigger = true;
        }

        // Add hit detector
        SectionHitDetector hitDetector = sectionObj.AddComponent<SectionHitDetector>();
        hitDetector.SetParentSection(section);
    }

    /// <summary>
    /// Ensures a directory path exists.
    /// </summary>
    private static void EnsureDirectoryExists(string path)
    {
        string[] folders = path.Split('/');
        string currentPath = folders[0];

        for (int i = 1; i < folders.Length; i++)
        {
            string parentPath = currentPath;
            currentPath = $"{currentPath}/{folders[i]}";

            if (!AssetDatabase.IsValidFolder(currentPath))
            {
                AssetDatabase.CreateFolder(parentPath, folders[i]);
            }
        }
    }

    /// <summary>
    /// Opens the shield test scene if it exists.
    /// </summary>
    [MenuItem("Hephaestus/Testing/Phase 3 - Damage/Open Shield Test Scene")]
    public static void OpenShieldTestScene()
    {
        string scenePath = "Assets/Scenes/Testing/ShieldTestScene.unity";

        if (System.IO.File.Exists(scenePath))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }
        else
        {
            if (EditorUtility.DisplayDialog("Scene Not Found", "Shield test scene does not exist. Create it now?", "Create", "Cancel"))
            {
                CreateShieldTestScene();
            }
        }
    }
}
