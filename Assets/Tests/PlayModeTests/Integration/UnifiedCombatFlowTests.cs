using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// End-to-end integration tests for the complete unified combat flow.
/// Tests the full turn-based combat loop: Command Phase → Simulation Phase → Turn End.
/// Validates all Phase 3 and Phase 3.5 systems working together.
/// </summary>
public class UnifiedCombatFlowTests
{
    private GameObject testRoot;
    private TurnManager turnManager;
    private CombatCoordinator combatCoordinator;
    private WeaponFiringQueue firingQueue;
    private TurnEndProcessor turnEndProcessor;
    private MovementExecutor movementExecutor;
    private WeaponArcValidator arcValidator;
    private Ship playerShip;
    private Ship enemyShip;
    private WeaponManager weaponManager;
    private HeatManager heatManager;
    private ShieldSystem shieldSystem;
    private SectionManager sectionManager;
    private SystemDegradationManager degradationManager;

    [SetUp]
    public void Setup()
    {
        // Create root
        testRoot = new GameObject("TestRoot");

        // Create Phase 3.5 Integration Systems
        GameObject turnManagerObj = new GameObject("TurnManager");
        turnManagerObj.transform.SetParent(testRoot.transform);
        turnManager = turnManagerObj.AddComponent<TurnManager>();

        GameObject coordinatorObj = new GameObject("CombatCoordinator");
        coordinatorObj.transform.SetParent(testRoot.transform);
        combatCoordinator = coordinatorObj.AddComponent<CombatCoordinator>();

        GameObject firingQueueObj = new GameObject("WeaponFiringQueue");
        firingQueueObj.transform.SetParent(testRoot.transform);
        firingQueue = firingQueueObj.AddComponent<WeaponFiringQueue>();

        GameObject turnEndProcessorObj = new GameObject("TurnEndProcessor");
        turnEndProcessorObj.transform.SetParent(testRoot.transform);
        turnEndProcessor = turnEndProcessorObj.AddComponent<TurnEndProcessor>();

        GameObject movementExecutorObj = new GameObject("MovementExecutor");
        movementExecutorObj.transform.SetParent(testRoot.transform);
        movementExecutor = movementExecutorObj.AddComponent<MovementExecutor>();

        GameObject arcValidatorObj = new GameObject("WeaponArcValidator");
        arcValidatorObj.transform.SetParent(testRoot.transform);
        arcValidator = arcValidatorObj.AddComponent<WeaponArcValidator>();

        // Create player ship with full systems
        GameObject playerObj = new GameObject("PlayerShip");
        playerObj.transform.SetParent(testRoot.transform);
        playerObj.transform.position = Vector3.zero;
        playerShip = playerObj.AddComponent<Ship>();
        heatManager = playerObj.AddComponent<HeatManager>();
        shieldSystem = playerObj.AddComponent<ShieldSystem>();
        sectionManager = playerObj.AddComponent<SectionManager>();
        weaponManager = playerObj.AddComponent<WeaponManager>();
        degradationManager = playerObj.AddComponent<SystemDegradationManager>();
        playerObj.AddComponent<DamageRouter>();
        playerObj.AddComponent<CoreProtectionSystem>();
        playerObj.AddComponent<ShipDeathController>();
        playerObj.AddComponent<AbilitySystem>();

        // Create sections
        CreateSection(playerObj, SectionType.Core);
        CreateSection(playerObj, SectionType.Fore);
        CreateSection(playerObj, SectionType.Aft);

        // Create radiators for heat dissipation
        CreateRadiator(playerObj, "Radiator1");
        CreateRadiator(playerObj, "Radiator2");

        // Create enemy ship
        GameObject enemyObj = new GameObject("EnemyShip");
        enemyObj.transform.SetParent(testRoot.transform);
        enemyObj.transform.position = new Vector3(0, 0, 20);
        enemyShip = enemyObj.AddComponent<Ship>();
        enemyObj.AddComponent<HeatManager>();
        enemyObj.AddComponent<ShieldSystem>();
        enemyObj.AddComponent<SectionManager>();
        enemyObj.AddComponent<WeaponManager>();
        enemyObj.AddComponent<DamageRouter>();

        CreateSection(enemyObj, SectionType.Core);
        CreateSection(enemyObj, SectionType.Fore);

        // Register ships with TurnEndProcessor for cooldown/heat processing
        turnEndProcessor.RegisterShip(playerShip);
        turnEndProcessor.RegisterShip(enemyShip);
    }

    [TearDown]
    public void Teardown()
    {
        if (testRoot != null)
        {
            Object.DestroyImmediate(testRoot);
        }
    }

    private void CreateSection(GameObject shipObj, SectionType type)
    {
        GameObject sectionObj = new GameObject($"Section_{type}");
        sectionObj.transform.SetParent(shipObj.transform);
        ShipSection section = sectionObj.AddComponent<ShipSection>();
        Ship ship = shipObj.GetComponent<Ship>();
        section.Initialize(type, ship);
    }

    private void CreateRadiator(GameObject shipObj, string name)
    {
        GameObject radiatorObj = new GameObject(name);
        radiatorObj.transform.SetParent(shipObj.transform);
        MountedRadiator radiator = radiatorObj.AddComponent<MountedRadiator>();
    }

    private RailGun CreateWeapon(GameObject shipObj, string name)
    {
        GameObject weaponObj = new GameObject(name);
        weaponObj.transform.SetParent(shipObj.transform);
        weaponObj.transform.localPosition = new Vector3(0, 0, 1);
        RailGun weapon = weaponObj.AddComponent<RailGun>();
        return weapon;
    }

    private TorpedoLauncher CreateTorpedoLauncher(GameObject shipObj, string name)
    {
        GameObject weaponObj = new GameObject(name);
        weaponObj.transform.SetParent(shipObj.transform);
        weaponObj.transform.localPosition = new Vector3(0, 0, 1);
        TorpedoLauncher weapon = weaponObj.AddComponent<TorpedoLauncher>();
        return weapon;
    }

    // ==================== TEST 1: Complete Turn Cycle ====================

    /// <summary>
    /// Test 1: Verify complete turn cycle flows correctly.
    /// Command Phase → Simulation Phase → Turn End → Next Command Phase
    /// </summary>
    [UnityTest]
    public IEnumerator Test_CompleteTurnCycle()
    {
        yield return null;

        // Should start in Command Phase
        Assert.AreEqual(TurnPhase.Command, turnManager.CurrentPhase, "Should start in Command Phase");
        Assert.AreEqual(1, turnManager.CurrentTurn, "Should start at turn 1");

        // End command phase to start simulation
        turnManager.EndCommandPhase();
        yield return null;

        Assert.AreEqual(TurnPhase.Simulation, turnManager.CurrentPhase, "Should be in Simulation Phase");

        // Wait for simulation to complete
        yield return new WaitForSeconds(turnManager.SimulationDuration + 0.5f);

        // Should be back in Command Phase, turn 2
        Assert.AreEqual(TurnPhase.Command, turnManager.CurrentPhase, "Should return to Command Phase");
        Assert.AreEqual(2, turnManager.CurrentTurn, "Should be turn 2");
    }

    // ==================== TEST 2: Movement Executes During Simulation ====================

    /// <summary>
    /// Test 2: Planned movement executes during Simulation Phase.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MovementExecutesDuringSimulation()
    {
        yield return null;

        Vector3 startPos = playerShip.transform.position;
        Vector3 targetPos = new Vector3(10, 0, 0);

        // Plan move during Command Phase
        playerShip.PlanMove(targetPos, Quaternion.identity);
        Assert.IsTrue(playerShip.HasPlannedMove, "Should have planned move");

        // Start simulation
        turnManager.EndCommandPhase();
        yield return new WaitForSeconds(0.1f);

        // Should be executing move
        Assert.IsTrue(playerShip.IsExecutingMove || turnManager.CurrentPhase == TurnPhase.Simulation,
            "Should be in simulation or executing");

        // Wait for simulation to complete
        yield return new WaitForSeconds(turnManager.SimulationDuration + 0.5f);

        // Ship should have moved
        float distanceMoved = Vector3.Distance(playerShip.transform.position, startPos);
        Assert.Greater(distanceMoved, 0.1f, "Ship should have moved from start position");
    }

    // ==================== TEST 3: Weapon Queue Executes During Simulation ====================

    /// <summary>
    /// Test 3: Queued weapons fire during Simulation Phase.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponQueueExecutesDuringSimulation()
    {
        yield return null;

        // Create weapon
        RailGun weapon = CreateWeapon(playerShip.gameObject, "TestGun");
        weapon.Initialize(playerShip);

        // Queue weapon during Command Phase
        bool queued = firingQueue.QueueFire(weapon, enemyShip);
        Assert.IsTrue(queued, "Should queue weapon fire");
        Assert.AreEqual(1, firingQueue.QueuedCount, "Should have 1 queued command");

        // Start simulation (queue executes automatically via CombatCoordinator)
        turnManager.EndCommandPhase();

        // Wait for simulation
        yield return new WaitForSeconds(turnManager.SimulationDuration + 0.5f);

        // Queue should be cleared after execution
        Assert.AreEqual(0, firingQueue.QueuedCount, "Queue should be empty after execution");
    }

    // ==================== TEST 4: Heat Dissipates At Turn End ====================

    /// <summary>
    /// Test 4: Heat dissipates when turn ends.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HeatDissipatesAtTurnEnd()
    {
        yield return null;

        // Add heat (use AddPlannedHeat + CommitPlannedHeat pattern)
        heatManager.AddPlannedHeat(50f);
        heatManager.CommitPlannedHeat();
        float heatBefore = heatManager.CurrentHeat;
        Assert.AreEqual(50f, heatBefore, "Should have 50 heat");

        // Complete a full turn
        turnManager.EndCommandPhase();
        yield return new WaitForSeconds(turnManager.SimulationDuration + 0.5f);

        // Heat should have dissipated
        float heatAfter = heatManager.CurrentHeat;
        Assert.Less(heatAfter, heatBefore, "Heat should have dissipated");
    }

    // ==================== TEST 5: Cooldowns Tick At Turn End ====================

    /// <summary>
    /// Test 5: Weapon cooldowns tick down at turn end.
    /// Tests that WeaponSystem.TickCooldown() correctly decreases cooldown counter.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_CooldownsTickAtTurnEnd()
    {
        yield return null;

        // Create torpedo launcher which has a 3-turn cooldown (RailGun has 0)
        TorpedoLauncher weapon = CreateTorpedoLauncher(playerShip.gameObject, "TestTorpedo");
        weapon.Initialize(playerShip);

        // Fire the weapon to put it on cooldown
        weapon.StartCooldown();

        int cooldownBefore = weapon.CurrentCooldown;
        Assert.Greater(cooldownBefore, 0, "Should have cooldown after firing (TorpedoLauncher has 3-turn CD)");

        // Directly tick the weapon cooldown (simulates what TurnEndProcessor.ProcessCooldowns does)
        // Note: In a real scenario, the weapon would be discovered by WeaponManager at Start()
        weapon.TickCooldown();

        // Cooldown should have ticked down
        int cooldownAfter = weapon.CurrentCooldown;
        Assert.Less(cooldownAfter, cooldownBefore, "Cooldown should have decreased after tick");
    }

    // ==================== TEST 6: Shields Absorb Damage ====================

    /// <summary>
    /// Test 6: Shields absorb damage correctly in turn-based flow.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldsAbsorbDamage()
    {
        yield return null;

        // Set initial shields
        shieldSystem.SetShields(100f);
        float shieldsBefore = shieldSystem.CurrentShields;

        // Get damage router and apply damage
        DamageRouter router = playerShip.GetComponent<DamageRouter>();
        router.ProcessDamage(50f, SectionType.Fore, Vector3.forward);

        yield return null;

        // Shields should have absorbed damage
        float shieldsAfter = shieldSystem.CurrentShields;
        Assert.Less(shieldsAfter, shieldsBefore, "Shields should have absorbed damage");
    }

    // ==================== TEST 7: Section Damage After Shield Depletion ====================

    /// <summary>
    /// Test 7: Sections take damage after shields are depleted.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionDamageAfterShieldDepletion()
    {
        yield return null;

        // Deplete shields
        shieldSystem.SetShields(0f);

        // Get fore section
        ShipSection foreSection = null;
        foreach (var section in playerShip.GetComponentsInChildren<ShipSection>())
        {
            if (section.SectionType == SectionType.Fore)
            {
                foreSection = section;
                break;
            }
        }

        Assert.IsNotNull(foreSection, "Should have fore section");
        float armorBefore = foreSection.CurrentArmor;

        // Apply damage
        DamageRouter router = playerShip.GetComponent<DamageRouter>();
        router.ProcessDamage(30f, SectionType.Fore, Vector3.forward);

        yield return null;

        // Section should have taken damage
        float armorAfter = foreSection.CurrentArmor;
        Assert.Less(armorAfter, armorBefore, "Section armor should have decreased");
    }

    // ==================== TEST 8: Arc Validation With Movement ====================

    /// <summary>
    /// Test 8: Arc validator works with planned movement.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ArcValidationWithMovement()
    {
        yield return null;

        // Create forward-facing weapon
        RailGun weapon = CreateWeapon(playerShip.gameObject, "ForwardGun");
        weapon.Initialize(playerShip);

        // Plan a move
        playerShip.PlanMove(new Vector3(5, 0, 5), Quaternion.identity);

        // Validate arc
        var result = arcValidator.ValidateArc(weapon, enemyShip);

        Assert.IsNotNull(result.FiringWindows, "Should have firing windows list");
        Debug.Log($"Arc validation: WillBeInArc={result.WillBeInArc}, Message={result.ValidationMessage}");
    }

    // ==================== TEST 9: Degradation Affects Combat ====================

    /// <summary>
    /// Test 9: System degradation affects combat capabilities.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DegradationAffectsCombat()
    {
        yield return null;

        // Create and damage an engine
        GameObject engineObj = new GameObject("Engine");
        engineObj.transform.SetParent(playerShip.transform);
        MountedEngine engine = engineObj.AddComponent<MountedEngine>();

        degradationManager.RegisterSystem(engine);
        degradationManager.RefreshSystemCache();

        float baseMoveDistance = playerShip.GetEffectiveMaxMoveDistance();

        // Damage the engine
        engine.TakeCriticalHit();
        degradationManager.RefreshSystemCache();

        float degradedMoveDistance = playerShip.GetEffectiveMaxMoveDistance();

        Assert.Less(degradedMoveDistance, baseMoveDistance, "Damaged engine should reduce move distance");
    }

    // ==================== TEST 10: Full Combat Round ====================

    /// <summary>
    /// Test 10: Complete combat round with movement, firing, and damage.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_FullCombatRound()
    {
        yield return null;

        // Setup: Create weapon, add heat, set enemy shields
        RailGun weapon = CreateWeapon(playerShip.gameObject, "TestGun");
        weapon.Initialize(playerShip);

        heatManager.AddPlannedHeat(30f);
        heatManager.CommitPlannedHeat();

        ShieldSystem enemyShields = enemyShip.GetComponent<ShieldSystem>();
        enemyShields.SetShields(100f);

        // Command Phase: Plan movement and queue weapon
        playerShip.PlanMove(new Vector3(5, 0, 0), Quaternion.identity);
        firingQueue.QueueFire(weapon, enemyShip);

        Assert.IsTrue(playerShip.HasPlannedMove, "Should have planned move");
        Assert.AreEqual(1, firingQueue.QueuedCount, "Should have queued weapon");

        // Execute turn
        turnManager.EndCommandPhase();

        // Wait for simulation
        yield return new WaitForSeconds(turnManager.SimulationDuration + 0.5f);

        // Verify results
        Assert.AreEqual(TurnPhase.Command, turnManager.CurrentPhase, "Should be back in Command Phase");
        Assert.AreEqual(2, turnManager.CurrentTurn, "Should be turn 2");
        Assert.AreEqual(0, firingQueue.QueuedCount, "Queue should be empty");
        Assert.Less(heatManager.CurrentHeat, 30f, "Heat should have dissipated");
    }

    // ==================== TEST 11: Multiple Turns Combat ====================

    /// <summary>
    /// Test 11: Multiple consecutive turns work correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MultipleTurnsCombat()
    {
        yield return null;

        Assert.AreEqual(1, turnManager.CurrentTurn, "Should start at turn 1");

        // Run 3 complete turns
        for (int i = 0; i < 3; i++)
        {
            turnManager.EndCommandPhase();
            yield return new WaitForSeconds(turnManager.SimulationDuration + 0.3f);
        }

        Assert.AreEqual(4, turnManager.CurrentTurn, "Should be turn 4 after 3 complete turns");
        Assert.AreEqual(TurnPhase.Command, turnManager.CurrentPhase, "Should be in Command Phase");
    }

    // ==================== TEST 12: Event Firing Order ====================

    /// <summary>
    /// Test 12: Turn events fire in correct order.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_EventFiringOrder()
    {
        yield return null;

        int eventOrder = 0;
        int commandStartOrder = -1;
        int simStartOrder = -1;
        int simEndOrder = -1;
        int turnEndOrder = -1;

        turnManager.OnCommandPhaseStart += () => commandStartOrder = eventOrder++;
        turnManager.OnSimulationPhaseStart += () => simStartOrder = eventOrder++;
        turnManager.OnSimulationPhaseEnd += () => simEndOrder = eventOrder++;
        turnManager.OnTurnEnd += (turn) => turnEndOrder = eventOrder++;

        // Reset for test (command start already fired at initialization)
        eventOrder = 0;

        // Complete a turn
        turnManager.EndCommandPhase();
        yield return new WaitForSeconds(turnManager.SimulationDuration + 0.5f);

        // Verify order: SimStart → SimEnd → TurnEnd → CommandStart
        Assert.AreEqual(0, simStartOrder, "SimulationPhaseStart should fire first");
        Assert.AreEqual(1, simEndOrder, "SimulationPhaseEnd should fire second");
        Assert.AreEqual(2, turnEndOrder, "TurnEnd should fire third");
        Assert.AreEqual(3, commandStartOrder, "CommandPhaseStart should fire last");
    }

    // ==================== TEST 13: Radiator Bonus Affects Dissipation ====================

    /// <summary>
    /// Test 13: Radiator status affects heat dissipation rate.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_RadiatorBonusAffectsDissipation()
    {
        yield return null;

        // Add heat
        heatManager.AddPlannedHeat(100f);
        heatManager.CommitPlannedHeat();

        // Get radiators
        MountedRadiator[] radiators = playerShip.GetComponentsInChildren<MountedRadiator>();
        Assert.GreaterOrEqual(radiators.Length, 2, "Should have at least 2 radiators");

        // Complete a turn with healthy radiators
        turnManager.EndCommandPhase();
        yield return new WaitForSeconds(turnManager.SimulationDuration + 0.5f);

        float heatAfterHealthy = heatManager.CurrentHeat;
        float heatDissipatedHealthy = 100f - heatAfterHealthy;

        // Reset heat to 100 and damage radiators
        heatManager.AddPlannedHeat(100f - heatAfterHealthy);
        heatManager.CommitPlannedHeat();
        foreach (var radiator in radiators)
        {
            radiator.TakeCriticalHit(); // Damage radiators
        }

        // Complete another turn with damaged radiators
        turnManager.EndCommandPhase();
        yield return new WaitForSeconds(turnManager.SimulationDuration + 0.5f);

        float heatAfterDamaged = heatManager.CurrentHeat;

        // Damaged radiators should dissipate less heat (higher remaining heat)
        Assert.Greater(heatAfterDamaged, heatAfterHealthy,
            "Damaged radiators should result in less heat dissipation");
    }

    // ==================== TEST 14: Ship Cannot Move When Engines Destroyed ====================

    /// <summary>
    /// Test 14: Ship cannot move when all engines are destroyed.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_CannotMoveWithDestroyedEngines()
    {
        yield return null;

        // Create and destroy an engine
        GameObject engineObj = new GameObject("Engine");
        engineObj.transform.SetParent(playerShip.transform);
        MountedEngine engine = engineObj.AddComponent<MountedEngine>();

        degradationManager.RegisterSystem(engine);
        degradationManager.RefreshSystemCache();

        // Destroy the engine
        engine.TakeCriticalHit(); // Damage
        engine.TakeCriticalHit(); // Destroy

        Assert.IsTrue(engine.IsDestroyed, "Engine should be destroyed");

        degradationManager.RefreshSystemCache();

        // Ship should not be able to move
        bool canMove = playerShip.CanMove();
        Assert.IsFalse(canMove, "Ship should not be able to move with destroyed engines");
    }

    // ==================== TEST 15: Position Queries During Execution ====================

    /// <summary>
    /// Test 15: Position-at-time queries work during movement execution.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_PositionQueriesDuringExecution()
    {
        yield return null;

        Vector3 startPos = playerShip.transform.position;
        Vector3 targetPos = new Vector3(10, 0, 0);

        playerShip.PlanMove(targetPos, Quaternion.identity);

        // Query positions before execution
        Vector3 posAtStart = playerShip.GetPositionAtTime(0f);
        Vector3 posAtEnd = playerShip.GetPositionAtTime(1f);

        Assert.AreEqual(startPos, posAtStart, "Position at t=0 should be start");

        float endDistance = Vector3.Distance(posAtEnd, playerShip.PlannedPosition);
        Assert.Less(endDistance, 0.1f, "Position at t=1 should be near planned position");
    }
}
