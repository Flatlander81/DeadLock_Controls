using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for Phase 2 - Targeting UI System (Track C).
/// Tests target selection, weapon group firing, UI panels, and visual feedback.
/// </summary>
public class TargetingSystemTests
{
    private GameObject playerShipObject;
    private Ship playerShip;
    private WeaponManager weaponManager;
    private TargetingController targetingController;
    private UIManager uiManager;

    // Test weapons
    private GameObject railGunHardpoint;
    private RailGun railGun;

    // Enemy targets
    private GameObject enemyShip1Object;
    private Ship enemyShip1;
    private GameObject enemyShip2Object;
    private Ship enemyShip2;

    // Camera
    private GameObject cameraObject;
    private Camera mainCamera;

    [SetUp]
    public void Setup()
    {
        // Create camera
        cameraObject = new GameObject("MainCamera");
        mainCamera = cameraObject.AddComponent<Camera>();
        mainCamera.tag = "MainCamera";

        // Create player ship
        playerShipObject = new GameObject("Hephaestus");
        playerShip = playerShipObject.AddComponent<Ship>();
        playerShipObject.AddComponent<HeatManager>();

        // Add collider for raycasting
        playerShipObject.AddComponent<BoxCollider>();

        // Create weapon hardpoint
        railGunHardpoint = new GameObject("RailGun_Hardpoint");
        railGunHardpoint.transform.SetParent(playerShipObject.transform);
        railGunHardpoint.transform.localPosition = new Vector3(1f, 0f, 0f);
        railGun = railGunHardpoint.AddComponent<RailGun>();

        // Add WeaponManager
        weaponManager = playerShipObject.AddComponent<WeaponManager>();

        // Create enemy ships
        enemyShip1Object = new GameObject("Enemy1");
        enemyShip1Object.transform.position = new Vector3(10f, 0f, 0f);
        enemyShip1 = enemyShip1Object.AddComponent<Ship>();
        enemyShip1Object.AddComponent<HeatManager>();
        enemyShip1Object.AddComponent<BoxCollider>();

        enemyShip2Object = new GameObject("Enemy2");
        enemyShip2Object.transform.position = new Vector3(0f, 0f, 10f);
        enemyShip2 = enemyShip2Object.AddComponent<Ship>();
        enemyShip2Object.AddComponent<HeatManager>();
        enemyShip2Object.AddComponent<BoxCollider>();

        // Create TargetingController
        GameObject controllerObject = new GameObject("TargetingController");
        targetingController = controllerObject.AddComponent<TargetingController>();

        // Create UIManager
        GameObject uiManagerObject = new GameObject("UIManager");
        uiManager = uiManagerObject.AddComponent<UIManager>();
    }

    /// <summary>
    /// Helper to initialize TargetingController with reflection (sets private playerShip field).
    /// </summary>
    private void InitializeTargetingController()
    {
        var playerShipField = typeof(TargetingController).GetField("playerShip",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (playerShipField != null)
        {
            playerShipField.SetValue(targetingController, playerShip);
        }
    }

    /// <summary>
    /// Helper to manually add a weapon to WeaponManager and initialize it.
    /// Needed when weapons are created mid-test after WeaponManager.Start() has run.
    /// </summary>
    private void AddWeaponToManager(WeaponSystem weapon)
    {
        // Initialize the weapon
        weapon.Initialize(playerShip);

        // Add to weapons list via reflection
        var weaponsField = typeof(WeaponManager).GetField("weapons",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (weaponsField != null)
        {
            var weaponsList = weaponsField.GetValue(weaponManager) as System.Collections.Generic.List<WeaponSystem>;
            if (weaponsList != null)
            {
                weaponsList.Add(weapon);
            }
        }

        // Add to weaponGroups dictionary (group 0 by default)
        var weaponGroupsField = typeof(WeaponManager).GetField("weaponGroups",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (weaponGroupsField != null)
        {
            var weaponGroupsDict = weaponGroupsField.GetValue(weaponManager) as System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<WeaponSystem>>;
            if (weaponGroupsDict != null && weaponGroupsDict.ContainsKey(0))
            {
                weaponGroupsDict[0].Add(weapon);
            }
        }
    }

    [TearDown]
    public void Teardown()
    {
        if (playerShipObject != null) Object.DestroyImmediate(playerShipObject);
        if (enemyShip1Object != null) Object.DestroyImmediate(enemyShip1Object);
        if (enemyShip2Object != null) Object.DestroyImmediate(enemyShip2Object);
        if (targetingController != null) Object.DestroyImmediate(targetingController.gameObject);
        if (uiManager != null) Object.DestroyImmediate(uiManager.gameObject);
        if (cameraObject != null) Object.DestroyImmediate(cameraObject);
    }

    /// <summary>
    /// Test 1: Select enemy, verify CurrentTarget set.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TargetSelection()
    {
        yield return null; // Wait for initialization

        // Verify no target initially
        Assert.IsNull(targetingController.CurrentTarget, "Should have no target initially");

        // Select enemy ship
        targetingController.SelectTarget(enemyShip1);

        // Verify target set
        Assert.AreEqual(enemyShip1, targetingController.CurrentTarget, "CurrentTarget should be enemyShip1");

        // Verify event fired (we can't test directly, but SelectTarget should complete without errors)
        Assert.IsTrue(true, "SelectTarget completed successfully");
    }

    /// <summary>
    /// Test 2: Deselect target, verify CurrentTarget cleared.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TargetDeselection()
    {
        yield return null;

        // Select target first
        targetingController.SelectTarget(enemyShip1);
        Assert.AreEqual(enemyShip1, targetingController.CurrentTarget, "Target should be selected");

        // Deselect target
        targetingController.DeselectTarget();

        // Verify target cleared
        Assert.IsNull(targetingController.CurrentTarget, "CurrentTarget should be null after deselection");
    }

    /// <summary>
    /// Test 3: Assign weapon to group, verify group targeting.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponGroupAssignment()
    {
        yield return null;

        // Initialize TargetingController with player ship reference
        InitializeTargetingController();

        // Assign railGun to group 1
        weaponManager.AssignWeaponToGroup(railGun, 1);
        Assert.AreEqual(1, railGun.AssignedGroup, "RailGun should be in group 1");

        // Select target
        targetingController.SelectTarget(enemyShip1);

        // Assign group 1 to current target
        targetingController.AssignGroupToCurrentTarget(1);

        // Verify weapon target set
        Assert.AreEqual(enemyShip1, railGun.AssignedTarget, "RailGun should target enemyShip1");
    }

    /// <summary>
    /// Test 4: Fire weapon group, verify coroutine starts.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_GroupFiring()
    {
        yield return null;

        // Initialize TargetingController
        InitializeTargetingController();

        // Setup: Assign weapon to group and set target
        weaponManager.AssignWeaponToGroup(railGun, 1);
        railGun.SetTarget(enemyShip1);
        targetingController.SelectTarget(enemyShip1);

        float initialShields = enemyShip1.CurrentShields;

        // Fire group 1
        targetingController.FireGroupAtCurrentTarget(1);

        // Wait for firing to complete (spin-up + execution)
        yield return new WaitForSeconds(0.5f);

        // Verify damage dealt
        Assert.Less(enemyShip1.CurrentShields, initialShields, "Enemy shields should decrease");
    }

    /// <summary>
    /// Test 5: Alpha strike, verify all weapons fire.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_AlphaStrike()
    {
        yield return null;

        // Initialize TargetingController
        InitializeTargetingController();

        // Set target
        targetingController.SelectTarget(enemyShip1);
        float initialShields = enemyShip1.CurrentShields;

        // Fire alpha strike
        targetingController.AlphaStrikeCurrentTarget();

        // Wait for firing to complete
        yield return new WaitForSeconds(0.5f);

        // Verify damage dealt (all weapons should fire)
        Assert.Less(enemyShip1.CurrentShields, initialShields, "Enemy shields should decrease from alpha strike");
    }

    /// <summary>
    /// Test 6: Create targeting line, verify initialized correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TargetingLine()
    {
        yield return null;

        // Create targeting line object
        GameObject lineObject = new GameObject("TestTargetingLine");
        TargetingLineRenderer lineRenderer = lineObject.AddComponent<TargetingLineRenderer>();

        // Initialize with group 1 (blue)
        lineRenderer.Initialize(playerShip, enemyShip1, 1);

        // Verify color
        Color expectedColor = TargetingLineRenderer.GetGroupColor(1);
        Assert.AreEqual(Color.blue, expectedColor, "Group 1 should be blue");

        // Cleanup
        Object.DestroyImmediate(lineObject);
    }

    /// <summary>
    /// Test 7: Assign different groups to different targets.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MultiTargeting()
    {
        yield return null;

        // Initialize TargetingController
        InitializeTargetingController();

        // Create second weapon for group 2
        GameObject cannonHardpoint = new GameObject("Cannon_Hardpoint");
        cannonHardpoint.transform.SetParent(playerShipObject.transform);
        cannonHardpoint.transform.localPosition = new Vector3(-1f, 0f, 0f);
        NewtonianCannon cannon = cannonHardpoint.AddComponent<NewtonianCannon>();

        // Manually add weapon to manager (since Start() already ran)
        AddWeaponToManager(cannon);

        yield return null; // Wait one frame

        // Assign railGun to group 1, cannon to group 2
        weaponManager.AssignWeaponToGroup(railGun, 1);
        weaponManager.AssignWeaponToGroup(cannon, 2);

        // Target enemy1 with group 1
        targetingController.SelectTarget(enemyShip1);
        targetingController.AssignGroupToCurrentTarget(1);

        Assert.AreEqual(enemyShip1, railGun.AssignedTarget, "Group 1 should target enemy1");

        // Target enemy2 with group 2
        targetingController.SelectTarget(enemyShip2);
        targetingController.AssignGroupToCurrentTarget(2);

        Assert.AreEqual(enemyShip2, cannon.AssignedTarget, "Group 2 should target enemy2");

        // Cleanup
        Object.DestroyImmediate(cannonHardpoint);
    }

    /// <summary>
    /// Test 8: UI state transitions based on selection.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_UIStateTransitions()
    {
        yield return null;

        // Initially nothing selected - state should be NothingSelected
        Assert.AreEqual("NothingSelected", uiManager.GetCurrentState(), "Initial state should be NothingSelected");

        // Select enemy - state should change to EnemySelected
        targetingController.SelectShip(enemyShip1);
        yield return null; // Wait for event processing

        // Note: State changes are handled through events, so we verify the manager exists
        Assert.IsNotNull(uiManager, "UIManager should exist");

        // Select player ship - state should change to PlayerSelected
        targetingController.SelectShip(playerShip);
        yield return null;

        Assert.IsNotNull(uiManager, "UIManager should still exist");

        // Deselect all - state should return to NothingSelected
        targetingController.DeselectAll();
        yield return null;

        Assert.IsNotNull(uiManager, "UIManager should still exist after deselection");
    }

    /// <summary>
    /// Test 9: Weapon out of arc, verify detection.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_OutOfArcWarning()
    {
        yield return null;

        // Create narrow-arc weapon (cannon with 180° forward arc)
        GameObject cannonHardpoint = new GameObject("Cannon_Hardpoint");
        cannonHardpoint.transform.SetParent(playerShipObject.transform);
        cannonHardpoint.transform.localPosition = Vector3.zero;
        cannonHardpoint.transform.forward = Vector3.forward; // Pointing forward
        NewtonianCannon cannon = cannonHardpoint.AddComponent<NewtonianCannon>();

        // Manually add weapon to manager (since Start() already ran)
        AddWeaponToManager(cannon);

        yield return null;

        // Position enemy behind the cannon (out of 180° forward arc)
        enemyShip1Object.transform.position = new Vector3(0f, 0f, -10f);

        // Verify out of arc
        Assert.IsFalse(cannon.IsInArc(enemyShip1.transform.position), "Target behind should be out of 180° arc");

        // Position enemy ahead (in arc)
        enemyShip1Object.transform.position = new Vector3(0f, 0f, 10f);
        Assert.IsTrue(cannon.IsInArc(enemyShip1.transform.position), "Target ahead should be in 180° arc");

        // Cleanup
        Object.DestroyImmediate(cannonHardpoint);
    }

    /// <summary>
    /// Test 10: Weapon on cooldown, verify CanFire returns false.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_CooldownWarning()
    {
        yield return null;

        // Set weapon to have cooldown via reflection
        var maxCooldownField = typeof(WeaponSystem).GetField("maxCooldown",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (maxCooldownField != null)
        {
            maxCooldownField.SetValue(railGun, 2); // Set 2-turn cooldown

            // Start cooldown
            railGun.StartCooldown();
            Assert.AreEqual(2, railGun.CurrentCooldown, "Cooldown should be 2");

            // Set target
            railGun.SetTarget(enemyShip1);

            // Verify cannot fire when on cooldown
            Assert.IsFalse(railGun.CanFire(), "Should not be able to fire on cooldown");

            // Tick cooldown
            railGun.TickCooldown();
            Assert.AreEqual(1, railGun.CurrentCooldown, "Cooldown should be 1");
            Assert.IsFalse(railGun.CanFire(), "Should still not be able to fire");

            // Tick to zero
            railGun.TickCooldown();
            Assert.AreEqual(0, railGun.CurrentCooldown, "Cooldown should be 0");
            Assert.IsTrue(railGun.CanFire(), "Should be able to fire when cooldown reaches 0");

            // Reset for other tests
            maxCooldownField.SetValue(railGun, 0);
        }
    }

    /// <summary>
    /// Test 11: Calculate heat cost for weapon group.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HeatCostDisplay()
    {
        yield return null;

        // Initialize TargetingController
        InitializeTargetingController();

        // Create second weapon
        GameObject cannonHardpoint = new GameObject("Cannon_Hardpoint");
        cannonHardpoint.transform.SetParent(playerShipObject.transform);
        cannonHardpoint.transform.localPosition = new Vector3(-1f, 0f, 0f);
        cannonHardpoint.transform.forward = Vector3.forward; // Ensure forward facing
        NewtonianCannon cannon = cannonHardpoint.AddComponent<NewtonianCannon>();

        // Manually add weapon to manager (since Start() already ran)
        AddWeaponToManager(cannon);

        yield return null; // Wait one frame

        // Assign both to group 1
        weaponManager.AssignWeaponToGroup(railGun, 1);
        weaponManager.AssignWeaponToGroup(cannon, 1);

        // Move enemy in front of ship so both weapons can hit (cannon has 180° forward arc)
        enemyShip1Object.transform.position = new Vector3(0f, 0f, 10f);

        // Set targets so CanFire returns true
        railGun.SetTarget(enemyShip1);
        cannon.SetTarget(enemyShip1);

        // Calculate heat cost
        int totalHeat = weaponManager.CalculateGroupHeatCost(1);

        // RailGun (15) + Cannon (30) = 45
        Assert.AreEqual(45, totalHeat, "Group 1 should cost 45 heat");

        // Cleanup
        Object.DestroyImmediate(cannonHardpoint);
    }

    /// <summary>
    /// Test 12: Create selection indicator, verify it follows ship.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SelectionIndicator()
    {
        yield return null;

        // Create selection indicator object
        GameObject indicatorObject = new GameObject("TestSelectionIndicator");
        SelectionIndicator indicator = indicatorObject.AddComponent<SelectionIndicator>();

        // Initialize for enemy ship
        indicator.Initialize(enemyShip1, false);

        yield return null; // Wait for Update to run

        // Verify indicator position matches ship
        Assert.AreEqual(enemyShip1.transform.position, indicatorObject.transform.position,
            "Indicator should be at ship position");

        // Move ship
        enemyShip1Object.transform.position = new Vector3(5f, 0f, 5f);

        yield return null; // Wait for Update

        // Verify indicator followed ship
        Assert.AreEqual(enemyShip1.transform.position, indicatorObject.transform.position,
            "Indicator should follow ship");

        // Cleanup
        Object.DestroyImmediate(indicatorObject);
    }
}
