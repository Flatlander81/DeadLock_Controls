using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor automation for creating section test scenes.
/// Provides menu items for quick test environment setup.
/// </summary>
public static class SectionTestSceneSetup
{
    /// <summary>
    /// Creates a new test scene with a ship pre-configured with sections.
    /// </summary>
    [MenuItem("Deadlock/Damage System/Create Section Test Scene")]
    public static void CreateSectionTestScene()
    {
        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create test ship
        GameObject shipObj = CreateTestShip();

        // Create test controller
        GameObject controllerObj = new GameObject("SectionTestController");
        SectionTestController controller = controllerObj.AddComponent<SectionTestController>();

        // Find and assign the ship's section manager
        SectionManager sectionManager = shipObj.GetComponent<SectionManager>();
        if (sectionManager != null)
        {
            // Use SerializedObject to set the field
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty prop = so.FindProperty("targetSectionManager");
            if (prop != null)
            {
                prop.objectReferenceValue = sectionManager;
                so.ApplyModifiedProperties();
            }
        }

        // Position camera to view ship
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0f, 10f, -15f);
            mainCamera.transform.LookAt(shipObj.transform);
        }

        // Save scene
        string scenePath = "Assets/Scenes/Testing/SectionTestScene.unity";
        EnsureDirectoryExists("Assets/Scenes/Testing");

        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log($"[SectionTestSceneSetup] Created test scene at {scenePath}");
        EditorUtility.DisplayDialog("Test Scene Created", $"Section test scene created at:\n{scenePath}\n\nUse the SectionTestController to apply damage via Inspector.", "OK");
    }

    /// <summary>
    /// Creates a test ship with all sections configured.
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

        // Add SectionManager
        SectionManager sectionManager = shipObj.AddComponent<SectionManager>();

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

        Debug.Log($"[SectionTestSceneSetup] Created test ship with 7 sections");

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
    /// Opens the section test scene if it exists.
    /// </summary>
    [MenuItem("Deadlock/Damage System/Open Section Test Scene")]
    public static void OpenSectionTestScene()
    {
        string scenePath = "Assets/Scenes/Testing/SectionTestScene.unity";

        if (System.IO.File.Exists(scenePath))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }
        else
        {
            if (EditorUtility.DisplayDialog("Scene Not Found", "Section test scene does not exist. Create it now?", "Create", "Cancel"))
            {
                CreateSectionTestScene();
            }
        }
    }
}
