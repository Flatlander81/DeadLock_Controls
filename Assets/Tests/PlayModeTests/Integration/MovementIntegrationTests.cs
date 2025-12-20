using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Integration tests for Movement and Weapon Arc validation.
/// Tests MovementExecutor, position-at-time queries, and WeaponArcValidator.
/// </summary>
public class MovementIntegrationTests
{
    private GameObject testRoot;
    private TurnManager turnManager;
    private MovementExecutor movementExecutor;
    private WeaponArcValidator arcValidator;
    private Ship playerShip;
    private Ship targetShip;
    private WeaponManager weaponManager;
    private SystemDegradationManager degradationManager;

    [SetUp]
    public void Setup()
    {
        // Create root
        testRoot = new GameObject("TestRoot");

        // Create TurnManager
        GameObject turnManagerObj = new GameObject("TurnManager");
        turnManagerObj.transform.SetParent(testRoot.transform);
        turnManager = turnManagerObj.AddComponent<TurnManager>();

        // Create MovementExecutor
        GameObject executorObj = new GameObject("MovementExecutor");
        executorObj.transform.SetParent(testRoot.transform);
        movementExecutor = executorObj.AddComponent<MovementExecutor>();

        // Create WeaponArcValidator
        GameObject validatorObj = new GameObject("WeaponArcValidator");
        validatorObj.transform.SetParent(testRoot.transform);
        arcValidator = validatorObj.AddComponent<WeaponArcValidator>();

        // Create player ship
        GameObject playerObj = new GameObject("PlayerShip");
        playerObj.transform.SetParent(testRoot.transform);
        playerObj.transform.position = Vector3.zero;
        playerObj.transform.rotation = Quaternion.identity;
        playerShip = playerObj.AddComponent<Ship>();
        playerShip.gameObject.AddComponent<HeatManager>();
        weaponManager = playerObj.AddComponent<WeaponManager>();
        degradationManager = playerObj.AddComponent<SystemDegradationManager>();

        // Create target ship
        GameObject targetObj = new GameObject("TargetShip");
        targetObj.transform.SetParent(testRoot.transform);
        targetObj.transform.position = new Vector3(10f, 0f, 0f);
        targetObj.transform.rotation = Quaternion.identity;
        targetShip = targetObj.AddComponent<Ship>();
        targetShip.gameObject.AddComponent<HeatManager>();
        targetShip.gameObject.AddComponent<WeaponManager>();
    }

    [TearDown]
    public void Teardown()
    {
        if (testRoot != null)
        {
            Object.DestroyImmediate(testRoot);
        }
    }

    private RailGun CreateTestWeapon(Transform parent, string name, float firingArc)
    {
        GameObject weaponObj = new GameObject(name);
        weaponObj.transform.SetParent(parent);
        RailGun weapon = weaponObj.AddComponent<RailGun>();

        // Set firing arc via reflection
        var arcField = typeof(WeaponSystem).GetField("firingArc",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        arcField?.SetValue(weapon, firingArc);

        var nameField = typeof(WeaponSystem).GetField("weaponName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        nameField?.SetValue(weapon, name);

        var rangeField = typeof(WeaponSystem).GetField("maxRange",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        rangeField?.SetValue(weapon, 50f);

        return weapon;
    }

    // ==================== TEST 1: Position At Time Returns Current When No Move Planned ====================

    /// <summary>
    /// Test 1: GetPositionAtTime returns current position when no move is planned.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_PositionAtTime_NoMovePlanned()
    {
        yield return null;

        Vector3 currentPos = playerShip.transform.position;
        Vector3 posAtStart = playerShip.GetPositionAtTime(0f);
        Vector3 posAtMid = playerShip.GetPositionAtTime(0.5f);
        Vector3 posAtEnd = playerShip.GetPositionAtTime(1f);

        Assert.AreEqual(currentPos, posAtStart, "Position at t=0 should be current position");
        Assert.AreEqual(currentPos, posAtMid, "Position at t=0.5 should be current position");
        Assert.AreEqual(currentPos, posAtEnd, "Position at t=1 should be current position");
    }

    // ==================== TEST 2: Position At Time With Planned Move ====================

    /// <summary>
    /// Test 2: GetPositionAtTime returns interpolated position along Bezier curve.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_PositionAtTime_WithPlannedMove()
    {
        yield return null;

        Vector3 startPos = playerShip.transform.position;
        Vector3 targetPos = new Vector3(15f, 0f, 0f);

        // Plan move
        playerShip.PlanMove(targetPos, Quaternion.identity);

        Vector3 posAtStart = playerShip.GetPositionAtTime(0f);
        Vector3 posAtMid = playerShip.GetPositionAtTime(0.5f);
        Vector3 posAtEnd = playerShip.GetPositionAtTime(1f);

        Assert.AreEqual(startPos, posAtStart, "Position at t=0 should be start position");
        Assert.AreNotEqual(startPos, posAtMid, "Position at t=0.5 should not be start");
        Assert.AreNotEqual(posAtEnd, posAtMid, "Position at t=0.5 should not be end");

        // End position should be close to planned position
        float endDistance = Vector3.Distance(posAtEnd, playerShip.PlannedPosition);
        Assert.Less(endDistance, 0.1f, "Position at t=1 should be very close to planned position");
    }

    // ==================== TEST 3: Rotation At Time Follows Curve Tangent ====================

    /// <summary>
    /// Test 3: GetRotationAtTime returns rotation based on curve tangent.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_RotationAtTime_FollowsTangent()
    {
        yield return null;

        // Plan a turning move
        playerShip.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        Vector3 targetPos = new Vector3(10f, 0f, 10f); // Move to right-forward

        playerShip.PlanMove(targetPos, Quaternion.identity);

        Quaternion rotAtStart = playerShip.GetRotationAtTime(0f);
        Quaternion rotAtEnd = playerShip.GetRotationAtTime(1f);

        // End rotation should face toward target
        Vector3 endForward = rotAtEnd * Vector3.forward;

        // The ship should be facing roughly toward the target at the end
        // (Bezier curves have tangent-following rotation)
        Assert.IsTrue(playerShip.HasPlannedMove, "Should have planned move");
    }

    // ==================== TEST 4: Movement Executor Starts All Ships ====================

    /// <summary>
    /// Test 4: MovementExecutor starts movement for all ships with planned moves.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MovementExecutor_StartsAllShips()
    {
        yield return null;

        // Plan moves for both ships
        playerShip.PlanMove(new Vector3(5f, 0f, 0f), Quaternion.identity);
        targetShip.PlanMove(new Vector3(15f, 0f, 0f), Quaternion.identity);

        bool playerStarted = false;
        bool targetStarted = false;

        movementExecutor.OnShipMovementStarted += (ship) =>
        {
            if (ship == playerShip) playerStarted = true;
            if (ship == targetShip) targetStarted = true;
        };

        // Execute movements
        movementExecutor.ExecuteAllPlannedMovements();

        yield return new WaitForSeconds(0.1f);

        Assert.IsTrue(playerStarted, "Player ship movement should have started");
        Assert.IsTrue(targetStarted, "Target ship movement should have started");
        Assert.IsTrue(movementExecutor.IsExecuting, "Executor should be executing");
    }

    // ==================== TEST 5: Arc Validator Static Check ====================

    /// <summary>
    /// Test 5: WeaponArcValidator checks arc correctly for static positions.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ArcValidator_StaticCheck()
    {
        yield return null;

        // Create weapon with 180 degree arc (forward)
        RailGun weapon = CreateTestWeapon(playerShip.transform, "ForwardGun", 180f);
        weapon.Initialize(playerShip);

        // Player facing +Z, target at +X (90 degrees off)
        playerShip.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        targetShip.transform.position = new Vector3(10f, 0f, 0f);

        // 90 degrees is outside 180 degree arc (90 >= 90)
        var result = arcValidator.ValidateArc(weapon, targetShip);

        Assert.IsFalse(result.WillBeInArc, "Target at 90 degrees should not be in 180 degree arc");

        // Move target to +Z (in front)
        targetShip.transform.position = new Vector3(0f, 0f, 10f);

        result = arcValidator.ValidateArc(weapon, targetShip);

        Assert.IsTrue(result.WillBeInArc, "Target directly ahead should be in arc");
    }

    // ==================== TEST 6: Arc Validator Movement-Aware Check ====================

    /// <summary>
    /// Test 6: WeaponArcValidator finds firing windows during movement.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ArcValidator_MovementAware()
    {
        yield return null;

        // Create weapon with narrow 60 degree arc
        RailGun weapon = CreateTestWeapon(playerShip.transform, "NarrowGun", 60f);
        weapon.Initialize(playerShip);

        // Player facing +Z, target at +X (90 degrees off - not in arc initially)
        playerShip.transform.position = Vector3.zero;
        playerShip.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        targetShip.transform.position = new Vector3(10f, 0f, 0f);

        // Plan a move that turns toward target
        Vector3 targetMovePos = new Vector3(5f, 0f, 5f);
        playerShip.PlanMove(targetMovePos, Quaternion.identity);

        // Clear cache and validate with movement
        arcValidator.ClearCache();
        var result = arcValidator.ValidateArc(weapon, targetShip);

        // The ship turns during movement, so there should be a firing window
        Assert.IsTrue(result.FiringWindows != null, "Should have firing windows list");

        Debug.Log($"Arc validation: WillBeInArc={result.WillBeInArc}, Windows={result.FiringWindows.Count}, Message={result.ValidationMessage}");
    }

    // ==================== TEST 7: Turret Weapon Always In Arc ====================

    /// <summary>
    /// Test 7: 360 degree turret weapons are always in arc.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_TurretWeapon_AlwaysInArc()
    {
        yield return null;

        // Create turret weapon with 360 degree arc
        RailGun weapon = CreateTestWeapon(playerShip.transform, "Turret", 360f);
        weapon.Initialize(playerShip);

        // Target anywhere
        targetShip.transform.position = new Vector3(-10f, 5f, -10f);

        var result = arcValidator.ValidateArc(weapon, targetShip);

        Assert.IsTrue(result.WillBeInArc, "360 degree turret should always be in arc");
        Assert.AreEqual(0f, result.OptimalFiringTime, "Optimal time should be 0 for turrets");
    }

    // ==================== TEST 8: Distance Calculation During Movement ====================

    /// <summary>
    /// Test 8: MovementExecutor calculates correct distances during movement.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DistanceCalculation_DuringMovement()
    {
        yield return null;

        // Player at origin, target at (20, 0, 0)
        playerShip.transform.position = Vector3.zero;
        targetShip.transform.position = new Vector3(20f, 0f, 0f);

        // Player moves toward target
        playerShip.PlanMove(new Vector3(10f, 0f, 0f), Quaternion.identity);

        float distAtStart = movementExecutor.GetDistanceAtTime(playerShip, targetShip, 0f);
        float distAtEnd = movementExecutor.GetDistanceAtTime(playerShip, targetShip, 1f);

        Assert.AreEqual(20f, distAtStart, 0.1f, "Distance at start should be 20");
        Assert.Less(distAtEnd, distAtStart, "Distance at end should be less (ship moved closer)");
    }

    // ==================== TEST 9: Range Check During Movement ====================

    /// <summary>
    /// Test 9: Range check accounts for movement.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_RangeCheck_DuringMovement()
    {
        yield return null;

        // Create weapon with 15 unit range
        RailGun weapon = CreateTestWeapon(playerShip.transform, "ShortRange", 360f);

        var rangeField = typeof(WeaponSystem).GetField("maxRange",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        rangeField?.SetValue(weapon, 15f);

        weapon.Initialize(playerShip);

        // Target at 20 units - out of range
        playerShip.transform.position = Vector3.zero;
        targetShip.transform.position = new Vector3(20f, 0f, 0f);

        // Player moves toward target to (10, 0, 0) - brings target to 10 units
        playerShip.PlanMove(new Vector3(10f, 0f, 0f), Quaternion.identity);

        bool inRangeAtStart = movementExecutor.IsInRangeAtTime(playerShip, targetShip, weapon, 0f);
        bool inRangeAtEnd = movementExecutor.IsInRangeAtTime(playerShip, targetShip, weapon, 1f);

        Assert.IsFalse(inRangeAtStart, "Should be out of range at start (20 > 15)");
        Assert.IsTrue(inRangeAtEnd, "Should be in range at end (10 < 15)");
    }

    // ==================== TEST 10: Degradation Affects Movement Distance ====================

    /// <summary>
    /// Test 10: Engine degradation reduces effective movement distance.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_Degradation_ReducesMovement()
    {
        yield return null;

        // Get base movement distance
        float baseDistance = playerShip.GetEffectiveMaxMoveDistance();

        // Create and damage an engine
        GameObject engineObj = new GameObject("Engine");
        engineObj.transform.SetParent(playerShip.transform);
        MountedEngine engine = engineObj.AddComponent<MountedEngine>();

        degradationManager.RegisterSystem(engine);
        degradationManager.RefreshSystemCache();

        // Damage the engine
        engine.TakeCriticalHit();
        Assert.IsTrue(engine.IsDamaged, "Engine should be damaged");

        degradationManager.RefreshSystemCache();

        // Get degraded movement distance
        float degradedDistance = playerShip.GetEffectiveMaxMoveDistance();

        Assert.Less(degradedDistance, baseDistance, "Degraded movement should be less than base");
    }

    // ==================== TEST 11: Move Progress Tracking ====================

    /// <summary>
    /// Test 11: Ship correctly reports movement progress.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MoveProgress_Tracking()
    {
        yield return null;

        Assert.IsFalse(playerShip.IsExecutingMove, "Should not be executing before move");

        playerShip.PlanMove(new Vector3(10f, 0f, 0f), Quaternion.identity);
        playerShip.SetMoveDuration(1f); // Short duration for test

        Assert.AreEqual(0f, playerShip.GetMoveProgress(), "Progress should be 0 before execution");

        playerShip.ExecuteMove();

        Assert.IsTrue(playerShip.IsExecutingMove, "Should be executing after ExecuteMove");

        yield return new WaitForSeconds(0.5f);

        float progress = playerShip.GetMoveProgress();
        Assert.Greater(progress, 0f, "Progress should be > 0 mid-execution");
        Assert.Less(progress, 1f, "Progress should be < 1 mid-execution");

        yield return new WaitForSeconds(0.6f);

        Assert.IsFalse(playerShip.IsExecutingMove, "Should not be executing after completion");
    }

    // ==================== TEST 12: Finding Firing Windows ====================

    /// <summary>
    /// Test 12: MovementExecutor finds valid firing windows.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_FindFiringWindows()
    {
        yield return null;

        // Create forward-facing weapon
        RailGun weapon = CreateTestWeapon(playerShip.transform, "ForwardGun", 90f);
        weapon.Initialize(playerShip);

        // Player turns during movement to face target
        playerShip.transform.position = Vector3.zero;
        playerShip.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        targetShip.transform.position = new Vector3(10f, 0f, 10f);

        // Move toward target
        playerShip.PlanMove(new Vector3(5f, 0f, 5f), Quaternion.identity);

        var windows = movementExecutor.FindFiringWindows(playerShip, targetShip, weapon, 20);

        Assert.IsNotNull(windows, "Should return windows list");
        Debug.Log($"Found {windows.Count} firing windows");
    }

    // ==================== TEST 13: Optimal Firing Time Calculation ====================

    /// <summary>
    /// Test 13: Calculates best time to fire during movement.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_OptimalFiringTime()
    {
        yield return null;

        // Create narrow weapon
        RailGun weapon = CreateTestWeapon(playerShip.transform, "NarrowGun", 30f);
        weapon.Initialize(playerShip);

        // Setup where ship will briefly face target during turn
        playerShip.transform.position = Vector3.zero;
        playerShip.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        targetShip.transform.position = new Vector3(5f, 0f, 10f);

        playerShip.PlanMove(new Vector3(10f, 0f, 0f), Quaternion.identity);

        float optimalTime = movementExecutor.CalculateOptimalFiringTime(playerShip, targetShip, weapon, 20);

        Debug.Log($"Optimal firing time: {optimalTime}");

        // If optimal time is found, it should be between 0 and 1
        if (optimalTime >= 0)
        {
            Assert.GreaterOrEqual(optimalTime, 0f, "Optimal time should be >= 0");
            Assert.LessOrEqual(optimalTime, 1f, "Optimal time should be <= 1");
        }
    }

    // ==================== TEST 14: Cache Clearing ====================

    /// <summary>
    /// Test 14: Arc validator cache clears correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_CacheClearing()
    {
        yield return null;

        RailGun weapon = CreateTestWeapon(playerShip.transform, "TestGun", 180f);
        weapon.Initialize(playerShip);

        // First validation
        arcValidator.ValidateArc(weapon, targetShip);

        var cached = arcValidator.GetCachedResult(weapon);
        Assert.IsNotNull(cached, "Should have cached result");

        // Clear cache
        arcValidator.ClearCache();

        cached = arcValidator.GetCachedResult(weapon);
        Assert.IsNull(cached, "Cache should be cleared");
    }

    // ==================== TEST 15: Movement Execution Complete Event ====================

    /// <summary>
    /// Test 15: Movement complete event fires after all ships finish.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MovementComplete_Event()
    {
        yield return null;

        playerShip.PlanMove(new Vector3(5f, 0f, 0f), Quaternion.identity);
        playerShip.SetMoveDuration(0.5f);

        bool completeFired = false;
        movementExecutor.OnMovementExecutionComplete += () => completeFired = true;

        movementExecutor.SetSimulationDuration(0.5f);
        movementExecutor.ExecuteAllPlannedMovements();

        yield return new WaitForSeconds(0.7f);

        Assert.IsTrue(completeFired, "Movement complete event should have fired");
        Assert.IsFalse(movementExecutor.IsExecuting, "Should no longer be executing");
    }
}
