using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor utility for setting up a System Degradation test scene.
/// Creates a scene with a ship, all system types, and degradation manager.
/// </summary>
public class DegradationTestSetup
{
    [MenuItem("Deadlock/Test Scenes/Create Degradation Test Scene")]
    public static void CreateDegradationTestScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create main test object
        GameObject testRoot = new GameObject("DegradationTest");

        // Create ship with full infrastructure
        GameObject shipObj = CreateShipWithSystems(testRoot.transform);
        Ship ship = shipObj.GetComponent<Ship>();
        SystemDegradationManager degradationManager = shipObj.GetComponent<SystemDegradationManager>();

        // Create target ship (for weapon testing)
        GameObject targetShip = CreateTargetShip(testRoot.transform);
        targetShip.transform.position = new Vector3(0, 0, 15);

        // Create UI controller
        GameObject uiObj = new GameObject("DegradationTestUI");
        uiObj.transform.SetParent(testRoot.transform);

        DegradationTestController testController = uiObj.AddComponent<DegradationTestController>();

        // Wire UI references
        SerializedObject controllerSO = new SerializedObject(testController);
        controllerSO.FindProperty("testShip").objectReferenceValue = ship;
        controllerSO.FindProperty("targetShip").objectReferenceValue = targetShip.GetComponent<Ship>();
        controllerSO.FindProperty("degradationManager").objectReferenceValue = degradationManager;
        controllerSO.ApplyModifiedPropertiesWithoutUndo();

        // Position camera
        Camera.main.transform.position = new Vector3(0, 15, -20);
        Camera.main.transform.rotation = Quaternion.Euler(35, 0, 0);

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(scene);

        // Save scene
        string scenePath = "Assets/Scenes/DegradationTestScene.unity";
        EnsureDirectoryExists("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);

        Debug.Log($"Degradation Test Scene created at {scenePath}");
        Debug.Log("Enter Play mode to test system degradation effects.");
        Debug.Log("Instructions:");
        Debug.Log("  Tab: Cycle through systems");
        Debug.Log("  D: Damage selected system");
        Debug.Log("  K: Kill (destroy) selected system");
        Debug.Log("  R: Repair selected system");
        Debug.Log("  F1-F7: Quick select by system type");

        // Select the test controller
        Selection.activeGameObject = uiObj;
    }

    private static GameObject CreateShipWithSystems(Transform parent)
    {
        GameObject shipObj = new GameObject("TestShip");
        shipObj.transform.SetParent(parent);

        // Add core ship components
        Ship ship = shipObj.AddComponent<Ship>();
        HeatManager heatManager = shipObj.AddComponent<HeatManager>();
        WeaponManager weaponManager = shipObj.AddComponent<WeaponManager>();

        // Add damage infrastructure
        DamageRouter damageRouter = shipObj.AddComponent<DamageRouter>();
        ShieldSystem shieldSystem = shipObj.AddComponent<ShieldSystem>();
        SectionManager sectionManager = shipObj.AddComponent<SectionManager>();
        CriticalHitSystem criticalHitSystem = shipObj.AddComponent<CriticalHitSystem>();

        // Add degradation manager
        SystemDegradationManager degradationManager = shipObj.AddComponent<SystemDegradationManager>();

        // Wire degradation manager
        SerializedObject degradationSO = new SerializedObject(degradationManager);
        degradationSO.FindProperty("ship").objectReferenceValue = ship;
        degradationSO.FindProperty("heatManager").objectReferenceValue = heatManager;
        degradationSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        degradationSO.ApplyModifiedPropertiesWithoutUndo();

        // Create a simple visual for the ship
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(shipObj.transform);
        visual.transform.localScale = new Vector3(2, 0.5f, 4);
        visual.name = "ShipVisual";

        // Create Core section with mounted systems
        CreateSectionWithMountedSystems(shipObj.transform, SectionType.Core, ship, sectionManager, new MountedSystemConfig[]
        {
            new MountedSystemConfig(typeof(MountedReactor), ShipSystemType.ReactorCore, 1),
            new MountedSystemConfig(typeof(MountedEngine), ShipSystemType.MainEngine, 21),
            new MountedSystemConfig(typeof(MountedRadiator), ShipSystemType.Radiator, 36)
        });

        // Create Fore section
        CreateSectionWithMountedSystems(shipObj.transform, SectionType.Fore, ship, sectionManager, new MountedSystemConfig[]
        {
            new MountedSystemConfig(typeof(MountedSensors), ShipSystemType.Sensors, 1),
            new MountedSystemConfig(typeof(MountedMagazine), ShipSystemType.TorpedoMagazine, 7)
        });

        // Create Port section with weapons
        CreateSectionWithMountedSystems(shipObj.transform, SectionType.Port, ship, sectionManager, new MountedSystemConfig[]
        {
            new MountedSystemConfig(typeof(MountedWeapon), ShipSystemType.NewtonianCannon, 1),
            new MountedSystemConfig(typeof(MountedPDTurret), ShipSystemType.PDTurret, 9),
            new MountedSystemConfig(typeof(MountedRadiator), ShipSystemType.Radiator, 13)
        });

        // Create Starboard section
        CreateSectionWithMountedSystems(shipObj.transform, SectionType.Starboard, ship, sectionManager, new MountedSystemConfig[]
        {
            new MountedSystemConfig(typeof(MountedWeapon), ShipSystemType.NewtonianCannon, 1),
            new MountedSystemConfig(typeof(MountedPDTurret), ShipSystemType.PDTurret, 9),
            new MountedSystemConfig(typeof(MountedRadiator), ShipSystemType.Radiator, 13)
        });

        // Create Aft section with missile systems
        CreateSectionWithMountedSystems(shipObj.transform, SectionType.Aft, ship, sectionManager, new MountedSystemConfig[]
        {
            new MountedSystemConfig(typeof(MountedMagazine), ShipSystemType.MissileMagazine, 1),
            new MountedSystemConfig(typeof(MountedPDTurret), ShipSystemType.PDTurret, 7)
        });

        // Wire up damage infrastructure
        SerializedObject damageRouterSO = new SerializedObject(damageRouter);
        damageRouterSO.FindProperty("shieldSystem").objectReferenceValue = shieldSystem;
        damageRouterSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        damageRouterSO.FindProperty("parentShip").objectReferenceValue = ship;
        damageRouterSO.ApplyModifiedPropertiesWithoutUndo();

        // Initialize shield system
        SerializedObject shieldSO = new SerializedObject(shieldSystem);
        shieldSO.FindProperty("maxShields").floatValue = 500f;
        shieldSO.FindProperty("currentShields").floatValue = 500f;
        shieldSO.ApplyModifiedPropertiesWithoutUndo();

        return shipObj;
    }

    private struct MountedSystemConfig
    {
        public System.Type ComponentType;
        public ShipSystemType SystemType;
        public int StartSlot;

        public MountedSystemConfig(System.Type componentType, ShipSystemType systemType, int startSlot)
        {
            ComponentType = componentType;
            SystemType = systemType;
            StartSlot = startSlot;
        }
    }

    private static void CreateSectionWithMountedSystems(Transform parent, SectionType type, Ship ship, SectionManager sectionManager, MountedSystemConfig[] configs)
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

        // Create mounted systems
        if (section.SlotLayout != null)
        {
            foreach (var config in configs)
            {
                // Create a child object for the system
                GameObject systemObj = new GameObject($"Mounted_{config.SystemType}");
                systemObj.transform.SetParent(sectionObj.transform);

                // Add the specific MountedSystem subclass
                MountedSystem system = (MountedSystem)systemObj.AddComponent(config.ComponentType);

                // Initialize the system
                system.Initialize(config.SystemType, config.StartSlot, section, ship);

                // Add to layout
                section.SlotLayout.AddSystem(system);

                // Configure specific system types
                if (system is MountedReactor reactor)
                {
                    SerializedObject reactorSO = new SerializedObject(reactor);
                    reactorSO.FindProperty("linkedShip").objectReferenceValue = ship;
                    reactorSO.FindProperty("linkedHeatManager").objectReferenceValue = ship.GetComponent<HeatManager>();
                    reactorSO.ApplyModifiedPropertiesWithoutUndo();
                }

                if (system is MountedMagazine magazine)
                {
                    // Set magazine type based on system type
                    SerializedObject magSO = new SerializedObject(magazine);
                    MountedMagazine.MagazineType magType = config.SystemType == ShipSystemType.TorpedoMagazine
                        ? MountedMagazine.MagazineType.Torpedo
                        : MountedMagazine.MagazineType.Missile;
                    magSO.FindProperty("magazineType").enumValueIndex = (int)magType;
                    magSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }
    }

    private static GameObject CreateTargetShip(Transform parent)
    {
        GameObject targetObj = new GameObject("TargetShip");
        targetObj.transform.SetParent(parent);

        Ship targetShip = targetObj.AddComponent<Ship>();

        // Create visual
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(targetObj.transform);
        visual.transform.localScale = new Vector3(2, 0.5f, 4);
        visual.name = "TargetVisual";

        // Make it red
        visual.GetComponent<Renderer>().material.color = Color.red;

        return targetObj;
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
