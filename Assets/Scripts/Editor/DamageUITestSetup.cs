using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to create a test scene for Damage UI.
/// Menu: "Hephaestus/Testing/Create Damage UI Test Scene"
/// </summary>
public class DamageUITestSetup : Editor
{
    [MenuItem("Hephaestus/Testing/Phase 3 - Damage/Create Damage UI Test Scene")]
    public static void CreateDamageUITestScene()
    {
        Debug.Log("=== Creating Damage UI Test Scene ===");

        // Create root object
        GameObject testRoot = new GameObject("DamageUITestScene");

        // Create ship with all damage components
        GameObject shipObj = CreateTestShip(testRoot);
        Ship ship = shipObj.GetComponent<Ship>();

        // Create UI Manager with damage UI
        GameObject uiManagerObj = new GameObject("UIManager");
        uiManagerObj.transform.SetParent(testRoot.transform);
        UIManager uiManager = uiManagerObj.AddComponent<UIManager>();

        // Create DamageUIManager
        GameObject damageUIObj = new GameObject("DamageUIManager");
        damageUIObj.transform.SetParent(uiManagerObj.transform);
        DamageUIManager damageUIManager = damageUIObj.AddComponent<DamageUIManager>();

        // Create UI panels
        GameObject sectionPanelObj = new GameObject("SectionStatusPanel");
        sectionPanelObj.transform.SetParent(damageUIObj.transform);
        SectionStatusPanel sectionPanel = sectionPanelObj.AddComponent<SectionStatusPanel>();

        GameObject detailPopupObj = new GameObject("SectionDetailPopup");
        detailPopupObj.transform.SetParent(damageUIObj.transform);
        SectionDetailPopup detailPopup = detailPopupObj.AddComponent<SectionDetailPopup>();

        GameObject combatLogObj = new GameObject("CombatLogPanel");
        combatLogObj.transform.SetParent(damageUIObj.transform);
        CombatLogPanel combatLog = combatLogObj.AddComponent<CombatLogPanel>();

        GameObject shieldBarObj = new GameObject("ShieldStatusBar");
        shieldBarObj.transform.SetParent(damageUIObj.transform);
        ShieldStatusBar shieldBar = shieldBarObj.AddComponent<ShieldStatusBar>();

        // Wire references using SerializedObject
        SerializedObject damageUISerializedObj = new SerializedObject(damageUIManager);
        damageUISerializedObj.FindProperty("sectionStatusPanel").objectReferenceValue = sectionPanel;
        damageUISerializedObj.FindProperty("sectionDetailPopup").objectReferenceValue = detailPopup;
        damageUISerializedObj.FindProperty("combatLogPanel").objectReferenceValue = combatLog;
        damageUISerializedObj.FindProperty("shieldStatusBar").objectReferenceValue = shieldBar;
        damageUISerializedObj.FindProperty("playerShip").objectReferenceValue = ship;
        damageUISerializedObj.FindProperty("autoCreateComponents").boolValue = false;
        damageUISerializedObj.ApplyModifiedProperties();

        // Create test controller
        GameObject controllerObj = new GameObject("DamageUITestController");
        controllerObj.transform.SetParent(testRoot.transform);
        DamageUITestController controller = controllerObj.AddComponent<DamageUITestController>();

        // Wire controller references
        SerializedObject controllerSerializedObj = new SerializedObject(controller);
        controllerSerializedObj.FindProperty("testShip").objectReferenceValue = ship;
        controllerSerializedObj.FindProperty("damageUIManager").objectReferenceValue = damageUIManager;
        controllerSerializedObj.FindProperty("sectionManager").objectReferenceValue = ship.GetComponent<SectionManager>();
        controllerSerializedObj.FindProperty("shieldSystem").objectReferenceValue = ship.GetComponent<ShieldSystem>();
        controllerSerializedObj.FindProperty("damageRouter").objectReferenceValue = ship.GetComponent<DamageRouter>();
        controllerSerializedObj.FindProperty("combatLog").objectReferenceValue = combatLog;
        controllerSerializedObj.ApplyModifiedProperties();

        // Create camera
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.transform.SetParent(testRoot.transform);
        cameraObj.transform.position = new Vector3(0, 5, -10);
        cameraObj.transform.rotation = Quaternion.Euler(20, 0, 0);
        cameraObj.AddComponent<Camera>();
        cameraObj.tag = "MainCamera";

        // Select the test root
        Selection.activeGameObject = testRoot;

        Debug.Log("=== Damage UI Test Scene Created ===");
        Debug.Log("Press Play to test. UI panels will appear on screen.");
        Debug.Log("Use the test controller buttons to simulate damage events.");
    }

    private static GameObject CreateTestShip(GameObject parent)
    {
        GameObject shipObj = new GameObject("TestShip");
        shipObj.transform.SetParent(parent.transform);

        // Add visual cube
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(shipObj.transform);
        visual.transform.localScale = new Vector3(2f, 0.5f, 3f);

        // Core components
        Ship ship = shipObj.AddComponent<Ship>();
        HeatManager heatManager = shipObj.AddComponent<HeatManager>();
        SectionManager sectionManager = shipObj.AddComponent<SectionManager>();
        ShieldSystem shieldSystem = shipObj.AddComponent<ShieldSystem>();
        DamageRouter damageRouter = shipObj.AddComponent<DamageRouter>();
        CoreProtectionSystem coreProtection = shipObj.AddComponent<CoreProtectionSystem>();
        ShipDeathController deathController = shipObj.AddComponent<ShipDeathController>();
        SystemDegradationManager degradationManager = shipObj.AddComponent<SystemDegradationManager>();
        CriticalHitSystem criticalHitSystem = shipObj.AddComponent<CriticalHitSystem>();

        // Create all 7 sections
        CreateSection(shipObj, SectionType.Core, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Fore, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Aft, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Port, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Starboard, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Dorsal, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Ventral, ship, criticalHitSystem);

        // Create sample systems for testing
        CreateEngine(shipObj, sectionManager, degradationManager);
        CreateWeapon(shipObj, sectionManager, degradationManager);
        CreateReactor(shipObj, sectionManager, degradationManager, heatManager);

        // Wire references
        SerializedObject shieldSerializedObj = new SerializedObject(shieldSystem);
        shieldSerializedObj.FindProperty("maxShields").floatValue = 200f;
        shieldSerializedObj.FindProperty("currentShields").floatValue = 200f;
        shieldSerializedObj.ApplyModifiedProperties();

        SerializedObject sectionManagerSerializedObj = new SerializedObject(sectionManager);
        sectionManagerSerializedObj.FindProperty("parentShip").objectReferenceValue = ship;
        sectionManagerSerializedObj.ApplyModifiedProperties();

        SerializedObject coreProtectionSerializedObj = new SerializedObject(coreProtection);
        coreProtectionSerializedObj.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        coreProtectionSerializedObj.FindProperty("parentShip").objectReferenceValue = ship;
        coreProtectionSerializedObj.ApplyModifiedProperties();

        SerializedObject damageRouterSerializedObj = new SerializedObject(damageRouter);
        damageRouterSerializedObj.FindProperty("shieldSystem").objectReferenceValue = shieldSystem;
        damageRouterSerializedObj.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        damageRouterSerializedObj.FindProperty("parentShip").objectReferenceValue = ship;
        damageRouterSerializedObj.FindProperty("coreProtection").objectReferenceValue = coreProtection;
        damageRouterSerializedObj.ApplyModifiedProperties();

        SerializedObject deathControllerSerializedObj = new SerializedObject(deathController);
        deathControllerSerializedObj.FindProperty("ship").objectReferenceValue = ship;
        deathControllerSerializedObj.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        deathControllerSerializedObj.FindProperty("degradationManager").objectReferenceValue = degradationManager;
        deathControllerSerializedObj.ApplyModifiedProperties();

        SerializedObject degradationSerializedObj = new SerializedObject(degradationManager);
        degradationSerializedObj.FindProperty("ship").objectReferenceValue = ship;
        degradationSerializedObj.FindProperty("heatManager").objectReferenceValue = heatManager;
        degradationSerializedObj.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        degradationSerializedObj.ApplyModifiedProperties();

        return shipObj;
    }

    private static void CreateSection(GameObject shipObj, SectionType type, Ship ship, CriticalHitSystem critSystem)
    {
        GameObject sectionObj = new GameObject($"Section_{type}");
        sectionObj.transform.SetParent(shipObj.transform);

        ShipSection section = sectionObj.AddComponent<ShipSection>();

        SerializedObject sectionSerializedObj = new SerializedObject(section);
        sectionSerializedObj.FindProperty("sectionType").enumValueIndex = (int)type;
        sectionSerializedObj.FindProperty("parentShip").objectReferenceValue = ship;
        sectionSerializedObj.FindProperty("criticalHitSystem").objectReferenceValue = critSystem;

        // Set armor/structure based on type
        var config = SectionDefinitions.GetConfig(type);
        sectionSerializedObj.FindProperty("maxArmor").floatValue = config.Armor;
        sectionSerializedObj.FindProperty("currentArmor").floatValue = config.Armor;
        sectionSerializedObj.FindProperty("maxStructure").floatValue = config.Structure;
        sectionSerializedObj.FindProperty("currentStructure").floatValue = config.Structure;
        sectionSerializedObj.ApplyModifiedProperties();
    }

    private static void CreateEngine(GameObject shipObj, SectionManager sectionManager, SystemDegradationManager degradationManager)
    {
        GameObject engineObj = new GameObject("MainEngine");
        engineObj.transform.SetParent(shipObj.transform);
        MountedEngine engine = engineObj.AddComponent<MountedEngine>();

        SerializedObject engineSerializedObj = new SerializedObject(engine);
        engineSerializedObj.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.MainEngine;
        engineSerializedObj.FindProperty("slotSize").intValue = 15;
        engineSerializedObj.ApplyModifiedProperties();
    }

    private static void CreateWeapon(GameObject shipObj, SectionManager sectionManager, SystemDegradationManager degradationManager)
    {
        GameObject weaponObj = new GameObject("NewtonianCannon");
        weaponObj.transform.SetParent(shipObj.transform);
        MountedWeapon weapon = weaponObj.AddComponent<MountedWeapon>();

        SerializedObject weaponSerializedObj = new SerializedObject(weapon);
        weaponSerializedObj.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.NewtonianCannon;
        weaponSerializedObj.FindProperty("slotSize").intValue = 8;
        weaponSerializedObj.ApplyModifiedProperties();
    }

    private static void CreateReactor(GameObject shipObj, SectionManager sectionManager, SystemDegradationManager degradationManager, HeatManager heatManager)
    {
        GameObject reactorObj = new GameObject("ReactorCore");
        reactorObj.transform.SetParent(shipObj.transform);
        MountedReactor reactor = reactorObj.AddComponent<MountedReactor>();

        SerializedObject reactorSerializedObj = new SerializedObject(reactor);
        reactorSerializedObj.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.ReactorCore;
        reactorSerializedObj.FindProperty("slotSize").intValue = 20;
        reactorSerializedObj.ApplyModifiedProperties();
    }
}
