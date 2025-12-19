using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor utility for setting up a Core Protection test scene.
/// Creates a scene with a ship, all sections, and Core protection system.
/// </summary>
public class CoreProtectionTestSetup
{
    [MenuItem("Hephaestus/Testing/Create Core Protection Test Scene")]
    public static void CreateCoreProtectionTestScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create main test object
        GameObject testRoot = new GameObject("CoreProtectionTest");

        // Create ship with full infrastructure
        GameObject shipObj = CreateShipWithSections(testRoot.transform);
        Ship ship = shipObj.GetComponent<Ship>();
        SectionManager sectionManager = shipObj.GetComponent<SectionManager>();
        CoreProtectionSystem coreProtection = shipObj.GetComponent<CoreProtectionSystem>();
        ShipDeathController deathController = shipObj.GetComponent<ShipDeathController>();

        // Create UI controllers
        GameObject coreTestUI = new GameObject("CoreProtectionTestUI");
        coreTestUI.transform.SetParent(testRoot.transform);
        CoreProtectionTestController coreTestController = coreTestUI.AddComponent<CoreProtectionTestController>();

        GameObject deathTestUI = new GameObject("ShipDeathTestUI");
        deathTestUI.transform.SetParent(testRoot.transform);
        ShipDeathTestController deathTestController = deathTestUI.AddComponent<ShipDeathTestController>();

        // Wire Core Protection Test UI references
        SerializedObject coreControllerSO = new SerializedObject(coreTestController);
        coreControllerSO.FindProperty("testShip").objectReferenceValue = ship;
        coreControllerSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        coreControllerSO.FindProperty("coreProtection").objectReferenceValue = coreProtection;
        coreControllerSO.FindProperty("damageRouter").objectReferenceValue = shipObj.GetComponent<DamageRouter>();
        coreControllerSO.ApplyModifiedPropertiesWithoutUndo();

        // Wire Ship Death Test UI references
        SerializedObject deathControllerSO = new SerializedObject(deathTestController);
        deathControllerSO.FindProperty("testShip").objectReferenceValue = ship;
        deathControllerSO.FindProperty("deathController").objectReferenceValue = deathController;
        deathControllerSO.FindProperty("degradationManager").objectReferenceValue = shipObj.GetComponent<SystemDegradationManager>();
        deathControllerSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        deathControllerSO.ApplyModifiedPropertiesWithoutUndo();

        // Position camera
        Camera.main.transform.position = new Vector3(0, 20, -25);
        Camera.main.transform.rotation = Quaternion.Euler(40, 0, 0);

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(scene);

        // Save scene
        string scenePath = "Assets/Scenes/CoreProtectionTestScene.unity";
        EnsureDirectoryExists("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);

        Debug.Log($"Core Protection Test Scene created at {scenePath}");
        Debug.Log("Enter Play mode to test Core protection and ship death mechanics.");
        Debug.Log("Instructions:");
        Debug.Log("  1-6: Breach specific sections (Fore/Aft/Port/Starboard/Dorsal/Ventral)");
        Debug.Log("  C: Attack Core directly");
        Debug.Log("  Arrow Keys: Change attack direction");
        Debug.Log("  L: Force lucky shot");
        Debug.Log("  W: Destroy all weapons");
        Debug.Log("  E: Destroy all engines");
        Debug.Log("  X: Destroy reactor (instant death)");
        Debug.Log("  R: Reset ship");

        // Select the test controller
        Selection.activeGameObject = coreTestUI;
    }

    private static GameObject CreateShipWithSections(Transform parent)
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

        // Add Core protection and death controller
        CoreProtectionSystem coreProtection = shipObj.AddComponent<CoreProtectionSystem>();
        ShipDeathController deathController = shipObj.AddComponent<ShipDeathController>();

        // Wire references
        SerializedObject degradationSO = new SerializedObject(degradationManager);
        degradationSO.FindProperty("ship").objectReferenceValue = ship;
        degradationSO.FindProperty("heatManager").objectReferenceValue = heatManager;
        degradationSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        degradationSO.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject coreProtectionSO = new SerializedObject(coreProtection);
        coreProtectionSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        coreProtectionSO.FindProperty("parentShip").objectReferenceValue = ship;
        coreProtectionSO.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject deathControllerSO = new SerializedObject(deathController);
        deathControllerSO.FindProperty("ship").objectReferenceValue = ship;
        deathControllerSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        deathControllerSO.FindProperty("degradationManager").objectReferenceValue = degradationManager;
        deathControllerSO.ApplyModifiedPropertiesWithoutUndo();

        // Create a simple visual for the ship (centered cube)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(shipObj.transform);
        visual.transform.localScale = new Vector3(4, 1f, 6);
        visual.name = "ShipVisual";
        visual.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.5f);

        // Create all 7 sections (6 outer + Core)
        CreateSection(shipObj.transform, SectionType.Core, ship, sectionManager, degradationManager, Vector3.zero);
        CreateSection(shipObj.transform, SectionType.Fore, ship, sectionManager, degradationManager, new Vector3(0, 0, 4));
        CreateSection(shipObj.transform, SectionType.Aft, ship, sectionManager, degradationManager, new Vector3(0, 0, -4));
        CreateSection(shipObj.transform, SectionType.Port, ship, sectionManager, degradationManager, new Vector3(-3, 0, 0));
        CreateSection(shipObj.transform, SectionType.Starboard, ship, sectionManager, degradationManager, new Vector3(3, 0, 0));
        CreateSection(shipObj.transform, SectionType.Dorsal, ship, sectionManager, degradationManager, new Vector3(0, 2, 0));
        CreateSection(shipObj.transform, SectionType.Ventral, ship, sectionManager, degradationManager, new Vector3(0, -2, 0));

        // Wire up damage infrastructure
        SerializedObject damageRouterSO = new SerializedObject(damageRouter);
        damageRouterSO.FindProperty("shieldSystem").objectReferenceValue = shieldSystem;
        damageRouterSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        damageRouterSO.FindProperty("parentShip").objectReferenceValue = ship;
        damageRouterSO.FindProperty("coreProtection").objectReferenceValue = coreProtection;
        damageRouterSO.ApplyModifiedPropertiesWithoutUndo();

        // Initialize shield system
        SerializedObject shieldSO = new SerializedObject(shieldSystem);
        shieldSO.FindProperty("maxShields").floatValue = 200f;
        shieldSO.FindProperty("currentShields").floatValue = 200f;
        shieldSO.ApplyModifiedPropertiesWithoutUndo();

        return shipObj;
    }

    private static void CreateSection(Transform parent, SectionType type, Ship ship, SectionManager sectionManager,
        SystemDegradationManager degradationManager, Vector3 position)
    {
        GameObject sectionObj = new GameObject($"Section_{type}");
        sectionObj.transform.SetParent(parent);
        sectionObj.transform.localPosition = position;

        ShipSection section = sectionObj.AddComponent<ShipSection>();
        section.Initialize(type, ship);
        sectionManager.RegisterSection(section);

        // Add visual representation
        GameObject sectionVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sectionVisual.transform.SetParent(sectionObj.transform);
        sectionVisual.transform.localPosition = Vector3.zero;
        sectionVisual.transform.localScale = Vector3.one * 1.5f;
        sectionVisual.name = "SectionVisual";

        // Color based on section type
        Color sectionColor = type switch
        {
            SectionType.Core => Color.red,
            SectionType.Fore => Color.green,
            SectionType.Aft => Color.blue,
            SectionType.Port => Color.yellow,
            SectionType.Starboard => Color.cyan,
            SectionType.Dorsal => Color.magenta,
            SectionType.Ventral => new Color(1f, 0.5f, 0f),
            _ => Color.white
        };
        sectionVisual.GetComponent<Renderer>().material.color = sectionColor;

        // Add mounted systems based on section type
        if (section.SlotLayout != null)
        {
            switch (type)
            {
                case SectionType.Core:
                    AddMountedSystem<MountedReactor>(sectionObj.transform, ShipSystemType.ReactorCore, 1, section, ship, degradationManager);
                    AddMountedSystem<MountedEngine>(sectionObj.transform, ShipSystemType.MainEngine, 21, section, ship, degradationManager);
                    break;
                case SectionType.Fore:
                    AddMountedSystem<MountedSensors>(sectionObj.transform, ShipSystemType.Sensors, 1, section, ship, degradationManager);
                    break;
                case SectionType.Port:
                case SectionType.Starboard:
                    AddMountedSystem<MountedWeapon>(sectionObj.transform, ShipSystemType.NewtonianCannon, 1, section, ship, degradationManager);
                    AddMountedSystem<MountedPDTurret>(sectionObj.transform, ShipSystemType.PDTurret, 9, section, ship, degradationManager);
                    break;
            }
        }
    }

    private static T AddMountedSystem<T>(Transform parent, ShipSystemType systemType, int slot,
        ShipSection section, Ship ship, SystemDegradationManager degradationManager) where T : MountedSystem
    {
        GameObject systemObj = new GameObject($"Mounted_{systemType}");
        systemObj.transform.SetParent(parent);

        T system = systemObj.AddComponent<T>();
        system.Initialize(systemType, slot, section, ship);

        if (section.SlotLayout != null)
        {
            section.SlotLayout.AddSystem(system);
        }

        if (degradationManager != null)
        {
            degradationManager.RegisterSystem(system);
        }

        // Configure specific system types
        if (system is MountedReactor reactor)
        {
            reactor.SetLinkedReferences(ship, ship.GetComponent<HeatManager>());
        }

        return system;
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
