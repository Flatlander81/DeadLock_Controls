using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor automation for creating projectile damage test scenes.
/// Provides menu item for quick test environment setup.
/// </summary>
public static class ProjectileDamageTestSetup
{
    /// <summary>
    /// Creates a new test scene for projectile damage integration testing.
    /// </summary>
    [MenuItem("Hephaestus/Testing/Create Projectile Damage Test Scene")]
    public static void CreateProjectileDamageTestScene()
    {
        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create target ship
        GameObject shipObj = CreateTestShip();

        // Create test controller
        GameObject controllerObj = new GameObject("ProjectileDamageTestController");
        ProjectileDamageTestController controller = controllerObj.AddComponent<ProjectileDamageTestController>();

        // Wire up references using SerializedObject
        Ship ship = shipObj.GetComponent<Ship>();
        ShieldSystem shieldSystem = shipObj.GetComponent<ShieldSystem>();
        SectionManager sectionManager = shipObj.GetComponent<SectionManager>();
        DamageRouter damageRouter = shipObj.GetComponent<DamageRouter>();

        SerializedObject so = new SerializedObject(controller);

        SerializedProperty shipProp = so.FindProperty("targetShip");
        if (shipProp != null) shipProp.objectReferenceValue = ship;

        SerializedProperty shieldProp = so.FindProperty("targetShieldSystem");
        if (shieldProp != null) shieldProp.objectReferenceValue = shieldSystem;

        SerializedProperty sectionProp = so.FindProperty("targetSectionManager");
        if (sectionProp != null) sectionProp.objectReferenceValue = sectionManager;

        SerializedProperty routerProp = so.FindProperty("targetDamageRouter");
        if (routerProp != null) routerProp.objectReferenceValue = damageRouter;

        so.ApplyModifiedProperties();

        // Position camera for good view
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0f, 15f, -25f);
            mainCamera.transform.LookAt(shipObj.transform);
        }

        // Save scene
        string scenePath = "Assets/Scenes/Testing/ProjectileDamageTestScene.unity";
        EnsureDirectoryExists("Assets/Scenes/Testing");

        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log($"[ProjectileDamageTestSetup] Created test scene at {scenePath}");
        EditorUtility.DisplayDialog("Test Scene Created",
            $"Projectile damage test scene created at:\n{scenePath}\n\n" +
            "Use the ProjectileDamageTestController to:\n" +
            "- Fire ballistic/homing projectiles at sections\n" +
            "- Toggle shields on/off\n" +
            "- View damage routing log\n\n" +
            "Hotkeys: 1=Ballistic, 2=Homing, S=Toggle Shields, R=Reset",
            "OK");
    }

    /// <summary>
    /// Creates a test ship with all damage system components.
    /// </summary>
    private static GameObject CreateTestShip()
    {
        // Create ship GameObject
        GameObject shipObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shipObj.name = "TestShip_Target";
        shipObj.transform.position = Vector3.zero;
        shipObj.transform.localScale = new Vector3(2f, 1f, 4f);

        // Remove default collider (sections will have their own)
        Object.DestroyImmediate(shipObj.GetComponent<Collider>());

        // Add Ship component
        Ship ship = shipObj.AddComponent<Ship>();

        // Add HeatManager
        shipObj.AddComponent<HeatManager>();

        // Add ShieldSystem
        ShieldSystem shieldSystem = shipObj.AddComponent<ShieldSystem>();
        shieldSystem.Initialize(200f);

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
            CreateTestSection(sectionsContainer.transform, sectionType, ship, damageRouter);
        }

        // Add debug visualizer
        shipObj.AddComponent<SectionDebugVisualizer>();

        Debug.Log($"[ProjectileDamageTestSetup] Created test ship with ShieldSystem, 7 sections, and DamageRouter");

        return shipObj;
    }

    /// <summary>
    /// Creates a single test section with hit detector.
    /// </summary>
    private static void CreateTestSection(Transform parent, SectionType sectionType, Ship ship, DamageRouter damageRouter)
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

        // Add hit detector with damage router reference
        SectionHitDetector hitDetector = sectionObj.AddComponent<SectionHitDetector>();
        hitDetector.SetParentSection(section);
        hitDetector.SetDamageRouter(damageRouter);
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
    /// Opens the projectile damage test scene if it exists.
    /// </summary>
    [MenuItem("Hephaestus/Testing/Open Projectile Damage Test Scene")]
    public static void OpenProjectileDamageTestScene()
    {
        string scenePath = "Assets/Scenes/Testing/ProjectileDamageTestScene.unity";

        if (System.IO.File.Exists(scenePath))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }
        else
        {
            if (EditorUtility.DisplayDialog("Scene Not Found", "Projectile damage test scene does not exist. Create it now?", "Create", "Cancel"))
            {
                CreateProjectileDamageTestScene();
            }
        }
    }
}
