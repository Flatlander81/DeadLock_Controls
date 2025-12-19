using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor utility for setting up a Critical Hit test scene.
/// Creates a scene with ship, sections, mounted systems, and test UI.
/// </summary>
public class CriticalHitTestSetup
{
    [MenuItem("Hephaestus/Testing/Create Critical Hit Test Scene")]
    public static void CreateCriticalHitTestScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create main test object
        GameObject testRoot = new GameObject("CriticalHitTest");

        // Create ship with full infrastructure
        GameObject shipObj = new GameObject("TestShip");
        shipObj.transform.SetParent(testRoot.transform);

        Ship ship = shipObj.AddComponent<Ship>();

        // Add damage infrastructure
        DamageRouter damageRouter = shipObj.AddComponent<DamageRouter>();
        ShieldSystem shieldSystem = shipObj.AddComponent<ShieldSystem>();
        SectionManager sectionManager = shipObj.AddComponent<SectionManager>();
        CriticalHitSystem criticalHitSystem = shipObj.AddComponent<CriticalHitSystem>();

        // Create section objects
        CreateSectionWithSystems(shipObj.transform, SectionType.Fore, ship, sectionManager, new SystemMount[]
        {
            new SystemMount(ShipSystemType.TorpedoLauncher, 1),
            new SystemMount(ShipSystemType.TorpedoMagazine, 9),
            new SystemMount(ShipSystemType.Sensors, 17)
        });

        CreateSectionWithSystems(shipObj.transform, SectionType.Core, ship, sectionManager, new SystemMount[]
        {
            new SystemMount(ShipSystemType.ReactorCore, 1),
            new SystemMount(ShipSystemType.MainEngine, 21),
            new SystemMount(ShipSystemType.Radiator, 36)
        });

        CreateSectionWithSystems(shipObj.transform, SectionType.Port, ship, sectionManager, new SystemMount[]
        {
            new SystemMount(ShipSystemType.NewtonianCannon, 1),
            new SystemMount(ShipSystemType.PDTurret, 9),
            new SystemMount(ShipSystemType.Radiator, 13)
        });

        CreateSectionWithSystems(shipObj.transform, SectionType.Starboard, ship, sectionManager, new SystemMount[]
        {
            new SystemMount(ShipSystemType.NewtonianCannon, 1),
            new SystemMount(ShipSystemType.PDTurret, 9),
            new SystemMount(ShipSystemType.Radiator, 13)
        });

        CreateSectionWithSystems(shipObj.transform, SectionType.Aft, ship, sectionManager, new SystemMount[]
        {
            new SystemMount(ShipSystemType.MissileBattery, 1),
            new SystemMount(ShipSystemType.MissileMagazine, 7),
            new SystemMount(ShipSystemType.PDTurret, 13)
        });

        // Wire up references using SerializedObject
        SerializedObject damageRouterSO = new SerializedObject(damageRouter);
        damageRouterSO.FindProperty("shieldSystem").objectReferenceValue = shieldSystem;
        damageRouterSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        damageRouterSO.FindProperty("parentShip").objectReferenceValue = ship;
        damageRouterSO.ApplyModifiedPropertiesWithoutUndo();

        // Initialize shield system
        SerializedObject shieldSO = new SerializedObject(shieldSystem);
        shieldSO.FindProperty("maxShields").floatValue = 500f;
        shieldSO.FindProperty("currentShields").floatValue = 500f;
        shieldSO.FindProperty("rechargeRate").floatValue = 10f;
        shieldSO.FindProperty("rechargeDelay").floatValue = 5f;
        shieldSO.ApplyModifiedPropertiesWithoutUndo();

        // Wire CriticalHitSystem
        SerializedObject critSO = new SerializedObject(criticalHitSystem);
        critSO.FindProperty("logCriticalRolls").boolValue = true;
        critSO.ApplyModifiedPropertiesWithoutUndo();

        // Create UI controller
        GameObject uiObj = new GameObject("CriticalHitUI");
        uiObj.transform.SetParent(testRoot.transform);

        CriticalHitTestController testController = uiObj.AddComponent<CriticalHitTestController>();
        SlotLayoutVisualizer slotVisualizer = uiObj.AddComponent<SlotLayoutVisualizer>();

        // Wire UI references
        SerializedObject controllerSO = new SerializedObject(testController);
        controllerSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        controllerSO.FindProperty("criticalHitSystem").objectReferenceValue = criticalHitSystem;
        controllerSO.FindProperty("slotVisualizer").objectReferenceValue = slotVisualizer;
        controllerSO.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject visualizerSO = new SerializedObject(slotVisualizer);
        visualizerSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        visualizerSO.ApplyModifiedPropertiesWithoutUndo();

        // Position camera
        Camera.main.transform.position = new Vector3(0, 10, -15);
        Camera.main.transform.rotation = Quaternion.Euler(30, 0, 0);

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(scene);

        // Save scene
        string scenePath = "Assets/Scenes/CriticalHitTestScene.unity";
        EnsureDirectoryExists("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);

        Debug.Log($"Critical Hit Test Scene created at {scenePath}");
        Debug.Log("Enter Play mode and use the GUI to test critical hits.");
        Debug.Log("Instructions: Use GUI panel on left to select sections, roll criticals, and force hit systems.");

        // Select the test controller
        Selection.activeGameObject = uiObj;
    }

    private struct SystemMount
    {
        public ShipSystemType Type;
        public int StartSlot;

        public SystemMount(ShipSystemType type, int startSlot)
        {
            Type = type;
            StartSlot = startSlot;
        }
    }

    private static void CreateSectionWithSystems(Transform parent, SectionType type, Ship ship, SectionManager sectionManager, SystemMount[] mounts)
    {
        GameObject sectionObj = new GameObject($"Section_{type}");
        sectionObj.transform.SetParent(parent);

        // Position sections spatially
        float xOffset = 0f;
        float zOffset = 0f;
        switch (type)
        {
            case SectionType.Fore: zOffset = 3f; break;
            case SectionType.Aft: zOffset = -3f; break;
            case SectionType.Port: xOffset = -2f; break;
            case SectionType.Starboard: xOffset = 2f; break;
        }
        sectionObj.transform.localPosition = new Vector3(xOffset, 0, zOffset);

        ShipSection section = sectionObj.AddComponent<ShipSection>();

        // Initialize section (this creates the SlotLayout)
        section.Initialize(type, ship);

        // Register with section manager
        sectionManager.RegisterSection(section);

        // Mount systems - need to create as MonoBehaviour components
        if (section.SlotLayout != null)
        {
            foreach (var mount in mounts)
            {
                // Create a child object for the system
                GameObject systemObj = new GameObject($"System_{mount.Type}");
                systemObj.transform.SetParent(sectionObj.transform);

                MountedSystem system = systemObj.AddComponent<MountedSystem>();
                int size = ShipSystemData.GetSize(mount.Type);

                // Initialize the system
                system.Initialize(mount.Type, mount.StartSlot, section, ship);

                // Add to layout
                section.SlotLayout.AddSystem(system);
            }
        }
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string newPath = currentPath + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }
                currentPath = newPath;
            }
        }
    }
}
