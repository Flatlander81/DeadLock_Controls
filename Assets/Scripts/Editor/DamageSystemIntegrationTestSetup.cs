using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to create a full integration test scene for the damage system.
/// Creates a complete test environment with ships, weapons, projectiles, and UI.
/// Menu: "Hephaestus/Testing/Create Damage System Integration Test Scene"
/// </summary>
public class DamageSystemIntegrationTestSetup : Editor
{
    [MenuItem("Hephaestus/Testing/Phase 3 - Damage/Create Damage System Integration Test Scene")]
    public static void CreateIntegrationTestScene()
    {
        Debug.Log("=== Creating Damage System Integration Test Scene ===");

        // Create root object
        GameObject testRoot = new GameObject("DamageSystemIntegrationTest");

        // Create player ship
        GameObject playerShipObj = CreateFullShip(testRoot, "PlayerShip", new Vector3(0, 0, 0), true);
        Ship playerShip = playerShipObj.GetComponent<Ship>();

        // Create enemy ship
        GameObject enemyShipObj = CreateFullShip(testRoot, "EnemyShip", new Vector3(20, 0, 0), false);
        Ship enemyShip = enemyShipObj.GetComponent<Ship>();

        // Create projectile manager
        GameObject projectileManagerObj = new GameObject("ProjectileManager");
        projectileManagerObj.transform.SetParent(testRoot.transform);
        ProjectileManager projectileManager = projectileManagerObj.AddComponent<ProjectileManager>();

        // Create targeting controller
        GameObject targetingObj = new GameObject("TargetingController");
        targetingObj.transform.SetParent(testRoot.transform);
        TargetingController targetingController = targetingObj.AddComponent<TargetingController>();

        // Create UI Manager with all UI
        GameObject uiManagerObj = new GameObject("UIManager");
        uiManagerObj.transform.SetParent(testRoot.transform);
        UIManager uiManager = uiManagerObj.AddComponent<UIManager>();

        // Create DamageUIManager
        GameObject damageUIObj = new GameObject("DamageUIManager");
        damageUIObj.transform.SetParent(uiManagerObj.transform);
        DamageUIManager damageUIManager = damageUIObj.AddComponent<DamageUIManager>();

        // Create all damage UI panels
        CreateDamageUIPanels(damageUIObj, damageUIManager, playerShip);

        // Create weapon panels
        GameObject weaponConfigObj = new GameObject("WeaponConfigPanel");
        weaponConfigObj.transform.SetParent(uiManagerObj.transform);
        WeaponConfigPanel weaponConfig = weaponConfigObj.AddComponent<WeaponConfigPanel>();

        GameObject weaponGroupObj = new GameObject("WeaponGroupPanel");
        weaponGroupObj.transform.SetParent(uiManagerObj.transform);
        WeaponGroupPanel weaponGroup = weaponGroupObj.AddComponent<WeaponGroupPanel>();

        // Wire UI Manager references
        SerializedObject uiManagerSO = new SerializedObject(uiManager);
        uiManagerSO.FindProperty("weaponConfigPanel").objectReferenceValue = weaponConfig;
        uiManagerSO.FindProperty("weaponGroupPanel").objectReferenceValue = weaponGroup;
        uiManagerSO.FindProperty("damageUIManager").objectReferenceValue = damageUIManager;
        uiManagerSO.FindProperty("targetingController").objectReferenceValue = targetingController;
        uiManagerSO.FindProperty("playerShip").objectReferenceValue = playerShip;
        uiManagerSO.ApplyModifiedProperties();

        // Create test controller
        GameObject controllerObj = new GameObject("FullCombatTestController");
        controllerObj.transform.SetParent(testRoot.transform);
        FullCombatTestController controller = controllerObj.AddComponent<FullCombatTestController>();

        // Wire controller references
        SerializedObject controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("playerShip").objectReferenceValue = playerShip;
        controllerSO.FindProperty("enemyShip").objectReferenceValue = enemyShip;
        controllerSO.FindProperty("projectileManager").objectReferenceValue = projectileManager;
        controllerSO.FindProperty("damageUIManager").objectReferenceValue = damageUIManager;
        controllerSO.FindProperty("targetingController").objectReferenceValue = targetingController;
        controllerSO.ApplyModifiedProperties();

        // Create camera
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.transform.SetParent(testRoot.transform);
        cameraObj.transform.position = new Vector3(10, 15, -20);
        cameraObj.transform.rotation = Quaternion.Euler(35, 0, 0);
        cameraObj.AddComponent<Camera>();
        cameraObj.tag = "MainCamera";

        // Create directional light
        GameObject lightObj = new GameObject("Directional Light");
        lightObj.transform.SetParent(testRoot.transform);
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;

        // Select the test root
        Selection.activeGameObject = testRoot;

        Debug.Log("=== Damage System Integration Test Scene Created ===");
        Debug.Log("Scene contains:");
        Debug.Log("  - Player Ship (with all systems)");
        Debug.Log("  - Enemy Ship (with all systems)");
        Debug.Log("  - ProjectileManager");
        Debug.Log("  - TargetingController");
        Debug.Log("  - Full UI (Damage UI, Weapon Config, Weapon Groups)");
        Debug.Log("  - FullCombatTestController");
        Debug.Log("Press Play and use the test controller to simulate combat.");
    }

    private static GameObject CreateFullShip(GameObject parent, string name, Vector3 position, bool isPlayer)
    {
        GameObject shipObj = new GameObject(name);
        shipObj.transform.SetParent(parent.transform);
        shipObj.transform.position = position;

        // Add visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(shipObj.transform);
        visual.transform.localScale = new Vector3(2f, 0.5f, 3f);

        // Color based on team
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = isPlayer ? new Color(0.2f, 0.4f, 0.8f) : new Color(0.8f, 0.2f, 0.2f);
            renderer.material = mat;
        }

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

        // Create mounted systems
        CreateEngine(shipObj, sectionManager, degradationManager);
        CreateWeapon(shipObj, sectionManager, degradationManager, "NewtonianCannon", ShipSystemType.NewtonianCannon);
        CreateWeapon(shipObj, sectionManager, degradationManager, "MissileBattery", ShipSystemType.MissileBattery);
        CreateReactor(shipObj, sectionManager, degradationManager, heatManager);

        // Wire component references
        SerializedObject shieldSO = new SerializedObject(shieldSystem);
        shieldSO.FindProperty("maxShields").floatValue = 200f;
        shieldSO.FindProperty("currentShields").floatValue = 200f;
        shieldSO.ApplyModifiedProperties();

        SerializedObject sectionManagerSO = new SerializedObject(sectionManager);
        sectionManagerSO.FindProperty("parentShip").objectReferenceValue = ship;
        sectionManagerSO.ApplyModifiedProperties();

        SerializedObject coreProtectionSO = new SerializedObject(coreProtection);
        coreProtectionSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        coreProtectionSO.FindProperty("parentShip").objectReferenceValue = ship;
        coreProtectionSO.ApplyModifiedProperties();

        SerializedObject damageRouterSO = new SerializedObject(damageRouter);
        damageRouterSO.FindProperty("shieldSystem").objectReferenceValue = shieldSystem;
        damageRouterSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        damageRouterSO.FindProperty("parentShip").objectReferenceValue = ship;
        damageRouterSO.FindProperty("coreProtection").objectReferenceValue = coreProtection;
        damageRouterSO.ApplyModifiedProperties();

        SerializedObject deathControllerSO = new SerializedObject(deathController);
        deathControllerSO.FindProperty("ship").objectReferenceValue = ship;
        deathControllerSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        deathControllerSO.FindProperty("degradationManager").objectReferenceValue = degradationManager;
        deathControllerSO.ApplyModifiedProperties();

        SerializedObject degradationSO = new SerializedObject(degradationManager);
        degradationSO.FindProperty("ship").objectReferenceValue = ship;
        degradationSO.FindProperty("heatManager").objectReferenceValue = heatManager;
        degradationSO.FindProperty("sectionManager").objectReferenceValue = sectionManager;
        degradationSO.ApplyModifiedProperties();

        return shipObj;
    }

    private static void CreateSection(GameObject shipObj, SectionType type, Ship ship, CriticalHitSystem critSystem)
    {
        GameObject sectionObj = new GameObject($"Section_{type}");
        sectionObj.transform.SetParent(shipObj.transform);

        ShipSection section = sectionObj.AddComponent<ShipSection>();

        SerializedObject sectionSO = new SerializedObject(section);
        sectionSO.FindProperty("sectionType").enumValueIndex = (int)type;
        sectionSO.FindProperty("parentShip").objectReferenceValue = ship;
        sectionSO.FindProperty("criticalHitSystem").objectReferenceValue = critSystem;

        var config = SectionDefinitions.GetConfig(type);
        sectionSO.FindProperty("maxArmor").floatValue = config.Armor;
        sectionSO.FindProperty("currentArmor").floatValue = config.Armor;
        sectionSO.FindProperty("maxStructure").floatValue = config.Structure;
        sectionSO.FindProperty("currentStructure").floatValue = config.Structure;
        sectionSO.ApplyModifiedProperties();
    }

    private static void CreateEngine(GameObject shipObj, SectionManager sectionManager, SystemDegradationManager degradationManager)
    {
        GameObject engineObj = new GameObject("MainEngine");
        engineObj.transform.SetParent(shipObj.transform);
        MountedEngine engine = engineObj.AddComponent<MountedEngine>();

        SerializedObject engineSO = new SerializedObject(engine);
        engineSO.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.MainEngine;
        engineSO.ApplyModifiedProperties();
    }

    private static void CreateWeapon(GameObject shipObj, SectionManager sectionManager, SystemDegradationManager degradationManager, string name, ShipSystemType weaponType)
    {
        GameObject weaponObj = new GameObject(name);
        weaponObj.transform.SetParent(shipObj.transform);
        MountedWeapon weapon = weaponObj.AddComponent<MountedWeapon>();

        SerializedObject weaponSO = new SerializedObject(weapon);
        weaponSO.FindProperty("systemType").enumValueIndex = (int)weaponType;
        weaponSO.ApplyModifiedProperties();
    }

    private static void CreateReactor(GameObject shipObj, SectionManager sectionManager, SystemDegradationManager degradationManager, HeatManager heatManager)
    {
        GameObject reactorObj = new GameObject("ReactorCore");
        reactorObj.transform.SetParent(shipObj.transform);
        MountedReactor reactor = reactorObj.AddComponent<MountedReactor>();

        SerializedObject reactorSO = new SerializedObject(reactor);
        reactorSO.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.ReactorCore;
        reactorSO.ApplyModifiedProperties();
    }

    private static void CreateDamageUIPanels(GameObject parent, DamageUIManager damageUIManager, Ship playerShip)
    {
        GameObject sectionPanelObj = new GameObject("SectionStatusPanel");
        sectionPanelObj.transform.SetParent(parent.transform);
        SectionStatusPanel sectionPanel = sectionPanelObj.AddComponent<SectionStatusPanel>();

        GameObject detailPopupObj = new GameObject("SectionDetailPopup");
        detailPopupObj.transform.SetParent(parent.transform);
        SectionDetailPopup detailPopup = detailPopupObj.AddComponent<SectionDetailPopup>();

        GameObject combatLogObj = new GameObject("CombatLogPanel");
        combatLogObj.transform.SetParent(parent.transform);
        CombatLogPanel combatLog = combatLogObj.AddComponent<CombatLogPanel>();

        GameObject shieldBarObj = new GameObject("ShieldStatusBar");
        shieldBarObj.transform.SetParent(parent.transform);
        ShieldStatusBar shieldBar = shieldBarObj.AddComponent<ShieldStatusBar>();

        SerializedObject damageUISO = new SerializedObject(damageUIManager);
        damageUISO.FindProperty("sectionStatusPanel").objectReferenceValue = sectionPanel;
        damageUISO.FindProperty("sectionDetailPopup").objectReferenceValue = detailPopup;
        damageUISO.FindProperty("combatLogPanel").objectReferenceValue = combatLog;
        damageUISO.FindProperty("shieldStatusBar").objectReferenceValue = shieldBar;
        damageUISO.FindProperty("playerShip").objectReferenceValue = playerShip;
        damageUISO.FindProperty("autoCreateComponents").boolValue = false;
        damageUISO.ApplyModifiedProperties();
    }
}
