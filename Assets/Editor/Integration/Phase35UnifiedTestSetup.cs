using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to create a unified Phase 3.5 test scene with all Phase 3 features
/// PLUS all Phase 3.5 integration systems (Turn System, Weapon Firing Queue, Heat/Cooldown).
///
/// Builds on the Phase 3 Unified Test by adding:
/// - TurnManager for turn/phase control
/// - CombatCoordinator for simulation orchestration
/// - WeaponFiringQueue for queued weapon execution
/// - TurnEndProcessor for heat dissipation and cooldown ticking
/// - Phase35UnifiedTestController with tabs for all Phase 3.5 features
///
/// Menu: "Hephaestus/Testing/Create Unified Phase 3.5 Test Level"
/// </summary>
public class Phase35UnifiedTestSetup : Editor
{
    [MenuItem("Hephaestus/Testing/Create Unified Phase 3.5 Test Level")]
    public static void CreateUnifiedTestLevel()
    {
        Debug.Log("=== Creating Phase 3.5 Unified Test Level ===");
        Debug.Log("Building on Phase 3 Unified Test with Integration Systems...");

        // Create root object
        GameObject testRoot = new GameObject("Phase35UnifiedTestLevel");

        // =====================================================
        // PHASE 3.5 INTEGRATION SYSTEMS
        // =====================================================

        // Create TurnManager (central turn/phase controller)
        GameObject turnManagerObj = new GameObject("TurnManager");
        turnManagerObj.transform.SetParent(testRoot.transform);
        TurnManager turnManager = turnManagerObj.AddComponent<TurnManager>();

        // Create CombatCoordinator (simulation orchestration)
        GameObject coordinatorObj = new GameObject("CombatCoordinator");
        coordinatorObj.transform.SetParent(testRoot.transform);
        CombatCoordinator coordinator = coordinatorObj.AddComponent<CombatCoordinator>();

        // Create WeaponFiringQueue (queued weapon execution)
        GameObject firingQueueObj = new GameObject("WeaponFiringQueue");
        firingQueueObj.transform.SetParent(testRoot.transform);
        WeaponFiringQueue firingQueue = firingQueueObj.AddComponent<WeaponFiringQueue>();

        // Create TurnEndProcessor (heat dissipation, cooldown ticking)
        GameObject turnEndProcessorObj = new GameObject("TurnEndProcessor");
        turnEndProcessorObj.transform.SetParent(testRoot.transform);
        TurnEndProcessor turnEndProcessor = turnEndProcessorObj.AddComponent<TurnEndProcessor>();

        // Create MovementExecutor (Phase 3.5.4 - movement coordination)
        GameObject movementExecutorObj = new GameObject("MovementExecutor");
        movementExecutorObj.transform.SetParent(testRoot.transform);
        MovementExecutor movementExecutor = movementExecutorObj.AddComponent<MovementExecutor>();

        // Create WeaponArcValidator (Phase 3.5.4 - movement-aware arc checking)
        GameObject arcValidatorObj = new GameObject("WeaponArcValidator");
        arcValidatorObj.transform.SetParent(testRoot.transform);
        WeaponArcValidator arcValidator = arcValidatorObj.AddComponent<WeaponArcValidator>();

        // =====================================================
        // PHASE 3 UNIFIED TEST BASE (Ships with full damage systems)
        // =====================================================

        // Create player ship with all systems (reusing Phase 3 ship creation)
        GameObject playerShipObj = CreateFullShipWithAllSystems(testRoot, "PlayerShip", new Vector3(0, 0, 0), true);
        Ship playerShip = playerShipObj.GetComponent<Ship>();

        // Create enemy ship with all systems
        GameObject enemyShipObj = CreateFullShipWithAllSystems(testRoot, "EnemyShip", new Vector3(0, 0, 30), false);
        Ship enemyShip = enemyShipObj.GetComponent<Ship>();

        // Create second enemy for multi-target testing
        GameObject enemy2Obj = CreateFullShipWithAllSystems(testRoot, "EnemyShip2", new Vector3(5, 0, 15), false);
        Ship enemy2 = enemy2Obj.GetComponent<Ship>();

        // =====================================================
        // PROJECTILE AND TARGETING SYSTEMS
        // =====================================================

        // Create projectile manager
        GameObject projectileManagerObj = new GameObject("ProjectileManager");
        projectileManagerObj.transform.SetParent(testRoot.transform);
        ProjectileManager projectileManager = projectileManagerObj.AddComponent<ProjectileManager>();

        // Create targeting controller
        GameObject targetingObj = new GameObject("TargetingController");
        targetingObj.transform.SetParent(testRoot.transform);
        TargetingController targetingController = targetingObj.AddComponent<TargetingController>();

        // Wire TargetingController playerShip reference
        SerializedObject targetingSO = new SerializedObject(targetingController);
        targetingSO.FindProperty("playerShip").objectReferenceValue = playerShip;
        targetingSO.ApplyModifiedProperties();

        // Create MovementController (for mouse-based movement planning)
        GameObject movementControllerObj = new GameObject("MovementController");
        movementControllerObj.transform.SetParent(testRoot.transform);
        MovementController movementController = movementControllerObj.AddComponent<MovementController>();

        // Wire MovementController references
        SerializedObject movementSO = new SerializedObject(movementController);
        movementSO.FindProperty("playerShip").objectReferenceValue = playerShip;
        movementSO.FindProperty("targetingController").objectReferenceValue = targetingController;
        movementSO.ApplyModifiedProperties();

        // Create InputManager (for centralized input handling)
        GameObject inputManagerObj = new GameObject("InputManager");
        inputManagerObj.transform.SetParent(testRoot.transform);
        InputManager inputManager = inputManagerObj.AddComponent<InputManager>();

        // Wire InputManager references
        SerializedObject inputSO = new SerializedObject(inputManager);
        inputSO.FindProperty("playerShip").objectReferenceValue = playerShip;
        inputSO.FindProperty("targetingController").objectReferenceValue = targetingController;
        inputSO.FindProperty("movementController").objectReferenceValue = movementController;
        inputSO.ApplyModifiedProperties();

        // =====================================================
        // UI SYSTEMS
        // =====================================================

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

        // =====================================================
        // TEST CONTROLLERS
        // =====================================================

        // Create Phase 3.5 unified test controller (combines all test features)
        GameObject controllerObj = new GameObject("Phase35UnifiedTestController");
        controllerObj.transform.SetParent(testRoot.transform);
        Phase35UnifiedTestController controller = controllerObj.AddComponent<Phase35UnifiedTestController>();

        // Wire controller references
        SerializedObject controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("playerShip").objectReferenceValue = playerShip;
        controllerSO.FindProperty("projectileManager").objectReferenceValue = projectileManager;
        controllerSO.FindProperty("damageUIManager").objectReferenceValue = damageUIManager;
        controllerSO.FindProperty("targetingController").objectReferenceValue = targetingController;
        controllerSO.FindProperty("turnManager").objectReferenceValue = turnManager;
        controllerSO.FindProperty("turnEndProcessor").objectReferenceValue = turnEndProcessor;
        controllerSO.FindProperty("firingQueue").objectReferenceValue = firingQueue;
        controllerSO.FindProperty("movementExecutor").objectReferenceValue = movementExecutor;
        controllerSO.FindProperty("arcValidator").objectReferenceValue = arcValidator;
        controllerSO.ApplyModifiedProperties();

        // =====================================================
        // SCENE SETUP (Camera, Lighting, Ground)
        // =====================================================

        // Create camera with good viewing angle
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.transform.SetParent(testRoot.transform);
        cameraObj.transform.position = new Vector3(0, 15, -25);
        cameraObj.transform.rotation = Quaternion.Euler(30, 0, 0);
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
        groundPlane.transform.position = new Vector3(0, -1, 15);
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

        Debug.Log("=== Phase 3.5 Unified Test Level Created ===");
        Debug.Log("");
        Debug.Log("PHASE 3.5 INTEGRATION SYSTEMS:");
        Debug.Log("  - TurnManager (turn/phase control)");
        Debug.Log("  - CombatCoordinator (simulation orchestration)");
        Debug.Log("  - WeaponFiringQueue (queued weapon execution)");
        Debug.Log("  - TurnEndProcessor (heat dissipation, cooldown ticking)");
        Debug.Log("  - MovementExecutor (movement coordination during simulation)");
        Debug.Log("  - WeaponArcValidator (movement-aware arc checking)");
        Debug.Log("  - MovementController (mouse-based movement planning)");
        Debug.Log("  - InputManager (centralized input handling)");
        Debug.Log("");
        Debug.Log("PHASE 3 UNIFIED FEATURES (inherited):");
        Debug.Log("  - Full damage system (shields, sections, armor, structure)");
        Debug.Log("  - Critical hit system with mounted systems");
        Debug.Log("  - System degradation (engines, radiators, weapons, etc.)");
        Debug.Log("  - Core protection and ship death conditions");
        Debug.Log("  - Damage UI (section panel, combat log, shield bar)");
        Debug.Log("  - Weapon systems with firing and projectiles");
        Debug.Log("");
        Debug.Log("MOVEMENT CONTROLS:");
        Debug.Log("  M        Toggle movement mode");
        Debug.Log("  Click+Drag on projection to plan move");
        Debug.Log("  E        Elevation adjustment mode (scroll wheel)");
        Debug.Log("  R        Rotation adjustment mode (arrow keys)");
        Debug.Log("  Enter/Space  Confirm move");
        Debug.Log("  Escape   Cancel move / Exit movement mode");
        Debug.Log("");
        Debug.Log("COMBAT CONTROLS:");
        Debug.Log("  1,2,3,4  Fire weapon groups");
        Debug.Log("  A        Alpha Strike (all weapons)");
        Debug.Log("  K        Cycle targets");
        Debug.Log("  J        Toggle test panel UI");
        Debug.Log("  Space    Toggle weapon config panel");
        Debug.Log("  R        Reset cooldowns (cheat)");
        Debug.Log("  L        Reload all ammo (cheat)");
        Debug.Log("  T        Advance turn");
        Debug.Log("");
        Debug.Log("TEST PANEL TABS:");
        Debug.Log("  Combat, Sections, Systems, Core/Death, Projectiles, Weapons, Status");
        Debug.Log("  Turns, Heat/Cooldown, FiringQueue, Movement (Phase 3.5 additions)");
    }

    /// <summary>
    /// Creates a full ship with all Phase 3 systems (damage, sections, shields, weapons, etc.)
    /// Copied from Phase3UnifiedTestSetup to ensure consistency.
    /// </summary>
    private static GameObject CreateFullShipWithAllSystems(GameObject parent, string name, Vector3 position, bool isPlayer)
    {
        GameObject shipObj = new GameObject(name);
        shipObj.transform.SetParent(parent.transform);
        shipObj.transform.position = position;

        // Create detailed hull visual
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.name = "Hull";
        hull.transform.SetParent(shipObj.transform);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localScale = new Vector3(3f, 1.5f, 6f);
        DestroyImmediate(hull.GetComponent<Collider>());

        // Add bridge superstructure
        GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = "Bridge";
        bridge.transform.SetParent(shipObj.transform);
        bridge.transform.localPosition = new Vector3(0, 1f, -1f);
        bridge.transform.localScale = new Vector3(1.5f, 1f, 2f);
        DestroyImmediate(bridge.GetComponent<Collider>());

        // Add nose cone
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = "Nose";
        nose.transform.SetParent(shipObj.transform);
        nose.transform.localPosition = new Vector3(0, 0, 3.5f);
        nose.transform.localScale = new Vector3(1.5f, 0.8f, 1.5f);
        nose.transform.localRotation = Quaternion.Euler(0, 45, 0);
        DestroyImmediate(nose.GetComponent<Collider>());

        // Add engine block
        GameObject engineVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        engineVisual.name = "EngineBlock";
        engineVisual.transform.SetParent(shipObj.transform);
        engineVisual.transform.localPosition = new Vector3(0, 0, -3.5f);
        engineVisual.transform.localScale = new Vector3(1.2f, 0.8f, 1.2f);
        engineVisual.transform.localRotation = Quaternion.Euler(90, 0, 0);
        DestroyImmediate(engineVisual.GetComponent<Collider>());

        // Add wing/fin details
        GameObject portWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portWing.name = "PortWing";
        portWing.transform.SetParent(shipObj.transform);
        portWing.transform.localPosition = new Vector3(-2f, 0, 0);
        portWing.transform.localScale = new Vector3(1.5f, 0.2f, 2f);
        DestroyImmediate(portWing.GetComponent<Collider>());

        GameObject starboardWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        starboardWing.name = "StarboardWing";
        starboardWing.transform.SetParent(shipObj.transform);
        starboardWing.transform.localPosition = new Vector3(2f, 0, 0);
        starboardWing.transform.localScale = new Vector3(1.5f, 0.2f, 2f);
        DestroyImmediate(starboardWing.GetComponent<Collider>());

        // Color based on team
        Material mat = new Material(Shader.Find("Standard"));
        Color shipColor = isPlayer ? new Color(0.2f, 0.4f, 0.8f) : new Color(0.8f, 0.2f, 0.2f);
        mat.color = shipColor;

        Material bridgeMat = new Material(Shader.Find("Standard"));
        bridgeMat.color = isPlayer ? new Color(0.3f, 0.5f, 0.9f) : new Color(0.9f, 0.3f, 0.3f);
        bridge.GetComponent<Renderer>().sharedMaterial = bridgeMat;

        ApplyMaterialRecursive(shipObj.transform, mat);
        bridge.GetComponent<Renderer>().sharedMaterial = bridgeMat;

        // Add target indicator above enemy ships
        if (!isPlayer)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.name = "TargetIndicator";
            indicator.transform.SetParent(shipObj.transform);
            indicator.transform.localPosition = new Vector3(0, 4, 0);
            indicator.transform.localScale = Vector3.one * 0.8f;
            DestroyImmediate(indicator.GetComponent<Collider>());

            Material indicatorMat = new Material(Shader.Find("Standard"));
            indicatorMat.color = Color.red;
            indicator.GetComponent<Renderer>().sharedMaterial = indicatorMat;
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

        // Add AbilitySystem and WeaponManager
        shipObj.AddComponent<AbilitySystem>();
        shipObj.AddComponent<WeaponManager>();

        // Create all 7 sections
        CreateSection(shipObj, SectionType.Core, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Fore, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Aft, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Port, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Starboard, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Dorsal, ship, criticalHitSystem);
        CreateSection(shipObj, SectionType.Ventral, ship, criticalHitSystem);

        // Create ALL mounted system types for comprehensive testing
        CreateEngine(shipObj, "MainEngine", ShipSystemType.MainEngine);
        CreateMountedWeapon(shipObj, "NewtonianCannon", ShipSystemType.NewtonianCannon);
        CreateMountedWeapon(shipObj, "MissileBattery", ShipSystemType.MissileBattery);
        CreateReactor(shipObj, "ReactorCore", ShipSystemType.ReactorCore);
        CreateRadiator(shipObj, "Radiator1");
        CreateRadiator(shipObj, "Radiator2");
        CreateRadiator(shipObj, "Radiator3");
        CreateSensors(shipObj, "Sensors");
        CreateMagazine(shipObj, "TorpedoMagazine", ShipSystemType.TorpedoMagazine);
        CreatePDTurret(shipObj, "PDTurret1");

        // Create actual firing weapons
        CreateFiringWeapon<RailGun>(shipObj, "RailGun_Hardpoint1", new Vector3(0.8f, 0.2f, 1.5f));
        CreateFiringWeapon<RailGun>(shipObj, "RailGun_Hardpoint2", new Vector3(-0.8f, 0.2f, 1.5f));
        CreateFiringWeapon<NewtonianCannon>(shipObj, "NewtonianCannon_Hardpoint", new Vector3(0, 0.3f, 2f));

        // Broadside weapons
        CreateBroadsideWeapon<MissileBattery>(shipObj, "MissileBattery_Hardpoint",
            centerPos: new Vector3(0, 0.5f, 0),
            portOffset: new Vector3(-2.5f, 0, 0),
            starboardOffset: new Vector3(2.5f, 0, 0));

        CreateBroadsideWeapon<TorpedoLauncher>(shipObj, "TorpedoLauncher_Hardpoint",
            centerPos: new Vector3(0, -0.2f, 0),
            portOffset: new Vector3(-2.5f, 0, 0),
            starboardOffset: new Vector3(2.5f, 0, 0));

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
        var config = SectionDefinitions.GetConfig(type);

        GameObject sectionObj = new GameObject($"Section_{type}");
        sectionObj.transform.SetParent(shipObj.transform);
        sectionObj.transform.localPosition = config.ColliderPosition;

        ShipSection section = sectionObj.AddComponent<ShipSection>();
        section.Initialize(type, ship);
        section.SetCriticalHitSystem(critSystem);

        if (config.UseSphereCollider)
        {
            SphereCollider col = sectionObj.AddComponent<SphereCollider>();
            col.radius = config.SphereRadius;
            col.isTrigger = true;
        }
        else
        {
            BoxCollider col = sectionObj.AddComponent<BoxCollider>();
            col.size = config.ColliderSize;
            col.isTrigger = true;
        }

        SectionHitDetector hitDetector = sectionObj.AddComponent<SectionHitDetector>();
        hitDetector.SetParentSection(section);
    }

    private static void CreateEngine(GameObject shipObj, string name, ShipSystemType type)
    {
        GameObject engineObj = new GameObject(name);
        engineObj.transform.SetParent(shipObj.transform);
        MountedEngine engine = engineObj.AddComponent<MountedEngine>();

        SerializedObject engineSO = new SerializedObject(engine);
        engineSO.FindProperty("systemType").enumValueIndex = (int)type;
        engineSO.ApplyModifiedProperties();
    }

    private static void CreateMountedWeapon(GameObject shipObj, string name, ShipSystemType weaponType)
    {
        GameObject weaponObj = new GameObject(name);
        weaponObj.transform.SetParent(shipObj.transform);
        MountedWeapon weapon = weaponObj.AddComponent<MountedWeapon>();

        SerializedObject weaponSO = new SerializedObject(weapon);
        weaponSO.FindProperty("systemType").enumValueIndex = (int)weaponType;
        weaponSO.ApplyModifiedProperties();
    }

    private static void CreateFiringWeapon<T>(GameObject shipObj, string name, Vector3 localPosition) where T : WeaponSystem
    {
        GameObject weaponObj = new GameObject(name);
        weaponObj.transform.SetParent(shipObj.transform);
        weaponObj.transform.localPosition = localPosition;
        weaponObj.transform.localRotation = Quaternion.identity;

        T weapon = weaponObj.AddComponent<T>();

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "WeaponVisual";
        visual.transform.SetParent(weaponObj.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * 0.4f;
        DestroyImmediate(visual.GetComponent<Collider>());

        Material visualMat = new Material(Shader.Find("Standard"));
        visualMat.color = GetWeaponColor<T>();
        visual.GetComponent<Renderer>().sharedMaterial = visualMat;
    }

    private static void CreateBroadsideWeapon<T>(GameObject shipObj, string name, Vector3 centerPos, Vector3 portOffset, Vector3 starboardOffset) where T : WeaponSystem
    {
        GameObject weaponObj = new GameObject(name);
        weaponObj.transform.SetParent(shipObj.transform);
        weaponObj.transform.localPosition = centerPos;
        weaponObj.transform.localRotation = Quaternion.identity;

        T weapon = weaponObj.AddComponent<T>();

        GameObject portHardpoint = new GameObject("Port_Hardpoint");
        portHardpoint.transform.SetParent(weaponObj.transform);
        portHardpoint.transform.localPosition = portOffset;
        portHardpoint.transform.localRotation = Quaternion.Euler(0, -90, 0);

        GameObject starboardHardpoint = new GameObject("Starboard_Hardpoint");
        starboardHardpoint.transform.SetParent(weaponObj.transform);
        starboardHardpoint.transform.localPosition = starboardOffset;
        starboardHardpoint.transform.localRotation = Quaternion.Euler(0, 90, 0);

        SerializedObject weaponSO = new SerializedObject(weapon);
        weaponSO.FindProperty("portHardpoint").objectReferenceValue = portHardpoint.transform;
        weaponSO.FindProperty("starboardHardpoint").objectReferenceValue = starboardHardpoint.transform;
        weaponSO.ApplyModifiedProperties();

        Color weaponColor = GetWeaponColor<T>();

        GameObject portVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        portVisual.name = "PortVisual";
        portVisual.transform.SetParent(portHardpoint.transform);
        portVisual.transform.localPosition = Vector3.zero;
        portVisual.transform.localScale = Vector3.one * 0.4f;
        DestroyImmediate(portVisual.GetComponent<Collider>());
        Material portMat = new Material(Shader.Find("Standard"));
        portMat.color = Color.Lerp(weaponColor, Color.red, 0.3f);
        portVisual.GetComponent<Renderer>().sharedMaterial = portMat;

        GameObject starboardVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        starboardVisual.name = "StarboardVisual";
        starboardVisual.transform.SetParent(starboardHardpoint.transform);
        starboardVisual.transform.localPosition = Vector3.zero;
        starboardVisual.transform.localScale = Vector3.one * 0.4f;
        DestroyImmediate(starboardVisual.GetComponent<Collider>());
        Material starboardMat = new Material(Shader.Find("Standard"));
        starboardMat.color = Color.Lerp(weaponColor, Color.green, 0.3f);
        starboardVisual.GetComponent<Renderer>().sharedMaterial = starboardMat;
    }

    private static Color GetWeaponColor<T>() where T : WeaponSystem
    {
        if (typeof(T) == typeof(RailGun)) return Color.cyan;
        if (typeof(T) == typeof(NewtonianCannon)) return Color.magenta;
        if (typeof(T) == typeof(TorpedoLauncher)) return new Color(1f, 0.5f, 0f);
        if (typeof(T) == typeof(MissileBattery)) return Color.yellow;
        return Color.white;
    }

    private static void CreateReactor(GameObject shipObj, string name, ShipSystemType type)
    {
        GameObject reactorObj = new GameObject(name);
        reactorObj.transform.SetParent(shipObj.transform);
        MountedReactor reactor = reactorObj.AddComponent<MountedReactor>();

        SerializedObject reactorSO = new SerializedObject(reactor);
        reactorSO.FindProperty("systemType").enumValueIndex = (int)type;
        reactorSO.ApplyModifiedProperties();
    }

    private static void CreateRadiator(GameObject shipObj, string name)
    {
        GameObject radiatorObj = new GameObject(name);
        radiatorObj.transform.SetParent(shipObj.transform);
        MountedRadiator radiator = radiatorObj.AddComponent<MountedRadiator>();

        SerializedObject radiatorSO = new SerializedObject(radiator);
        radiatorSO.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.Radiator;
        radiatorSO.FindProperty("baseCoolingContribution").floatValue = 5f;
        radiatorSO.ApplyModifiedProperties();
    }

    private static void CreateSensors(GameObject shipObj, string name)
    {
        GameObject sensorsObj = new GameObject(name);
        sensorsObj.transform.SetParent(shipObj.transform);
        MountedSensors sensors = sensorsObj.AddComponent<MountedSensors>();

        SerializedObject sensorsSO = new SerializedObject(sensors);
        sensorsSO.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.Sensors;
        sensorsSO.ApplyModifiedProperties();
    }

    private static void CreateMagazine(GameObject shipObj, string name, ShipSystemType type)
    {
        GameObject magazineObj = new GameObject(name);
        magazineObj.transform.SetParent(shipObj.transform);
        MountedMagazine magazine = magazineObj.AddComponent<MountedMagazine>();

        SerializedObject magazineSO = new SerializedObject(magazine);
        magazineSO.FindProperty("systemType").enumValueIndex = (int)type;
        magazineSO.ApplyModifiedProperties();
    }

    private static void CreatePDTurret(GameObject shipObj, string name)
    {
        GameObject pdObj = new GameObject(name);
        pdObj.transform.SetParent(shipObj.transform);
        MountedPDTurret pd = pdObj.AddComponent<MountedPDTurret>();

        SerializedObject pdSO = new SerializedObject(pd);
        pdSO.FindProperty("systemType").enumValueIndex = (int)ShipSystemType.PDTurret;
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
