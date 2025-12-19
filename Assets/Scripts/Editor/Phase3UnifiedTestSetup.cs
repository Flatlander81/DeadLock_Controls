using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to create a unified Phase 3 test scene with all damage system features.
/// Creates a complete test environment for testing shields, sections, criticals, degradation,
/// core protection, ship death, projectiles, and UI - all in one scene.
/// Menu: "Hephaestus/Testing/Create Phase 3 Unified Test Level"
/// </summary>
public class Phase3UnifiedTestSetup : Editor
{
    [MenuItem("Hephaestus/Testing/Create Phase 3 Unified Test Level")]
    public static void CreateUnifiedTestLevel()
    {
        Debug.Log("=== Creating Phase 3 Unified Test Level ===");

        // Create root object
        GameObject testRoot = new GameObject("Phase3UnifiedTestLevel");

        // Create player ship with all systems
        GameObject playerShipObj = CreateFullShipWithAllSystems(testRoot, "PlayerShip", new Vector3(0, 0, 0), true);
        Ship playerShip = playerShipObj.GetComponent<Ship>();

        // Create enemy ship with all systems
        GameObject enemyShipObj = CreateFullShipWithAllSystems(testRoot, "EnemyShip", new Vector3(25, 0, 0), false);
        Ship enemyShip = enemyShipObj.GetComponent<Ship>();

        // Create second enemy for multi-target testing
        GameObject enemy2Obj = CreateFullShipWithAllSystems(testRoot, "EnemyShip2", new Vector3(15, 0, 15), false);
        Ship enemy2 = enemy2Obj.GetComponent<Ship>();

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

        // Create unified test controller
        GameObject controllerObj = new GameObject("Phase3UnifiedTestController");
        controllerObj.transform.SetParent(testRoot.transform);
        Phase3UnifiedTestController controller = controllerObj.AddComponent<Phase3UnifiedTestController>();

        // Wire controller references
        SerializedObject controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("playerShip").objectReferenceValue = playerShip;
        controllerSO.FindProperty("projectileManager").objectReferenceValue = projectileManager;
        controllerSO.FindProperty("damageUIManager").objectReferenceValue = damageUIManager;
        controllerSO.FindProperty("targetingController").objectReferenceValue = targetingController;
        controllerSO.ApplyModifiedProperties();

        // Create camera with better viewing angle
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.transform.SetParent(testRoot.transform);
        cameraObj.transform.position = new Vector3(12, 20, -25);
        cameraObj.transform.rotation = Quaternion.Euler(40, 0, 0);
        cameraObj.AddComponent<Camera>();
        cameraObj.tag = "MainCamera";

        // Create directional light
        GameObject lightObj = new GameObject("Directional Light");
        lightObj.transform.SetParent(testRoot.transform);
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;

        // Create ambient light
        GameObject ambientLight = new GameObject("Ambient Light");
        ambientLight.transform.SetParent(testRoot.transform);
        Light ambient = ambientLight.AddComponent<Light>();
        ambient.type = LightType.Point;
        ambient.range = 100f;
        ambient.intensity = 0.5f;
        ambientLight.transform.position = new Vector3(10, 30, 0);

        // Create ground plane for reference
        GameObject groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        groundPlane.name = "GroundReference";
        groundPlane.transform.SetParent(testRoot.transform);
        groundPlane.transform.position = new Vector3(10, -1, 0);
        groundPlane.transform.localScale = new Vector3(10, 1, 10);
        Renderer groundRenderer = groundPlane.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            groundRenderer.material = groundMat;
        }

        // Select the test root
        Selection.activeGameObject = testRoot;

        Debug.Log("=== Phase 3 Unified Test Level Created ===");
        Debug.Log("");
        Debug.Log("SCENE CONTENTS:");
        Debug.Log("  - Player Ship (Blue) at origin - fully equipped");
        Debug.Log("  - Enemy Ship 1 (Red) at (25, 0, 0)");
        Debug.Log("  - Enemy Ship 2 (Red) at (15, 0, 15)");
        Debug.Log("  - ProjectileManager");
        Debug.Log("  - TargetingController");
        Debug.Log("  - Full UI System (Damage UI, Weapon Config, Weapon Groups)");
        Debug.Log("  - Phase3UnifiedTestController (comprehensive test UI)");
        Debug.Log("");
        Debug.Log("EACH SHIP HAS:");
        Debug.Log("  - All 7 sections (Fore, Aft, Port, Starboard, Dorsal, Ventral, Core)");
        Debug.Log("  - Shield System (200 shields)");
        Debug.Log("  - Main Engine, 2 Weapons, Reactor, Radiator, Sensors, Magazine");
        Debug.Log("  - Critical Hit System, Core Protection, Death Controller");
        Debug.Log("  - System Degradation Manager");
        Debug.Log("");
        Debug.Log("Press Play and use 'J' to toggle test UI, 'K' to switch targets.");
        Debug.Log("The UI provides buttons for ALL Phase 3 features in organized tabs.");
    }

    private static GameObject CreateFullShipWithAllSystems(GameObject parent, string name, Vector3 position, bool isPlayer)
    {
        GameObject shipObj = new GameObject(name);
        shipObj.transform.SetParent(parent.transform);
        shipObj.transform.position = position;

        // Add visual representation - ship-like shape
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Hull";
        visual.transform.SetParent(shipObj.transform);
        visual.transform.localScale = new Vector3(2f, 0.6f, 4f);

        // Add a nose cone
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = "Nose";
        nose.transform.SetParent(shipObj.transform);
        nose.transform.localPosition = new Vector3(0, 0, 2.5f);
        nose.transform.localScale = new Vector3(1f, 0.4f, 1f);
        nose.transform.localRotation = Quaternion.Euler(0, 45, 0);

        // Add engine block
        GameObject engine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        engine.name = "EngineBlock";
        engine.transform.SetParent(shipObj.transform);
        engine.transform.localPosition = new Vector3(0, 0, -2.5f);
        engine.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
        engine.transform.localRotation = Quaternion.Euler(90, 0, 0);

        // Color based on team
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = isPlayer ? new Color(0.2f, 0.4f, 0.8f) : new Color(0.8f, 0.2f, 0.2f);

        ApplyMaterialRecursive(shipObj.transform, mat);

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

        // Create ALL mounted system types for comprehensive testing
        CreateEngine(shipObj, "MainEngine", ShipSystemType.MainEngine, 15);
        CreateWeapon(shipObj, "NewtonianCannon", ShipSystemType.NewtonianCannon, 8);
        CreateWeapon(shipObj, "MissileBattery", ShipSystemType.MissileBattery, 6);
        CreateReactor(shipObj, "ReactorCore", ShipSystemType.ReactorCore, 20);
        CreateRadiator(shipObj, "Radiator1", 5);
        CreateSensors(shipObj, "Sensors", 6);
        CreateMagazine(shipObj, "TorpedoMagazine", ShipSystemType.TorpedoMagazine, 8);
        CreatePDTurret(shipObj, "PDTurret1", 4);

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

    private static void ApplyMaterialRecursive(Transform parent, Material mat)
    {
        Renderer renderer = parent.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = mat;
        }

        foreach (Transform child in parent)
        {
            ApplyMaterialRecursive(child, mat);
        }
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

    private static void CreateEngine(GameObject shipObj, string name, ShipSystemType type, int slotSize)
    {
        GameObject engineObj = new GameObject(name);
        engineObj.transform.SetParent(shipObj.transform);
        MountedEngine engine = engineObj.AddComponent<MountedEngine>();

        SerializedObject engineSO = new SerializedObject(engine);
        engineSO.FindProperty("systemType").enumValueIndex = (int)type;
        engineSO.FindProperty("slotSize").intValue = slotSize;
        engineSO.ApplyModifiedProperties();
    }

    private static void CreateWeapon(GameObject shipObj, string name, ShipSystemType weaponType, int slotSize)
    {
        GameObject weaponObj = new GameObject(name);
        weaponObj.transform.SetParent(shipObj.transform);
        MountedWeapon weapon = weaponObj.AddComponent<MountedWeapon>();

        SerializedObject weaponSO = new SerializedObject(weapon);
        weaponSO.FindProperty("systemType").enumValueIndex = (int)weaponType;
        weaponSO.FindProperty("slotSize").intValue = slotSize;
        weaponSO.ApplyModifiedProperties();
    }

    private static void CreateReactor(GameObject shipObj, string name, ShipSystemType type, int slotSize)
    {
        GameObject reactorObj = new GameObject(name);
        reactorObj.transform.SetParent(shipObj.transform);
        MountedReactor reactor = reactorObj.AddComponent<MountedReactor>();

        SerializedObject reactorSO = new SerializedObject(reactor);
        reactorSO.FindProperty("systemType").enumValueIndex = (int)type;
        reactorSO.FindProperty("slotSize").intValue = slotSize;
        reactorSO.ApplyModifiedProperties();
    }

    private static void CreateRadiator(GameObject shipObj, string name, int slotSize)
    {
        GameObject radiatorObj = new GameObject(name);
        radiatorObj.transform.SetParent(shipObj.transform);
        MountedRadiator radiator = radiatorObj.AddComponent<MountedRadiator>();

        SerializedObject radiatorSO = new SerializedObject(radiator);
        radiatorSO.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.Radiator;
        radiatorSO.FindProperty("slotSize").intValue = slotSize;
        radiatorSO.ApplyModifiedProperties();
    }

    private static void CreateSensors(GameObject shipObj, string name, int slotSize)
    {
        GameObject sensorsObj = new GameObject(name);
        sensorsObj.transform.SetParent(shipObj.transform);
        MountedSensors sensors = sensorsObj.AddComponent<MountedSensors>();

        SerializedObject sensorsSO = new SerializedObject(sensors);
        sensorsSO.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.Sensors;
        sensorsSO.FindProperty("slotSize").intValue = slotSize;
        sensorsSO.ApplyModifiedProperties();
    }

    private static void CreateMagazine(GameObject shipObj, string name, ShipSystemType type, int slotSize)
    {
        GameObject magazineObj = new GameObject(name);
        magazineObj.transform.SetParent(shipObj.transform);
        MountedMagazine magazine = magazineObj.AddComponent<MountedMagazine>();

        SerializedObject magazineSO = new SerializedObject(magazine);
        magazineSO.FindProperty("systemType").enumValueIndex = (int)type;
        magazineSO.FindProperty("slotSize").intValue = slotSize;
        magazineSO.ApplyModifiedProperties();
    }

    private static void CreatePDTurret(GameObject shipObj, string name, int slotSize)
    {
        GameObject pdObj = new GameObject(name);
        pdObj.transform.SetParent(shipObj.transform);
        MountedPDTurret pd = pdObj.AddComponent<MountedPDTurret>();

        SerializedObject pdSO = new SerializedObject(pd);
        pdSO.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.PDTurret;
        pdSO.FindProperty("slotSize").intValue = slotSize;
        pdSO.ApplyModifiedProperties();
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
